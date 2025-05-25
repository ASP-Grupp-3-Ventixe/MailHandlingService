namespace Application.DTOs;

public class FolderCountsDto
{
    public int Inbox { get; set; }
    public int Unread { get; set; }
    public int Starred { get; set; }
    public int Sent { get; set; }
    public int Drafts { get; set; }
    public int Spam { get; set; }
    public int Trash { get; set; }
}