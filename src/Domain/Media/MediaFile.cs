using Domain.Common;
using Domain.Users;

namespace Domain.Media;

public record MediaFileId(Guid Value)
{
    public static MediaFileId New() => new(Guid.NewGuid());
}

public class MediaFile : Entity<MediaFileId>
{
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long Size { get; private set; }
    public string Url { get; private set; } // URL в Azure Blob / Local storage
    public UserId UploadedBy { get; private set; }
    public DateTime UploadedAt { get; private set; }

    public MediaFile(MediaFileId id, string fileName, string contentType, long size, string url, UserId uploadedBy) : base(id)
    {
        FileName = fileName;
        ContentType = contentType;
        Size = size;
        Url = url;
        UploadedBy = uploadedBy;
        UploadedAt = DateTime.UtcNow;
    }
}