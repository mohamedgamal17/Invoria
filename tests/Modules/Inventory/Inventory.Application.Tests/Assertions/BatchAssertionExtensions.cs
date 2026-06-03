using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Inventory.Application.Batches.Commands.CreateBatch;
using Invoria.Inventory.Application.Batches.Commands.UpdateBatch;
using Invoria.Inventory.Contracts.Batches.Dtos;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Tests.Assertions;

public static class BatchAssertionExtensions
{
    public static void AssertBatch(
        this Batch batch,
        string productId,
        int quantity,
        decimal purchasePrice,
        string? purchaseOrderItemId = null,
        int reservedQuantity = 0,
        BatchState? state = null)
    {
        batch.ProductId.Should().Be(productId);
        batch.PurchaseOrderItemId.Should().Be(purchaseOrderItemId);
        batch.Quantity.Should().Be(quantity);
        batch.PurchasePrice.Should().Be(purchasePrice);
        batch.ReservedQuantity.Should().Be(reservedQuantity);
        if (state.HasValue)
        {
            batch.State.Should().Be(state.Value);
        }
    }

    public static void AssertCreateBatchCommand(this Batch batch, CreateBatchCommand command)
    {
        batch.AssertBatch(command.ProductId, command.Quantity, command.PurchasePrice);
    }

    public static void AssertBatchDto(this BatchDto dto, Batch batch)
    {
        dto.Id.Should().Be(batch.Id);
        dto.ProductId.Should().Be(batch.ProductId);
        dto.PurchaseOrderItemId.Should().Be(batch.PurchaseOrderItemId);
        dto.Quantity.Should().Be(batch.Quantity);
        dto.PurchasePrice.Should().Be(batch.PurchasePrice);
        dto.ReservedQuantity.Should().Be(batch.ReservedQuantity);
    }

    public static void AssertBatchDto(this BatchDto dto, CreateBatchCommand command, int reservedQuantity = 0)
    {
        dto.Id.Should().NotBeNullOrWhiteSpace();
        dto.ProductId.Should().Be(command.ProductId);
        dto.PurchaseOrderItemId.Should().BeNull();
        dto.Quantity.Should().Be(command.Quantity);
        dto.PurchasePrice.Should().Be(command.PurchasePrice);
        dto.ReservedQuantity.Should().Be(reservedQuantity);
    }

    public static void AssertUpdateBatchCommand(
        this Batch batch,
        UpdateBatchCommand command,
        string productId,
        int reservedQuantity = 0)
    {
        batch.AssertBatch(productId, command.Quantity, command.PurchasePrice, reservedQuantity: reservedQuantity);
    }

    public static void AssertBatchDto(
        this BatchDto dto,
        UpdateBatchCommand command,
        string productId,
        int reservedQuantity = 0)
    {
        dto.Id.Should().NotBeNullOrWhiteSpace();
        dto.ProductId.Should().Be(productId);
        dto.PurchaseOrderItemId.Should().BeNull();
        dto.Quantity.Should().Be(command.Quantity);
        dto.PurchasePrice.Should().Be(command.PurchasePrice);
        dto.ReservedQuantity.Should().Be(reservedQuantity);
    }

    public static void AssertPagingDto(
        this PagingDto<BatchDto> page,
        int expectedSkip,
        int expectedLength,
        long expectedTotalCount,
        int? expectedDataCount = null)
    {
        page.Info.Skip.Should().Be(expectedSkip);
        page.Info.Length.Should().Be(expectedLength);
        page.Info.TotalCount.Should().Be(expectedTotalCount);

        if (expectedDataCount.HasValue)
        {
            page.Data.Count().Should().Be(expectedDataCount.Value);
        }
    }
}
