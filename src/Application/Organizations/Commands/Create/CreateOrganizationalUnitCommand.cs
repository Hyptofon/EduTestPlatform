using Application.Common.Interfaces;
using Domain.Organizations;
using LanguageExt;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Organizations.Commands.Create;

public record CreateOrganizationalUnitCommand : IRequest<Either<Exception, Guid>>
{
    public required string Name { get; init; }
    public required string Type { get; init; } // "Root", "Faculty", etc.
    public Guid? ParentId { get; init; }
    public bool GenerateInvite { get; init; }
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
            // 1. Валідація типу
            if (!Enum.TryParse<OrganizationalUnitType>(request.Type, true, out var type))
            {
                return new ArgumentException($"Invalid organization type: {request.Type}");
            }

            // 2. Перевірка батьківського елементу (якщо вказаний)
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
                // Якщо це не корінь (Універ/Школа), то батько обовязковий
                return new ArgumentException("ParentId is required for non-root organizations.");
            }

            // 3. Створення сутності
            var orgUnit = OrganizationalUnit.Create(request.Name, type, parentId);

            // 4. Генерація інвайт-коду (якщо треба)
            if (request.GenerateInvite)
            {
                // Генеруємо код: "KPI-2024" або "CLASS-11A"
                // Простий генератор для прикладу
                var prefix = request.Name.Replace(" ", "").ToUpper().Substring(0, Math.Min(3, request.Name.Length));
                var code = $"{prefix}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
                
                orgUnit.SetInviteCode(code);
            }

            // 5. Збереження
            context.OrganizationalUnits.Add(orgUnit);
            await context.SaveChangesAsync(cancellationToken);

            return orgUnit.Id.Value;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}