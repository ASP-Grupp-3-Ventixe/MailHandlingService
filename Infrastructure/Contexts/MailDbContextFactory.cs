using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Contexts;

public class MailDbContextFactory : IDesignTimeDbContextFactory<MailDbContext>
{
    public MailDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../Presentation"));
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<MailDbContext>();
        var connectionString = configuration.GetConnectionString("SqlServer");
        optionsBuilder.UseSqlServer(connectionString);

        return new MailDbContext(optionsBuilder.Options);
    }
}