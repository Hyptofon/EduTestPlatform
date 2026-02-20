using Application.Common.Interfaces.Repositories;
using Application.Subjects.Exceptions;
using Domain.Subjects;
using LanguageExt;
using MediatR;

namespace Application.Subjects.Queries;

/// <summary>
/// Query для перевірки Access Key предмету БЕЗ enrollment.
/// Використовується для UI preview перед входом на приватний предмет.
/// </summary>
public record ValidateSubjectAccessKeyQuery : IRequest<Either<SubjectException, SubjectAccessValidationResult>>
{
    public required Guid SubjectId { get; init; }
    public string? AccessKey { get; init; }
}

public record SubjectAccessValidationResult
{
    public bool IsValid { get; init; }
    public bool IsPublic { get; init; }
    public string SubjectName { get; init; } = string.Empty;
}

public class ValidateSubjectAccessKeyQueryHandler(
    ISubjectRepository subjectRepository)
    : IRequestHandler<ValidateSubjectAccessKeyQuery, Either<SubjectException, SubjectAccessValidationResult>>
{
    public async Task<Either<SubjectException, SubjectAccessValidationResult>> Handle(
        ValidateSubjectAccessKeyQuery request,
        CancellationToken cancellationToken)
    {
        var subjectId = new SubjectId(request.SubjectId);
        var subjectOption = await subjectRepository.GetByIdAsync(subjectId, cancellationToken);

        if (subjectOption.IsNone)
        {
            return new SubjectNotFoundException(subjectId);
        }

        var subject = subjectOption.IfNone(() => throw new InvalidOperationException());

        // Якщо предмет публічний, ключ не потрібен
        if (subject.AccessType == SubjectAccessType.Public)
        {
            return new SubjectAccessValidationResult
            {
                IsValid = true,
                IsPublic = true,
                SubjectName = subject.Name
            };
        }

        // Для приватного предмету перевіряємо ключ
        var isValid = subject.ValidateAccessKey(request.AccessKey);

        return new SubjectAccessValidationResult
        {
            IsValid = isValid,
            IsPublic = false,
            SubjectName = subject.Name
        };
    }
}
