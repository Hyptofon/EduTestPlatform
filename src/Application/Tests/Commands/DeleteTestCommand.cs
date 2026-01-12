using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Tests.Exceptions;
using Domain.Tests;
using LanguageExt;
using MediatR;

namespace Application.Tests.Commands;

public record DeleteTestCommand(Guid TestId)
    : IRequest<Either<TestException, Test>>;

public class DeleteTestCommandHandler(
    ITestRepository testRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<DeleteTestCommand, Either<TestException, Test>>
{
    public async Task<Either<TestException, Test>> Handle(
        DeleteTestCommand request,
        CancellationToken cancellationToken)
    {
        var testId = new TestId(request.TestId);
        var existingTest = await testRepository.GetByIdAsync(testId, cancellationToken);

        return await existingTest.MatchAsync(
            test => DeleteEntity(test, cancellationToken),
            () => Task.FromResult<Either<TestException, Test>>(
                new TestNotFoundException(testId)));
    }

    private async Task<Either<TestException, Test>> DeleteEntity(
        Test test,
        CancellationToken cancellationToken)
    {
        try
        {
            testRepository.Delete(test);
            await dbContext.SaveChangesAsync(cancellationToken);

            return test;
        }
        catch (Exception exception)
        {
            return new UnhandledTestException(test.Id, exception);
        }
    }
}