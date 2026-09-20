using Eleganza.Domain.Entities;
using Eleganza.Domain.Enums;

namespace Eleganza.UnitTests.Vendors;

public sealed class VendorTests
{
    [Fact]
    public void New_application_starts_as_pending()
    {
        var vendor = CreateVendor();

        Assert.Equal(VendorStatus.Pending, vendor.Status);
    }

    [Fact]
    public void Pending_vendor_can_be_reviewed_and_approved()
    {
        var vendor = CreateVendor();

        vendor.StartReview();
        vendor.Approve();

        Assert.Equal(VendorStatus.Approved, vendor.Status);
    }

    [Fact]
    public void Rejected_vendor_can_reenter_review()
    {
        var vendor = CreateVendor();

        vendor.Reject("Missing store details");
        vendor.StartReview();

        Assert.Equal(VendorStatus.UnderReview, vendor.Status);
        Assert.Null(vendor.ReviewNote);
    }

    [Fact]
    public void Approved_vendor_can_be_suspended_and_reactivated()
    {
        var vendor = CreateVendor();
        vendor.Approve();

        vendor.Suspend("Policy violation");
        Assert.Equal(VendorStatus.Suspended, vendor.Status);

        vendor.Reactivate();
        Assert.Equal(VendorStatus.Approved, vendor.Status);
    }

    [Fact]
    public void Suspended_vendor_cannot_be_approved_directly()
    {
        var vendor = CreateVendor();
        vendor.Approve();
        vendor.Suspend("Policy violation");

        Assert.Throws<InvalidOperationException>(() => vendor.Approve());
    }

    private static Vendor CreateVendor()
        => Vendor.CreateApplication(
            Guid.NewGuid(),
            "Al Shaqabi",
            "al-shaqabi",
            "0910000000",
            "Tripoli");
}
