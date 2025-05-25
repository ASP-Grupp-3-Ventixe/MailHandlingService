using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CreateReplyDto
{
    [Required]
    public Guid OriginalEmailId { get; set; }
    
    [Required]
    public string Body { get; set; } = null!;
    
    public List<RecipientDto>? AdditionalRecipients { get; set; } = [];
    
    public List<Guid>? LabelIds { get; set; } = [];
    
    public List<Guid>? AttachmentIds { get; set; } = [];
    
    [Required]
    public SenderDto Sender { get; set; } = null!;
}