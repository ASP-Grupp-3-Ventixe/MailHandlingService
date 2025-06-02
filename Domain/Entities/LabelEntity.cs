using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class LabelEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string? Name { get; set; }

    [Required]
    public Guid UserId { get; set; } // connects to UserId 

    public string? Color { get; set; } 

    public DateTime CreatedAt { get; set; }
    
    public virtual ICollection<EmailLabelEntity> EmailLabels { get; set; } = new List<EmailLabelEntity>();
}