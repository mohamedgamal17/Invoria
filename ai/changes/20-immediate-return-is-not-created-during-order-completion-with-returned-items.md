---
branch: 20-immediate-return-is-not-created-during-order-completion-with-returned-items
base: master
pr: ""
last_updated: 2026-07-11
author: ""
---

# Features

## Ordering
### Rename `Items` to `ReturnItems` in `CompleteOrderRequest`
- Renamed the `Items` property on `CompleteOrderRequest` to `ReturnItems` to align with the domain terminology used in `OrderDto.ReturnItems`, `OrderReturnSaga`, and `OrderReturnLineModel`.

# API Changes

## Ordering
### Complete Order (`POST /orders/{id}/complete`)
- **Request body change**: The property `Items` has been renamed to `ReturnItems`. No type or semantic change — existing callers sending `Items` will need to update to `ReturnItems`.

# Code Changes

## Ordering

### Domain (`Invoria.Ordering.Domain`)
- No changes introduced in this layer.

### Application (`Invoria.Ordering.Application`)
- No changes introduced in this layer.

### Infrastructure (`Invoria.Ordering.Infrastructure`)
- No changes introduced in this layer.

### Endpoints / Presentation (`Invoria.Ordering.Endpoints`)
- `CompleteOrderRequest.cs`: Renamed `Items` property to `ReturnItems`; updated validator to reference `x.ReturnItems`.
- `CompleteOrderEndpoint.cs`: Updated handler to read `req.ReturnItems` instead of `req.Items`.

### Contracts (`Invoria.Ordering.Contracts`)
- No changes introduced in this layer.

### Testing
- `Invoria.Ordering.Endpoints.Tests`: Updated `CompleteOrderEndpointTests.cs` and `ListOrdersEndpointTests.cs` to use `ReturnItems` in object initializers.

# Cross-cutting

_Omit — all changes are scoped to the Ordering Endpoints and test layers._

# Integration and messaging

_Omit — no messaging changes._
