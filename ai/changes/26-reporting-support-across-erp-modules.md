---
branch: 26-reporting-support-across-erp-modules
base: master
pr: ""
last_updated: 2026-08-17
author: ""
---

# Features

Adds cross-module **reporting support** backed by a new background-job abstraction, replaces the legacy Reporting module with per-module report metrics, and records sales/profit metrics from order execution events.

## Ordering
- **Report order sales metrics** — Recorded on order completion via a Saga activity into `ReportOrderSalesMetrics`; exposes `ThisDay/ThisMonth/ThisYear/AllTime` dashboard + paged history.
- **Report order completed metrics** — Count of completed orders per period, recorded on completion via a Saga activity.
- **Report order sales profit metrics** — Profit per period computed from allocation consumption (LIFO batch cost deduction, returns deducted last-to-first at the batch's actual unit price); recorded on `OrderAllocationConsumptionCreated` domain event.
- **Order allocation consumption** — New aggregate recording which allocation batches were consumed (`OrderAllocationConsumption`) with created domain event and integration flow.
- **Order state transition history** — State-change log surfaced in order responses.

## Procurement
- **Supplier metrics** — Background job computing supplier counts per period, paged history + dashboard.
- **Purchase sales metrics** — Consumer-driven recording on purchase order completion.
- **Purchase orders completed metrics** — Consumer-driven recording on purchase order completion.

## Catalog
- **Product metrics** — Background job with checkpointed batching counting products per date bucket.

## CustomerManagement
- **Customer metrics** — Checkpointed background job counting customers per period, with paged history + dashboard.

## Inventory
- **Order allocation consumption query** — Consumes the Ordering allocation-consumption request and exposes which batches/lines were consumed per order.

## Cross-cutting
- **BackgroundJobs** — New `Invoria.BackgroundJob.Core` abstraction + Hangfire provider (`Invoria.BackgroundJobs.Hangfire`) with `IJob`, recurrence scheduling, checkpoints, middleware pipeline, and job execution context; applied to the customer/product/supplier report jobs.

# API Changes

## Ordering
### Report Order Sales Metrics (`/report/orders/sales`)
- `GET /report/orders/sales/overview` — dashboard totals (no request params; returns `ReportOrderSalesMetricsDto`).
- `GET /report/orders/sales/metrics` — paged history (`Period`, `Skip`, `Length`).

### Report Order Completed Metrics (`/report/orders/completion`)
- `GET /report/orders/completion/overview` — dashboard totals.
- `GET /report/orders/completion/metrics` — paged history.

### Report Order Sales Profit Metrics (`/report/orders/sales-profit`)
- `GET /report/orders/sales-profit/overview` — dashboard totals (`TotalRevenue`/`TotalCost`/`TotalProfit`/`TotalReturnAmount` per period).
- `GET /report/orders/sales-profit/metrics` — paged history (`Period`, `Skip`, `Length`).

## Procurement
### Report Supplier Metrics (`/report/suppliers/creation`)
- `GET /report/suppliers/creation/overview` — dashboard totals.
- `GET /report/suppliers/creation/metrics` — paged history.

### Report Purchase Sales Metrics (`/report/purchase-orders/sales`)
- `GET /report/purchase-orders/sales/overview` / `GET /report/purchase-orders/sales/metrics`.

### Report Purchase Orders Completed Metrics (`/report/purchase-orders/completion`)
- `GET /report/purchase-orders/completion/overview` / `GET /report/purchase-orders/completion/metrics`.

## Catalog
### Report Product Metrics (`/report/products/creation`)
- `GET /report/products/creation/overview` / `GET /report/products/creation/metrics`.

## CustomerManagement
### Report Customer Metrics (`/report/customers/creation`)
- `GET /report/customers/creation/overview` / `GET /report/customers/creation/metrics`.

# Code Changes

## Ordering

### Domain (`Invoria.Ordering.Domain`)
- Report entities: `ReportOrderSalesMetrics`, `ReportOrderCompletedMetrics`, `ReportOrderSalesProfitMetrics`.
- `OrderAllocationConsumption` aggregate with `OrderAllocationConsumptionCreatedDomainEvent`.
- `OrderStateTransitionHistory` entity.
- `Order` now records a `StateTransitionHistory` entry on every status change (`RecordTransition`) and exposes `TotalReturnAmount`.

### Application (`Invoria.Ordering.Application`)
- Report commands/queries/factories for all three Ordering report features (paged `List` + `Get` dashboard via `ResponseFactory`).
- `RecordOrderSalesProfitMetricsCommandHandler` — LIFO batch deduction with returns deducted from the last batch first at that batch's unit price; upserts one entity per period.
- `OrderAllocationConsumptionCreatedDomainEventHandler` — resolves a fresh `IServiceScope`/`IMediator` via `IServiceScopeFactory` to avoid same-`DbContext` re-entrancy deadlock when the domain-event dispatcher runs inside the after-save hook.
- Saga activities recording sales and completed metrics on order completion.
- `OrderResponseFactory` and `GetOrderByIdQueryHandler` surface `StateTransitionHistory` (ordered by `ChangedAt`) in order responses.

### Infrastructure (`Invoria.Ordering.Infrastructure`)
- Tables, migrations, EF configurations (unique period-date index/constraint), saga activity wiring.

### Endpoints / Presentation (`Invoria.Ordering.Endpoints`)
- `ReportOrderSalesMetrics`, `ReportOrderCompletedMetrics`, `ReportOrderSalesProfitMetrics` routing groups + endpoints (`/report/orders/sales-profit/overview` and `/report/orders/sales-profit/metrics`).

### Contracts (`Invoria.Ordering.Contracts`)
- Report DTOs (`ReportOrderSalesMetricsDto`/`PeriodDto`, `ReportOrderCompletedMetrics*`, `ReportOrderSalesProfitMetricsDto`/`PeriodDto`), `OrderStateTransitionHistoryDto`, allocation-consumption integration events.

### Testing
- `Invoria.Ordering.Application.Tests`: record command/handler tests, consumption-created event handler tests, multi-batch LIFO return deduction cases, profit metrics List/Get query tests, saga activity coverage.
- `Invoria.Ordering.Endpoints.Tests`: report endpoint tests.

## Inventory

### Application (`Invoria.Inventory.Application`)
- `RequestOrderAllocationIntegrationEventConsumer` — consumes the Ordering `RequestOrderAllocationIntegrationEvent` and maps allocation consumption.
- `GetOrderAllocationConsumptionQuery` / handler — reads order allocation consumption per order.
- `AllocationMappingExtensions` — maps allocation/batch data to the consumption contract models.

### Contracts (`Invoria.Inventory.Contracts`)
- `OrderAllocationConsumptionIntegrationEvent` with `OrderAllocationConsumptionBatchModel` / `OrderAllocationConsumptionLineModel` under `Allocations/`.

### Infrastructure (`Invoria.Inventory.Infrastructure`)
- `RebusHandlersServiceInstaller` registers the consumption consumer; `InventoryModuleBootStrapper` subscribes to the Ordering request event.

## Procurement

### Domain (`Invoria.Procurement.Domain`)
- Report entities: `ReportSupplierMetrics`, `ReportPurchaseSalesMetrics`, `ReportPurchaseOrdersCompletedMetrics`.

### Application (`Invoria.Procurement.Application`)
- Supplier metrics: checkpointed `ReportSupplierMetricsJob` + queries/factory.
- Purchase features: `RecordPurchaseSalesMetrics` / `RecordPurchaseOrdersCompletedMetrics` commands with Rebus consumers + queries/factories.

### Infrastructure (`Invoria.Procurement.Infrastructure`)
- Tables, migrations, recurring job registration and Rebus subscription wiring.

### Endpoints (`Invoria.Procurement.Endpoints`)
- Report routing groups + endpoints for the three reporting features.

### Contracts (`Invoria.Procurement.Contracts`)
- Report DTOs.

### Testing
- `Invoria.Procurement.Application.Tests` and `Invoria.Procurement.Endpoints.Tests`: job, command/consumer, and endpoint coverage.

## Catalog

- Domain `ReportProductMetrics`; Application `ReportProductMetricsJob` (date-bucket counting) + queries/factory; Infrastructure table/migration/recurring registration; Endpoints routing group; endpoint tests.

## CustomerManagement

- Domain `ReportCustomerMetrics`; Application `ReportCustomerMetricsJob` (checkpointed batching, per-date-bucket counts) + queries/factory; Infrastructure table/migration/recurring registration; Endpoints group; job + endpoint tests.

# Cross-cutting

## Host / API (`Invoria.Api`)
- BackgroundJobs module installed in the API composition root; Hangfire schema guarded against missing connection string.
- Legacy Reporting module removed from solution, host wiring, and docs; `LegacyReportingCleanupBootstrapper` drops the legacy Reporting tables on startup.

## BuildingBlocks (`Invoria.BuildingBlocks.*`)
- `ReportPeriod` enum in `BuildingBlocks.Domain.Enums` (values spaced by 5, starting at 5).
- `EndpointBaseWithoutRequest` for no-request report endpoints.

## BackgroundJobs (`Invoria.BackgroundJob.Core`, `Invoria.BackgroundJobs.*`)
- `IJob`/`IJob<T>`, `JobId`, `JobCheckpoint` (string JSON state, mutable for upsert), `IJobScheduler`/`IRecurringJobScheduler`, `JobExecutionContext`, middleware pipeline + logging middleware, `HangfireJobDispatcher`, builder extensions, `BackgroundJobsDbContext` (Hangfire + EF storage), recurring job support.
- Shared recurring interval const and `ReportJobCheckPoint` in `BackgroundJobs.Abstractions`.
- Respawn test fixtures exclude Hangfire schema from resets.

## Tooling and docs
- `.config/dotnet-tools.json` adds `dotnet-ef` 8.0.29; new `OrderingDbContextFactory` supports design-time migrations.
- `README.md` drops the legacy Reporting bullet; `ai/CodingStyle.md` and `.cursor/rules/report-class-naming.mdc` document the `Report` class-prefix convention.
- `ai/Test-Conventions.md` and AGENTS.md document the mandatory DB reset per SQL Server-touching fixture.

## Documented conventions
- `ai/Report-Jobs.md` (checkpointed batched report-job recipe), `ai/BackgroundJobs.md`, AGENTS.md updates (`Report` prefix, enum spacing, DB-reset + report-job testing rules).

# Integration and messaging

- Order allocation consumption flow: Ordering publishes `RequestOrderAllocationIntegrationEvent` (Ordering.Contracts); Inventory consumes it and publishes `OrderAllocationConsumptionIntegrationEvent` (Inventory.Contracts); Ordering's `OrderAllocationConsumptionIntegrationEventConsumer` sends `CreateOrderAllocationConsumptionCommand`, and the resulting `OrderAllocationConsumptionCreatedDomainEvent` triggers profit-metrics recording.
- Procurement report recording is driven by Rebus consumers (`RecordPurchaseSalesMetricsIntegrationEventConsumer`, `RecordPurchaseOrdersCompletedMetricsIntegrationEventConsumer`).
- Ordering sales/completed metrics recording driven by Saga activities inside the order-completion pipeline.