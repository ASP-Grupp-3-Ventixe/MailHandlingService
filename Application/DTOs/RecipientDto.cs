using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class RecipientDto
{
    public Guid? Id { get; set; }
    
    [Required]
    public string? Name { get; set; }
    
    [Required]
    [Display(Name = "Email address")]
    [EmailAddress]
    public string? Email { get; set; }
    
    [Required]
    public string RecipientType { get; set; } = null!; // To, CC, BCC
}

public static class RecipientTypes
{
    public const string To = "To";
    public const string CC = "CC";
    public const string BCC = "BCC";
}