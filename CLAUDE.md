Invoria - CLAUDE.md

This file provides guidance for working with the Invoria codebase using Claude Code.

## Project Overview and Purpose

Invoria is a production-ready ERP (Enterprise Resource Planning) backend API designed to centralize and streamline core business processes. It handles inventory management, order processing, customer and supplier management, procurement, and financial reporting through a clean, consistent, and well-documented API surface.

The project follows a modular clean architecture with CQRS and MediatR patterns, ensuring loose coupling between modules and maintainability.

## Technology Stack and Important Dependencies

- **Runtime**: .NET 8 (`net8.0`)
- **Web/API**: ASP.NET Core, FastEndpoints, FastEndpoints.Swagger
- **Application pattern**: CQRS + MediatR-style handler flow
- **Validation**: FluentValidation
- **Persistence**: Entity Framework Core + SQL Server/LocalDB
- **Messaging**: Rebus + SQL Server transport/subscriptions
- **Logging**: Serilog
- **Dependency injection/container**: Microsoft DI + Autofac integration

## Architecture and Directory Structure

The solution follows a modular clean architecture with clearly separated layers per module:

```
src/
├── Invoria.Api/                 # Host application
├── BuildingBlocks/              # Shared abstractions (Core, Domain, Application, EntityFramework, Infrastructure)
├── Infrastructure/              # Cross-cutting infrastructure concerns
└── Modules/                     # Business modules (vertical slices)
    ├── Catalog/
    ├── CustomerManagement/
    ├── Financial/
    ├── Inventory/
    ├── Ordering/
    ├── Procurement/
    └── Reporting/
```

Each module contains the following layers:
- **Domain**: Entities, aggregates, value objects, domain events, domain services
- **Application**: CQRS commands/queries, handlers, use cases
- **Infrastructure**: Persistence (EF Core), external service integrations (Rebus), bootstrapping
- **Endpoints**: HTTP API endpoints (FastEndpoints)
- **Contracts**: DTOs, events, models for communication between modules and with clients

Tests mirror this structure under `tests/` with application and endpoint tests for each module.

Key architectural characteristics:
- Vertical module boundaries minimize cross-module coupling
- Communication between modules occurs via Rebus integration events
- Each module owns its database schema and migrations
- Shared building blocks provide common abstractions (repositories, unit of work, result types, etc.)

## Coding Conventions and Naming Rules

See `ai/CodingStyle.md` for detailed conventions, including:
- Domain report/read-model classes are prefixed with `Report`
- Always assign method results to named variables before further use (avoid chaining)
- Prefer explicit nested `foreach` loops over LINQ `SelectMany` + `GroupBy` for batch operations

Additional conventions from `.cursor/rules/`:
- **Module Contracts Layout**: Each `{Module}.Contracts` project must use the context-folder structure:
  ```
  {Module}.Contracts/
    AssemblyReference.cs
    {Context}/
      Enums/       → {Module}.Contracts.{Context}.Enums
      Dtos/        → {Module}.Contracts.{Context}.Dtos
      Events/      → {Module}.Contracts.{Context}.Events
      Models/      → {Module}.Contracts.{Context}.Models
  ```
- Keep only `AssemblyReference.cs` at the contracts project root when context folders exist
- Put enums in `Enums/` (not under `Events/`)
- Folder path must match namespace

## Build, Run, and Test Commands

### Prerequisites
- .NET 8 SDK
- SQL Server LocalDB or SQL Server instance

### Build
```bash
dotnet restore
dotnet build Invoria.sln
```

### Run the API
```bash
dotnet run --project src/Invoria.Api/Invoria.Api.csproj
```
The API will be available at:
- http://localhost:5069
- https://localhost:7012
Swagger UI is enabled in Development.

### Run Tests
```bash
# Run all tests
dotnet test Invoria.sln

# Run tests for a specific module
dotnet test tests/Modules/<Module>/<Module>.Application.Tests/<Module>.Application.Tests.csproj
```

### Database Migrations
Migrations are applied automatically at startup for each module via their respective module bootstrappers. Ensure `ConnectionStrings:Default` in `appsettings.json` points to a reachable database.

## Testing Requirements

- Write tests at the application layer (command/query handlers) and endpoint layer
- Use the existing test structure as a template:
  - Application tests: `tests/Modules/<Module>/<Module>.Application.Tests/`
  - Endpoint tests: `tests/Modules/<Module>/<Module>.Endpoints.Tests/`
- Tests should follow the Arrange-Act-Assert pattern
- Use the provided test fixtures and custom assertions where available
- For modules with Rebus integration, test event consumers and saga handlers

## Important Implementation Constraints

1. **Module Independence**: Modules must not have direct dependencies on other modules' internal layers. Communication must occur through:
   - Shared contracts (DTOs, events) in the `.Contracts` projects
   - Rebus integration events for cross-module workflows
   - Never bypass module boundaries by referencing another module's Domain, Application, or Infrastructure layers directly

2. **Dependency Direction**: 
   - Endpoints depend only on Application and shared infrastructure
   - Application depends on Domain and Contracts
   - Domain depends only on BuildingBlocks.Domain
   - Infrastructure depends on Domain and BuildingBlocks.EntityFramework
   - Contracts have no dependencies (except for AssemblyReference markers)

3. **Transactional Boundaries**: 
   - Use module-specific unit of work abstractions (e.g., `IInventoryUnitOfWork`) for persistence transactions
   - Never commit transactions across module boundaries; use eventual consistency via Rebus events

4. **Event-Driven Architecture**: 
   - All inter-module communication must use Rebus integration events
   - Events must be defined in the owning module's `.Contracts` project under `{Context}/Events/`
   - Consumers must be registered in the consuming module's Infrastructure layer

5. **Validation**: 
   - Use FluentValidation for request validation
   - Reuse domain constants (e.g., `ProductTableConsts`) in validators to maintain alignment with persistence constraints

6. **Error Handling**: 
   - Application handlers must return `Result<T>` (success/failure)
   - Domain exceptions (e.g., `NotFoundException`) should be used for business rule violations
   - Global exception handler maps exceptions to HTTP Problem Details responses

7. **Naming and Structure**: 
   - Follow the module contracts layout convention strictly
   - Use `Report` prefix for domain read-model classes only
   - Assign method results to named variables before further use (no chaining)
   - Prefer explicit nested loops over LINQ grouping for batch operations
