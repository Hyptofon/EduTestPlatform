using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Invites.Exceptions;
using Domain.Invites;
using LanguageExt;
using MediatR;

namespace Application.Invites.Commands;

public record ValidateAndUseInviteCodeCommand(string Code)
    : IRequest<Either<InviteCodeException, InviteCode>>;

public class ValidateAndUseInviteCodeCommandHandler(
    IInviteCodeRepository inviteCodeRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<ValidateAndUseInviteCodeCommand, Either<InviteCodeException, InviteCode>>
{
    public async Task<Either<InviteCodeException, InviteCode>> Handle(
        ValidateAndUseInviteCodeCommand request,
        CancellationToken cancellationToken)
    {
        var existingCode = await inviteCodeRepository.GetByCodeAsync(request.Code, cancellationToken);

        return await existingCode.MatchAsync(
            code => UseCode(code, cancellationToken),
            () => Task.FromResult<Either<InviteCodeException, InviteCode>>(
                new InviteCodeNotFoundByCodeException(request.Code)));
    }

    private async Task<Either<InviteCodeException, InviteCode>> UseCode(
        InviteCode inviteCode,
        CancellationToken cancellationToken)
    {
        if (!inviteCode.IsValid())
        {
            return new InviteCodeNotValidException(
                inviteCode.Code,
                "Code is expired, deactivated, or has reached maximum uses");
        }

        try
        {
            inviteCode.Use();

            inviteCodeRepository.Update(inviteCode);
            await dbContext.SaveChangesAsync(cancellationToken);

            return inviteCode;
        }
        catch (Exception exception)
        {
            return new UnhandledInviteCodeException(inviteCode.Id, exception);
        }
    }
}