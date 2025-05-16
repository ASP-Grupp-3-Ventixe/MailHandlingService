using MailHandlingServiceProvider.Data.Contexts;
using MailHandlingServiceProvider.Data.Entities;

namespace MailHandlingServiceProvider.Data.Repositories;

public interface IEmailRepository : IBaseRepository<EmailEntity>
{
}

public class EmailRepository(MailDbContext context) : BaseRepository<EmailEntity>(context), IEmailRepository
{
}
