using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Ordering.Contracts.Invoices.Dtos;

namespace Invoria.Ordering.Application.Tests.Assertions;

public static class InvoiceAssertionExtensions
{
    public static void AssertPagingDto(
        this PagingDto<InvoiceDto> page,
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
