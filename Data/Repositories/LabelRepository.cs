using MailHandlingServiceProvider.Data.Contexts;
using MailHandlingServiceProvider.Data.Entities;

namespace MailHandlingServiceProvider.Data.Repositories;

public interface ILabelRepository : IBaseRepository<LabelEntity>
{
}

public class LabelRepository(MailDbContext context) : BaseRepository<LabelEntity>(context), ILabelRepository
{
}