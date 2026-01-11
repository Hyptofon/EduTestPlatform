using Application.Common.Interfaces;
using Application.Organizations.Models; // <-- Новий namespace
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Organizations.Queries.GetById;

public record GetOrganizationalUnitByIdQuery(Guid Id) : IRequest<Either<Exception, OrganizationDto>>;

public class GetOrganizationalUnitByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetOrganizationalUnitByIdQuery, Either<Exception, OrganizationDto>>
{
    public async Task<Either<Exception, OrganizationDto>> Handle(
        GetOrganizationalUnitByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var org = await context.OrganizationalUnits
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == new Domain.Organizations.OrganizationalUnitId(request.Id), cancellationToken);

        if (org == null)
        {
            return new KeyNotFoundException($"Organization with ID {request.Id} not found.");
        }

        return new OrganizationDto(
            org.Id.Value,
            org.Name,
            org.Type.ToString(),
            org.ParentId?.Value,
            null
        );
    }
}