using System.Net.Http.Headers;
using System.Net.Http.Json;
using Eleganza.Application.Abstractions;
using Eleganza.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Eleganza.Infrastructure.Shipping;

public sealed class VanexOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string ValidatePath { get; set; } = string.Empty;
    public string CreateOrderPath { get; set; } = string.Empty;
}

public sealed class VanexShippingProvider(
    HttpClient httpClient,
    IOptions<VanexOptions> options) : IShippingProvider
{
    public string ProviderName => "Vanex";

    public async Task<ShippingCredentialsValidationResult> ValidateCredentialsAsync(
        VendorShippingAccount account,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled)
        {
            return new(false, "Vanex integration is disabled until the official API contract is configured.");
        }

        var path = options.Value.ValidatePath;
        if (string.IsNullOrWhiteSpace(path))
        {
            return new(false, "Vanex validation endpoint is not configured.");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(account, path));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode
                ? new(true, null)
                : new(false, $"Vanex rejected the credentials with HTTP {(int)response.StatusCode}.");
        }
        catch (HttpRequestException exception)
        {
            return new(false, $"Vanex connection failed: {exception.Message}");
        }
    }

    public async Task<ShippingOrderResult> CreateOrderAsync(
        VendorShippingAccount account,
        string accessToken,
        ShippingOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled || string.IsNullOrWhiteSpace(options.Value.CreateOrderPath))
        {
            return new(false, null, null, "Vanex create-order endpoint is not configured.");
        }

        var payload = new
        {
            merchant_id = account.MerchantId,
            order_number = request.OrderNumber,
            customer_name = request.CustomerName,
            customer_phone = request.CustomerPhone,
            city = request.City,
            address = request.Address,
            notes = request.Notes,
            cash_on_delivery_amount = request.CashOnDeliveryAmount,
            items = request.Items.Select(item => new
            {
                name = item.ProductName,
                size = item.Size,
                color = item.Color,
                quantity = item.Quantity,
                unit_price = item.UnitPrice,
            }),
        };

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildUri(account, options.Value.CreateOrderPath))
            {
                Content = JsonContent.Create(payload),
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new(false, null, null, $"Vanex rejected the order with HTTP {(int)response.StatusCode}.");
            }

            var result = await response.Content.ReadFromJsonAsync<VanexCreateOrderResponse>(cancellationToken: cancellationToken);
            return new(true, result?.ExternalOrderId, result?.TrackingNumber, null);
        }
        catch (HttpRequestException exception)
        {
            return new(false, null, null, $"Vanex connection failed: {exception.Message}");
        }
    }

    private Uri BuildUri(VendorShippingAccount account, string path)
    {
        var baseUrl = string.IsNullOrWhiteSpace(account.BaseUrl) ? options.Value.BaseUrl : account.BaseUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Vanex base URL is not configured.");
        }

        return new Uri($"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}");
    }

    private sealed record VanexCreateOrderResponse(string? ExternalOrderId, string? TrackingNumber);
}
