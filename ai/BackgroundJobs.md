# Invoria Background Jobs Architecture

> **Last updated**: 2026-07-26
> **Applies to**: All modules that need async/background work, job checkpoints, recurring or delayed scheduling.

## Overview

The background job system is split into two layers:

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Abstractions** | `src/Infrastructure/Invoria.BackgroundJob.Core/` | Interfaces, primitives (`JobId`, `Recurrence` types), middleware pipeline, context, checkpoints — **no runtime dependency** |
| **Hangfire implementation** | `src/Infrastructure/Invoria.BackgroundJobs.Hangfire/` | Hangfire server, storage, `HangfireJobDispatcher`, EF checkpoint store, execution context wiring |
| **Module** | `src/Modules/BackgroundJobs/BackgroundJobs.{Abstractions,Application,Infrastructure}/` | Module shell that wires Core + Hangfire into the app via `IModuleInstaller` |

---

## Project Layout

```
src/Infrastructure/
├── Invoria.BackgroundJob.Core/                      # Abstractions (no runtime dep)
│   ├── IJobScheduler.cs                             # Enqueue<T>, Schedule<T>(delay), Delete
│   ├── IRecurringJobScheduler.cs                    # AddOrUpdate<T>, Remove, Trigger
│   ├── Identifiers/JobId.cs                         # readonly record struct, implicit string
│   ├── Jobs/
│   │   ├── IJob.cs                                  # Task Execute(CancellationToken)
│   │   └── IJobOfT.cs                               # Task Execute(T arg, CancellationToken)
│   ├── Configuration/
│   │   ├── IBackgroundJobsBuilder.cs                # DI builder interface
│   │   └── BackgroundJobsBuilder.cs                 # Default implementation
│   ├── Extensions/
│   │   └── BackgroundJobsServiceCollectionExtensions.cs  # AddBackgroundJobs, UseLoggingMiddleware, AddJob<T>, AddJobsFromAssembly
│   ├── Context/
│   │   ├── IJobExecutionContext.cs                  # JobId + CancellationToken
│   │   ├── IJobExecutionContextAccessor.cs          # Current context accessor
│   │   └── JobExecutionContextAccessor.cs           # AsyncLocal-backed singleton
│   ├── Middlewares/
│   │   ├── IJobMiddleware.cs                        # InvokeAsync(context, next)
│   │   ├── IJobMiddlewarePipeline.cs                # ExecuteAsync(context, terminal)
│   │   ├── JobExecutionDelegate.cs                  # delegate Task(IJobExecutionContext)
│   │   └── LoggingJobMiddleware.cs                  # Log start/complete/failure
│   ├── Checkpoints/
│   │   ├── IJobCheckpointStore.cs                   # Save, Restore, Delete
│   │   └── JobCheckpoint.cs                         # JobId, Name, State (string JSON), CreatedAt, UpdatedAt
│   └── Scheduling/
│       ├── Recurrence.cs                            # abstract record
│       ├── IntervalRecurrence.cs                    # TimeSpan
│       ├── DailyRecurrence.cs                       # TimeOnly At
│       ├── WeeklyRecurrence.cs                      # DayOfWeek + TimeOnly
│       ├── MonthlyRecurrence.cs                     # int Day + TimeOnly
│       └── CustomRecurrence.cs                      # string Expression (raw cron)
│
└── Invoria.BackgroundJobs.Hangfire/                 # Hangfire implementation
    ├── Configuration/
    │   ├── HangfireOptions.cs                       # ConnectionString, CheckpointsConnectionString, SchemaName
    │   └── HangfireBuilder.cs                       # Extends BackgroundJobsBuilder
    ├── Context/
    │   └── HangfireExecutionContext.cs              # IJobExecutionContext impl
    ├── Extensions/
    │   ├── HangfireBuilderExtensions.cs             # .UseHangfire(configure?) — registers dispatcher, pipeline, scheduler, server
    │   └── EntityFrameworkServiceCollectionExtensions.cs  # .AddBackgroundJobDbContext<TContext>(configure)
    ├── Execution/
    │   └── HangfireJobDispatcher.cs                 # Internal — resolves IJob by type name, wraps in pipeline + context
    ├── Middleware/
    │   ├── HangfireJobMiddlewarePipeline.cs         # Composite pipeline runner
    │   ├── JobExecutionContextMiddleware.cs         # Sets/clears AsyncLocal accessor
    │   └── HangfireJobIdCaptureFilter.cs            # IServerFilter — captures RecurringJobId or job type as JobId
    ├── HangfireJobScheduler.cs                      # IJobScheduler impl (Hangfire IBackgroundJobClient wrapper)
    ├── HangfireRecurringJobScheduler.cs             # IRecurringJobScheduler impl (maps Recurrence → Cron)
    └── EntityFramework/
        ├── BackgroundJobsDbContext.cs               # Standalone DbContext for job checkpoints
        ├── HangfireDbContext.cs                     # Abstract base for module EF contexts
        ├── JobCheckpointStore.cs                    # IJobCheckpointStore EF impl (upsert by JobId)
        └── Configuration/
            └── JobCheckpointEntityTypeConfiguration.cs  # EF config — table "JobCheckpoints", PK JobId

src/Modules/BackgroundJobs/
├── BackgroundJobs.Abstractions/                     # Empty — placeholder for module-level contracts
├── BackgroundJobs.Application/
│   └── AssemblyReference.cs                         # Assembly marker for job scanning
└── BackgroundJobs.Infrastructure/
    ├── BackgroundJobsModuleInstaller.cs             # IModuleInstaller — discovers installers from assembly
    ├── BackgroundJobsModuleBootStrapper.cs          # Applies EF migrations on startup
    ├── Installers/
    │   ├── BackgroundJobCoreServiceInstaller.cs     # Wires Core + Hangfire + EF
    │   └── JobsServiceInstaller.cs                  # Scans Application assembly for IJob impls, registers as Transient
    └── EntityFramework/
        ├── BackgroundJobsDbContext.cs               # Module DB context (extends HangfireDbContext)
        └── Migrations/                              # EF migrations for JobCheckpoints table
```

---

## Adding a New Background Job

### 1. Create the job class

Place in the `BackgroundJobs.Application` project (or your module's Application layer):

```csharp
using Invoria.BackgroundJob.Core.Jobs;

namespace Invoria.BackgroundJobs.Application.Jobs;

public sealed class MyJob : IJob
{
    private readonly ISomeDependency _dep;

    public MyJob(ISomeDependency dep)
    {
        _dep = dep;
    }

    public async Task Execute(CancellationToken cancellationToken = default)
    {
        // ... business logic
    }
}
```

### 2. Register the job

The `JobsServiceInstaller` auto-scans `BackgroundJobs.Application` assembly for all classes implementing `IJob` and registers them as `Transient`. If your job lives in another module, add manual registration or extend the installer.

You can also register jobs manually via the builder:

```csharp
services.AddBackgroundJobs()
    .AddJob<MyJob>()                                                  // Single job
    .AddJobsFromAssemblyContaining<SomeType>();                        // Scan assembly
```

### 3. Schedule the job

Inject `IJobScheduler` (for one-off/delayed) or `IRecurringJobScheduler` (for recurring):

```csharp
// One-off
IJobScheduler scheduler;
string jobId = scheduler.Enqueue<MyJob>();

// Delayed
string delayedId = scheduler.Schedule<MyJob>(TimeSpan.FromMinutes(30));

// Recurring — use any Recurrence subclass
IRecurringJobScheduler recurring;
recurring.AddOrUpdate<MyJob>("my-recurring-id", new DailyRecurrence(new TimeOnly(3, 0)));
recurring.AddOrUpdate<MyJob>("my-interval-id", new IntervalRecurrence(TimeSpan.FromMinutes(15)));
recurring.AddOrUpdate<MyJob>("my-custom-id", new CustomRecurrence("0 0 * * *"));

// Remove / trigger
recurring.Remove("my-recurring-id");
recurring.Trigger("my-recurring-id");
```

**The schedulers delegate to `HangfireJobDispatcher`** which resolves the job type by `AssemblyQualifiedName`, creates a `HangfireExecutionContext`, and runs the job through the middleware pipeline.

### 4. Use job context (optional)

The `IJobExecutionContextAccessor` provides the current `JobId` within any middleware or the job itself:

```csharp
public class MyJob : IJob
{
    private readonly IJobExecutionContextAccessor _ctx;

    public MyJob(IJobExecutionContextAccessor ctx) { _ctx = ctx; }

    public async Task Execute(CancellationToken ct)
    {
        JobId currentJobId = _ctx.JobId!.Value;
        // ...
    }
}
```

### 5. Use job checkpoints (for long-running or resumable jobs)

```csharp
public class MyResumableJob : IJob
{
    private readonly IJobCheckpointStore _checkpoints;
    private readonly IJobExecutionContextAccessor _ctx;

    public MyResumableJob(IJobCheckpointStore checkpoints, IJobExecutionContextAccessor ctx)
    {
        _checkpoints = checkpoints;
        _ctx = ctx;
    }

    public async Task Execute(CancellationToken ct)
    {
        JobId jobId = _ctx.JobId!.Value;
        JobCheckpoint? cp = await _checkpoints.RestoreAsync(jobId, ct);

        int processed = cp is not null
            ? int.Parse(cp.State)
            : 0;

        // Process next batch...
        processed += 10;

        await _checkpoints.SaveAsync(new JobCheckpoint
        {
            JobId = jobId,
            Name = nameof(MyResumableJob),
            State = processed.ToString()
        }, ct);
    }
}
```

The `JobCheckpointStore<BackgroundJobsDbContext>` uses **upsert** logic: loads existing by `JobId`, updates `State` + `UpdatedAt` if found, otherwise inserts.

---

## Middleware Pipeline

The pipeline is pre-configured with two built-in middlewares (order matters):

1. **`JobExecutionContextMiddleware`** — Sets `IJobExecutionContextAccessor.Current` before job execution; clears on completion/failure.
2. **`LoggingJobMiddleware`** — Logs job start, completion, and failure (with exception).

To add custom middleware, implement `IJobMiddleware` and register it as a singleton:

```csharp
public sealed class MyMiddleware : IJobMiddleware
{
    public async Task InvokeAsync(IJobExecutionContext context, JobExecutionDelegate next)
    {
        // Pre-execution logic
        await next(context);
        // Post-execution logic
    }
}

// Registration:
builder.Services.AddSingleton<IJobMiddleware, MyMiddleware>();
```

The `HangfireJobMiddlewarePipeline` runs all registered `IJobMiddleware` instances in registration order via a recursive local function, then calls the terminal delegate (the job's `Execute`).

---

## Wiring / Registration

### In `ApiModuleInstaller`

```csharp
services.InstallModule<BackgroundJobsModuleInstaller>(configuration);
```

### In `BackgroundJobCoreServiceInstaller`

```csharp
var connectionString = configuration.GetConnectionString("Default");

services.AddBackgroundJobs()
    .UseLoggingJobMiddleware()
    .UseHangfire(options =>
    {
        options.ConnectionString = connectionString;
    })
    .AddBackgroundJobDbContext<BackgroundJobsDbContext>(cfg =>
    {
        cfg.UseSqlServer(connectionString, sqlCfg =>
            sqlCfg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
    });
```

This registers:

| Service | Implementation | Lifetime |
|---------|---------------|----------|
| `IJobScheduler` | `HangfireJobScheduler` | Scoped |
| `IRecurringJobScheduler` | `HangfireRecurringJobScheduler` | Scoped |
| `IJobExecutionContextAccessor` | `JobExecutionContextAccessor` | Singleton |
| `IJobMiddlewarePipeline` | `HangfireJobMiddlewarePipeline` | Singleton |
| `IJobCheckpointStore` | `JobCheckpointStore<BackgroundJobsDbContext>` | Scoped |
| `HangfireJobDispatcher` | HangfireJobDispatcher | Transient |
| Hangfire server | via `AddHangfireServer()` | — |
| Hangfire SQL storage | via `UseSqlServerStorage` | — |
| `HangfireJobIdCaptureFilter` | as server filter | — |
| All `IJobMiddleware` | registered instances | Singleton |

### EF Database for Checkpoints

The `JobCheckpoints` table is managed by `BackgroundJobsDbContext` (module-level), which extends `HangfireDbContext<BackgroundJobsDbContext>` → `InvoriaDbContext<BackgroundJobsDbContext>`. Migrations are auto-applied at startup by `BackgroundJobsModuleBootStrapper`.

---

## Recurrence → Cron Mapping

| Recurrence type | Cron result |
|----------------|-------------|
| `IntervalRecurrence(TimeSpan)` < 1h | `Cron.MinuteInterval(n)` |
| `IntervalRecurrence(TimeSpan)` < 24h | `Cron.HourInterval(n)` |
| `IntervalRecurrence(TimeSpan)` >= 24h | `Cron.DayInterval(n)` |
| `DailyRecurrence(TimeOnly)` | `Cron.Daily(hour, minute)` |
| `WeeklyRecurrence(DayOfWeek, TimeOnly)` | `Cron.Weekly(day, hour, minute)` |
| `MonthlyRecurrence(int day, TimeOnly)` | `Cron.Monthly(day, hour, minute)` |
| `CustomRecurrence(string)` | Used as-is (raw cron expression) |

---

## JobId Semantics

- `JobId` is a `readonly record struct` with implicit conversion to `string`.
- For **one-off jobs**, `JobId` = the Hangfire job type name (e.g., `"Invoria.BackgroundJobs.Application.Jobs.MyJob"`).
- For **recurring jobs**, `JobId` = the `RecurringJobId` passed to `AddOrUpdate<T>(recurringJobId, ...)`.
- Captured by `HangfireJobIdCaptureFilter` (`IServerFilter`), stored in `AsyncLocal<string>`.
- Accessible at runtime via `IJobExecutionContextAccessor.JobId`.

### HangfireJobIdCaptureFilter Logic

```csharp
// OnPerforming:
//   1. Try to read "RecurringJobId" from job parameters (for recurring jobs)
//   2. Fall back to parsing the first argument (job type name) from Args[0]
//   3. Store in AsyncLocal<string> CurrentJobName
// OnPerformed:
//   Clear CurrentJobName (set to null)
```

---

## Checkpoints Table Schema

| Column | Type | Notes |
|--------|------|-------|
| `JobId` | `nvarchar(256)` PK | Mapped via `JobIdValueConverter` (`JobId` ↔ `string`) |
| `Name` | `nvarchar(256)` required | Job class name or identifier |
| `State` | `nvarchar(256)` required | JSON string — mutable for upsert |
| `CreatedAt` | `datetimeoffset` | Set on creation |
| `UpdatedAt` | `datetimeoffset` | Updated on every save |

---

## When to Use Background Jobs vs Rebus

| Scenario | Mechanism | Example |
|----------|-----------|---------|
| Fire-and-forget async work within a module | `IJobScheduler.Enqueue<T>()` | Sending notification emails, generating reports |
| Delayed execution within a module | `IJobScheduler.Schedule<T>(delay)` | Expiring pending reservations after 30 min |
| Recurring scheduled tasks within a module | `IRecurringJobScheduler.AddOrUpdate<T>()` | Daily sales rollup, nightly data cleanup |
| Long-running resumable work | `IJobCheckpointStore` + `IJob` | Processing a large CSV in batches with progress |
| **Cross-module orchestration** | **Rebus sagas + integration events** | Order → Inventory allocation flow |

**Key rule**: Use `IJob` for **within-module async processing** that doesn't need cross-module event-driven orchestration. Use **Rebus** for cross-module integration events and sagas.

---

## Recent Changes (2026-07)

### Order Pricing Property Renames

In `Ordering.Contracts`, the following pricing properties were renamed on `OrderDto` and `OrderModel`:

| Old name | New name |
|----------|----------|
| `TotalOrderAmount` | `SubtotalAmount` |
| *(new)* | `ReturnsAmount` |
| `NetOfTotalOrderAmount` | `NetOrderAmount` |
| *(new)* | `AmountDue` |

Affected files:
- `Ordering.Contracts/Orders/Dtos/OrderDto.cs` — added `ReturnsAmount`, `AmountDue`; renamed properties
- `Ordering.Contracts/Orders/Models/OrderModel.cs` — renamed property
- `Ordering.Application/Orders/Factories/OrderResponseFactory.cs` — maps `TotalOrderAmount` → `SubtotalAmount`, `NetOfTotalOrderAmount` → `NetOrderAmount`, computes `ReturnsAmount` + `AmountDue`
- `Ordering.Application/Orders/Extensions/OrderMappingExtensions.cs` — maps `TotalOrderAmount` → `SubtotalAmount`

### ApproveReturn Endpoint

New endpoint in `Inventory.Endpoints`: `POST /returns/{id}/approve` (commit `bf5c9c2`).

Flow: `ApproveReturnCommand` → `Return.Approve()` → `ReturnApprovedDomainEvent` → (if `Immediate` type) publishes `ProcessImmediateReturnIntegrationEvent` → `ProcessImmediateReturnCommandHandler` → `IReturnDomainService.ProcessImmediateReturn` (restores stock via `Batch.AddReturn`).

### BackgroundJobs Module Initial Creation (commits `3f9732b`..`bf09c5f`)

- `Invoria.BackgroundJob.Core` created with job contracts, scheduling abstractions, middleware pipeline, execution context, checkpoint store, and DI builder.
- `Invoria.BackgroundJobs.Hangfire` created with Hangfire server wiring, `HangfireJobDispatcher`, EF checkpoint store, scheduler implementations.
- Module shell (`BackgroundJobs.Abstractions`, `.Application`, `.Infrastructure`) created with installer, bootstrapper, migration, and job scanning.
- Wired into `ApiModuleInstaller` via `services.InstallModule<BackgroundJobsModuleInstaller>(configuration)`.

---

## Conventions Summary

1. **Job classes** implement `IJob` (parameterless) or `IJob<T>` (parameterized). Place in `BackgroundJobs.Application` or your module's Application layer.
2. **Job registration** is automatic via assembly scanning in `JobsServiceInstaller` (looks at `BackgroundJobs.Application` assembly). Manual registration available via `AddJob<T>()` / `AddJobsFromAssembly()`.
3. **Scheduling APIs** — inject `IJobScheduler` (Scoped) for one-off/delayed, `IRecurringJobScheduler` (Scoped) for recurring.
4. **Context access** — inject `IJobExecutionContextAccessor` (Singleton) to get current `JobId` at runtime.
5. **Checkpoints** — inject `IJobCheckpointStore` (Scoped) for save/restore/delete of job state. Uses upsert by `JobId`.
6. **Middleware** — implement `IJobMiddleware` and register as Singleton. Built-ins: `LoggingJobMiddleware`, `JobExecutionContextMiddleware` (wired by default).
7. **Do NOT** use background jobs for cross-module orchestration — use Rebus sagas + integration events instead.
8. **Always assign named variables** — no inline chaining like `GetFoo().DoBar()`. Per `CodingStyle.md` — applies to both production and test code.
9. **DTO completeness** — when writing `PrepareDto` or `MapTo*` methods, explicitly list **every** property of both the source entity and target DTO (including inherited properties like `Id`). Cross-check against the full inheritance chain. Do not rely on default values for unmapped properties.
10. **Solution build order**: `dotnet restore Invoria.sln` then `dotnet build Invoria.sln` — ensure no compilation errors after changes.
