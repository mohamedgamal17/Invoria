# Invoria — Test Conventions

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
