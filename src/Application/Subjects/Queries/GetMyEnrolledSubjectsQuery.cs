using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Domain.Subjects;
using MediatR;

namespace Application.Subjects.Queries;

public record GetMyEnrolledSubjectsQuery : IRequest<IReadOnlyList<SubjectEnrollment>>;

public class GetMyEnrolledSubjectsQueryHandler(
    ISubjectEnrollmentRepository enrollmentRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetMyEnrolledSubjectsQuery, IReadOnlyList<SubjectEnrollment>>
{
    public async Task<IReadOnlyList<SubjectEnrollment>> Handle(
        GetMyEnrolledSubjectsQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            return Array.Empty<SubjectEnrollment>();
        }

        return await enrollmentRepository.GetByUserIdAsync(
            currentUserService.UserId.Value,
            cancellationToken);
    }
}