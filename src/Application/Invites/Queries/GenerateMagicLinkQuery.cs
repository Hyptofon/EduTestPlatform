using Application.Common.Interfaces.Repositories;
using Application.Invites.Exceptions;
using Domain.Invites;
using LanguageExt;
using MediatR;

namespace Application.Invites.Queries;

/// <summary>
/// Query для генерації Magic Link URL з вшитим інвайт-кодом.
/// URL автоматично заповнює поле коду на сторінці реєстрації.
/// </summary>
public record GenerateMagicLinkQuery : IRequest<Either<InviteCodeException, MagicLinkResult>>
{
    public required Guid InviteCodeId { get; init; }
    public required string BaseUrl { get; init; }
}

public record MagicLinkResult
{
    public string MagicLink { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string OrganizationName { get; init; } = string.Empty;
    public DateTime? ExpiresAt { get; init; }
}

public class GenerateMagicLinkQueryHandler(
    IInviteCodeRepository inviteCodeRepository)
    : IRequestHandler<GenerateMagicLinkQuery, Either<InviteCodeException, MagicLinkResult>>
{
    public async Task<Either<InviteCodeException, MagicLinkResult>> Handle(
        GenerateMagicLinkQuery request,
        CancellationToken cancellationToken)
    {
        var inviteCodeId = new InviteCodeId(request.InviteCodeId);
        var inviteCodeOption = await inviteCodeRepository.GetByIdAsync(inviteCodeId, cancellationToken);

        if (inviteCodeOption.IsNone)
        {
            return new InviteCodeNotFoundException(inviteCodeId);
        }

        var inviteCode = inviteCodeOption.IfNone(() => throw new InvalidOperationException());

        if (!inviteCode.IsValid())
        {
            return new InviteCodeNotValidException(inviteCode.Code, "Invite code is expired or has reached maximum uses.");
        }

        // Формуємо Magic Link URL
        var baseUrl = request.BaseUrl.TrimEnd('/');
        var magicLink = $"{baseUrl}/register?code={Uri.EscapeDataString(inviteCode.Code)}";

        return new MagicLinkResult
        {
            MagicLink = magicLink,
            Code = inviteCode.Code,
            OrganizationName = inviteCode.Organization?.Name ?? "Unknown",
            ExpiresAt = inviteCode.ExpiresAt
        };
    }
}
