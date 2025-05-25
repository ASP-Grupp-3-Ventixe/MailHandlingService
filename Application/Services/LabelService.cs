using Application.DTOs;
using Application.Interfaces;
using Application.Responses;

namespace Application.Services;

public interface ILabelService
{ 
    
    Task<EmailListResult<LabelDto>> GetLabelsAsync(Guid userId);
    
    Task<LabelResult<LabelDto>> GetLabelByIdAsync(Guid labelId, Guid userId);
    Task<LabelResult<LabelDto>> CreateLabelAsync(CreateLabelDto createDto, Guid userId);
    Task<LabelResult<LabelDto>> UpdateLabelAsync(Guid labelId, UpdateLabelDto updateDto, Guid userId);
    Task<LabelResult> DeleteLabelAsync(Guid labelId, Guid userId);
    
    // connection between email and labels
    Task<LabelResult> AddLabelToEmailAsync(Guid emailId, Guid labelId, Guid userId);
    Task<LabelResult> RemoveLabelFromEmailAsync(Guid emailId, Guid labelId, Guid userId);
    
    // get all emails for a label
    Task<EmailListResult<EmailDto>> GetEmailsByLabelIdAsync(Guid labelId, Guid userId);
}

public class LabelService(ILabelRepository labelRepository, IEmailRepository emailRepository) : ILabelService
{
    private readonly ILabelRepository _labelRepository = labelRepository;
    private readonly IEmailRepository _emailRepository = emailRepository;
    

    public Task<EmailListResult<LabelDto>> GetLabelsAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<LabelResult<LabelDto>> GetLabelByIdAsync(Guid labelId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<LabelResult<LabelDto>> CreateLabelAsync(CreateLabelDto createDto, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<LabelResult<LabelDto>> UpdateLabelAsync(Guid labelId, UpdateLabelDto updateDto, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<LabelResult> DeleteLabelAsync(Guid labelId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<LabelResult> AddLabelToEmailAsync(Guid emailId, Guid labelId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<LabelResult> RemoveLabelFromEmailAsync(Guid emailId, Guid labelId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<EmailListResult<EmailDto>> GetEmailsByLabelIdAsync(Guid labelId, Guid userId)
    {
        throw new NotImplementedException();
    }
}