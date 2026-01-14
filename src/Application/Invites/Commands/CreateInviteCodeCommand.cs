using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Invites.Exceptions;
using Domain.Invites;
using Domain.Organizations;
using Domain.Users;
using LanguageExt;
using MediatR;

namespace Application.Invites.Commands;

public record CreateInviteCodeCommand : IRequest<Either<InviteCodeException, InviteCode>>
{
    public required Guid OrganizationId { get; init; }
    public Guid? OrganizationalUnitId { get; init; }
    public string? Code { get; init; } 
    public required InviteCodeType Type { get; init; }
    public required string AssignedRole { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public int? MaxUses { get; init; }
}

public class CreateInviteCodeCommandHandler(
    IInviteCodeRepository inviteCodeRepository,
    IOrganizationRepository organizationRepository,
    IOrganizationalUnitRepository unitRepository,
    ICurrentUserService currentUserService, 
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateInviteCodeCommand, Either<InviteCodeException, InviteCode>>
{
    public async Task<Either<InviteCodeException, InviteCode>> Handle(
        CreateInviteCodeCommand request,
        CancellationToken cancellationToken)
    {
        // --- SECURITY CHECK START ---
        // 1. Вчителі можуть створювати ТІЛЬКИ коди для студентів
        if (currentUserService.IsInRole(ApplicationRole.Teacher))
        {
            if (request.Type != InviteCodeType.Student)
            {
                return new InviteCodeNotValidException(
                    request.Code ?? "new", 
                    "Teachers are only allowed to generate Student invite codes.");
            }
        }

        // 2. Адміни організації не можуть створювати Master-коди (це для Супер-адміна)
        if (currentUserService.IsInRole(ApplicationRole.OrganizationAdmin))
        {
            if (request.Type == InviteCodeType.Master)
            {
                return new InviteCodeNotValidException(
                    request.Code ?? "new", 
                    "Organization Admins cannot generate Master invite codes.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var existingCode = await inviteCodeRepository.GetByCodeAsync(request.Code, cancellationToken);
            if (existingCode.IsSome)
            {
                return new InviteCodeAlreadyExistsException(request.Code);
            }
        }

        var organizationId = new OrganizationId(request.OrganizationId);
        var organizationExists = await organizationRepository.GetByIdAsync(organizationId, cancellationToken);

        if (organizationExists.IsNone)
        {
            return new InviteCodeNotValidException(request.Code ?? "Auto-generated", "Organization not found");
        }

        if (request.OrganizationalUnitId.HasValue)
        {
            var unitId = new OrganizationalUnitId(request.OrganizationalUnitId.Value);
            var unitExists = await unitRepository.GetByIdAsync(unitId, cancellationToken);

            if (unitExists.IsNone)
            {
                return new InviteCodeNotValidException(request.Code ?? "Auto-generated", "Organizational unit not found");
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
            
            var codeToUse = !string.IsNullOrWhiteSpace(request.Code) 
                ? request.Code 
                : await GenerateUniqueCodeAsync(cancellationToken);

            var inviteCode = InviteCode.New(
                InviteCodeId.New(),
                organizationId,
                unitId,
                codeToUse,
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

    private async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        
        while (true)
        {
            var code = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            
            var exists = await inviteCodeRepository.GetByCodeAsync(code, cancellationToken);
            if (exists.IsNone)
            {
                return code;
            }
        }
    }
}