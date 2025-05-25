using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class AttachmentMapper
{
    public static AttachmentDto? ToDto(this AttachmentEntity? entity)
    {
        if (entity == null) return null;
        
        return new AttachmentDto
        {
            Id = entity.Id,
            FileName = entity.FileName,
            ContentType = entity.ContentType,
            Size = entity.Size
        };
    }
    
    public static AttachmentEntity? ToEntity(this AttachmentDto? dto, Guid emailId, string storagePath)
    {
        if (dto == null) return null;
        
        return new AttachmentEntity
        {
            EmailId = emailId,
            FileName = dto.FileName,
            ContentType = dto.ContentType,
            Size = dto.Size,
            StoragePath = storagePath,
            UploadedAt = DateTime.UtcNow
        };
    }
}