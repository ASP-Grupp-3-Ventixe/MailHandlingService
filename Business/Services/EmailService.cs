using System.Linq.Expressions;
using MailHandlingServiceProvider.Business.DTOs;
using MailHandlingServiceProvider.Business.Mappers;
using MailHandlingServiceProvider.Business.Responses;
using MailHandlingServiceProvider.Data.Entities;
using MailHandlingServiceProvider.Data.Repositories;

namespace MailHandlingServiceProvider.Business.Services;

public interface IEmailService
{
    Task<EmailResult<EmailDto>> CreateEmailAsync(CreateEmailDto createDto, Guid userId);
    Task<EmailListResult<EmailDto>> GetEmailsAsync(Guid userId, string folder = "inbox", bool unreadOnly = false, string? searchQuery = null);
    Task<EmailNavigationResult<EmailDetailsDto>> GetEmailByIdAsync(Guid emailId, Guid userId);
    Task<EmailResult> DeleteEmailAsync(Guid emailId, Guid userId);
    Task<EmailResult> EmptyTrashAsync(Guid emailId, Guid userId);
    
    Task<EmailResult> MarkAsReadAsync(Guid emailId, Guid userId);
    Task<EmailResult> MarkAsUnreadAsync(Guid emailId, Guid userId);
    Task<CountResult> GetUnreadCountAsync(Guid userId, string folder = "inbox");
    
    Task<EmailResult<EmailDto>> ReplyToEmailAsync(Guid emailId, CreateReplyDto replyDto);
    Task<EmailResult<EmailDto>> ForwardEmailAsync(Guid emailId, CreateForwardDto forwardDto);
    
    Task<EmailResult> MoveToFolderAsync(Guid emailId, Guid userId, string folderName);
    
    // star operations
    Task<EmailResult> StarEmailAsync(Guid emailId, Guid userId);
    Task<EmailResult> UnstarEmailAsync(Guid emailId, Guid userId);
    Task<EmailListResult<EmailDto>> GetStarredEmailsAsync(Guid userId);
}

public class EmailService(IEmailRepository emailRepository, ILabelRepository labelRepository, IFolderRepository folderRepository, IAttachmentRepository attachmentRepository, IRecipientRepository recipientRepository) : IEmailService
{
    private readonly IEmailRepository _emailRepository = emailRepository;
    private readonly ILabelRepository _labelRepository = labelRepository;
    private readonly IFolderRepository _folderRepository = folderRepository;
    private readonly IAttachmentRepository _attachmentRepository = attachmentRepository;
    private readonly IRecipientRepository _recipientRepository = recipientRepository;


    public async Task<EmailResult<EmailDto>> CreateEmailAsync(CreateEmailDto? createDto, Guid userId)
    {
        // validate input
        if (createDto == null)
            return new EmailResult<EmailDto> { Succeeded = false, StatusCode = 400, Error = "Invalid email data" };
        
        // find or create the "Sent" folder
        var folderResult = await _folderRepository.GetAsync(f => f.Name == "Sent" && f.IsSystemFolder);
        if (!folderResult.Succeeded)
            return new EmailResult<EmailDto> { Succeeded = false, StatusCode = 500, Error = "Could not find or create Sent folder" };
        
        // convert DTO to entity
        var emailEntity = createDto.ToEntity(userId);
        emailEntity.FolderId = folderResult.Result.Id;
        
        // save email
        var result = await _emailRepository.AddAsync(emailEntity);
        if (!result.Succeeded)
            return new EmailResult<EmailDto> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
        
        // add recipient
        if (createDto.Recipients?.Any() == true)
        {
            foreach (var recipientDto in createDto.Recipients)
            {
                var recipientEntity = recipientDto.ToEntity(emailEntity.Id);
                await _recipientRepository.AddAsync(recipientEntity);
            }
        }
        
        // return result
        var emailDto = emailEntity.ToDto(createDto.Sender);
        return new EmailResult<EmailDto> { Succeeded = true, StatusCode = 201, Result = emailDto };
    }

    public async Task<EmailListResult<EmailDto>> GetEmailsAsync(Guid userId, string folder = "inbox", bool unreadOnly = false, string? searchQuery = null)
    {
        // find folder
        var folderResult = await _folderRepository.GetAsync(f => f.Name.ToLower() == folder.ToLower());
        if (!folderResult.Succeeded)
            return new EmailListResult<EmailDto> { Succeeded = false, StatusCode = 404, Error = $"Folder '{folder}' not found" };

        Guid folderId = folderResult.Result.Id;
        
        // build filter expression
        Expression<Func<EmailEntity, bool>> filter = e => e.FolderId == folderId;
        
        if (unreadOnly)
            filter = e => e.FolderId == folderId && !e.IsRead;

        if (!string.IsNullOrWhiteSpace(searchQuery))
            filter = e => e.FolderId == folderId && 
                          (e.Subject.Contains(searchQuery) || 
                           e.Body.Contains(searchQuery) || 
                           e.Preview.Contains(searchQuery));
        
        // get emails
        var emailsResult = await _emailRepository.GetAllAsync(
            orderByDescending: true,
            sortByColumn: e => e.SentAt,
            filterBy: filter,
            includes:
            [
                e => e.Recipients,
                e => e.Labels
            ]);

        if (!emailsResult.Succeeded)
            return new EmailListResult<EmailDto> { Succeeded = false, StatusCode = emailsResult.StatusCode, Error = emailsResult.Error };
        
        // convert to DTOs
        List<EmailDto> emailDtos = emailsResult.Result.Select(e => e.ToDto()).ToList();
        
        // count unread emails
        int unreadCount = emailsResult.Result.Count(e => !e.IsRead);

        return new EmailListResult<EmailDto> 
        { Succeeded = true, StatusCode = 200, Items = emailDtos, TotalCount = emailDtos.Count, UnreadCount = unreadCount };
    }

    public async Task<EmailNavigationResult<EmailDetailsDto>> GetEmailByIdAsync(Guid emailId, Guid userId)
    {
        // get email with all related data
        var emailResult = await _emailRepository.GetAsync(
            e => e.Id == emailId,
            e => e.Recipients,
            e => e.Labels,
            e => e.Attachments);

        if (!emailResult.Succeeded)
            return new EmailNavigationResult<EmailDetailsDto> { Succeeded = false, StatusCode = emailResult.StatusCode, Error = emailResult.Error };

        var email = emailResult.Result;
        
        // mark as read 
        if (email != null && !email.IsRead)
        {
            email.IsRead = true;
            await _emailRepository.UpdateAsync(email);
        }
        
        // convert to DTO
        var emailDetailsDto = email.ToDetailsDto();
        
        // get previous and next email in the same folder
        var navigationResult = new EmailNavigationResult<EmailDetailsDto>
        {
            Succeeded = true,
            StatusCode = 200,
            Item = emailDetailsDto,
            CurrentPosition = 0,  // calculated based on sorting order?
            TotalCount = 0,       // get from database
            PreviousEmailId = null, // calculated how?
            NextEmailId = null     // calculated how?
        };

        return navigationResult;
    }

    // move email to trash instead of deleting permanently, should also implement a permanent delete method
    public async Task<EmailResult> DeleteEmailAsync(Guid emailId, Guid userId)
    {
        // check if the email exists
        var emailResult = await _emailRepository.GetAsync(e => e.Id == emailId);
        if (!emailResult.Succeeded)
            return new EmailResult { Succeeded = false, StatusCode = emailResult.StatusCode, Error = emailResult.Error };
        
        // find trash folder
        var trashFolderResult = await _folderRepository.GetAsync(f => f.Name.ToLower() == "trash" && f.IsSystemFolder);
        if (!trashFolderResult.Succeeded)
            return new EmailResult { Succeeded = false, StatusCode = 500, Error = "Trash folder not found" };
        
        // move email to trash instead of deleting permanently
        var email = emailResult.Result;
        email.FolderId = trashFolderResult.Result.Id;
        
        var updateResult = await _emailRepository.UpdateAsync(email);
        if (!updateResult.Succeeded)
            return new EmailResult { Succeeded = false, StatusCode = updateResult.StatusCode, Error = updateResult.Error };

        return new EmailResult { Succeeded = true, StatusCode = 200, Result = "Email moved to trash" };
    }
    
    public async Task<EmailResult> EmptyTrashAsync(Guid emailId, Guid userId)
    {
        // find trash folder
        var trashFolderResult = await _folderRepository.GetAsync(f => f.Name.ToLower() == "trash" && f.IsSystemFolder);
        if (!trashFolderResult.Succeeded)
            return new EmailResult { Succeeded = false, StatusCode = 500, Error = "Trash folder not found" };
        
        // get emails in the trash
        var emailsResult = await _emailRepository.GetAllAsync(
            orderByDescending: true,
            sortByColumn: e => e.SentAt,
            filterBy: e => e.FolderId == trashFolderResult.Result.Id);
        
        if (!emailsResult.Succeeded)
            return new EmailResult { Succeeded = false, StatusCode = emailsResult.StatusCode, Error = emailsResult.Error };

        int count = emailsResult.Result.Count();
        
        // Delete emails one by one using the correct expression syntax
        foreach (var email in emailsResult.Result)
        {
            await _emailRepository.DeleteAsync(e => e.Id == email.Id);
        }
        
        return new EmailResult { Succeeded = true, StatusCode = 200, Result = $"{count} emails deleted from trash" }; 
    }

    public Task<EmailResult> MarkAsReadAsync(Guid emailId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<EmailResult> MarkAsUnreadAsync(Guid emailId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<CountResult> GetUnreadCountAsync(Guid userId, string folder = "inbox")
    {
        throw new NotImplementedException();
    }

    public Task<EmailResult<EmailDto>> ReplyToEmailAsync(Guid emailId, CreateReplyDto replyDto)
    {
        throw new NotImplementedException();
    }

    public Task<EmailResult<EmailDto>> ForwardEmailAsync(Guid emailId, CreateForwardDto forwardDto)
    {
        throw new NotImplementedException();
    }

    public Task<EmailResult> MoveToFolderAsync(Guid emailId, Guid userId, string folderName)
    {
        throw new NotImplementedException();
    }

    public Task<EmailResult> StarEmailAsync(Guid emailId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<EmailResult> UnstarEmailAsync(Guid emailId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<EmailListResult<EmailDto>> GetStarredEmailsAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}