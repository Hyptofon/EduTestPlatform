using Application.Common.Interfaces;
using Domain.Organizations;
using Domain.Users;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Organizations.Commands.GenerateInviteCode;

public record GenerateInviteCodeCommand : IRequest<Either<Exception, string>>
{
    public required Guid OrganizationalUnitId { get; init; }
    public required UserRole TargetRole { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public int MaxUses { get; init; } = 1;
}

public class GenerateInviteCodeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<GenerateInviteCodeCommand, Either<Exception, string>>
{
    public async Task<Either<Exception, string>> Handle(
        GenerateInviteCodeCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var orgUnitId = new OrganizationalUnitId(request.OrganizationalUnitId);
            
            var orgUnit = await context.OrganizationalUnits
                .FirstOrDefaultAsync(x => x.Id == orgUnitId, cancellationToken);

            if (orgUnit == null)
            {
                return new KeyNotFoundException($"OrganizationalUnit with ID {request.OrganizationalUnitId} not found.");
            }

            // Генерація унікального коду: "KPI-ADMIN-A1B2"
            var prefix = orgUnit.Name.Replace(" ", "").ToUpper().Substring(0, Math.Min(3, orgUnit.Name.Length));
            var rolePrefix = request.TargetRole.ToString().ToUpper().Substring(0, Math.Min(4, request.TargetRole.ToString().Length));
            var randomPart = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            
            var code = $"{prefix}-{rolePrefix}-{randomPart}";

            // Перевірка унікальності
            var exists = await context.InviteCodes
                .AnyAsync(x => x.Code == code, cancellationToken);

            if (exists)
            {
                // Регенеруємо, якщо конфлікт (дуже рідкісно)
                randomPart = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
                code = $"{prefix}-{rolePrefix}-{randomPart}";
            }

            var inviteCode = InviteCode.Generate(
                code,
                request.TargetRole,
                orgUnitId,
                request.ExpiryDate,
                request.MaxUses);

            context.InviteCodes.Add(inviteCode);
            await context.SaveChangesAsync(cancellationToken);

            return code;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}