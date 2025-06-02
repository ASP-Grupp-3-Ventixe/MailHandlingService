using Domain.Entities;

namespace Infrastructure.Configurations;

public static class SeedDataConfiguration
{

    public static List<FolderEntity> GetSystemFolders()
    {
        return
        [
            new FolderEntity { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Inbox), IsSystemFolder = true },
            new FolderEntity { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Starred), IsSystemFolder = true },
            new FolderEntity { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Sent), IsSystemFolder = true },
            new FolderEntity { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Drafts), IsSystemFolder = true },
            new FolderEntity { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Spam), IsSystemFolder = true },
            new FolderEntity { Id = Guid.NewGuid(), Name = nameof(SystemFolderType.Trash), IsSystemFolder = true }
        ];
    }
} 