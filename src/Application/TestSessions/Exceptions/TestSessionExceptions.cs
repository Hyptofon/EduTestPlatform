using Domain.Tests;

namespace Application.TestSessions.Exceptions;

public abstract class TestSessionException(TestSessionId sessionId, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public TestSessionId SessionId { get; } = sessionId;
}

public class TestSessionNotFoundException(TestSessionId sessionId)
    : TestSessionException(sessionId, $"Test session not found under id {sessionId}");

public class TestSessionAlreadyExistsException(TestId testId, Guid studentId)
    : TestSessionException(TestSessionId.Empty(), $"Active test session already exists for test {testId} and student {studentId}");

public class MaxAttemptsReachedException(TestId testId, Guid studentId, int maxAttempts)
    : TestSessionException(TestSessionId.Empty(), $"Student {studentId} has reached maximum attempts ({maxAttempts}) for test {testId}");

public class TestSessionNotInProgressException(TestSessionId sessionId)
    : TestSessionException(sessionId, $"Test session {sessionId} is not in progress");

public class UnauthorizedSessionAccessException(TestSessionId sessionId)
    : TestSessionException(sessionId, "User is not authorized to access this test session");

public class UnhandledTestSessionException(TestSessionId sessionId, Exception? innerException)
    : TestSessionException(sessionId, "Unexpected error occurred while processing test session", innerException);