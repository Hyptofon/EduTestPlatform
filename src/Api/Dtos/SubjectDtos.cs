using Domain.Subjects;

namespace Api.Dtos;

public record SubjectDto(
    Guid Id,
    Guid OrganizationalUnitId,
    Guid CreatedByUserId,
    string Name,
    string? Description,
    string AccessType,
    bool RequiresAccessKey,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static SubjectDto FromDomainModel(Subject subject)
        => new(
            subject.Id.Value,
            subject.OrganizationalUnitId.Value,
            subject.CreatedByUserId,
            subject.Name,
            subject.Description,
            subject.AccessType.ToString(),
            subject.AccessType == SubjectAccessType.Private,
            subject.CreatedAt,
            subject.UpdatedAt);
}

public record CreateSubjectDto(
    Guid OrganizationalUnitId,
    string Name,
    string? Description,
    SubjectAccessType AccessType,
    string? AccessKey);

public record UpdateSubjectDto(
    string Name,
    string? Description,
    SubjectAccessType AccessType,
    string? AccessKey);

public record EnrollInSubjectDto(string? AccessKey);

public record SubjectEnrollmentDto(
    Guid Id,
    Guid SubjectId,
    Guid UserId,
    DateTime EnrolledAt)
{
    public static SubjectEnrollmentDto FromDomainModel(SubjectEnrollment enrollment)
        => new(
            enrollment.Id.Value,
            enrollment.SubjectId.Value,
            enrollment.UserId,
            enrollment.EnrolledAt);
}