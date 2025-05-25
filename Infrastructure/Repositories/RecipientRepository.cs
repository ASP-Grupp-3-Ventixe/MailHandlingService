using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Contexts;

namespace Infrastructure.Repositories;

public class RecipientRepository(MailDbContext context) : BaseRepository<RecipientEntity>(context), IRecipientRepository
{
}