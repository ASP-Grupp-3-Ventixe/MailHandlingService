using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ContextRegistrationExtension
{
    public static IServiceCollection AddContexts(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MailDbContext>(x => x.UseSqlServer(connectionString));
        return services;
    }
}