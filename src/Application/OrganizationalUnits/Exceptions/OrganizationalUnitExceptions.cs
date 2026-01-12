using Domain.Organizations;

namespace Application.OrganizationalUnits.Exceptions;

public abstract class OrganizationalUnitException(OrganizationalUnitId unitId, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public OrganizationalUnitId UnitId { get; } = unitId;
}

public class OrganizationalUnitNotFoundException(OrganizationalUnitId unitId)
    : OrganizationalUnitException(unitId, $"Organizational unit not found under id {unitId}");

public class OrganizationNotFoundForUnitException(OrganizationId organizationId)
    : OrganizationalUnitException(OrganizationalUnitId.Empty(), $"Organization {organizationId} not found");

public class ParentUnitNotFoundException(OrganizationalUnitId parentId)
    : OrganizationalUnitException(OrganizationalUnitId.Empty(), $"Parent unit {parentId} not found");

public class OrganizationalUnitCannotBeDeletedException(OrganizationalUnitId unitId)
    : OrganizationalUnitException(unitId, $"Organizational unit {unitId} cannot be deleted because it has children");

public class UnhandledOrganizationalUnitException(OrganizationalUnitId unitId, Exception? innerException)
    : OrganizationalUnitException(unitId, "Unexpected error occurred while processing organizational unit", innerException);