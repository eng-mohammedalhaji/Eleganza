# Vanex integration contract

The adapter follows the public Vanex Postman collection supplied for this
project: [Vanex API collection](https://www.postman.com/restless-station-814174/vanex-api/collection/25026561-1dcb33e0-7e75-4ea8-be8c-887c0236ef1c?action=share&source=copy-link&creator=0).

## Environment and authentication

Vanex uses a bearer token in the `Authorization` header. Eleganza does not
generate or expose this token: an approved vendor pastes the token issued for
their own Vanex account into the vendor portal. The token is encrypted before
it is persisted and is never returned to the browser or written to logs.

The sandbox base URL and all provider paths are configuration, not source-code
constants:

```text
Vanex:BaseUrl       = https://api.vanextest.com.ly/api/v1
Vanex:CitiesPath    = city/names
Vanex:SubCitiesPath = city/{cityId}/subs
Vanex:CreateOrderPath = customer/package
```

Production values must be supplied through deployment configuration or a
secret/configuration provider. Do not commit production tokens or override a
vendor's base URL without an explicit allowlist policy.

## Implemented endpoints

| Use case | Method | Path | Format | Adapter state |
|---|---:|---|---|---|
| Validate token and load cities | GET | `/city/names` | JSON response | Implemented |
| Load sub-cities | GET | `/city/{cityId}/subs` | JSON response | Implemented |
| Create a delivery package | POST | `/customer/package` | `multipart/form-data` | Implemented |
| List packages | GET | `/customer/package?page={page}` | JSON response | Contract captured; next use case |
| Track a package | GET | `/customer/packages/{package-code}` | JSON response | Contract captured; next use case |
| List collection requests | GET | `/customer/collects/status?status={status}&page={page}` | JSON response | Contract captured; next use case |
| Create a collection request | POST | `/customer/collects` | JSON body | Contract captured; next use case |
| Show a collection request | GET | `/customer/collects/{id}/show` | JSON response | Contract captured; next use case |

The current Eleganza delivery flow uses the first three endpoints. Package
tracking and collection operations are intentionally separate application use
cases and must not be added to the order-creation controller.

## Create package mapping

Vanex's package endpoint expects form fields. The adapter maps the server-owned
Eleganza order to the following fields:

- customer/address: `reciever`, `phone`, `phone_b`, `address`, `map`;
- location: `city`, `address_child` using IDs loaded from Vanex;
- cash on delivery: `price`, `payment_methode`;
- package data: `description`, `qty`, `type`, `type_id`, dimensions, and the
  configured service flags;
- merchant policy: `paid_by`, `extra_size_by`, `commission_by`, and
  `sticker_notes`.

The local order total, item prices, inventory, and vendor ownership are always
the source of truth. Client totals are ignored. The outbox worker sends the
request only after the vendor confirms the order.

## Reliability and failure behavior

```text
Local order
  -> vendor confirmation
  -> SubmitShippingOrder outbox message
  -> bounded background attempt
  -> Vanex package code saved locally
```

- A failed Vanex call never deletes or rolls back the local order.
- Failed attempts are persisted with retry scheduling and dead-letter state.
- A successful response must contain a package identifier; otherwise it is
  treated as a failed integration response.
- `ExternalShippingOrderId` stores the package code used for later tracking.
- Retry and idempotency behavior must remain safe if the worker is restarted.
- Provider response bodies and tokens are not sent to clients. The API returns
  a stable ProblemDetails error code and a safe message.

## Required production gates

Before enabling `Vanex:Enabled=true` in production:

1. Configure the production base URL and all package defaults through a secret
   or deployment configuration source.
2. Run a sandbox order with a real vendor test token and verify the returned
   package code and tracking behavior.
3. Add deterministic contract tests for authentication, locations,
   multipart field names, successful response parsing, 401/422 responses,
   timeout, duplicate delivery attempts, and malformed JSON.
4. Confirm that the deployment has persistent ASP.NET Data Protection keys;
   otherwise a restart would make encrypted vendor tokens unreadable.
5. Enable monitoring for outbox dead letters and require an operational retry
   path before accepting live orders.
