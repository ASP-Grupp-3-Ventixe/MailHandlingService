using Infrastructure.Configurations;
using Infrastructure.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ApplicationSeedDataExtensions
{
    public static IApplicationBuilder UseSeedData(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MailDbContext>();

        if (!context.Folders.Any())
        {
            context.Folders.AddRange(SeedDataConfiguration.GetSystemFolders());
            context.SaveChanges();
        }
        
        return app;
    }
}
