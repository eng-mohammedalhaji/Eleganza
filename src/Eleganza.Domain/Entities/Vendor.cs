using Eleganza.Domain.Enums;

namespace Eleganza.Domain.Entities;

public sealed class Vendor
{
    private Vendor()
    {
    }

    private Vendor(Guid ownerId, string businessName, string slug, string phone, string city)
    {
        Id = Guid.NewGuid();
        OwnerId = ownerId;
        BusinessName = businessName;
        Slug = slug;
        Phone = phone;
        City = city;
        Status = VendorStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string BusinessName { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public VendorStatus Status { get; private set; }
    public string? ReviewNote { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Vendor CreateApplication(
        Guid ownerId,
        string businessName,
        string slug,
        string phone,
        string city)
    {
        return new Vendor(ownerId, businessName, slug, phone, city);
    }

    public void StartReview()
    {
        if (Status is not VendorStatus.Pending and not VendorStatus.Rejected)
        {
            throw new InvalidOperationException("Only pending or rejected vendors can enter review.");
        }

        Status = VendorStatus.UnderReview;
        ReviewNote = null;
        Touch();
    }

    public void Approve()
    {
        if (Status is not VendorStatus.Pending and not VendorStatus.UnderReview)
        {
            throw new InvalidOperationException("Only pending vendors can be approved.");
        }

        Status = VendorStatus.Approved;
        ReviewNote = null;
        Touch();
    }

    public void Reject(string? note)
    {
        if (Status is not VendorStatus.Pending and not VendorStatus.UnderReview)
        {
            throw new InvalidOperationException("Only pending vendors can be rejected.");
        }

        Status = VendorStatus.Rejected;
        ReviewNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        Touch();
    }

    public void Suspend(string? note)
    {
        if (Status != VendorStatus.Approved)
        {
            throw new InvalidOperationException("Only approved vendors can be suspended.");
        }

        Status = VendorStatus.Suspended;
        ReviewNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        Touch();
    }

    public void Reactivate()
    {
        if (Status != VendorStatus.Suspended)
        {
            throw new InvalidOperationException("Only suspended vendors can be reactivated.");
        }

        Status = VendorStatus.Approved;
        ReviewNote = null;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
