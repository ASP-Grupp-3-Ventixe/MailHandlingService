using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Contexts;

namespace Infrastructure.Repositories;

public class EmailRepository(MailDbContext context) : BaseRepository<EmailEntity>(context), IEmailRepository
{
}
