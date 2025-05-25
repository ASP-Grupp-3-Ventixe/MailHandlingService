using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class EmailEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SenderId { get; set; } // connects to UserId

    [Required]
    public string Subject { get; set; } = null!;

    [Required]
    public string Body { get; set; } = null!;

    [Required]
    public string Preview { get; set; } = null!; // Generated from body

    [Required]
    public DateTime SentAt { get; set; }

    [Required]
    public bool IsRead { get; set; }

    [Required]
    public bool IsStarred { get; set; }

    [ForeignKey(nameof(Folder))]
    public Guid FolderId { get; set; }
    public virtual FolderEntity Folder { get; set; } = null!; // Inbox, Sent, Drafts, Trash, etc.

    public Guid? ReplyToId { get; set; }
    public Guid? ForwardOfId { get; set; }

    [ForeignKey(nameof(ReplyToId))]
    public virtual EmailEntity? ReplyTo { get; set; }

    [ForeignKey(nameof(ForwardOfId))]
    public virtual EmailEntity? ForwardOf { get; set; }

    public virtual ICollection<RecipientEntity> Recipients { get; set; } = new List<RecipientEntity>();
    public virtual ICollection<EmailLabelEntity> Labels { get; set; } = new List<EmailLabelEntity>();
    public virtual ICollection<AttachmentEntity> Attachments { get; set; } = new List<AttachmentEntity>();
}