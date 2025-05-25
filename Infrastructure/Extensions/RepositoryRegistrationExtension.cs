using Application.Interfaces;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class RepositoryRegistrationExtension
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailRepository, EmailRepository>();
        services.AddScoped<ILabelRepository, LabelRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IRecipientRepository, RecipientRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();

        return services;
    }
    
}