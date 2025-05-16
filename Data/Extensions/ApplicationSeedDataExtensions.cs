using MailHandlingServiceProvider.Data.Configurations;
using MailHandlingServiceProvider.Data.Contexts;

namespace MailHandlingServiceProvider.Data.Extensions;

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
