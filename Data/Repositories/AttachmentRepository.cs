using MailHandlingServiceProvider.Data.Contexts;
using MailHandlingServiceProvider.Data.Entities;

namespace MailHandlingServiceProvider.Data.Repositories;

public interface IAttachmentRepository : IBaseRepository<AttachmentEntity>
{
}

public class AttachmentRepository(MailDbContext context) : BaseRepository<AttachmentEntity>(context), IAttachmentRepository
{
}