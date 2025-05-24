using System.ComponentModel.DataAnnotations;

namespace MailHandlingServiceProvider.Business.DTOs;

public class CreateLabelDto
{
    [Required]
    public string? Name { get; set; }
    
    [Required]
    public string? Color { get; set; }
}