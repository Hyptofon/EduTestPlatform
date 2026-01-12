namespace Domain.Subjects;

public record SubjectEnrollmentId(Guid Value)
{
    public static SubjectEnrollmentId Empty() => new(Guid.Empty);
    public static SubjectEnrollmentId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}