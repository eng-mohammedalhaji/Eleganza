using System.Net;
using Eleganza.Application.Abstractions;
using Eleganza.Domain.Entities;
using Eleganza.Infrastructure.Shipping;
using Microsoft.Extensions.Options;

namespace Eleganza.UnitTests.Shipping;

public sealed class VanexShippingProviderTests
{
    [Fact]
    public async Task CreateOrder_UsesBearerTokenAndVanexMultipartContract()
    {
        HttpRequestMessage? capturedRequest = null;
        var handler = new StubHandler(async request =>
        {
            capturedRequest = request;
            return JsonResponse("{\"data\":{\"package_code\":\"VX-123\",\"tracking_number\":\"TRK-123\"}}");
        });
        var provider = CreateProvider(handler);
        var account = VendorShippingAccount.Create(Guid.NewGuid(), "Vanex");

        var result = await provider.CreateOrderAsync(account, "secret-token", new ShippingOrderRequest(
            "ELG-123",
            "Sara",
            "0910000000",
            "Tripoli",
            2,
            122,
            "Street 1",
            null,
            "Leave at reception",
            485m,
            [new ShippingOrderItem("Rose Dress", "M", "Black", 1, 485m)]));

        Assert.True(result.Succeeded);
        Assert.Equal("VX-123", result.ExternalOrderId);
        Assert.Equal("TRK-123", result.TrackingNumber);
        Assert.NotNull(capturedRequest);
        Assert.Equal("Bearer", capturedRequest!.Headers.Authorization?.Scheme);
        Assert.Equal("secret-token", capturedRequest.Headers.Authorization?.Parameter);
        Assert.StartsWith("multipart/form-data", capturedRequest.Content!.Headers.ContentType?.MediaType);

        var formBody = await capturedRequest.Content.ReadAsStringAsync();
        Assert.Contains("name=\"city\"", formBody);
        Assert.Contains("\r\n\r\n2\r\n", formBody);
        Assert.Contains("name=\"address_child\"", formBody);
        Assert.Contains("\r\n\r\n122\r\n", formBody);
        Assert.Contains("\r\n\r\n485\r\n", formBody);
        Assert.Contains("\r\n\r\n1\r\n", formBody);
        Assert.Contains("Rose Dress (M/Black) x1", formBody);
    }

    [Fact]
    public async Task GetCities_ParsesNestedVanexResponse()
    {
        var provider = CreateProvider(new StubHandler(_ => Task.FromResult(
            JsonResponse("{\"data\":[{\"id\":2,\"name\":\"Tripoli\"},{\"id\":3,\"name\":\"Benghazi\"}] }"))));
        var account = VendorShippingAccount.Create(Guid.NewGuid(), "Vanex");

        var result = await provider.GetCitiesAsync(account, "secret-token");

        Assert.True(result.Succeeded);
        Assert.Collection(result.Locations,
            city => { Assert.Equal("2", city.Id); Assert.Equal("Tripoli", city.Name); },
            city => { Assert.Equal("3", city.Id); Assert.Equal("Benghazi", city.Name); });
    }

    [Fact]
    public async Task CreateOrder_RejectsMissingVanexLocationIdsBeforeHttpCall()
    {
        var called = false;
        var provider = CreateProvider(new StubHandler(_ =>
        {
            called = true;
            return Task.FromResult(JsonResponse("{}"));
        }));
        var account = VendorShippingAccount.Create(Guid.NewGuid(), "Vanex");

        var result = await provider.CreateOrderAsync(account, "secret-token", new ShippingOrderRequest(
            "ELG-123", "Sara", "0910000000", "Tripoli", null, null, "Street 1", null, null, 485m, []));

        Assert.False(result.Succeeded);
        Assert.Contains("city", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.False(called);
    }

    [Fact]
    public async Task CreateOrder_RejectsVendorProvidedBaseUrlOutsideAllowlist()
    {
        var called = false;
        var provider = CreateProvider(new StubHandler(_ =>
        {
            called = true;
            return Task.FromResult(JsonResponse("{}"));
        }));
        var account = VendorShippingAccount.Create(Guid.NewGuid(), "Vanex");
        account.MarkPendingValidation("protected", null, "https://attacker.example/api/v1");

        var result = await provider.CreateOrderAsync(account, "secret-token", new ShippingOrderRequest(
            "ELG-123", "Sara", "0910000000", "Tripoli", 2, 122, "Street 1", null, null, 485m, []));

        Assert.False(result.Succeeded);
        Assert.Contains("not allowed", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.False(called);
    }

    private static VanexShippingProvider CreateProvider(HttpMessageHandler handler)
        => new(new HttpClient(handler), Options.Create(new VanexOptions
        {
            Enabled = true,
            BaseUrl = "https://api.example.test/api/v1",
            AllowedBaseUrls = ["https://api.example.test/api/v1"],
            CitiesPath = "city/names",
            SubCitiesPath = "city/{cityId}/subs",
            CreateOrderPath = "customer/package",
            DefaultPackageType = "1",
            DefaultLength = "36",
            DefaultWidth = "35",
            DefaultHeight = "35",
            DefaultBreakable = "0",
            DefaultReceiptMoney = "false",
            DefaultMeasuringIsAllowed = "1",
            DefaultStorageSubscription = "false",
            DefaultInspectionAllowed = "1",
            DefaultHeatIntolerance = "1",
            DefaultCasing = "false",
            DefaultPaidBy = "market",
            DefaultExtraSizeBy = "market",
            DefaultCommissionBy = "market",
            DefaultSaveAddress = "false",
            DefaultPaymentMethod = "cash",
            DefaultPackageSubType = "1",
            DefaultTypeId = "1",
        }));

    private static HttpResponseMessage JsonResponse(string body)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"),
        };

    private sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> callback) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => callback(request);
    }
}
