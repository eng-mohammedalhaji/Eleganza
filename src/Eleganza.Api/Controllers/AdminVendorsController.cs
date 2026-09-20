using Eleganza.Application.Vendors;
using Eleganza.Contracts.Vendors;
using Eleganza.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eleganza.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/vendors")]
public sealed class AdminVendorsController(VendorService vendorService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VendorResponse>>> List(
        [FromQuery] VendorStatus? status,
        CancellationToken cancellationToken)
        => Ok(await vendorService.ListAsync(status, cancellationToken));

    [HttpPost("{id:guid}/review")]
    public async Task<ActionResult<VendorResponse>> StartReview(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await vendorService.StartReviewAsync(id, cancellationToken));

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<VendorResponse>> Approve(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await vendorService.ApproveAsync(id, cancellationToken));

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<VendorResponse>> Reject(
        Guid id,
        RejectVendorRequest request,
        CancellationToken cancellationToken)
        => Ok(await vendorService.RejectAsync(id, request.Note, cancellationToken));

    [HttpPost("{id:guid}/suspend")]
    public async Task<ActionResult<VendorResponse>> Suspend(
        Guid id,
        SuspendVendorRequest request,
        CancellationToken cancellationToken)
        => Ok(await vendorService.SuspendAsync(id, request.Note, cancellationToken));

    [HttpPost("{id:guid}/reactivate")]
    public async Task<ActionResult<VendorResponse>> Reactivate(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await vendorService.ReactivateAsync(id, cancellationToken));
}
