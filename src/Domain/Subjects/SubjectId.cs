namespace Domain.Subjects;

public record SubjectId(Guid Value)
{
    public static SubjectId Empty() => new(Guid.Empty);
    public static SubjectId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}