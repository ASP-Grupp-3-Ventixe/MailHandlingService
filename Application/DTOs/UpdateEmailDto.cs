namespace Application.DTOs;

public class UpdateEmailDto
{
    public bool? IsRead { get; set; }
    public bool? IsStarred { get; set; }
    public string? FolderName { get; set; }
    
    public List<Guid> AddLabelIds { get; set; } = [];
    public List<Guid> RemoveLabelIds { get; set; } = [];
}