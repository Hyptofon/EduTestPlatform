namespace Domain.Invites;

public record InviteCodeId(Guid Value)
{
    public static InviteCodeId Empty() => new(Guid.Empty);
    public static InviteCodeId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}