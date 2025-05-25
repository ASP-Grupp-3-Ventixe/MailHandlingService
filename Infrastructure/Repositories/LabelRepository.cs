using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Contexts;

namespace Infrastructure.Repositories;

public class LabelRepository(MailDbContext context) : BaseRepository<LabelEntity>(context), ILabelRepository
{
}