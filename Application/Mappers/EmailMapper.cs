using System.Text.RegularExpressions;
using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class EmailMapper
{
    // map from entity to DTO (list view)
    public static EmailDto? ToDto(this EmailEntity? entity, SenderDto? sender = null)
    {
        if (entity == null) return null;
        
        return new EmailDto
        {
            Id = entity.Id,
            Sender = sender,
            Subject = entity.Subject,
            Preview = entity.Preview,
            SentAt = entity.SentAt,
            Time = entity.SentAt.ToString("HH:mm"),
            Date = entity.SentAt.ToString("yyyy-MM-dd"),
            IsRead = entity.IsRead,
            IsStarred = entity.IsStarred,
            Labels = entity.Labels.Select(el => el.Label.Name).ToList() ?? [],
            Recipients = entity.Recipients?.Select(r => r.ToDto()).ToList() ?? []
        };
    }
    
    // map from entity to DTO (details view)
    public static EmailDetailsDto? ToDetailsDto(this EmailEntity? entity, SenderDto? sender = null)
    {
        if (entity == null) return null;
        
        var dto = new EmailDetailsDto
        {
            Id = entity.Id,
            Sender = sender,
            Subject = entity.Subject,
            Preview = entity.Preview,
            Body = entity.Body,
            SentAt = entity.SentAt,
            Time = entity.SentAt.ToString("HH:mm"),
            Date = entity.SentAt.ToString("yyyy-MM-dd"),
            IsRead = entity.IsRead,
            IsStarred = entity.IsStarred,
            ReplyToId = entity.ReplyToId,
            ForwardOfId = entity.ForwardOfId,
            Labels = entity.Labels?.Select(el => el.Label.Name).ToList() ?? [],
            Recipients = entity.Recipients?.Select(r => r.ToDto()).ToList() ?? [],
            Attachments = entity.Attachments?.Select(a => a.ToDto()).ToList() ?? []
        };
        
        return dto;
    }
    
    // map from CreateEmailDto to EmailEntity
    public static EmailEntity? ToEntity(this CreateEmailDto? dto, Guid senderId)
    {
        if (dto == null) return null;
        
        var preview = GeneratePreview(dto.Body);
        
        var entity = new EmailEntity
        {
            Subject = dto.Subject,
            Body = dto.Body,
            Preview = preview,
            SenderId = senderId,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            IsStarred = false
        };
        
        return entity;
    }
    
    // Helper method to generate preview from body
    private static string GeneratePreview(string body, int maxLength = 200)
    {
        if (string.IsNullOrEmpty(body)) return string.Empty;
    
        // Remove all HTML tags
        string plainText = Regex.Replace(body, "<[^>]*>", "");
    
        // Remove extra whitespace
        plainText = Regex.Replace(plainText, @"\s+", " ").Trim();

        // Limit length
        return plainText.Length > maxLength ? plainText[..maxLength] : plainText;
    }
    
    // update existing entity based on update DTO
    public static void UpdateFromDto(this EmailEntity? entity, UpdateEmailDto? dto)
    {
        if (entity == null || dto == null) return;
        
        if (dto.IsRead.HasValue)
            entity.IsRead = dto.IsRead.Value;
            
        if (dto.IsStarred.HasValue)
            entity.IsStarred = dto.IsStarred.Value;
    }
}