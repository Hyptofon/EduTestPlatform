namespace Application.Users.Exceptions;

public abstract class UserException(Guid userId, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public Guid UserId { get; } = userId;
}

public class UserNotFoundException(Guid userId)
    : UserException(userId, $"User not found: {userId}");

public class UserNotAuthenticatedException()
    : UserException(Guid.Empty, "User is not authenticated");

public class UserNotInOrganizationException(Guid userId, Guid organizationId)
    : UserException(userId, $"User {userId} is not a member of organization {organizationId}");

public class UserAlreadyInOrganizationException(Guid userId, Guid organizationId)
    : UserException(userId, $"User {userId} is already a member of organization {organizationId}");

public class UnhandledUserException(Guid userId, Exception? innerException)
    : UserException(userId, "Unexpected error occurred while processing user operation", innerException);
