using MailHandlingServiceProvider.Data.Contexts;
using MailHandlingServiceProvider.Data.Entities;

namespace MailHandlingServiceProvider.Data.Repositories;

public interface IRecipientRepository : IBaseRepository<RecipientEntity>
{
}

public class RecipientRepository(MailDbContext context) : BaseRepository<RecipientEntity>(context), IRecipientRepository
{
}