namespace Eleganza.Domain.Enums;

public enum ShippingStatus
{
    NotSubmitted = 0,
    Submitting = 1,
    Submitted = 2,
    Accepted = 3,
    OutForDelivery = 4,
    Delivered = 5,
    Failed = 6,
    Cancelled = 7,
}
