# Branch-tracked PR documentation

This guide defines how to document **deltas** on a feature branch so pull requests stay aligned with the solution’s layered architecture.

## Relationship to Architecture

- [`Architecture.md`](Architecture.md) describes the **baseline**: modules, layers (`Domain`, `Application`, `Infrastructure`, `Endpoints`, `Contracts`), test projects, and cross-cutting concerns (host, Rebus, BuildingBlocks).
- Files under [`changes/`](changes/) describe **what changed on a branch** using the **same vocabulary** (module names, layer names, project names like `Invoria.Ordering.Application`).
- When implementing or reviewing a change, read **`Architecture.md`** for context and **`changes/<branch-slug>.md`** for the branch narrative.

## Workflow

1. When you create a feature branch, copy the [PR documentation template](#pr-documentation-template) into `changes/<branch-slug>.md` (see [naming rules](#branch-file-naming)).
2. Fill the YAML metadata (`branch`, `base`, `last_updated`; add `pr` when the PR exists).
3. While developing, update **Features**, **API Changes**, and **Code Changes** (and optional **Cross-cutting** / **Integration and messaging**).
4. When you open or update the pull request, paste the markdown body (from `# Features` through the last section, or the whole file if your team keeps metadata in the PR) into the PR description. Update `pr:` in the YAML to the PR URL or number.

### Branch file naming

- Use the git branch name with `/` replaced by `-` for a safe filename, e.g. `feature/order-list-filters` → `changes/feature-order-list-filters.md`.
- Keep the `branch:` field in the YAML equal to the actual git branch name (may contain `/`).

### Git: commit or ignore

- **Default (recommended):** Commit `changes/<branch-slug>.md` with the PR so reviewers see the narrative next to the diff. Remove or archive the file after merge if you want a clean tree, or keep it for history.
- **Alternative:** Add `ai/changes/*.md` to `.gitignore` and maintain the doc only locally or paste exclusively into the PR body—useful if you do not want branch files in the repo.

### Agent hint

When working on a branch, prefer reading and updating `ai/changes/<branch-slug>.md` together with [`ai/Architecture.md`](Architecture.md) so layer and module naming stay consistent.

---

## PR documentation template

Copy the following into a new file under `changes/` and replace placeholders.

    ---
    branch: feature/your-branch-name
    base: main
    pr: ""
    last_updated: 2026-05-09
    author: ""
    ---

    # Features

    ## Ordering
    ### Order List Filters
    - (Describe user-facing or behavioral changes.)

    ## Procurement
    ### Purchase Order Filters
    - (Describe user-facing or behavioral changes.)

    # API Changes

    ## Ordering
    ### List Orders (`/orders`)
    - (New/changed query parameters, request/response shapes, breaking changes.)

    ## Procurement
    ### List Purchase Orders (`/purchase-orders`)
    - (New/changed query parameters, request/response shapes, breaking changes.)

    # Code Changes

    ## Ordering

    ### Domain (`Invoria.Ordering.Domain`)
    - (Or: No changes introduced in this layer.)

    ### Application (`Invoria.Ordering.Application`)
    - (Commands, queries, handlers, factories, domain event handlers.)

    ### Infrastructure (`Invoria.Ordering.Infrastructure`)
    - (Or: No changes introduced in this layer.)
    - (DbContext, migrations, repositories, installers, Rebus registration when applicable.)

    ### Endpoints / Presentation (`Invoria.Ordering.Endpoints`)
    - (FastEndpoints, requests, validators, mapping to application layer.)

    ### Contracts (`Invoria.Ordering.Contracts`)
    - (Or: No changes introduced in this layer.)
    - (DTOs, integration events.)

    ### Testing
    - `Invoria.Ordering.Application.Tests`: (unit/integration coverage.)
    - `Invoria.Ordering.Endpoints.Tests`: (HTTP/endpoint coverage.)

    ## Procurement

    ### Domain (`Invoria.Procurement.Domain`)
    - (Or: No changes introduced in this layer.)

    ### Application (`Invoria.Procurement.Application`)
    - (Commands, queries, handlers, factories.)

    ### Infrastructure (`Invoria.Procurement.Infrastructure`)
    - (Or: No changes introduced in this layer.)

    ### Endpoints / Presentation (`Invoria.Procurement.Endpoints`)
    - (FastEndpoints, requests, validators.)

    ### Contracts (`Invoria.Procurement.Contracts`)
    - (Or: No changes introduced in this layer.)

    ### Testing
    - `Invoria.Procurement.Application.Tests`: (…)
    - `Invoria.Procurement.Endpoints.Tests`: (…)

    # Cross-cutting

    _Omit this section if everything fits under a single module._

    ## Host / API (`Invoria.Api`)
    - (Module installation, Swagger, exception handling, Rebus host config.)

    ## BuildingBlocks (`Invoria.BuildingBlocks.*`)
    - (Shared CQRS, EF, endpoint base types.)

    ## Shared validation or global behaviors
    - (…)

    # Integration and messaging

    _Omit if no integration events, consumers, or transport registration changed._

    - (e.g. Contract types such as `AllocateOrderIntegrationEvent`, `IHandleMessages<T>` implementations, `RebusHandlersServiceInstaller` or equivalent.)

---

## Filled example

See [`changes/EXAMPLE-filled-order-and-po-filters.md`](changes/EXAMPLE-filled-order-and-po-filters.md) for a complete sample (ordering and procurement list filters) that matches the Architecture-aligned layer sections.
