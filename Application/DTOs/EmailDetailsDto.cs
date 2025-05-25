namespace Application.DTOs;

public class EmailDetailsDto : EmailDto
{
    public string Body { get; set; } = null!;
    public List<RecipientDto?> Recipients { get; set; } = [];
    public List<AttachmentDto> Attachments { get; set; } = [];

    public Guid? ReplyToId { get; set; }
    public Guid? ForwardOfId { get; set; }
    public List<EmailDto>? Replies { get; set; } = [];

    public DateTime? ReadAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}