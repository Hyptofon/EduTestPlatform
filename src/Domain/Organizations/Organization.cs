namespace Domain.Organizations;

public class Organization
{
    public OrganizationId Id { get; }
    public string Name { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? HeroImageUrl { get; private set; }
    public string? WelcomeText { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<OrganizationalUnit>? Units { get; private set; } = [];

    private Organization(
        OrganizationId id,
        string name,
        string? logoUrl,
        string? heroImageUrl,
        string? welcomeText,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        Id = id;
        Name = name;
        LogoUrl = logoUrl;
        HeroImageUrl = heroImageUrl;
        WelcomeText = welcomeText;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Organization New(
        OrganizationId id,
        string name,
        string? logoUrl = null,
        string? heroImageUrl = null,
        string? welcomeText = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name cannot be empty", nameof(name));

        return new Organization(
            id,
            name,
            logoUrl,
            heroImageUrl,
            welcomeText,
            DateTime.UtcNow,
            null);
    }

    public void UpdateBranding(string? logoUrl, string? heroImageUrl, string? welcomeText)
    {
        LogoUrl = logoUrl;
        HeroImageUrl = heroImageUrl;
        WelcomeText = welcomeText;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name cannot be empty", nameof(name));

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
}