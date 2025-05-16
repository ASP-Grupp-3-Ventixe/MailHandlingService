using MailHandlingServiceProvider.Data.Contexts;
using MailHandlingServiceProvider.Data.Entities;

namespace MailHandlingServiceProvider.Data.Repositories;

public interface IFolderRepository : IBaseRepository<FolderEntity>
{
    
}

public class FolderRepository(MailDbContext context) : BaseRepository<FolderEntity>(context), IFolderRepository
{
    
}
