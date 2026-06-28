using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Contracts.Services;
using Invoria.Inventory.Contracts.Returns.Dtos;
using Invoria.Inventory.Domain.Returns;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Inventory.Application.Returns.Factories;

public class ImmediateReturnResponseFactory
    : ResponseFactory<ImmediateReturn, ReturnDto>, IImmediateReturnResponseFactory
{
    private readonly IProductService _productService;

    public ImmediateReturnResponseFactory(IProductService productService)
    {
        _productService = productService;
    }
    public override async Task<List<ReturnDto>> PrepareListDto(List<ImmediateReturn> views)
    {
        var allProductIds = views
            .SelectMany(v => v.ReturnLines.Select(l => l.ProductId))
            .Distinct()
            .ToHashSet();

        var productResult = await _productService.ListProductsByIdsAsync(allProductIds);
        var productDict = productResult.Value?
            .ToDictionary(p => p.Id, p => p)
            ?? new Dictionary<string, ProductDto>();

        var results = new List<ReturnDto>(views.Count);
        foreach (var view in views)
        {
            results.Add(PrepareDto(view, productDict));
        }

        return results;
    }

    public override async Task<ReturnDto> PrepareDto(ImmediateReturn view)
    {
        var productIds = view.ReturnLines
            .Select(l => l.ProductId)
            .Distinct()
            .ToHashSet();

        var productResult = await _productService.ListProductsByIdsAsync(productIds);
        var productDict = productResult.Value?
            .ToDictionary(p => p.Id, p => p)
            ?? new Dictionary<string, ProductDto>();

        return PrepareDto(view, productDict);
    }

    public ReturnDto PrepareDto(ImmediateReturn view, Dictionary<string, ProductDto> products)
    {
        var dto = new ReturnDto
        {
            Id = view.Id,
            Type = (Invoria.Inventory.Contracts.Returns.Enums.ReturnType)view.Type,
            Status = view.Status,
            ReturnLines = view.ReturnLines.Select(l => new ReturnLineDto
            {
                Id = l.Id,
                ReturnId = l.ReturnId,
                OrderItemId = l.OrderItemId,
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                Product = products.TryGetValue(l.ProductId, out var product)
                    ? new ReturnProductDto
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price
                    }
                    : null
            }).ToList(),
            AllocationId = view.AllocationId,
            OrderId = view.OrderId
        };

        MapAudited(view, dto);

        return dto;
    }
 
}
