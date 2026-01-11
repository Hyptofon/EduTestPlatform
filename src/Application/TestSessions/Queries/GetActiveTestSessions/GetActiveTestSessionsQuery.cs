using Application.Common.Interfaces;
using Domain.TestSessions;
using Domain.Tests;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TestSessions.Queries.GetActiveTestSessions;

public record GetActiveTestSessionsQuery : IRequest<Either<Exception, IReadOnlyList<TestSession>>>
{
    public required Guid TestId { get; init; }
}

public class GetActiveTestSessionsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetActiveTestSessionsQuery, Either<Exception, IReadOnlyList<TestSession>>>
{
    public async Task<Either<Exception, IReadOnlyList<TestSession>>> Handle(
        GetActiveTestSessionsQuery request,
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

            // Перевірка прав доступу
            var test = await context.Tests
                .FirstOrDefaultAsync(x => x.Id == testId, cancellationToken);

            if (test == null)
            {
                return new KeyNotFoundException("Test not found");
            }

            // Тільки автор або Admin/Manager можуть дивитись активні сесії
            if (test.AuthorId != userId)
            {
                var isAdminOrManager = currentUserService.IsInRole("Admin") || 
                                       currentUserService.IsInRole("Manager");
                
                if (!isAdminOrManager)
                {
                    return new UnauthorizedAccessException("Unauthorized");
                }
            }

            var sessions = await context.TestSessions
                .Where(x => x.TestId == testId && x.Status == TestSessionStatus.InProgress)
                .OrderBy(x => x.StartedAt)
                .ToListAsync(cancellationToken);

            return sessions;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}