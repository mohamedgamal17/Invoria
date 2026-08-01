# Invoria — Test Conventions

## Database isolation in integration test fixtures

Every fixture that persists to SQL Server (module `TestFixture` subclasses) must start and end with a **clean database**. Without this, stale rows left by earlier fixtures or aborted runs corrupt exact-count and cumulative-upsert assertions (e.g. a report job's all-time total came back `295` instead of `100`).

**Rule — reset at fixture start AND teardown:**

- `BeforeAllTestRunAsync`: run the module bootstrapper (applies migrations) first, then Respawn-reset the database ignoring `__EFMigrationsHistory`.
- `AfterAllTestTearDown`: Respawn-reset the database again (call `base.AfterAllTestTearDown()` first).
- Factor the reset into a shared private `ResetDatabaseAsync` helper using `Configuration.GetConnectionString("Default")` + `Respawner` (see `RespawnerOptions { TablesToIgnore = new Table[] { "__EFMigrationsHistory" } }`).
- Reference implementation: `CustomerBackgroundJobTestFixture` (`tests/Modules/CustomerManagement/CustomerManagement.Application.Tests/CustomerBackgroundJobTestFixture.cs`).

```csharp
protected override async Task BeforeAllTestRunAsync()
{
    await ServiceProvider.RunModulesBootstrapperAsync();
    await ResetDatabaseAsync();
}

protected override async Task AfterAllTestTearDown()
{
    await base.AfterAllTestTearDown();
    await ResetDatabaseAsync();
}
```

The fixture-level Respawn reset is the general default. Module-specific per-test cleanup (e.g. Reporting's `AfterAnyTestTearDown` deletions) may still be used where the module convention requires it.

## Folder Structure

Each bounded context has its own top-level folder mirroring the module's domain.
Tests are organized by feature group (e.g. `Orders/`, `Invoices/`) with
subfolders for `Commands/`, `Queries/`, `Factories/`, `Handlers/`,
`Consumers/`, and `Sagas/` as appropriate.

No nested `Integration/` wrapper — tests sit directly under the
bounded-context folder.

### Example

```
Ordering.Application.Tests/
├── Orders/
│   ├── Commands/
│   ├── Queries/
│   ├── Factories/
│   ├── Handlers/
│   └── Sagas/
├── Invoices/
│   ├── Commands/
│   ├── Queries/
│   ├── Consumers/
│   └── Sagas/
├── Domain/
├── Infrastructure/
└── Assertions/
```
