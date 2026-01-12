using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Invites.Exceptions;
using Domain.Invites;
using LanguageExt;
using MediatR;

namespace Application.Invites.Commands;

public record DeactivateInviteCodeCommand(Guid InviteCodeId)
    : IRequest<Either<InviteCodeException, InviteCode>>;

public class DeactivateInviteCodeCommandHandler(
    IInviteCodeRepository inviteCodeRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<DeactivateInviteCodeCommand, Either<InviteCodeException, InviteCode>>
{
    public async Task<Either<InviteCodeException, InviteCode>> Handle(
        DeactivateInviteCodeCommand request,
        CancellationToken cancellationToken)
    {
        var inviteCodeId = new InviteCodeId(request.InviteCodeId);
        var existingCode = await inviteCodeRepository.GetByIdAsync(inviteCodeId, cancellationToken);

        return await existingCode.MatchAsync(
            code => DeactivateEntity(code, cancellationToken),
            () => Task.FromResult<Either<InviteCodeException, InviteCode>>(
                new InviteCodeNotFoundException(inviteCodeId)));
    }

    private async Task<Either<InviteCodeException, InviteCode>> DeactivateEntity(
        InviteCode inviteCode,
        CancellationToken cancellationToken)
    {
        try
        {
            inviteCode.Deactivate();

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