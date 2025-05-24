using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MailHandlingServiceProvider.Data.Entities;

public class LabelEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public Guid UserId { get; set; } // connects to UserId 

    public string Color { get; set; } 

    public DateTime CreatedAt { get; set; }
    
    public virtual ICollection<EmailLabelEntity> EmailLabels { get; set; } = new List<EmailLabelEntity>();
}