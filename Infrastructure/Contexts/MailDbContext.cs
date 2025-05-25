using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts;

public class MailDbContext : DbContext
{
    public MailDbContext(DbContextOptions<MailDbContext> options) : base(options)
    {
    }
    
    public DbSet<EmailEntity> Emails { get; set; }
    public DbSet<RecipientEntity> EmailRecipients { get; set; }
    public DbSet<LabelEntity> Labels { get; set; }
    public DbSet<EmailLabelEntity> EmailLabels { get; set; }
    public DbSet<AttachmentEntity> Attachments { get; set; }
    public DbSet<FolderEntity> Folders { get; set; }
    
}