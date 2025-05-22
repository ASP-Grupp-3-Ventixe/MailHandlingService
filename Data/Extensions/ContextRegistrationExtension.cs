using MailHandlingServiceProvider.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MailHandlingServiceProvider.Data.Extensions;

public static class ContextRegistrationExtension
{
    public static IServiceCollection AddContexts(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MailDbContext>(x => x.UseSqlServer(connectionString));
        return services;
    }
}