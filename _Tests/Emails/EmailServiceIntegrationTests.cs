using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Infrastructure.Configurations;
using Infrastructure.Contexts;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace _Tests.Emails;

// took some help from GPT to create these tests
public class EmailServiceIntegrationTests
{
    private static EmailService CreateEmailService(MailDbContext context)
    {
        return new EmailService(
            new EmailRepository(context),
            new LabelRepository(context),
            new FolderRepository(context),
            new AttachmentRepository(context),
            new RecipientRepository(context)
        );
    }
    
    [Fact]
    public async Task CreateEmailAsync_ShouldPersistEmailAndRecipients()
    {
        // Setup test database and seed system folders
        var options = new DbContextOptionsBuilder<MailDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // auto dispose context after test
        await using var context = new MailDbContext(options);
        
        // Seed system folders
        var systemFolders = SeedDataConfiguration.GetSystemFolders();
        context.Folders.AddRange(systemFolders);
        await context.SaveChangesAsync();

        var service = CreateEmailService(context);
        
        var createDto = new CreateEmailDto
        {
            Subject = "Test",
            Body = "Body",
            Recipients = [ new RecipientDto { Email = "test@example.com" } ]
        };
        var userId = Guid.NewGuid();

        // Act: Create email
        var result = await service.CreateEmailAsync(createDto, userId);
        
        // Assert: Email and recipient are persisted, and email is in Sent folder
        Assert.True(result.Succeeded);
        Assert.Equal(201, result.StatusCode);
        
        var sentFolder = await context.Folders.FirstOrDefaultAsync(f => f.Name == "Sent" && f.IsSystemFolder);

        var emailInDb = await context.Emails.Include(e => e.Recipients)
            .FirstOrDefaultAsync(e => e.Subject == "Test");
        
        Assert.NotNull(sentFolder);
        Assert.NotNull(emailInDb); 
        Assert.Equal(sentFolder.Id, emailInDb.FolderId);
        Assert.Equal("Test", emailInDb.Subject);
        Assert.Single(emailInDb.Recipients);
        Assert.Equal("test@example.com", emailInDb.Recipients.First().EmailAddress);
    }
    
    [Fact]
    public async Task CreateEmailAsync_ShouldFail_WhenSentFolderMissing()
    {
        var options = new DbContextOptionsBuilder<MailDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new MailDbContext(options);

        var service = CreateEmailService(context);

        // Try to create an email (Sent folder missing)
        var createDto = new CreateEmailDto
        {
            Subject = "Test",
            Body = "Body",
            Recipients = [ new RecipientDto { Email = "test@example.com" } ]
        };

        var result = await service.CreateEmailAsync(createDto, Guid.NewGuid());

        // Assert: Should fail with correct error
        Assert.False(result.Succeeded);
        Assert.Equal(500, result.StatusCode);
        Assert.Equal("Could not find Sent folder", result.Error);
    }
    
    [Fact]
    public async Task GetEmailsAsync_ShouldReturnEmails_ForExistingUserAndFolder()
    {
        var options = new DbContextOptionsBuilder<MailDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new MailDbContext(options);

        // Seed system folders
        var systemFolders = SeedDataConfiguration.GetSystemFolders();
        context.Folders.AddRange(systemFolders);
        await context.SaveChangesAsync();
        var inbox = systemFolders.First(f => f.Name == "Inbox");

        // Add two emails for the same user in Inbox
        var userId = Guid.NewGuid();
        var email1 = new EmailEntity
        {
            Id = Guid.NewGuid(),
            Subject = "Testmail 1",
            FolderId = inbox.Id,
            SenderId = userId,
            Preview = "Preview 1",
            Body = "Body 1",
            SentAt = DateTime.UtcNow,
        };
        var email2 = new EmailEntity
        {
            Id = Guid.NewGuid(),
            Subject = "Testmail 2",
            FolderId = inbox.Id,
            SenderId = userId,
            Preview = "Preview 2",
            Body = "Body 2",
            SentAt = DateTime.UtcNow,

        };
        context.Emails.AddRange(email1, email2);
        await context.SaveChangesAsync();

        // Setup repositories och service
        var service = CreateEmailService(context);

        // Act: Fetch emails for user in Inbox
        var result = await service.GetEmailsAsync(userId, "Inbox");

        // Assert: Both emails are returned
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Items);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, e => e.Subject == "Testmail 1");
        Assert.Contains(result.Items, e => e.Subject == "Testmail 2");
    }
    
    [Fact]
    public async Task GetEmailByIdAsync_ShouldFail_WhenUserTriesToAccessOthersEmail()
    {
        var options = new DbContextOptionsBuilder<MailDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new MailDbContext(options);
        
        var systemFolders = SeedDataConfiguration.GetSystemFolders();
        context.Folders.AddRange(systemFolders);
        await context.SaveChangesAsync();
        var inbox = systemFolders.First(f => f.Name == "Inbox");

        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var email = new EmailEntity
        {
            Id = Guid.NewGuid(),
            Subject = "Privat mejl",
            FolderId = inbox.Id,
            SenderId = ownerId,
            Preview = "Preview",
            Body = "Body",
            SentAt = DateTime.UtcNow
        };
        context.Emails.Add(email);
        await context.SaveChangesAsync();

        var service = CreateEmailService(context);

        // Act: Other user tries to fetch owner's email
        var result = await service.GetEmailByIdAsync(email.Id, otherUserId);

        // Assert: Should fail and return null
        Assert.False(result.Succeeded);
        Assert.Null(result.Item);
    }
    
    [Fact]
    public async Task SoftDeleteEmailAsync_ShouldMoveEmailToTrash_AndGetEmailByIdReflectsChange()
    {
        var options = new DbContextOptionsBuilder<MailDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new MailDbContext(options);
        
        var systemFolders = SeedDataConfiguration.GetSystemFolders();
        context.Folders.AddRange(systemFolders);
        await context.SaveChangesAsync();
        var inbox = systemFolders.First(f => f.Name == "Inbox");
        var trash = systemFolders.First(f => f.Name == "Trash");

        var userId = Guid.NewGuid();
        var email = new EmailEntity
        {
            Id = Guid.NewGuid(),
            Subject = "Softdelete test",
            FolderId = inbox.Id,
            SenderId = userId,
            Preview = "Preview",
            Body = "Body",
            SentAt = DateTime.UtcNow,
        };
        context.Emails.Add(email);
        await context.SaveChangesAsync();

        var service = CreateEmailService(context);

        // Act: Soft-delete email
        var softDeleteResult = await service.SoftDeleteEmailAsync(email.Id, userId);

        // Assert: 
        Assert.True(softDeleteResult.Succeeded);
        
        var getResult = await service.GetEmailByIdAsync(email.Id, userId);

        Assert.True(getResult.Succeeded);
        Assert.NotNull(getResult.Item);
        Assert.Equal(trash.Id, getResult.Item.FolderId);
    }
    
    [Fact]
    public async Task HardDeleteEmailAsync_ShouldRemoveEmailFromDatabase()
    {
        var options = new DbContextOptionsBuilder<MailDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new MailDbContext(options);
        var systemFolders = SeedDataConfiguration.GetSystemFolders();
        context.Folders.AddRange(systemFolders);
        await context.SaveChangesAsync();
        var inbox = systemFolders.First(f => f.Name == "Inbox");

        var userId = Guid.NewGuid();
        var email = new EmailEntity
        {
            Id = Guid.NewGuid(),
            Subject = "Permanent delete test",
            FolderId = inbox.Id,
            SenderId = userId,
            Preview = "Preview",
            Body = "Body",
            SentAt = DateTime.UtcNow
        };
        context.Emails.Add(email);
        await context.SaveChangesAsync();

        var service = CreateEmailService(context);

        // Act: Hard-delete email
        var deleteResult = await service.HardDeleteEmailAsync(email.Id, userId);

        // Assert: Email should be removed from database
        Assert.True(deleteResult.Succeeded);
        var emailInDb = await context.Emails.FindAsync(email.Id);
        Assert.Null(emailInDb);
    }

    [Fact]
    public async Task HardDeleteEmailAsync_ShouldFail_WhenEmailDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MailDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new MailDbContext(options);
        var service = CreateEmailService(context);

        // Act: Try to delete a non-existent email
        var result = await service.HardDeleteEmailAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert: Should fail
        Assert.False(result.Succeeded);
    }
    
    [Fact]
    public async Task EmptyTrashAsync_ShouldRemoveAllEmailsInTrash_ForUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MailDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new MailDbContext(options);
        
        var systemFolders = SeedDataConfiguration.GetSystemFolders();
        
        context.Folders.AddRange(systemFolders);
        await context.SaveChangesAsync();
        var trash = systemFolders.First(f => f.Name == "Trash");
        var inbox = systemFolders.First(f => f.Name == "Inbox");
        var userId = Guid.NewGuid();
        
        // one email in Trash, one in Inbox
        var emailInTrash = new EmailEntity
        {
            Id = Guid.NewGuid(),
            Subject = "Trash mail",
            FolderId = trash.Id,
            SenderId = userId,
            Preview = "Trash",
            Body = "Body",
            SentAt = DateTime.UtcNow
        };
        var emailInInbox = new EmailEntity
        {
            Id = Guid.NewGuid(),
            Subject = "Inbox mail",
            FolderId = inbox.Id,
            SenderId = userId,
            Preview = "Inbox",
            Body = "Body",
            SentAt = DateTime.UtcNow
        };
        context.Emails.AddRange(emailInTrash, emailInInbox);
        await context.SaveChangesAsync();

        var service = CreateEmailService(context);

        // Act: Empty Trash for user
        var result = await service.EmptyTrashAsync(userId);

        // Assert: Email in Trash is deleted, email in Inbox remains
        Assert.True(result.Succeeded);
        Assert.Null(await context.Emails.FindAsync(emailInTrash.Id));
        Assert.NotNull(await context.Emails.FindAsync(emailInInbox.Id));
    }

    [Fact]
    public async Task EmptyTrashAsync_ShouldSucceed_WhenTrashIsAlreadyEmpty()
    {
        var options = new DbContextOptionsBuilder<MailDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new MailDbContext(options);
        var systemFolders = SeedDataConfiguration.GetSystemFolders();
        context.Folders.AddRange(systemFolders);
        await context.SaveChangesAsync();
        var userId = Guid.NewGuid();

        var service = CreateEmailService(context);

        // Act: Empty Trash for user when Trash is already empty
        var result = await service.EmptyTrashAsync(userId);

        // Assert: Should succeed, nothing to delete
        Assert.True(result.Succeeded);
    }
}