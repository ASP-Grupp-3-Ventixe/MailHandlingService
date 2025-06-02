using System.Linq.Expressions;
using Application.DTOs;
using Application.Interfaces;
using Application.Mappers;
using Application.Responses;
using Domain.Entities;

namespace Application.Services;

public interface IEmailService
{
    Task<EmailResult<EmailDto>> CreateEmailAsync(CreateEmailDto createDto, Guid userId);
    Task<EmailListResult<EmailDto>> GetEmailsAsync(Guid userId, string folder = "inbox", bool unreadOnly = false, string? searchQuery = null);
    Task<EmailNavigationResult<EmailDetailsDto>> GetEmailByIdAsync(Guid emailId, Guid userId);
    
    Task<EmailResult> SoftDeleteEmailAsync(Guid emailId, Guid userId);
    Task<EmailResult> HardDeleteEmailAsync(Guid emailId, Guid userId);
    Task<EmailResult> EmptyTrashAsync(Guid userId);
    
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

public class CreateEmail(IEmailRepository emailRepository, ILabelRepository labelRepository, IFolderRepository folderRepository, IAttachmentRepository attachmentRepository, IRecipientRepository recipientRepository) : IEmailService
{
    private readonly IEmailRepository _emailRepository = emailRepository;
    private readonly ILabelRepository _labelRepository = labelRepository;
    private readonly IFolderRepository _folderRepository = folderRepository;
    private readonly IAttachmentRepository _attachmentRepository = attachmentRepository;
    private readonly IRecipientRepository _recipientRepository = recipientRepository;


    public async Task<EmailResult<EmailDto>> CreateEmailAsync(CreateEmailDto? createDto, Guid userId)
    {
        if (createDto == null)
            return new EmailResult<EmailDto> { Succeeded = false, StatusCode = 400, Error = "Invalid email data" };
    
        // Get "Sent" folder: Needed to store sent emails in correct place
        var folderResult = await _folderRepository.GetAsync(f => f.Name == "Sent" && f.IsSystemFolder);
        if (!folderResult.Succeeded)
            return new EmailResult<EmailDto> { Succeeded = false, StatusCode = 500, Error = "Could not find Sent folder" };
    
        // Map DTO to entity: Prepare data for DB insert
        var emailEntity = createDto.ToEntity(userId);
        emailEntity.FolderId = folderResult.Result.Id;
    
        // Save email: Persist new email to DB
        var result = await _emailRepository.AddAsync(emailEntity);
        if (!result.Succeeded)
            return new EmailResult<EmailDto> { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    
        // Add recipients: Ensure all recipients are linked to the email
        if (createDto.Recipients?.Any() == true)
        {
            var tasks = createDto.Recipients.Select(r => 
                _recipientRepository.AddAsync(r.ToEntity(emailEntity.Id)));
            await Task.WhenAll(tasks); // Improves performance by parallelizing DB calls
        }
        
        var senderDto = CreateSenderDto(emailEntity);
        var emailDto = emailEntity.ToDto(senderDto);
        return new EmailResult<EmailDto> { Succeeded = true, StatusCode = 201, Result = emailDto };
    }

    public async Task<EmailListResult<EmailDto>> GetEmailsAsync(Guid userId, string folder = "inbox", bool unreadOnly = false, string? searchQuery = null)
    {
        // Find folder: Needed to scope emails to a specific folder
        var folderResult = await _folderRepository.GetAsync(f => f.Name.ToLower() == folder.ToLower());
        if (!folderResult.Succeeded)
            return new EmailListResult<EmailDto> { Succeeded = false, StatusCode = 404, Error = $"Folder '{folder}' not found" };

        var folderId = folderResult.Result.Id;
        
        // Build filter: Controls which emails are included in the result
        var filter = BuildEmailFilter(folderId, userId, unreadOnly, searchQuery);
        
        // Query emails: Fetch emails from DB with filter and includes
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
        
        // Map to DTOs: Convert entities to API result objects
        List<EmailDto> emailDtos = emailsResult.Result.Select(e => e.ToDto(CreateSenderDto(e))).ToList();
        
        // Count unread: For UI badge or summary
        int unreadCount = emailsResult.Result.Count(e => !e.IsRead);

        // Return result: All emails, total count, and unread count
        return new EmailListResult<EmailDto> 
        { Succeeded = true, StatusCode = 200, Items = emailDtos, TotalCount = emailDtos.Count, UnreadCount = unreadCount };
    }

    public async Task<EmailNavigationResult<EmailDetailsDto>> GetEmailByIdAsync(Guid emailId, Guid userId)
    {
        // Fetch email with all related data: recipients, labels, attachments for details view
        var emailResult = await _emailRepository.GetAsync(
            e => e.Id == emailId,
            e => e.Recipients,
            e => e.Labels,
            e => e.Attachments);

        if (!emailResult.Succeeded)
            return new EmailNavigationResult<EmailDetailsDto> { Succeeded = false, StatusCode = emailResult.StatusCode, Error = emailResult.Error };

        var email = emailResult.Result;

        // Authorization: Only sender or recipient can access this email
        if (email.SenderId != userId && email.Recipients.All(r => r.UserId != userId))
            return new EmailNavigationResult<EmailDetailsDto>
            { Succeeded = false, StatusCode = 403, Error = "You do not have permission to access this email" };

        // Mark as read: Updates status for UI and unread counts
        if (!email.IsRead)
        {
            email.IsRead = true;
            await _emailRepository.UpdateAsync(email);
        }
        // Create sender DTO: Needed for details view to show who sent the email
        // Map to details DTO: Convert email entity to details view object
        var senderDto = CreateSenderDto(email);
        var emailDetailsDto = email.ToDetailsDto(senderDto);

        // TODO: Calculate navigation logic (previous/next/position/total) for UI navigation in InboxDetail header
        var navigationResult = new EmailNavigationResult<EmailDetailsDto>
        {
            Succeeded = true,
            StatusCode = 200,
            Item = emailDetailsDto,
            CurrentPosition = 0,      // Needs sorting logic for real value
            TotalCount = 0,           // Should fetch count of emails in folder
            PreviousEmailId = null,   // Should fetch previous email in folder
            NextEmailId = null        // Should fetch next email in folder
        };

        return navigationResult;
    }

    // Move email to trash: Soft delete so user can restore later
    public async Task<EmailResult> SoftDeleteEmailAsync(Guid emailId, Guid userId)
    {
        // Fetch the email with recipients for permission check
        var emailResult = await _emailRepository.GetAsync(
            e => e.Id == emailId,
            e => e.Recipients);

        if (!emailResult.Succeeded)
            return new EmailResult { Succeeded = false, StatusCode = emailResult.StatusCode, Error = emailResult.Error };

        var email = emailResult.Result;

        // Authorization: Only sender or recipient can move the email to trash
        if (email.SenderId != userId && email.Recipients.All(r => r.UserId != userId))
            return new EmailResult { Succeeded = false, StatusCode = 403, Error = "You don't have permission to move this email." };

        // Get the Trash folder
        var trashFolderResult = await _folderRepository.GetAsync(f => f.Name.ToLower() == "trash" && f.IsSystemFolder);
        if (!trashFolderResult.Succeeded)
            return new EmailResult { Succeeded = false, StatusCode = 500, Error = "Trash folder not found" };

        // Move the email to Trash
        email.FolderId = trashFolderResult.Result.Id;

        // Save changes to the database
        var updateResult = await _emailRepository.UpdateAsync(email);
        if (!updateResult.Succeeded)
            return new EmailResult { Succeeded = false, StatusCode = updateResult.StatusCode, Error = updateResult.Error };

        return new EmailResult { Succeeded = true, StatusCode = 200, Result = "Email moved to trash" };
    }
    
    // Hard delete: Permanently remove email from DB, cannot be restored
    public async Task<EmailResult> HardDeleteEmailAsync(Guid emailId, Guid userId)
    {
        // Fetch the email with recipients/labels for permission check and cleanup
        var emailResult = await _emailRepository.GetAsync(
            e => e.Id == emailId,
            e => e.Recipients,
            e => e.Labels);

        if (!emailResult.Succeeded)
            return new EmailResult { Succeeded = false, StatusCode = emailResult.StatusCode, Error = emailResult.Error };

        var email = emailResult.Result;

        // Permission check: Only sender or recipient can delete
        if (email.SenderId != userId && email.Recipients.All(r => r.UserId != userId))
            return new EmailResult { Succeeded = false, StatusCode = 403, Error = "You don't have permission to delete this email" };

        // Delete from DB: Remove email permanently
        var deleteResult = await _emailRepository.DeleteAsync(e => e.Id == emailId);
        if (!deleteResult.Succeeded)
            return new EmailResult { Succeeded = false, StatusCode = deleteResult.StatusCode, Error = deleteResult.Error };

        return new EmailResult { Succeeded = true, StatusCode = 200, Result = "Email permanently deleted" };
    }
    
    // Empty trash: Permanently delete all emails in trash folder
    public async Task<EmailResult> EmptyTrashAsync(Guid userId)
    {
        // Get Trash folder: Needed to find emails to delete
        var trashFolderResult = await _folderRepository.GetAsync(f => f.Name.ToLower() == "trash" && f.IsSystemFolder);
        if (!trashFolderResult.Succeeded)
            return new EmailResult { Succeeded = false, StatusCode = 500, Error = "Trash folder not found" };

        var trashFolderId = trashFolderResult.Result.Id;

        // Fetch all emails in trash 
        var emailsResult = await _emailRepository.GetAllAsync(
            filterBy: e => e.FolderId == trashFolderId && 
                           (e.SenderId == userId || e.Recipients.Any(r => r.UserId == userId)));

        if (!emailsResult.Succeeded)
            return new EmailResult { Succeeded = false, StatusCode = emailsResult.StatusCode, Error = emailsResult.Error };

        var count = emailsResult.Result.Count();
        if (count == 0)
            return new EmailResult { Succeeded = true, StatusCode = 200, Result = "Trash is already empty" };

        // Delete all emails in trash in one DB call
        var deleteResult = await _emailRepository.DeleteManyAsync(
            e => e.FolderId == trashFolderId && 
                 (e.SenderId == userId || e.Recipients.Any(r => r.UserId == userId)));

        if (!deleteResult.Succeeded)
            return new EmailResult { Succeeded = false, StatusCode = deleteResult.StatusCode, Error = deleteResult.Error };

        return new EmailResult { Succeeded = true, StatusCode = 200, Result = $"{count} emails permanently deleted" };
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
    
    
    /* Helpers and extensions */ 
    private SenderDto CreateSenderDto(EmailEntity entity)
    {
        return new SenderDto
        {
            Id = entity.SenderId,
        };
    }
    
    // Build filter: Controls which emails are included in the result
    private static Expression<Func<EmailEntity, bool>> BuildEmailFilter(Guid folderId, Guid userId, bool unreadOnly, string? searchQuery)
    {
        return e =>
            e.FolderId == folderId &&
            (e.SenderId == userId || e.Recipients.Any(r => r.UserId == userId)) &&
            (string.IsNullOrWhiteSpace(searchQuery) ||
             e.Subject.Contains(searchQuery) ||
             e.Body.Contains(searchQuery) ||
             e.Preview.Contains(searchQuery)) &&
            (!unreadOnly || !e.IsRead);
    }
    
}