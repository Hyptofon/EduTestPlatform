using Application.Common.Interfaces.Repositories;
using Domain.Tests;
using LanguageExt;
using MediatR;

namespace Application.Tests.Queries;

public record GetTestByIdQuery(Guid TestId) : IRequest<Option<Test>>;

public class GetTestByIdQueryHandler(ITestRepository testRepository)
    : IRequestHandler<GetTestByIdQuery, Option<Test>>
{
    public async Task<Option<Test>> Handle(
        GetTestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var testId = new TestId(request.TestId);
        return await testRepository.GetByIdAsync(testId, cancellationToken);
    }
}