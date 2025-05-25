using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CreateLabelDto
{
    [Required]
    public string? Name { get; set; }
    
    [Required]
    public string? Color { get; set; }
}