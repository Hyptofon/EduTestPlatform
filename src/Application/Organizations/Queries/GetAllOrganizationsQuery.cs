using Application.Common.Interfaces.Repositories;
using Domain.Organizations;
using MediatR;

namespace Application.Organizations.Queries;

public record GetAllOrganizationsQuery : IRequest<IReadOnlyList<Organization>>;

public class GetAllOrganizationsQueryHandler(IOrganizationRepository organizationRepository)
    : IRequestHandler<GetAllOrganizationsQuery, IReadOnlyList<Organization>>
{
    public async Task<IReadOnlyList<Organization>> Handle(
        GetAllOrganizationsQuery request,
        CancellationToken cancellationToken)
    {
        return await organizationRepository.GetAllAsync(cancellationToken);
    }
}