# Invoria Architecture Guidelines

## System overview
The Invoria project is built as a **scalable modular monolith** utilizing ASP.NET Core, SQL Server, and FastEndpoints. It leverages the **CQRS** pattern (Commands and Queries) and follows **Modular Clean Architecture** principles.

The system separates cross-cutting concerns (Building Blocks) from self-contained business areas (Modules). All these are wired together by a single API Host.

**High-Level Structure:**
- `src/Api`: The single ASP.NET Core host (Composition Root).
- `src/BuildingBlocks`: Shared kernels and cross-cutting concerns.
  - `Invoria.BuildingBlocks.Domain`: Base entities, Exceptions, Global Result Object.
  - `Invoria.BuildingBlocks.Application`: Shared CQRS interfaces, behaviors, MediatR pipeline.
  - `Invoria.BuildingBlocks.Infrastructure`: EventBus, Caching, Global API Envelope, IResultMapper.
- `src/Modules`: Independent vertical business slices (e.g., Identity, Catalog, Orders).
- `tests`: Test projects including `Invoria.Application.Tests` (NUnit & AutoFac base test containers) and architecture tests.

## Module structure
Inside each module (e.g., `src/Modules/Catalog`), we strictly enforce **Clean Architecture**. This keeps the application deeply modular, making it easy to extract a module into a microservice later if needed.

**Example Module Structure:**
- `Catalog.Domain`: Core rules (Entities, Value Objects, Domain Events).
- `Catalog.Application`: Use Cases (CQRS Commands/Queries, Validators, DTOs).
- `Catalog.Infrastructure`: Data Access (EF Core DbContext, Migrations, Repositories).
- `Catalog.Endpoints`: Presentation (FastEndpoints REPR pattern implementations).
- `Catalog.Contracts`: (Optional) Public interface for cross-module synchronous calls.

## Layer responsibilities
Within any given module, responsibilities are strictly divided across layers:

- **Domain**: Highly isolated with no external dependencies. Contains Entities, Enums, Value Objects, Domain Exceptions, and Repository Interfaces.
- **Application**: Defines the system's use cases via CQRS. Implements Commands, Queries, and references `Invoria.BuildingBlocks.Domain` for the `Result` pattern. Depends heavily on the Domain layer.
- **Infrastructure**: Connects to SQL Server via EF Core. Implements repository interfaces defined in the Domain. Handles external API calls, third-party SDKs, and database schema configurations using `IEntityTypeConfiguration<T>`.
- **Endpoints**: Driven by FastEndpoints. Responsible *only* for receiving HTTP requests, mapping them to Commands/Queries, and formatting the outgoing HTTP responses. Zero business logic belongs here.

## Dependency rules
- **The Golden Dependency Rule**: Inner layers define interfaces; outer layers implement them. Dependencies flow inwards: `Endpoints` & `Infrastructure` &rarr; `Application` &rarr; `Domain`.
- **Strict Module Isolation**: Modules **MUST NOT** directly reference each other's Domain, Application, Infrastructure, or Endpoint layers.
- **The API Host (`src/Api`)**: The only project allowed to reference every module's infrastructure and endpoints to register everything during startup (e.g., global FastEndpoint Assembly Registration and AutoFac container composition).
- **Module Communication**:
  - *Asynchronous (Preferred)*: Modules communicate by publishing Integration Events to an EventBus (e.g., In-Memory MediatR, RabbitMQ).
  - *Synchronous*: If a module must fetch data from another instantly, it should only reference the target module's lightweight `Contracts` project (interfaces and simple DTOs), resolved via DI.

## Code conventions
- **Naming Conventions**:
  - *Endpoints*: `<Action><Resource>Endpoint` (e.g., `CreateUserEndpoint`, `GetOrderByIdEndpoint`).
  - *CQRS Records/Classes*: `<Action><Resource>Command` or `<Action><Resource>Query` (e.g., `UpdateProductCommand`).
  - *Handlers*: `<Action><Resource>CommandHandler` (must live in the Application layer).
  - *HTTP Models*: `<Action><Resource>Request` / `Response` (keep these models in the exact same file as the FastEndpoint using them to maximize cohesion).
  - *Database Tables*: Prefix table names with the module name to prevent collisions in the shared SQL Server database (e.g., `Catalog_Products`, `Identity_Users`).
- **Control Flow**: Never throw exceptions to control standard business logic flows. Use the `Result` or `Result<T>` pattern. Map these results using `IResultMapper` to the global API Response Envelope inside FastEndpoints.
- **Domain Modeling**: Favor a Rich Domain over an Anemic Domain. Keep logic out of Handlers. The Entity itself should guard its invariants using private setters and rich methods (e.g., `order.ApplyDiscount()`).
- **Endpoint Validation**: Utilize FastEndpoints' built-in FluentValidation integration. Reject invalid HTTP requests with `400 Bad Request` envelopes *before* they ever reach the CQRS pipeline.
- **Database Configurations**: Do not use data annotations (`[Required]`, `[Table]`) on Entities. Configure EF Core schemas in separate classes inside the Infrastructure layer.
- **Bounded Contexts**: Each module should have its own localized DbContext (e.g., `CatalogDbContext`) containing only DbSets relevant to that module.

## Testing conventions
- **Unit Tests (`Invoria.Application.Tests`)**: Use **NUnit**, mocking libraries (NSubstitute/Moq), and **AutoFac**. Test Application Handlers and Domain Entities thoroughly without hitting a real database.
- **Integration Tests**: Leverage **Testcontainers** to spin up an ephemeral SQL Server instance. Utilize hooks within the AutoFac `BaseTestContainer` (`BeforeAnyTestRunAsync`, `TearDownAsync`) to cleanly reset the database state between tests.
- **Endpoint Tests**: Use FastEndpoints' built-in endpoint testing helpers combined with `WebApplicationFactory` to execute fast, end-to-end REST calls without mocking the HTTP stack.
- **Architecture Tests**: Enforce modularity strictly via CI pipelines using `NetArchTest.Rules` (e.g., assert that `Catalog.Application` does not have a dependency on `Orders.Application`). Ensure layer dependency rules are not violated.
