using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class AttachmentEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid EmailId { get; set; }
    [ForeignKey(nameof(EmailId))]
    public virtual EmailEntity EmailEntity { get; set; } = null!;

    [Required]
    public string? FileName { get; set; } 

    [Required]
    public string? ContentType { get; set; } 

    [Required]
    public long Size { get; set; }

    [Required]
    public string StoragePath { get; set; } = null!;

    [Required]
    public DateTime UploadedAt { get; set; }
}