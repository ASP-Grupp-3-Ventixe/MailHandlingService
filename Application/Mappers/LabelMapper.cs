using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class LabelMapper
{
    public static LabelDto? ToDto(this LabelEntity? entity)
    {
        if (entity == null) return null;
        
        return new LabelDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Color = entity.Color
        };
    }
    
    public static LabelEntity? ToEntity(this CreateLabelDto? dto, Guid userId)
    {
        if (dto == null) return null;
        
        return new LabelEntity
        {
            Name = dto.Name,
            Color = dto.Color,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public static void UpdateFromDto(this LabelEntity? entity, UpdateLabelDto? dto)
    {
        if (entity == null || dto == null) return;
        
        if (!string.IsNullOrEmpty(dto.Name))
            entity.Name = dto.Name;
            
        if (!string.IsNullOrEmpty(dto.Color))
            entity.Color = dto.Color;
    }
}