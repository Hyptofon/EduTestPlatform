using Application.Common.Interfaces.Repositories;
using Domain.Organizations;
using LanguageExt;
using MediatR;

namespace Application.Organizations.Queries;

public record GetOrganizationByIdQuery(Guid OrganizationId) : IRequest<Option<Organization>>;

public class GetOrganizationByIdQueryHandler(IOrganizationRepository organizationRepository)
    : IRequestHandler<GetOrganizationByIdQuery, Option<Organization>>
{
    public async Task<Option<Organization>> Handle(
        GetOrganizationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId(request.OrganizationId);
        return await organizationRepository.GetByIdAsync(organizationId, cancellationToken);
    }
}