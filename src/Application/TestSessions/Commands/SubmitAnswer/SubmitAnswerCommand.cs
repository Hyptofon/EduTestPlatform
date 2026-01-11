using Application.Common.Interfaces;
using Domain.TestSessions;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TestSessions.Commands.SubmitAnswer;

public record SubmitAnswerCommand : IRequest<Either<Exception, TestSession>>
{
    public required Guid SessionId { get; init; }
    public required Guid QuestionId { get; init; }
    public List<Guid> SelectedOptionIds { get; init; } = new();
    public string? TextAnswer { get; init; }
}

public class SubmitAnswerCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<SubmitAnswerCommand, Either<Exception, TestSession>>
{
    public async Task<Either<Exception, TestSession>> Handle(
        SubmitAnswerCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            {
                return new UnauthorizedAccessException("User must be authenticated");
            }

            var userId = currentUserService.UserId.Value;
            var sessionId = new TestSessionId(request.SessionId);

            var session = await context.TestSessions
                .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

            if (session == null)
            {
                return new KeyNotFoundException("Session not found");
            }

            if (session.StudentId != userId)
            {
                return new UnauthorizedAccessException("Unauthorized access to session");
            }

            if (session.Status != TestSessionStatus.InProgress)
            {
                return new InvalidOperationException("Session is not active");
            }

            // Додаємо/оновлюємо відповідь
            session.AddAnswer(request.QuestionId, request.SelectedOptionIds, request.TextAnswer);

            context.TestSessions.Update(session);
            await context.SaveChangesAsync(cancellationToken);

            return session;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}