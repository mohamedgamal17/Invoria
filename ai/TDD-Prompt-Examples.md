# TDD Prompt Examples

Use these prompts to enforce the project TDD workflow (Red -> Green -> Refactor).

## 1) Red phase only

Short prompt:
- "Use TDD. Red phase only: write failing tests for create order handler. Do not implement production code yet."

Strict prompt:
- "Follow TDD Red phase only. Add or update tests that define expected behavior for `CreateOrderCommandHandler` in `Ordering.Application.Tests`. Keep them failing on purpose, explain why they fail, and stop before implementation."

## 2) Green phase only

Short prompt:
- "Green phase: implement the minimum code needed to make current failing create-order tests pass."

Strict prompt:
- "Use TDD Green phase. Implement only the minimum production changes required to pass existing failing tests for `CreateOrderCommand`. Do not refactor unrelated code. Run targeted tests and report pass/fail."

## 3) Refactor phase only

Short prompt:
- "Refactor phase: improve readability and structure without changing behavior."

Strict prompt:
- "Use TDD Refactor phase. Refactor only code touched by create-order flow, keep behavior identical, keep tests green, and run the same test suite before/after."

## 4) Full TDD flow in one request

Short prompt:
- "Implement create-order using TDD end-to-end (Red, Green, Refactor), with progress updates per phase."

Strict prompt:
- "Apply full TDD workflow for create-order. Step 1 Red: add failing tests for success and validation paths. Step 2 Green: implement minimum handler/repository/factory code to pass. Step 3 Refactor: clean naming/duplication while preserving behavior. Run targeted tests each step and summarize what changed."

## 5) DTO assertion strengthening (like other modules)

Short prompt:
- "Strengthen DTO assertions to match Catalog style with assertion extensions."

Strict prompt:
- "Refactor tests to use assertion extensions (similar to Catalog/CustomerManagement). Assert full `OrderDto`, including top-level fields and each order item field (`ProductId`, `Quantity`, `Price`). Keep failure-path tests intact."

## 6) Useful acceptance-criteria template

Use this block in prompts:

```text
Acceptance criteria:
1) Tests are written first and fail before implementation.
2) Production changes are minimal and scoped to passing tests.
3) DTO assertions verify full object graph, including nested items.
4) Targeted tests are executed and results reported.
5) Refactor step keeps all tests passing.
```

## 7) Quick prompt for future Ordering tasks

- "According to `ai/Architecture.md`, use TDD. Red first in `tests/Modules/Ordering`, then minimum implementation in `src/Modules/Ordering`, then refactor with tests green."
