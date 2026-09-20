# Vanex Integration Contract

The application is ready for a provider adapter, but the final HTTP mapping must be filled from Vanex's official Swagger/Postman contract before enabling production calls.

## Required provider information

- API base URL.
- Access token header format and expiry rules.
- Token validation endpoint.
- Create delivery/order endpoint.
- Cancel delivery endpoint.
- Track/status endpoint.
- Webhook signature and event payload, or polling rules.
- Required city/area identifiers.
- Cash-on-delivery field name and limits.
- Sandbox credentials and test order procedure.

## Current safe defaults

- `Vanex:Enabled` is `false`.
- No real token is stored in configuration or source control.
- Vendor tokens are encrypted before persistence with ASP.NET Data Protection.
- The API never returns the token to the frontend.
- The adapter returns a controlled configuration error until the official paths are configured.

## Expected internal mapping

```text
Eleganza Order
  -> ShippingOrderRequest
  -> IShippingProvider
  -> VanexShippingProvider
  -> ExternalOrderId / TrackingNumber
```

The provider adapter must not change catalog, pricing, or order ownership rules. It only creates and tracks the external delivery order.
