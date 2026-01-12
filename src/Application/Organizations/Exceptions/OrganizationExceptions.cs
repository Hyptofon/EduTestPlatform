using Domain.Organizations;

namespace Application.Organizations.Exceptions;

public abstract class OrganizationException(OrganizationId organizationId, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public OrganizationId OrganizationId { get; } = organizationId;
}

public class OrganizationNotFoundException(OrganizationId organizationId)
    : OrganizationException(organizationId, $"Organization not found under id {organizationId}");

public class OrganizationAlreadyExistsException(string name)
    : OrganizationException(OrganizationId.Empty(), $"Organization with name '{name}' already exists");

public class OrganizationCannotBeDeletedException(OrganizationId organizationId)
    : OrganizationException(organizationId, $"Organization {organizationId} cannot be deleted because it has associated data");

public class UnhandledOrganizationException(OrganizationId organizationId, Exception? innerException)
    : OrganizationException(organizationId, "Unexpected error occurred while processing organization", innerException);