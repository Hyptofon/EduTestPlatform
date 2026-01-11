using Domain.Common;
using Domain.Organizations;

namespace Domain.Enrollments;

public record StudentSubjectId(Guid Value)
{
    public static StudentSubjectId New() => new(Guid.NewGuid());
    public static StudentSubjectId Empty() => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}

public class StudentSubject : Entity<StudentSubjectId>
{
    public Guid StudentId { get; private set; }
    public OrganizationalUnitId SubjectId { get; private set; }
    public DateTime EnrolledAt { get; private set; }
    public bool IsActive { get; private set; }

    public virtual OrganizationalUnit? Subject { get; private set; }

    private StudentSubject(
        StudentSubjectId id,
        Guid studentId,
        OrganizationalUnitId subjectId,
        DateTime enrolledAt) : base(id)
    {
        StudentId = studentId;
        SubjectId = subjectId;
        EnrolledAt = enrolledAt;
        IsActive = true;
    }

    public static StudentSubject Enroll(Guid studentId, OrganizationalUnitId subjectId)
    {
        return new StudentSubject(
            StudentSubjectId.New(),
            studentId,
            subjectId,
            DateTime.UtcNow);
    }

    public void Unenroll()
    {
        IsActive = false;
    }

    public void Reenroll()
    {
        IsActive = true;
    }
}