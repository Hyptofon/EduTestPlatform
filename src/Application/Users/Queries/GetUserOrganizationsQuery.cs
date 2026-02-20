using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Users.Exceptions;
using Application.Users.Queries;
using LanguageExt;
using MediatR;

namespace Application.Users.Queries;

/// <summary>
/// Query для отримання списку всіх організацій користувача.
/// </summary>
public record GetUserOrganizationsQuery : IRequest<Either<UserException, IReadOnlyList<UserOrganizationInfo>>>;

public class GetUserOrganizationsQueryHandler(
    ICurrentUserService currentUserService,
    IUserOrganizationRepository userOrganizationRepository)
    : IRequestHandler<GetUserOrganizationsQuery, Either<UserException, IReadOnlyList<UserOrganizationInfo>>>
{
    public async Task<Either<UserException, IReadOnlyList<UserOrganizationInfo>>> Handle(
        GetUserOrganizationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
        {
            return new UserNotAuthenticatedException();
        }

        var userId = currentUserService.UserId.Value;
        var organizations = await userOrganizationRepository.GetByUserIdAsync(userId, cancellationToken);

        var orgInfos = organizations.Select(uo => new UserOrganizationInfo
        {
            OrganizationId = uo.OrganizationId.Value,
            OrganizationName = uo.Organization?.Name ?? "Unknown",
            Role = uo.Role,
            OrganizationalUnitId = uo.OrganizationalUnitId?.Value
        }).ToList();

        return orgInfos;
    }
}
