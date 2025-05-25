namespace Application.DTOs;

public class AttachmentDto
{
    public Guid Id { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long Size { get; set; }
}