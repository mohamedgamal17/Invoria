using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Contracts.Services;
using Invoria.Inventory.Contracts.Returns.Dtos;
using Invoria.Inventory.Domain.Returns;

namespace Invoria.Inventory.Application.Returns.Factories;

public class ReturnResponseFactory
    : ResponseFactory<Return, ReturnDto>, IReturnResponseFactory
{
    private readonly IImmediateReturnResponseFactory _immediateReturnFactory;
    private readonly IProductService _productService;

    public ReturnResponseFactory(
        IImmediateReturnResponseFactory immediateReturnFactory,
        IProductService productService)
    {
        _immediateReturnFactory = immediateReturnFactory;
        _productService = productService;
    }

    public override async Task<ReturnDto> PrepareDto(Return view)
    {
        var productIds = view.ReturnLines
            .Select(l => l.ProductId)
            .Distinct()
            .ToHashSet();

        var productResult = await _productService.ListProductsByIdsAsync(productIds);
        var productDict = productResult.Value?
            .ToDictionary(p => p.Id, p => p)
            ?? new Dictionary<string, ProductDto>();

        return view switch
        {
            ImmediateReturn immediate => _immediateReturnFactory.PrepareDto(immediate, productDict),
            _ => throw new InvalidOperationException(
                $"Unknown return type: {view.GetType().Name}")
        };
    }

    public override async Task<List<ReturnDto>> PrepareListDto(List<Return> views)
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
            var dto = view switch
            {
                ImmediateReturn immediate => _immediateReturnFactory.PrepareDto(immediate, productDict),
                _ => throw new InvalidOperationException(
                    $"Unknown return type: {view.GetType().Name}")
            };

            results.Add(dto);
        }

        return results;
    }
}
