---
branch: feature/order-and-po-filters
base: main
pr: ""
last_updated: 2026-05-09
author: ""
---

# Features

## Ordering
### Order List Filters
- Filter by payment type (Deferred/Immediate)
- Filter by payment status (Paid/Partial/Unpaid)
- Filter by customer ID (required to enable front-end decomposition and enrich the customer details view)
- Filter by order status (Pending/Accepted/Complete/Reopened/Cancelled/Refused)
- Filter by fulfillment status (Pending/Allocating/Allocated/OnHold/Releasing/Dispatched/Cancelled)

## Procurement
### Purchase Order Filters
- Filter by status (Draft, Submitted, Approved, Reopened, Completed, Cancelled, Rejected)
- Filter by supplier ID (required to enable front-end decomposition and enrich the supplier details view)

# API Changes

## Ordering
### List Orders (`/orders`)
New optional query parameters have been added:
- `Status` (`OrderStatus`): Filter orders by order status
- `FulfillmentStatus` (`FulfillmentStatus`): Filter orders by fulfillment status
- `PaymentStatus` (`PaymentStatus`): Filter orders by payment status
- `PaymentType` (`OrderPaymentType`): Filter orders by payment type
- `CustomerId`: Filter orders by customer ID

## Procurement
### List Purchase Orders (`/purchase-orders`)
New optional query parameters have been added:
- `Status` (`PurchaseState`): Filter purchase orders by status
- `SupplierId`: Filter purchase orders by supplier ID

# Code Changes

## Ordering
### Domain (`Invoria.Ordering.Domain`)
- No changes introduced in this layer.

### Application (`Invoria.Ordering.Application`)
- Added optional properties to `ListOrdersQuery` (`CustomerId`, `PaymentType`, `PaymentStatus`, `Status`, `FulfillmentStatus`) to support the new filtering requirements.
- Updated `ListOrdersQueryHandler` with conditional logic to apply the new filters during query execution.

### Infrastructure (`Invoria.Ordering.Infrastructure`)
- No changes introduced in this layer.

### Endpoints / Presentation (`Invoria.Ordering.Endpoints`)
- Added optional properties to `ListOrdersRequest` to expose the new query parameters to clients.
- Implemented mapping logic in `ListOrdersEndpoint` to translate request parameters into the query object.

### Contracts (`Invoria.Ordering.Contracts`)
- No changes introduced in this layer.

### Testing
- Added unit tests in `ListOrdersQueryHandlerTests` to verify filter functionality.
- Added endpoint tests in `ListOrdersEndpointTests` to validate correct parameter handling.

## Procurement
### Domain (`Invoria.Procurement.Domain`)
- No changes introduced in this layer.

### Application (`Invoria.Procurement.Application`)
- Added optional properties to `ListPurchaseOrdersQuery` (`SupplierId`, `Status`) to support the new filtering requirements.
- Updated `ListPurchaseOrdersQueryHandler` with conditional logic to apply the new filters during query execution.

### Infrastructure (`Invoria.Procurement.Infrastructure`)
- No changes introduced in this layer.

### Endpoints / Presentation (`Invoria.Procurement.Endpoints`)
- Added optional properties to `ListPurchaseOrdersRequest` to expose the new query parameters to clients.
- Implemented mapping logic in `ListPurchaseOrdersEndpoint` to translate request parameters into the query object.

### Contracts (`Invoria.Procurement.Contracts`)
- No changes introduced in this layer.

### Testing
- Added unit tests in `ListPurchaseOrdersQueryHandlerTests` to verify filter functionality.
- Added endpoint tests in `ListPurchaseOrdersEndpointTests` to validate correct parameter handling.
