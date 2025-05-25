using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class RecipientEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey(nameof(Email))]
    public Guid EmailId { get; set; }
    public virtual EmailEntity Email { get; set; } = null!;

    public Guid? UserId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string EmailAddress { get; set; } = null!;

    [Required]
    public string RecipientType { get; set; } = null!; // To, CC, BCC
}

public enum RecipientType
{
    To,
    CC,
    BCC
}