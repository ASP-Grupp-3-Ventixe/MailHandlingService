using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class EmailLabelEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(Email))]
    public Guid EmailId { get; set; }
    public virtual EmailEntity Email { get; set; } = null!;

    [ForeignKey(nameof(Label))]
    public Guid LabelId { get; set; }
    public virtual LabelEntity Label { get; set; } = null!;
}