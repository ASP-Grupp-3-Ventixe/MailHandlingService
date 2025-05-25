using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class RecipientMapper
{
    public static RecipientDto? ToDto(this RecipientEntity? entity)
    {
        if (entity == null) return null;
        
        return new RecipientDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Email = entity.EmailAddress,
            RecipientType = entity.RecipientType
        };
    }
    
    public static RecipientEntity? ToEntity(this RecipientDto? dto, Guid emailId)
    {
        if (dto == null) return null;
        
        return new RecipientEntity
        {
            EmailId = emailId,
            Name = dto.Name,
            EmailAddress = dto.Email,
            RecipientType = dto.RecipientType,
            UserId = dto.Id 
        };
    }
}