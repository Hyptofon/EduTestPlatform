using Domain.Organizations;

namespace Domain.Subjects;

public class Subject
{
    public SubjectId Id { get; }
    public OrganizationalUnitId OrganizationalUnitId { get; }
    public Guid CreatedByUserId { get; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public SubjectAccessType AccessType { get; private set; }
    public string? AccessKey { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }

    public OrganizationalUnit? OrganizationalUnit { get; private set; }
    public ICollection<SubjectEnrollment>? Enrollments { get; private set; } = [];

    private Subject(
        SubjectId id,
        OrganizationalUnitId organizationalUnitId,
        Guid createdByUserId,
        string name,
        string? description,
        SubjectAccessType accessType,
        string? accessKey,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        Id = id;
        OrganizationalUnitId = organizationalUnitId;
        CreatedByUserId = createdByUserId;
        Name = name;
        Description = description;
        AccessType = accessType;
        AccessKey = accessKey;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Subject New(
        SubjectId id,
        OrganizationalUnitId organizationalUnitId,
        Guid createdByUserId,
        string name,
        string? description,
        SubjectAccessType accessType,
        string? accessKey = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Subject name cannot be empty", nameof(name));

        if (accessType == SubjectAccessType.Private && string.IsNullOrWhiteSpace(accessKey))
            throw new ArgumentException("Private subject must have an access key", nameof(accessKey));

        return new Subject(
            id,
            organizationalUnitId,
            createdByUserId,
            name,
            description,
            accessType,
            accessKey,
            DateTime.UtcNow,
            null);
    }

    public void UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Subject name cannot be empty", nameof(name));

        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAccessType(SubjectAccessType accessType, string? accessKey)
    {
        if (accessType == SubjectAccessType.Private && string.IsNullOrWhiteSpace(accessKey))
            throw new ArgumentException("Private subject must have an access key", nameof(accessKey));

        AccessType = accessType;
        AccessKey = accessType == SubjectAccessType.Private ? accessKey : null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool ValidateAccessKey(string? providedKey)
    {
        if (AccessType == SubjectAccessType.Public)
            return true;

        return !string.IsNullOrWhiteSpace(providedKey) && 
               AccessKey == providedKey;
    }
}