using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Inventory.Contracts.Returns.Dtos;
using Invoria.Inventory.Domain.Returns;

namespace Invoria.Inventory.Application.Tests.Assertions;

public static class ReturnAssertionExtensions
{
    public static void AssertReturnDto(this ReturnDto dto, ImmediateReturn entity)
    {
        dto.Id.Should().Be(entity.Id);
        dto.Type.Should().Be((Invoria.Inventory.Contracts.Returns.Enums.ReturnType)entity.Type);
        dto.Status.Should().Be(entity.Status);
        dto.AllocationId.Should().Be(entity.AllocationId);
        dto.OrderId.Should().Be(entity.OrderId);

        dto.ReturnLines.Should().HaveSameCount(entity.ReturnLines);
        foreach (var (lineDto, lineEntity) in dto.ReturnLines.Zip(entity.ReturnLines))
        {
            lineDto.Id.Should().Be(lineEntity.Id);
            lineDto.ReturnId.Should().Be(lineEntity.ReturnId);
            lineDto.OrderItemId.Should().Be(lineEntity.OrderItemId);
            lineDto.ProductId.Should().Be(lineEntity.ProductId);
            lineDto.Quantity.Should().Be(lineEntity.Quantity);
        }
    }

    public static void AssertPagingDto(
        this PagingDto<ReturnDto> page,
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
