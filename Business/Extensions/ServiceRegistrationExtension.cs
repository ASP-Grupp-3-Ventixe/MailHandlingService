using MailHandlingServiceProvider.Business.Services;
using MailHandlingServiceProvider.Data.Contexts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MailHandlingServiceProvider.Business.Extensions;

public static class ServiceRegistrationExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ILabelService, LabelService>();
        services.AddScoped<MailDbContext, MailDbContext>();
        
        return services; 
    }
}
