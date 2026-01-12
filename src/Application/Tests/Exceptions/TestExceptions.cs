using Domain.Tests;

namespace Application.Tests.Exceptions;

public abstract class TestException(TestId testId, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public TestId TestId { get; } = testId;
}

public class TestNotFoundException(TestId testId)
    : TestException(testId, $"Test not found under id {testId}");

public class TestNotAccessibleException(TestId testId, string reason)
    : TestException(testId, $"Test {testId} is not accessible: {reason}");

public class InvalidTestContentException(TestId testId, string reason)
    : TestException(testId, $"Invalid test content: {reason}");

public class UnhandledTestException(TestId testId, Exception? innerException)
    : TestException(testId, "Unexpected error occurred while processing test", innerException);