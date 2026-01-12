using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Tests.Exceptions;
using Domain.Tests;
using LanguageExt;
using MediatR;

namespace Application.Tests.Commands;

public record UpdateTestCommand : IRequest<Either<TestException, Test>>
{
    public required Guid TestId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required TestSettings Settings { get; init; }
}

public class UpdateTestCommandHandler(
    ITestRepository testRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateTestCommand, Either<TestException, Test>>
{
    public async Task<Either<TestException, Test>> Handle(
        UpdateTestCommand request,
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
        UpdateTestCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            test.UpdateDetails(request.Title, request.Description, request.Settings);

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