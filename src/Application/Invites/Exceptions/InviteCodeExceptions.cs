using Domain.Invites;

namespace Application.Invites.Exceptions;

public abstract class InviteCodeException(InviteCodeId inviteCodeId, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public InviteCodeId InviteCodeId { get; } = inviteCodeId;
}

public class InviteCodeNotFoundException(InviteCodeId inviteCodeId)
    : InviteCodeException(inviteCodeId, $"Invite code not found under id {inviteCodeId}");

public class InviteCodeNotFoundByCodeException(string code)
    : InviteCodeException(InviteCodeId.Empty(), $"Invite code '{code}' not found");

public class InviteCodeAlreadyExistsException(string code)
    : InviteCodeException(InviteCodeId.Empty(), $"Invite code '{code}' already exists");

public class InviteCodeNotValidException(string code, string reason)
    : InviteCodeException(InviteCodeId.Empty(), $"Invite code '{code}' is not valid: {reason}");

public class UnhandledInviteCodeException(InviteCodeId inviteCodeId, Exception? innerException)
    : InviteCodeException(inviteCodeId, "Unexpected error occurred while processing invite code", innerException);