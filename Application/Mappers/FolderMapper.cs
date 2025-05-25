using Application.DTOs;
using Domain.Entities;

namespace Application.Mappers;

public static class FolderMapper
{
    public static FolderCountsDto ToCountsDto(IEnumerable<FolderEntity> folders, int unreadCount)
    {
        var counts = new FolderCountsDto
        {
            Unread = unreadCount
        };
        
        foreach (var folder in folders)
        {
            switch (folder.Name)
            {
                case nameof(SystemFolderType.Inbox):
                    counts.Inbox = folder.Emails.Count;
                    break;
                case nameof(SystemFolderType.Starred):
                    counts.Starred = folder.Emails.Count;
                    break;
                case nameof(SystemFolderType.Sent):
                    counts.Sent = folder.Emails.Count;
                    break;
                case nameof(SystemFolderType.Drafts):
                    counts.Drafts = folder.Emails.Count;
                    break;
                case nameof(SystemFolderType.Spam):
                    counts.Spam = folder.Emails.Count;
                    break;
                case nameof(SystemFolderType.Trash):
                    counts.Trash = folder.Emails.Count;
                    break;
            }
        }
        
        return counts;
    }
}