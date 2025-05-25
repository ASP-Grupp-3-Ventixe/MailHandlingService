using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CreateEmailDto
{
    [Required]
    public string? Subject { get; set; }
    
    [Required]
    public string Body { get; set; } = null!;
    
    // [Required]
    // public SenderDto Sender { get; set; } = null!; // Is the logged-in user

    [Required]
    public List<RecipientDto> Recipients { get; set; } = [];
    
    public List<Guid>? LabelIds { get; set; } = [];
    
    public List<Guid>? AttachmentIds { get; set; } = [];

}