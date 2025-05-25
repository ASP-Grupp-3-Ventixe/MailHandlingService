using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CreateForwardDto
{
    [Required]
    public Guid OriginalEmailId { get; set; }
    
    [Required]
    public List<RecipientDto> Recipients { get; set; } = [];
    
    public string? AdditionalComment { get; set; }
    
    public List<Guid>? LabelIds { get; set; } = [];
    
    public List<Guid>? AttachmentIds { get; set; } = [];
    
    [Required]
    public SenderDto Sender { get; set; } = null!;
}
