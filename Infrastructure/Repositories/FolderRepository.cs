using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Contexts;

namespace Infrastructure.Repositories;

public class FolderRepository(MailDbContext context) : BaseRepository<FolderEntity>(context), IFolderRepository
{
}
