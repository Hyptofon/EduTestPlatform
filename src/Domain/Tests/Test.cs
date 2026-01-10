using Domain.Common;
using Domain.Organizations;
using Domain.Users;

namespace Domain.Tests;

public class Test : Entity<TestId>
{
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public OrganizationalUnitId SubjectId { get; private set; }
    public UserId AuthorId { get; private set; }

    public TestContent Content { get; private set; }
    public required TestSettings Settings { get; set; } 
    
    public bool IsDraft { get; private set; } = true;

    private Test(TestId id, OrganizationalUnitId subjectId, UserId authorId) : base(id)
    {
        SubjectId = subjectId;
        AuthorId = authorId;
        Content = new TestContent();
    }

    public static Test Create(string title, string description, OrganizationalUnitId subjectId, UserId authorId, TestSettings settings)
    {
        return new Test(TestId.New(), subjectId, authorId)
        {
            Title = title,
            Description = description,
            Settings = settings
        };
    }

    public void UpdateContent(TestContent content)
    {
        if (!IsDraft) throw new InvalidOperationException("Cannot modify published test.");
        Content = content;
    }

    public void Publish()
    {
        if (Content.Sections.Count == 0) throw new InvalidOperationException("Cannot publish empty test.");
        IsDraft = false;
    }
}