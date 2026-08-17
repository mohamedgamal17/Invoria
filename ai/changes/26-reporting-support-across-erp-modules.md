---
branch: 26-reporting-support-across-erp-modules
base: master
pr: ""
last_updated: 2026-08-16
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

## Cross-cutting
- **BackgroundJobs** — New `Invoria.BackgroundJob.Core` abstraction + Hangfire provider (`Invoria.BackgroundJobs.Hangfire`) with `IJob`, recurrence scheduling, checkpoints, middleware pipeline, and job execution context; applied to the customer/product/supplier report jobs.

# API Changes

## Ordering
### Report Order Sales Metrics (`/report/order-sales-metrics`)
- `GET GetOrderSalesMetrics` — dashboard totals (no request params; returns `ReportOrderSalesMetricsDto`).
- `GET ListOrderSalesMetrics` — paged history (`Period`, `Skip`, `Length`).

### Report Order Completed Metrics (`/report/order-completed-metrics`)
- `GET GetOrderCompletedMetrics` — dashboard totals.
- `GET ListOrderCompletedMetrics` — paged history.

### Report Order Sales Profit Metrics (`/report/orders/sales-profit`)
- `GET GetOrderSalesProfitMetrics` — dashboard totals (`TotalRevenue`/`TotalCost`/`TotalProfit`/`TotalReturnAmount` per period).
- `GET ListOrderSalesProfitMetrics` — paged history (`Period`, `Skip`, `Length`).

## Procurement
### Report Supplier Metrics (`/report/supplier-metrics`)
- `GET GetSupplierMetrics` — dashboard totals.
- `GET ListSupplierMetrics` — paged history.

### Report Purchase Sales Metrics (`/report/purchase-sales-metrics`)
- `GET GetPurchaseSalesMetrics` / `GET ListPurchaseSalesMetrics`.

### Report Purchase Orders Completed Metrics (`/report/purchase-orders-completed-metrics`)
- `GET GetPurchaseOrdersCompletedMetrics` / `GET ListPurchaseOrdersCompletedMetrics`.

## Catalog
### Report Product Metrics (`/report/product-metrics`)
- `GET GetProductMetrics` / `GET ListProductMetrics`.

## CustomerManagement
### Report Customer Metrics (`/report/customer-metrics`)
- `GET GetCustomerMetrics` / `GET ListCustomerMetrics`.

# Code Changes

## Ordering

### Domain (`Invoria.Ordering.Domain`)
- Report entities: `ReportOrderSalesMetrics`, `ReportOrderCompletedMetrics`, `ReportOrderSalesProfitMetrics`.
- `OrderAllocationConsumption` aggregate with `OrderAllocationConsumptionCreatedDomainEvent`.
- `OrderStateTransitionHistory` entity.

### Application (`Invoria.Ordering.Application`)
- Report commands/queries/factories for all three Ordering report features (paged `List` + `Get` dashboard via `ResponseFactory`).
- `RecordOrderSalesProfitMetricsCommandHandler` — LIFO batch deduction with returns deducted from the last batch first at that batch's unit price; upserts one entity per period.
- `OrderAllocationConsumptionCreatedDomainEventHandler` — resolves a fresh `IServiceScope`/`IMediator` via `IServiceScopeFactory` to avoid same-`DbContext` re-entrancy deadlock when the domain-event dispatcher runs inside the after-save hook.
- Saga activities recording sales and completed metrics on order completion.

### Infrastructure (`Invoria.Ordering.Infrastructure`)
- Tables, migrations, EF configurations (unique period-date index/constraint), saga activity wiring.

### Endpoints / Presentation (`Invoria.Ordering.Endpoints`)
- `ReportOrderSalesMetrics`, `ReportOrderCompletedMetrics`, `ReportOrderSalesProfitMetrics` routing groups + endpoints (`/report/orders/sales-profit/overview` and `/report/orders/sales-profit/metrics`).

### Contracts (`Invoria.Ordering.Contracts`)
- Report DTOs (`ReportOrderSalesMetricsDto`/`PeriodDto`, `ReportOrderCompletedMetrics*`, `ReportOrderSalesProfitMetricsDto`/`PeriodDto`), `OrderStateTransitionHistoryDto`, allocation-consumption integration events.

### Testing
- `Invoria.Ordering.Application.Tests`: record command/handler tests, consumption-created event handler tests, multi-batch LIFO return deduction cases, profit metrics List/Get query tests, saga activity coverage.
- `Invoria.Ordering.Endpoints.Tests`: report endpoint tests.

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
- Legacy Reporting module removed from solution, host wiring, and docs.

## BuildingBlocks (`Invoria.BuildingBlocks.*`)
- `ReportPeriod` enum in `BuildingBlocks.Domain.Enums` (values spaced by 5, starting at 5).
- `EndpointBaseWithoutRequest` for no-request report endpoints.

## BackgroundJobs (`Invoria.BackgroundJob.Core`, `Invoria.BackgroundJobs.*`)
- `IJob`/`IJob<T>`, `JobId`, `JobCheckpoint` (string JSON state, mutable for upsert), `IJobScheduler`/`IRecurringJobScheduler`, `JobExecutionContext`, middleware pipeline + logging middleware, `HangfireJobDispatcher`, builder extensions, `BackgroundJobsDbContext` (Hangfire + EF storage), recurring job support.
- Shared recurring interval const and `ReportJobCheckPoint` in `BackgroundJobs.Abstractions`.
- Respawn test fixtures exclude Hangfire schema from resets.

## Documented conventions
- `ai/Report-Jobs.md` (checkpointed batched report-job recipe), `ai/BackgroundJobs.md`, AGENTS.md updates (`Report` prefix, enum spacing, DB-reset + report-job testing rules).

# Integration and messaging

- Order allocation consumption flow: `RequestOrderAllocationConsumption` integration event (Contracts) consumed for consumption creation; `OrderAllocationConsumptionCreatedDomainEvent` triggers profit-metrics recording.
- Procurement report recording is driven by Rebus consumers (`RecordPurchaseSalesMetricsIntegrationEventConsumer`, `RecordPurchaseOrdersCompletedMetricsIntegrationEventConsumer`).
- Ordering sales/completed metrics recording driven by Saga activities inside the order-completion pipeline.