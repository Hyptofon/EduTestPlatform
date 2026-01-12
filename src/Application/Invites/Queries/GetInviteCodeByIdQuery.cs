using Application.Common.Interfaces.Repositories;
using Domain.Invites;
using LanguageExt;
using MediatR;

namespace Application.Invites.Queries;

public record GetInviteCodeByIdQuery(Guid InviteCodeId) : IRequest<Option<InviteCode>>;

public class GetInviteCodeByIdQueryHandler(IInviteCodeRepository inviteCodeRepository)
    : IRequestHandler<GetInviteCodeByIdQuery, Option<InviteCode>>
{
    public async Task<Option<InviteCode>> Handle(
        GetInviteCodeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var inviteCodeId = new InviteCodeId(request.InviteCodeId);
        return await inviteCodeRepository.GetByIdAsync(inviteCodeId, cancellationToken);
    }
}