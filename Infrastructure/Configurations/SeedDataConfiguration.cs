using Domain.Entities;

namespace Infrastructure.Configurations;

public static class SeedDataConfiguration
{

    public static List<FolderEntity> GetSystemFolders()
    {
        return new List<FolderEntity>
        {
            new() { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Inbox), IsSystemFolder = true },
            new() { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Starred), IsSystemFolder = true },
            new() { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Sent), IsSystemFolder = true },
            new() { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Drafts), IsSystemFolder = true },
            new() { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Spam), IsSystemFolder = true },
            new() { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Trash), IsSystemFolder = true }

        };
    }
    
    // Helper to get the folder name from the enum value
    public static string GetFolderName(SystemFolderType folderType)
    {
        return folderType.ToString();
    }
} 