using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Subjects.Exceptions;
using Application.Tests.Exceptions;
using Domain.Subjects;
using Domain.Tests;
using LanguageExt;
using MediatR;

namespace Application.Tests.Commands;

public record CreateTestCommand : IRequest<Either<TestException, Test>>
{
    public required Guid SubjectId { get; init; }
    public required Guid CreatedByUserId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required TestSettings Settings { get; init; }
    public required string ContentJson { get; init; }
}

public class CreateTestCommandHandler(
    ITestRepository testRepository,
    ISubjectRepository subjectRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateTestCommand, Either<TestException, Test>>
{
    public async Task<Either<TestException, Test>> Handle(
        CreateTestCommand request,
        CancellationToken cancellationToken)
    {
        var subjectId = new SubjectId(request.SubjectId);
        var subjectExists = await subjectRepository.GetByIdAsync(subjectId, cancellationToken);

        if (subjectExists.IsNone)
        {
            return new TestNotAccessibleException(TestId.Empty(), "Subject not found");
        }

        return await CreateEntity(request, subjectId, cancellationToken);
    }

    private async Task<Either<TestException, Test>> CreateEntity(
        CreateTestCommand request,
        SubjectId subjectId,
        CancellationToken cancellationToken)
    {
        try
        {
            var test = Test.New(
                TestId.New(),
                subjectId,
                request.CreatedByUserId,
                request.Title,
                request.Description,
                request.Settings,
                request.ContentJson);

            testRepository.Add(test);
            await dbContext.SaveChangesAsync(cancellationToken);

            return test;
        }
        catch (Exception exception)
        {
            return new UnhandledTestException(TestId.Empty(), exception);
        }
    }
}