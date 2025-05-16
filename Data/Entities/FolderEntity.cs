using System.ComponentModel.DataAnnotations;

namespace MailHandlingServiceProvider.Data.Entities;

public class FolderEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public bool IsSystemFolder { get; set; }  // True för Inbox, Sent, etc.

    public virtual ICollection<EmailEntity> Emails { get; set; } = new List<EmailEntity>();
    
}

public enum SystemFolderType
{
    Inbox,
    Starred,
    Sent,
    Drafts,
    Spam,
    Trash
}
