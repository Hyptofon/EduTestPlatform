using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Tests.Exceptions;
using Domain.Tests;
using LanguageExt;
using MediatR;

namespace Application.Tests.Commands;

public record UpdateTestContentCommand : IRequest<Either<TestException, Test>>
{
    public required Guid TestId { get; init; }
    public required string ContentJson { get; init; }
}

public class UpdateTestContentCommandHandler(
    ITestRepository testRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateTestContentCommand, Either<TestException, Test>>
{
    public async Task<Either<TestException, Test>> Handle(
        UpdateTestContentCommand request,
        CancellationToken cancellationToken)
    {
        var testId = new TestId(request.TestId);
        var existingTest = await testRepository.GetByIdAsync(testId, cancellationToken);

        return await existingTest.MatchAsync(
            test => UpdateEntity(test, request, cancellationToken),
            () => Task.FromResult<Either<TestException, Test>>(
                new TestNotFoundException(testId)));
    }

    private async Task<Either<TestException, Test>> UpdateEntity(
        Test test,
        UpdateTestContentCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            test.UpdateContent(request.ContentJson);

            testRepository.Update(test);
            await dbContext.SaveChangesAsync(cancellationToken);

            return test;
        }
        catch (Exception exception)
        {
            return new UnhandledTestException(test.Id, exception);
        }
    }
}