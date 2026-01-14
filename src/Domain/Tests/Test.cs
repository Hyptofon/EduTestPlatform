using Domain.Subjects;

namespace Domain.Tests;

public class Test
{
    public TestId Id { get; }
    public SubjectId SubjectId { get; }
    public Guid CreatedByUserId { get; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public TestSettings Settings { get; private set; } = null!;
    public string ContentJson { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }

    public Subject? Subject { get; private set; }
    public ICollection<TestSession>? Sessions { get; private set; } = [];

    private Test(
        TestId id,
        SubjectId subjectId,
        Guid createdByUserId,
        string title,
        string? description,
        string contentJson,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        Id = id;
        SubjectId = subjectId;
        CreatedByUserId = createdByUserId;
        Title = title;
        Description = description;
        ContentJson = contentJson;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Test New(
        TestId id,
        SubjectId subjectId,
        Guid createdByUserId,
        string title,
        string? description,
        TestSettings settings,
        string contentJson)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Test title cannot be empty", nameof(title));

        if (string.IsNullOrWhiteSpace(contentJson))
            throw new ArgumentException("Test content cannot be empty", nameof(contentJson));

        var test = new Test(
            id,
            subjectId,
            createdByUserId,
            title,
            description,
            contentJson,
            DateTime.UtcNow,
            null);
            
        test.Settings = settings; 

        return test;
    }

    public void UpdateDetails(string title, string? description, TestSettings settings)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Test title cannot be empty", nameof(title));

        Title = title;
        Description = description;
        Settings = settings;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContent(string contentJson)
    {
        if (string.IsNullOrWhiteSpace(contentJson))
            throw new ArgumentException("Test content cannot be empty", nameof(contentJson));

        ContentJson = contentJson;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsAccessible()
    {
        var now = DateTime.UtcNow;

        if (Settings.StartDate.HasValue && now < Settings.StartDate.Value)
            return false;

        if (Settings.EndDate.HasValue && now > Settings.EndDate.Value)
            return false;

        return true;
    }
}