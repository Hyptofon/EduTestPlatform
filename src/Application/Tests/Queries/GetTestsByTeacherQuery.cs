using Application.Common.Interfaces.Repositories;
using Domain.Tests;
using MediatR;

namespace Application.Tests.Queries;

public record GetTestsByTeacherQuery(Guid TeacherId) : IRequest<IReadOnlyList<Test>>;

public class GetTestsByTeacherQueryHandler(ITestRepository testRepository)
    : IRequestHandler<GetTestsByTeacherQuery, IReadOnlyList<Test>>
{
    public async Task<IReadOnlyList<Test>> Handle(
        GetTestsByTeacherQuery request,
        CancellationToken cancellationToken)
    {
        return await testRepository.GetByTeacherIdAsync(request.TeacherId, cancellationToken);
    }
}