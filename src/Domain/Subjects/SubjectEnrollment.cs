namespace Domain.Subjects;

public class SubjectEnrollment
{
    public SubjectEnrollmentId Id { get; }
    public SubjectId SubjectId { get; }
    public Guid UserId { get; }
    public DateTime EnrolledAt { get; }

    public Subject? Subject { get; private set; }

    private SubjectEnrollment(
        SubjectEnrollmentId id,
        SubjectId subjectId,
        Guid userId,
        DateTime enrolledAt)
    {
        Id = id;
        SubjectId = subjectId;
        UserId = userId;
        EnrolledAt = enrolledAt;
    }

    public static SubjectEnrollment New(
        SubjectId subjectId,
        Guid userId)
    {
        return new SubjectEnrollment(
            SubjectEnrollmentId.New(),
            subjectId,
            userId,
            DateTime.UtcNow);
    }
}