using Application.Common.Interfaces;
using Domain.TestSessions;
using Domain.Tests;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TestSessions.Queries.GetMyTestSession;

public record GetMyTestSessionQuery : IRequest<Either<Exception, TestSession?>>
{
    public required Guid TestId { get; init; }
}

public class GetMyTestSessionQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetMyTestSessionQuery, Either<Exception, TestSession?>>
{
    public async Task<Either<Exception, TestSession?>> Handle(
        GetMyTestSessionQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            {
                return new UnauthorizedAccessException("User must be authenticated");
            }

            var userId = currentUserService.UserId.Value;
            var testId = new TestId(request.TestId);

            // Шукаємо активну або останню завершену сесію
            var session = await context.TestSessions
                .Where(x => x.TestId == testId && x.StudentId == userId)
                .OrderByDescending(x => x.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);

            return session;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}