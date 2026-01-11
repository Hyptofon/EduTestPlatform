using Application.Common.Interfaces;
using Domain.Organizations;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Organizations.Commands.Create;

public record CreateOrganizationalUnitCommand : IRequest<Either<Exception, Guid>>
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public Guid? ParentId { get; init; }
    public bool GenerateInvite { get; init; }
    public UserRole? InviteTargetRole { get; init; } // Роль для інвайту
}

public class CreateOrganizationalUnitCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateOrganizationalUnitCommand, Either<Exception, Guid>>
{
    public async Task<Either<Exception, Guid>> Handle(
        CreateOrganizationalUnitCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<OrganizationalUnitType>(request.Type, true, out var type))
            {
                return new ArgumentException($"Invalid organization type: {request.Type}");
            }

            OrganizationalUnitId? parentId = null;
            if (request.ParentId.HasValue)
            {
                parentId = new OrganizationalUnitId(request.ParentId.Value);
                var parentExists = await context.OrganizationalUnits
                    .AnyAsync(x => x.Id == parentId, cancellationToken);
                
                if (!parentExists)
                {
                    return new KeyNotFoundException($"Parent organization with ID {request.ParentId} not found.");
                }
            }
            else if (type != OrganizationalUnitType.Root)
            {
                return new ArgumentException("ParentId is required for non-root organizations.");
            }

            var orgUnit = OrganizationalUnit.Create(request.Name, type, parentId);

            context.OrganizationalUnits.Add(orgUnit);
            await context.SaveChangesAsync(cancellationToken);

            // Генерація InviteCode (якщо запитано)
            if (request.GenerateInvite && request.InviteTargetRole.HasValue)
            {
                var prefix = request.Name.Replace(" ", "").ToUpper().Substring(0, Math.Min(3, request.Name.Length));
                var rolePrefix = request.InviteTargetRole.Value.ToString().ToUpper().Substring(0, Math.Min(4, request.InviteTargetRole.Value.ToString().Length));
                var randomPart = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
                
                var code = $"{prefix}-{rolePrefix}-{randomPart}";

                var inviteCode = InviteCode.Generate(
                    code,
                    request.InviteTargetRole.Value,
                    orgUnit.Id,
                    null, // Без терміну дії
                    100); // Багаторазовий для Root організацій

                context.InviteCodes.Add(inviteCode);
                await context.SaveChangesAsync(cancellationToken);
            }

            return orgUnit.Id.Value;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}