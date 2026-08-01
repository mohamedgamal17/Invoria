# Invoria — AGENTS.md (cross-tool)

> Workspace rules for any AI coding tool (opencode, Cursor, Claude Code, Codex, etc.).

**.NET 8 ERP backend API** — Modular Clean Architecture + CQRS. Solution: `Invoria.sln`.

The canonical, full convention set lives in `.opencode/AGENTS.md` (architecture, CQRS, endpoints, coding style). Detailed recipes are under `ai/` (`Architecture.md`, `Report-Jobs.md`, `Test-Conventions.md`, `CodingStyle.md`). Two mandatory rules that must always be followed:

## 1. DB reset per test fixture (mandatory)

Every SQL Server-touching test fixture must start and end with a clean database.

- `BeforeAllTestRunAsync`: run the module bootstrapper (migrations), then Respawn-reset ignoring `__EFMigrationsHistory`.
- `AfterAllTestTearDown`: Respawn-reset again (`base.AfterAllTestTearDown()` first).
- Use a shared private `ResetDatabaseAsync` helper via `Configuration.GetConnectionString("Default")`.
- Why: stale rows from prior fixtures or aborted runs corrupt exact-count and cumulative-upsert assertions.
- Reference: `CustomerBackgroundJobTestFixture`. Details: `ai/Test-Conventions.md`.

## 2. Background/report job testing recipe (mandatory)

- Location: test project mirroring the Application folder — `tests/Modules/{Module}/{Module}.Application.Tests/{Feature}/Jobs/{Name}JobTests.cs`.
- Class inherits the module's `{Module}BackgroundJobTestFixture` (in-memory checkpoint store + JobId context).
- Seed N source entities with `CreatedAt` variance (different day/month/year) — set the protected `CreatedAt` via reflection before `Add` (the audit hook only fills it when `default`).
- Resolve the job via `ServiceProvider.GetRequiredService<TJob>()` and `await job.Execute(...)`.
- Assert: reload source rows from the DB, compute expected per period with the job's own predicates, load report rows, `Single` per period, assert `TotalCount` equals the DB-derived ground truth (never hard-coded counts).
- Reference: `ReportCustomerMetricsJobTests`. Details: `ai/Report-Jobs.md`.

## Key commands

```powershell
dotnet build Invoria.sln
dotnet test tests/Modules/CustomerManagement/CustomerManagement.Application.Tests/Invoria.CustomerManagement.Application.Tests.csproj
```
