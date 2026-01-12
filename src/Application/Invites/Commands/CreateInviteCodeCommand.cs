using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Invites.Exceptions;
using Domain.Invites;
using Domain.Organizations;
using LanguageExt;
using MediatR;

namespace Application.Invites.Commands;

public record CreateInviteCodeCommand : IRequest<Either<InviteCodeException, InviteCode>>
{
    public required Guid OrganizationId { get; init; }
    public Guid? OrganizationalUnitId { get; init; }
    public required string Code { get; init; }
    public required InviteCodeType Type { get; init; }
    public required string AssignedRole { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public int? MaxUses { get; init; }
}

public class CreateInviteCodeCommandHandler(
    IInviteCodeRepository inviteCodeRepository,
    IOrganizationRepository organizationRepository,
    IOrganizationalUnitRepository unitRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateInviteCodeCommand, Either<InviteCodeException, InviteCode>>
{
    public async Task<Either<InviteCodeException, InviteCode>> Handle(
        CreateInviteCodeCommand request,
        CancellationToken cancellationToken)
    {
        var existingCode = await inviteCodeRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existingCode.IsSome)
        {
            return new InviteCodeAlreadyExistsException(request.Code);
        }

        var organizationId = new OrganizationId(request.OrganizationId);
        var organizationExists = await organizationRepository.GetByIdAsync(organizationId, cancellationToken);

        if (organizationExists.IsNone)
        {
            return new InviteCodeNotValidException(request.Code, "Organization not found");
        }

        if (request.OrganizationalUnitId.HasValue)
        {
            var unitId = new OrganizationalUnitId(request.OrganizationalUnitId.Value);
            var unitExists = await unitRepository.GetByIdAsync(unitId, cancellationToken);

            if (unitExists.IsNone)
            {
                return new InviteCodeNotValidException(request.Code, "Organizational unit not found");
            }
        }

        return await CreateEntity(request, organizationId, cancellationToken);
    }

    private async Task<Either<InviteCodeException, InviteCode>> CreateEntity(
        CreateInviteCodeCommand request,
        OrganizationId organizationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var unitId = request.OrganizationalUnitId.HasValue 
                ? new OrganizationalUnitId(request.OrganizationalUnitId.Value) 
                : null;

            var inviteCode = InviteCode.New(
                InviteCodeId.New(),
                organizationId,
                unitId,
                request.Code,
                request.Type,
                request.AssignedRole,
                request.ExpiresAt,
                request.MaxUses);

            inviteCodeRepository.Add(inviteCode);
            await dbContext.SaveChangesAsync(cancellationToken);

            return inviteCode;
        }
        catch (Exception exception)
        {
            return new UnhandledInviteCodeException(InviteCodeId.Empty(), exception);
        }
    }
}