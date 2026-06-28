# Invoria Coding Style

## Intermediate variables

Always assign the result of a method call to a named variable before further use. Do **not** chain calls inline (e.g., `GetFoo().DoBar()`).

```csharp
// Good
var batch = batchesById[batchAllocation.BatchId];
batch.SettleAllocatedQuantity(batchAllocation.QuantityAllocated);

// Avoid
batchesById[batchAllocation.BatchId].SettleAllocatedQuantity(batchAllocation.QuantityAllocated);
```

This rule applies in both production and test code.

## Nested loops over LINQ grouping

For operations that iterate batch allocations by line, prefer explicit nested `foreach` loops over `SelectMany` + `GroupBy`:

```csharp
// Good
foreach (var line in allocation.Lines)
{
    if (line.Status != AllocationLineStatus.Allocated) continue;

    foreach (var batchAllocation in line.BatchAllocations)
    {
        var batch = batchesById[batchAllocation.BatchId];
        batch.RestoreAllocatedQuantity(batchAllocation.QuantityAllocated);
    }
}

// Avoid
var byBatch = allocation.Lines
    .Where(l => l.Status == AllocationLineStatus.Allocated)
    .SelectMany(l => l.BatchAllocations)
    .GroupBy(a => a.BatchId);

foreach (var group in byBatch)
{
    batchesById[group.Key].RestoreAllocatedQuantity(group.Sum(a => a.QuantityAllocated));
}
```

Rationale: The nested-loop style makes it easier to set breakpoints and inspect state per line or per batch allocation during debugging.
