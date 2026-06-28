# Invoria — AGENTS.md

## Project identity

- **.NET 8** ERP backend API, Modular Clean Architecture + CQRS.
- Solution: `Invoria.sln`. Entry point: `src/Invoria.Api/Program.cs`.

## Module layout

Every module lives under `src/Modules/{Module}/{Module}.{Layer}/` with layers:
`Domain` → `Application` → `Infrastructure` → `Endpoints` → `Contracts`.

| Module | Subdirectories |
|--------|---------------|
| Catalog, CustomerManagement, Ordering, Inventory, Procurement, Reporting | Same 5-layer shape |

Shared primitives in `src/BuildingBlocks/`: Core, Domain, Application, EntityFramework, Infrastructure.

## Key commands

```powershell
dotnet restore Invoria.sln
dotnet build Invoria.sln
dotnet run --project src/Invoria.Api/Invoria.Api.csproj
dotnet test tests/Modules/Ordering/Ordering.Application.Tests/Invoria.Ordering.Application.Tests.csproj
```

CI runs each test project separately in Release mode. Tests need SQL Server (LocalDB locally) — set env vars `ConnectionStrings__Default` and `ConnectionStrings__Rebus`.

## Architecture reference

- `ai/Architecture.md` — detailed module wiring, migrations, Rebus flow.
- `ai/Branch-Changes.md` — PR documentation template for feature branches.
- `ai/TDD-Prompt-Examples.md` — example prompts.
- `ai/Test-Conventions.md` — test folder structure conventions.

## Conventions to follow

- **CQRS**: Queries implement `IQuery<T>`, commands `ICommand<T>`, handlers implement `IApplicatonRequestHandler<T, TResponse>`.
- **Endpoints**: Inherit from `EndpointBase<TRequest, TResponse>`, register via `Group<T>` with `Get("")`, `Post("")`, etc.
- **Results**: `Result<T>` from BuildingBlocks — handlers return `Result<T>`, endpoints use `SendResultAsync(result)`.
- **Contracts layout**: Each bounded context has `Enums/`, `Dtos/`, `Events/`, `Models/` subfolders — never flat at root.
- **Repository**: Module-specific like `IOrderingRepository<T>` with `.AsQuerable()` (note: not `.AsQueryable()`).
- **Domain aggregates**: Inherit from `AuditedAggregateRoot`. DTOs inherit from `AuditedEntityDto`.
- **Intermediate variables**: Always assign method results to named variables before further use — no inline chaining like `GetFoo().DoBar()`. Applies to production and test code.
- **Nested loops over LINQ grouping**: Iterate lines → batch allocations explicitly rather than `SelectMany` + `GroupBy` for batch-allocation operations. See `ai/CodingStyle.md`.
- **DTO factory completeness**: When writing a `PrepareDto` or `MapTo*` method in a response factory, explicitly list **every** property of the DTO and the entity being mapped — including inherited properties like `Id`. Cross-check against the full inheritance chain of both types (`Entity`→`AuditedAggregateRoot`→`Return` vs `EntityDto`→`AuditedEntityDto`) to catch unmapped members. Do not rely on default values. For nested DTOs (e.g., line items) that don't inherit `AuditedEntityDto`, auditing fields may be omitted intentionally.

## Testing patterns (NUnit + FluentAssertions)

- Integration tests need a running SQL Server — the test fixture (`OrderingTestFixture` etc.) manages a shared DB context.
- **Test data**: Static helpers in `Invoria.*.Tests.Fakes` (e.g., `OrderTestData.PersistRandomOrdersAsync`).
- **Test fixtures**: Inherit from module fixture (e.g., `OrderingTestFixture`), expose `Mediator` via `Scope.Resolve<IMediator>()`.
- **Assertion extensions**: DTO assertions in `Assertions/` folder (e.g., `OrderAssertionExtensions.AssertOrderDto`). Prefer these over inline property checks.
- Follow **Red-Green-Refactor** TDD as documented in `.cursor/rules/tdd-strategy.mdc`.

## Tech stack

ASP.NET Core, FastEndpoints, MediatR, EF Core, SQL Server, Rebus, FluentValidation, Serilog, Autofac.
