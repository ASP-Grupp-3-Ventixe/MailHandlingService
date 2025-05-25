using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class SenderDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    public string? Initials { get; set; }
    public string? AvatarUrl { get; set; }
    public string? SenderType { get; set; } // Customer, Sponsor, Partner, etc.
}