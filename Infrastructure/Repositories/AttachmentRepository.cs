using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Contexts;

namespace Infrastructure.Repositories;

public class AttachmentRepository(MailDbContext context) : BaseRepository<AttachmentEntity>(context), IAttachmentRepository
{
}