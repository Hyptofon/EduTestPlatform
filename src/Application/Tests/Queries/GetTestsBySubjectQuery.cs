using Application.Common.Interfaces.Repositories;
using Domain.Subjects;
using Domain.Tests;
using MediatR;

namespace Application.Tests.Queries;

public record GetTestsBySubjectQuery(Guid SubjectId) : IRequest<IReadOnlyList<Test>>;

public class GetTestsBySubjectQueryHandler(ITestRepository testRepository)
    : IRequestHandler<GetTestsBySubjectQuery, IReadOnlyList<Test>>
{
    public async Task<IReadOnlyList<Test>> Handle(
        GetTestsBySubjectQuery request,
        CancellationToken cancellationToken)
    {
        var subjectId = new SubjectId(request.SubjectId);
        return await testRepository.GetBySubjectIdAsync(subjectId, cancellationToken);
    }
}