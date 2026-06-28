using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Contracts.Services;
using Invoria.Inventory.Application.Allocations.Dtos;
using Invoria.Inventory.Domain.Allocations;
using ContractAllocationLineStatus = Invoria.Inventory.Contracts.Allocations.Enums.AllocationLineStatus;
using ContractAllocationStatus = Invoria.Inventory.Contracts.Allocations.Enums.AllocationStatus;

namespace Invoria.Inventory.Application.Allocations.Factories;

public class AllocationResponseFactory
    : ResponseFactory<Allocation, AllocationDto>, IAllocationResponseFactory
{
    private readonly IProductService _productService;

    public AllocationResponseFactory(IProductService productService)
    {
        _productService = productService;
    }

    public override async Task<AllocationDto> PrepareDto(Allocation view)
    {
        var productIds = view.Lines
            .Select(l => l.ProductId)
            .Distinct()
            .ToHashSet();

        var productResult = await _productService.ListProductsByIdsAsync(productIds);
        var productDict = productResult.Value?
            .ToDictionary(p => p.Id, p => p)
            ?? new Dictionary<string, ProductDto>();

        return PrepareDto(view, productDict);
    }

    public override async Task<List<AllocationDto>> PrepareListDto(List<Allocation> views)
    {
        var allProductIds = views
            .SelectMany(v => v.Lines.Select(l => l.ProductId))
            .Distinct()
            .ToHashSet();

        var productResult = await _productService.ListProductsByIdsAsync(allProductIds);
        var productDict = productResult.Value?
            .ToDictionary(p => p.Id, p => p)
            ?? new Dictionary<string, ProductDto>();

        var results = new List<AllocationDto>(views.Count);
        foreach (var view in views)
        {
            results.Add(PrepareDto(view, productDict));
        }

        return results;
    }

    private AllocationDto PrepareDto(Allocation view, Dictionary<string, ProductDto> products)
    {
        var dto = new AllocationDto
        {
            Id = view.Id!,
            OrderId = view.OrderId,
            Status = (ContractAllocationStatus)view.Status,
            Lines = view.Lines.Select(l => new AllocationLineDto
            {
                Id = l.Id!,
                AllocationId = l.AllocationId,
                OrderItemId = l.OrderItemId,
                ProductId = l.ProductId,
                QuantityRequested = l.QuantityRequested,
                Status = (ContractAllocationLineStatus)l.Status,
                Product = products.TryGetValue(l.ProductId, out var product)
                    ? product
                    : null
            }).ToList()
        };

        MapAudited(view, dto);

        return dto;
    }
}
