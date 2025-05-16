using MailHandlingServiceProvider.Data.Entities;

namespace MailHandlingServiceProvider.Data.Configurations;

public static class SeedDataConfiguration
{

    public static List<FolderEntity> GetSystemFolders()
    {
        return new List<FolderEntity>
        {
            new FolderEntity { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Inbox), IsSystemFolder = true },
            new FolderEntity { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Starred), IsSystemFolder = true },
            new FolderEntity { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Sent), IsSystemFolder = true },
            new FolderEntity { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Drafts), IsSystemFolder = true },
            new FolderEntity { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Spam), IsSystemFolder = true },
            new FolderEntity { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Trash), IsSystemFolder = true }

        };
    }
    
    // Helper to get the folder name from the enum value
    public static string GetFolderName(SystemFolderType folderType)
    {
        return folderType.ToString();
    }
} 