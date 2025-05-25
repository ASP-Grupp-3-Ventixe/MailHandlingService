namespace Application.DTOs;

public class EmailDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = null!;
    public string Preview { get; set; } = null!;
    public SenderDto? Sender { get; set; } = null!;
    
    public DateTime SentAt { get; set; }
    public string Time { get; set; } = null!;
    public string Date { get; set; } = null!;
    
    public bool IsRead { get; set; }
    public bool IsStarred { get; set; }
    
    public List<string?> Labels { get; set; } = [];
    
    public List<RecipientDto?> Recipients { get; set; } = [];
}