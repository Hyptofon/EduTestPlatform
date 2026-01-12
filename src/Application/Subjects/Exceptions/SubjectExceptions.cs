using Domain.Subjects;

namespace Application.Subjects.Exceptions;

public abstract class SubjectException(SubjectId subjectId, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public SubjectId SubjectId { get; } = subjectId;
}

public class SubjectNotFoundException(SubjectId subjectId)
    : SubjectException(subjectId, $"Subject not found under id {subjectId}");

public class SubjectAccessDeniedException(SubjectId subjectId, string reason)
    : SubjectException(subjectId, $"Access denied to subject {subjectId}: {reason}");

public class InvalidAccessKeyException(SubjectId subjectId)
    : SubjectException(subjectId, $"Invalid access key for subject {subjectId}");

public class AlreadyEnrolledException(SubjectId subjectId, Guid userId)
    : SubjectException(subjectId, $"User {userId} is already enrolled in subject {subjectId}");

public class NotEnrolledException(SubjectId subjectId, Guid userId)
    : SubjectException(subjectId, $"User {userId} is not enrolled in subject {subjectId}");

public class UnhandledSubjectException(SubjectId subjectId, Exception? innerException)
    : SubjectException(subjectId, "Unexpected error occurred while processing subject", innerException);