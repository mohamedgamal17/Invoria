# Invoria — Report Jobs Guide

> Reference implementation: `ReportCustomerMetricsJob` (CustomerManagement).
> Applies to any "Report" background job that batches over an entity and upserts period-based totals.

## 1. Job file

- Location: `src/Modules/{Module}/{Module}.Application/{Feature}/Jobs/{Name}Job.cs`
- Implements `IJob` (`Invoria.BackgroundJob.Core.Jobs`) → `Task Execute(CancellationToken ct)`
- Namespace mirrors folder: `Invoria.{Module}.Application.{Feature}.Jobs`

```csharp
public sealed class ReportCustomerMetricsJob : IJob
{
    public const string Name = "ReportCustomerMetricsJob";
    public static int BatchSize { get; set; } = 100;
}
```

`Name` is the checkpoint key and the RecurringJobId. `BatchSize` drives the do-while loop.

## 2. Dependencies (constructor)

Inject all as `private readonly` fields, assign each in the constructor (no inline chaining):

- `IJobCheckpointStore` (`Invoria.BackgroundJob.Core.Checkpoints`)
- `IJobExecutionContextAccessor` (`Invoria.BackgroundJob.Core.Context`)
- Module repository for the source entity, e.g. `ICustomerRepository<Customer>`
- Module repository for the report entity, e.g. `ICustomerRepository<ReportCustomerMetrics>`

## 3. Restore checkpoint + state

Private `RestoreStateAsync`:

```csharp
private async Task<JobCheckpoint> RestoreStateAsync(CancellationToken cancellationToken)
{
    JobId jobId = _jobExecutionContextAccessor.JobId ?? new JobId(Name);

    var reportJobCheckPoint = await _jobCheckpointStore.RestoreAsync(jobId, cancellationToken);

    if (reportJobCheckPoint is null)
    {
        reportJobCheckPoint = new JobCheckpoint { JobId = jobId, Name = Name };
    }

    return reportJobCheckPoint;
}
```

Restore typed state with a **ternary** (not `if`):

```csharp
var jobState = reportJobCheckPoint.GetState<ReportJobCheckPoint>()
    ?? new ReportJobCheckPoint();
```

State type is `ReportJobCheckPoint` from `Invoria.BackgroundJobs.Abstractions.CheckPoints` (`LastId` string, `LastIndex` long).

## 4. Batch loop (do-while)

- Declare `customers` **inside** the loop body (local `var`).
- Use a `bool` flag for the continuation condition (`do ... while (isBatchFull);`).
- Break on empty batch.

```csharp
bool isBatchFull;

do
{
    var customerQuery = _customerRepository.AsQuerable();

    var orderedCustomers = customerQuery
        .OrderBy(x => x.Id)
        .Skip((int)jobState.LastIndex)
        .Take(BatchSize);

    var customers = await orderedCustomers.ToListAsync(cancellationToken);

    if (customers.Count == 0) { break; }

    // ... one Update method per period ...

    var lastCustomer = customers.LastOrDefault();
    if (lastCustomer is not null) { jobState.LastId = lastCustomer.Id; }

    jobState.LastIndex += customers.Count;

    reportJobCheckPoint.SetState(jobState);

    isBatchFull = customers.Count == BatchSize;
}
while (isBatchFull);

await _jobCheckpointStore.SaveAsync(reportJobCheckPoint, cancellationToken);
```

Rules: `OrderBy(Id)` ascending; `Skip((int)jobState.LastIndex)`; `.AsQuerable()` (typo is intentional, matches repo). Save checkpoint exactly once, after the loop. `LastIndex` is previous index + batch count.

## 5. One private method per ReportPeriod

Each counts the batch contribution and delegates to a shared upsert:

```csharp
private async Task UpdateDailyReportAsync(List<Customer> customers, DateTimeOffset now, CancellationToken ct)
{
    long contribution = customers.Count(c => c.CreatedAt.Date == now.Date);
    await UpsertReportAsync(ReportPeriod.Daily, contribution, ct);
}

private async Task UpdateMonthlyReportAsync(List<Customer> customers, DateTimeOffset now, CancellationToken ct)
{
    long contribution = customers.Count(c => c.CreatedAt.Year == now.Year && c.CreatedAt.Month == now.Month);
    await UpsertReportAsync(ReportPeriod.Monthly, contribution, ct);
}

private async Task UpdateYearlyReportAsync(List<Customer> customers, DateTimeOffset now, CancellationToken ct)
{
    long contribution = customers.Count(c => c.CreatedAt.Year == now.Year);
    await UpsertReportAsync(ReportPeriod.Yearly, contribution, ct);
}

private async Task UpdateAllTimeReportAsync(List<Customer> customers, CancellationToken ct)
{
    long contribution = customers.Count;
    await UpsertReportAsync(ReportPeriod.AllTheTime, contribution, ct);
}
```

Shared upsert — retrieve-or-create, cumulative across batches:

```csharp
private async Task UpsertReportAsync(ReportPeriod period, long contribution, CancellationToken ct)
{
    var existing = await _reportCustomerMetricsRepository
        .SingleOrDefault(x => x.Period == period, ct);

    if (existing is null)
    {
        var newReport = new ReportCustomerMetrics(contribution, period);
        await _reportCustomerMetricsRepository.Add(newReport, ct);
        return;
    }

    existing.UpdateCount(existing.TotalCount + contribution);
    await _reportCustomerMetricsRepository.Update(existing, ct);
}
```

## 6. Report entity (Domain)

- Prefix with `Report`, inherit `Entity` (not `AuditedAggregateRoot`).
- Properties: `long TotalCount { get; private set; }`, `Period` (`ReportPeriod`).
- Private parameterless ctor + public `(long, ReportPeriod)` ctor + `UpdateCount(long)`.

`ReportPeriod` enum values are spaced by 5 from 5: `Daily = 5, Monthly = 10, Yearly = 15, AllTheTime = 20` (`Invoria.BuildingBlocks.Domain.Enums`).

## 7. EF mapping + migration

- `{Report}EntityTypeConfiguration` in `{Module}.Infrastructure/EntityFramework/Configuration/` with `MapId()`, `HasMaxLength` from a `{Report}TableConsts` (mirror `CustomerTableConsts`), and map value properties.
- Generate migration: `dotnet ef migrations add Add{Report} --context {Module}DbContext --project src/Modules/{Module}/{Module}.Infrastructure --startup-project src/Invoria.Api --output-dir EntityFramework/Migrations`

## 8. Project references (Application csproj)

- `Invoria.BackgroundJob.Core`
- `Invoria.BackgroundJobs.Abstractions`

## 9. DI + recurring registration

- Register the job transient in the module's ApplicationServiceInstaller: `services.AddTransient<ReportCustomerMetricsJob>();`
  (the shared `JobsServiceInstaller` only scans `BackgroundJobs.Application`, not other modules).
- In the module bootstrapper, after migrations, scope-resolve `IRecurringJobScheduler` and call `AddOrUpdate`:

```csharp
using var scope = serviceProvider.CreateScope();

var recurringScheduler = scope.ServiceProvider.GetRequiredService<IRecurringJobScheduler>();

recurringScheduler.AddOrUpdate<ReportCustomerMetricsJob>(
    ReportCustomerMetricsJob.Name,
    new IntervalRecurrence(TimeSpan.FromMinutes(
        RecurringJobIntervalConsts.ReportCustomerMetricsInMinutes)));
```

- Shared interval consts live in `Invoria.BackgroundJobs.Abstractions` (`RecurringJobIntervalConsts`) so other jobs reuse them.
- RecurringJobId = `{Job}.Name` so the runtime `JobId` (via `IJobExecutionContextAccessor`) matches the checkpoint key.

## 10. Cross-cutting conventions

- No inline chaining — assign method results to named variables (production and test code).
- No code comments unless asked.
- Jobs are not CQRS handlers; they don't return `Result<T>`.

## 11. Testing report jobs (integration)

Reference implementation: `ReportCustomerMetricsJobTests` (`tests/Modules/CustomerManagement/CustomerManagement.Application.Tests/Customers/Jobs/ReportCustomerMetricsJobTests.cs`).

### 11.1 Location and fixture

- Test project: `tests/Modules/{Module}/{Module}.Application.Tests`.
- Mirror the Application folder: `{Feature}/Jobs/{Name}JobTests.cs`, namespace matches the folder.
- Class inherits the module's **`{Module}BackgroundJobTestFixture`** (extends `BackgroundJobTestFixture`), which provides an in-memory `FakeJobCheckpointStore` and sets the `JobId` context before each test — no real checkpoint DB needed.

### 11.2 Seed source entities with `CreatedAt` variance

- Seed N entities (e.g. 100) split across groups with **different day / month / year** for `CreatedAt` (e.g. `now`, `now.AddDays(-10)`, `now.AddMonths(-3)`, `now.AddYears(-2)`) so every period bucket is exercised.
- `CreatedAt` has a `protected set`. Set it **via reflection before `Add`**, because `AuditAndIdBeforeSaveHook` only fills `CreatedAt` when it is `default`:

```csharp
var property = typeof(AuditedAggregateRoot).GetProperty(nameof(AuditedAggregateRoot.CreatedAt));
property!.SetValue(customer, createdAt);
await CustomerRepository.Add(customer);
```

### 11.3 Execute the job

- Resolve the job through DI (it is registered transient in the module installer), then `Execute`:

```csharp
var job = ServiceProvider.GetRequiredService<ReportCustomerMetricsJob>();
await job.Execute(CancellationToken.None);
```

### 11.4 Assert against DB-derived ground truth

- Re-query the source entities from the DB via the module repository, then compute the **expected** contribution per period using the **exact predicates the job uses** (never hard-code counts — date arithmetic like `AddDays(-10)` / `AddMonths(-3)` can cross month/year boundaries).
- Load the report rows, take `Single` per period, and assert `TotalCount` equals the grouped value.

```csharp
var customers = await CustomerRepository.AsQuerable().ToListAsync();

var expectedDaily = customers.Count(c => c.CreatedAt.Date == now.Date);
var expectedMonthly = customers.Count(c => c.CreatedAt.Year == now.Year && c.CreatedAt.Month == now.Month);
var expectedYearly = customers.Count(c => c.CreatedAt.Year == now.Year);
var expectedAllTime = customers.Count;

var reports = await ReportRepository.AsQuerable().ToListAsync();

var dailyReport = reports.Single(x => x.Period == ReportPeriod.Daily);
dailyReport.TotalCount.Should().Be(expectedDaily);
// ... one Single + Be(expected) per period ...
```

### 11.5 Fixture DB isolation

These fixtures must follow the DB reset rule in `ai/Test-Conventions.md` (Respawn at fixture start and teardown), otherwise leftover report rows accumulate in the upsert and exact-count assertions fail.
