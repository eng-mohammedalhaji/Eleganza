using System.Net.Http.Headers;
using System.Globalization;
using System.Text.Json;
using Eleganza.Application.Abstractions;
using Eleganza.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Eleganza.Infrastructure.Shipping;

public sealed class VanexOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string[] AllowedBaseUrls { get; set; } = [];
    public bool AllowInsecureHttp { get; set; }
    public int HttpTimeoutSeconds { get; set; }
    public string CitiesPath { get; set; } = string.Empty;
    public string SubCitiesPath { get; set; } = string.Empty;
    public string CreateOrderPath { get; set; } = string.Empty;
    public string DefaultPackageType { get; set; } = string.Empty;
    public string DefaultLength { get; set; } = string.Empty;
    public string DefaultWidth { get; set; } = string.Empty;
    public string DefaultHeight { get; set; } = string.Empty;
    public string DefaultBreakable { get; set; } = string.Empty;
    public string DefaultReceiptMoney { get; set; } = string.Empty;
    public string DefaultMeasuringIsAllowed { get; set; } = string.Empty;
    public string DefaultStorageSubscription { get; set; } = string.Empty;
    public string DefaultInspectionAllowed { get; set; } = string.Empty;
    public string DefaultHeatIntolerance { get; set; } = string.Empty;
    public string DefaultCasing { get; set; } = string.Empty;
    public string DefaultPaidBy { get; set; } = string.Empty;
    public string DefaultExtraSizeBy { get; set; } = string.Empty;
    public string DefaultCommissionBy { get; set; } = string.Empty;
    public string DefaultSaveAddress { get; set; } = string.Empty;
    public string DefaultPaymentMethod { get; set; } = string.Empty;
    public string DefaultPackageSubType { get; set; } = string.Empty;
    public string DefaultTypeId { get; set; } = string.Empty;
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
        try
        {
            var result = await GetCitiesAsync(account, accessToken, cancellationToken);
            return result.Succeeded
                ? new(true, null)
                : new(false, result.Error ?? "Vanex rejected the credentials.");
        }
        catch (HttpRequestException exception)
        {
            return new(false, $"Vanex connection failed: {exception.Message}");
        }
        catch (InvalidOperationException exception)
        {
            return new(false, exception.Message);
        }
    }

    public async Task<ShippingOrderResult> CreateOrderAsync(
        VendorShippingAccount account,
        string accessToken,
        ShippingOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled)
        {
            return new(false, null, null, "Vanex integration is disabled.");
        }

        if (!HasCreateOrderConfiguration())
        {
            return new(false, null, null, "Vanex create-order configuration is incomplete.");
        }

        if (request.DeliveryCityId is null || request.DeliverySubCityId is null)
        {
            return new(false, null, null, "Vanex city and sub-city identifiers are required before submitting the order.");
        }

        using var form = new MultipartFormDataContent();
        Add(form, "type", options.Value.DefaultPackageType);
        Add(form, "description", string.Join("، ", request.Items.Select(item => $"{item.ProductName} ({item.Size}/{item.Color}) x{item.Quantity}")));
        Add(form, "qty", request.Items.Sum(item => item.Quantity).ToString(CultureInfo.InvariantCulture));
        Add(form, "leangh", options.Value.DefaultLength);
        Add(form, "width", options.Value.DefaultWidth);
        Add(form, "height", options.Value.DefaultHeight);
        Add(form, "breakable", options.Value.DefaultBreakable);
        Add(form, "receipt_money", options.Value.DefaultReceiptMoney);
        Add(form, "measuring_is_allowed", options.Value.DefaultMeasuringIsAllowed);
        Add(form, "storage_subscription", options.Value.DefaultStorageSubscription);
        Add(form, "inspection_allowed", options.Value.DefaultInspectionAllowed);
        Add(form, "heat_intolerance", options.Value.DefaultHeatIntolerance);
        Add(form, "casing", options.Value.DefaultCasing);
        Add(form, "address", request.Address);
        Add(form, "reciever", request.CustomerName);
        Add(form, "phone", request.CustomerPhone);
        Add(form, "phone_b", request.CustomerPhone);
        Add(form, "city", request.DeliveryCityId.Value.ToString(CultureInfo.InvariantCulture));
        Add(form, "address_child", request.DeliverySubCityId.Value.ToString(CultureInfo.InvariantCulture));
        Add(form, "price", request.CashOnDeliveryAmount.ToString("0.##", CultureInfo.InvariantCulture));
        Add(form, "sticker_notes", request.Notes ?? request.OrderNumber);
        Add(form, "paid_by", options.Value.DefaultPaidBy);
        Add(form, "extra_size_by", options.Value.DefaultExtraSizeBy);
        Add(form, "commission_by", options.Value.DefaultCommissionBy);
        Add(form, "save_address", options.Value.DefaultSaveAddress);
        Add(form, "payment_methode", options.Value.DefaultPaymentMethod);
        Add(form, "map", request.MapUrl ?? string.Empty);
        Add(form, "package_sub_type", options.Value.DefaultPackageSubType);
        Add(form, "type_id", options.Value.DefaultTypeId);

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildUri(account, options.Value.CreateOrderPath))
            {
                Content = form,
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new(false, null, null, await BuildHttpErrorAsync(response, "Vanex rejected the order."));
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = ParseOrderResult(body);
            return result.ExternalOrderId is null
                ? new(false, null, result.TrackingNumber, "Vanex returned a successful response without a package code.")
                : new(true, result.ExternalOrderId, result.TrackingNumber, null);
        }
        catch (HttpRequestException exception)
        {
            return new(false, null, null, $"Vanex connection failed: {exception.Message}");
        }
        catch (InvalidOperationException exception)
        {
            return new(false, null, null, exception.Message);
        }
    }

    public Task<ShippingLocationsResult> GetCitiesAsync(
        VendorShippingAccount account,
        string accessToken,
        CancellationToken cancellationToken = default)
        => GetLocationsAsync(account, accessToken, options.Value.CitiesPath, cancellationToken);

    public Task<ShippingLocationsResult> GetSubCitiesAsync(
        VendorShippingAccount account,
        string accessToken,
        string cityId,
        CancellationToken cancellationToken = default)
    {
        var path = options.Value.SubCitiesPath.Replace("{cityId}", Uri.EscapeDataString(cityId), StringComparison.OrdinalIgnoreCase);
        return GetLocationsAsync(account, accessToken, path, cancellationToken);
    }

    private async Task<ShippingLocationsResult> GetLocationsAsync(
        VendorShippingAccount account,
        string accessToken,
        string path,
        CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return new(false, [], "Vanex integration is disabled.");
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            return new(false, [], "Vanex locations endpoint is not configured.");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(account, path));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new(false, [], await BuildHttpErrorAsync(response, "Vanex rejected the location request."));
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return new(true, ParseLocations(body), null);
        }
        catch (HttpRequestException exception)
        {
            return new(false, [], $"Vanex connection failed: {exception.Message}");
        }
        catch (InvalidOperationException exception)
        {
            return new(false, [], exception.Message);
        }
        catch (JsonException)
        {
            return new(false, [], "Vanex returned an invalid locations response.");
        }
    }

    private bool HasCreateOrderConfiguration()
    {
        var value = options.Value;
        return new[]
        {
            value.BaseUrl, value.CreateOrderPath, value.DefaultPackageType,
            value.DefaultLength, value.DefaultWidth, value.DefaultHeight,
            value.DefaultBreakable, value.DefaultReceiptMoney,
            value.DefaultMeasuringIsAllowed, value.DefaultStorageSubscription,
            value.DefaultInspectionAllowed, value.DefaultHeatIntolerance,
            value.DefaultCasing, value.DefaultPaidBy, value.DefaultExtraSizeBy,
            value.DefaultCommissionBy, value.DefaultSaveAddress,
            value.DefaultPaymentMethod, value.DefaultPackageSubType, value.DefaultTypeId,
        }.All(item => !string.IsNullOrWhiteSpace(item));
    }

    private static void Add(MultipartFormDataContent form, string name, string value)
        => form.Add(new StringContent(value ?? string.Empty), name);

    private static async Task<string> BuildHttpErrorAsync(HttpResponseMessage response, string fallback)
    {
        var body = await response.Content.ReadAsStringAsync();
        var message = TryFindString(body, "message", "error", "errors", "detail");
        return string.IsNullOrWhiteSpace(message)
            ? $"{fallback} HTTP {(int)response.StatusCode}."
            : $"{fallback} {message}";
    }

    private static IReadOnlyList<ShippingLocation> ParseLocations(string body)
    {
        using var document = JsonDocument.Parse(body);
        var locations = new List<ShippingLocation>();
        CollectLocations(document.RootElement, locations);
        return locations
            .GroupBy(location => location.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
    }

    private static void CollectLocations(JsonElement element, ICollection<ShippingLocation> locations)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            string? id = null;
            string? name = null;
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals("id", StringComparison.OrdinalIgnoreCase)
                    || property.Name.Equals("city_id", StringComparison.OrdinalIgnoreCase)
                    || property.Name.Equals("subcity_id", StringComparison.OrdinalIgnoreCase))
                {
                    id = property.Value.ToString();
                }
                else if (property.Name.Equals("name", StringComparison.OrdinalIgnoreCase)
                    || property.Name.Equals("title", StringComparison.OrdinalIgnoreCase)
                    || property.Name.Equals("city_name", StringComparison.OrdinalIgnoreCase)
                    || property.Name.Equals("subcity_name", StringComparison.OrdinalIgnoreCase))
                {
                    name = property.Value.ToString();
                }
            }

            if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(name))
            {
                locations.Add(new(id, name));
            }

            foreach (var property in element.EnumerateObject())
            {
                CollectLocations(property.Value, locations);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                CollectLocations(item, locations);
            }
        }
    }

    private static (string? ExternalOrderId, string? TrackingNumber) ParseOrderResult(string body)
        => (TryFindString(body, "package_code", "packageCode", "package-code", "external_order_id", "externalOrderId", "code", "package_id", "packageId", "id"),
            TryFindString(body, "tracking_number", "trackingNumber", "tracking", "waybill"));

    private static string? TryFindString(string body, params string[] propertyNames)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            return FindString(document.RootElement, propertyNames);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? FindString(JsonElement element, IReadOnlyCollection<string> propertyNames)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (propertyNames.Any(name => property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    && property.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number)
                {
                    var value = property.Value.ToString();
                    if (!string.IsNullOrWhiteSpace(value)) return value;
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                var value = FindString(property.Value, propertyNames);
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var value = FindString(item, propertyNames);
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }
        }

        return null;
    }

    private Uri BuildUri(VendorShippingAccount account, string path)
    {
        var baseUrl = string.IsNullOrWhiteSpace(account.BaseUrl) ? options.Value.BaseUrl : account.BaseUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Vanex base URL is not configured.");
        }

        var vendorProvidedBaseUrl = !string.IsNullOrWhiteSpace(account.BaseUrl);
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)
            || (!options.Value.AllowInsecureHttp && baseUri.Scheme != Uri.UriSchemeHttps)
            || (vendorProvidedBaseUrl
                && (options.Value.AllowedBaseUrls.Length == 0
                    || !options.Value.AllowedBaseUrls.Any(allowed => string.Equals(
                        allowed.TrimEnd('/'),
                        baseUri.ToString().TrimEnd('/'),
                        StringComparison.OrdinalIgnoreCase))))
            || (options.Value.AllowedBaseUrls.Length > 0
                && !options.Value.AllowedBaseUrls.Any(allowed => string.Equals(
                    allowed.TrimEnd('/'),
                    baseUri.ToString().TrimEnd('/'),
                    StringComparison.OrdinalIgnoreCase))))
        {
            throw new InvalidOperationException("Vanex base URL is not allowed by deployment configuration.");
        }

        return new Uri(baseUri, path.TrimStart('/'));
    }

}
