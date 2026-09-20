namespace Eleganza.Domain.Entities;

public sealed class Category
{
    private Category()
    {
    }

    private Category(string name, string slug, string? description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Slug = slug;
        Description = description;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Category Create(string name, string slug, string? description = null)
        => new(name, slug, description);

    public void Rename(string name, string slug, string? description)
    {
        Name = name;
        Slug = slug;
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
