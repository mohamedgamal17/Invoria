using Invoria.BuildingBlocks.Application.Factories;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Contracts.Services;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Domain.Receivables;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Contracts.Orders.Services;

namespace Invoria.Financial.Application.Receivables.Factories;

public sealed class ReceivableResponseFactory : ResponseFactory<Receivable, ReceivableDto>, IReceivableResponseFactory
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;

    public ReceivableResponseFactory(IOrderService orderService, ICustomerService customerService)
    {
        _orderService = orderService;
        _customerService = customerService;
    }

    public override async Task<ReceivableDto> PrepareDto(Receivable view)
    {
        var sourceIds = new[] { view.SourceId };
        var partyIds = new[] { view.PartyId };

        var sourceById = await LoadSourcesByIdsAsync(sourceIds, CancellationToken.None);
        var partyById = await LoadPartiesByIdsAsync(partyIds, CancellationToken.None);

        return MapToDto(view, sourceById, partyById);
    }

    public override async Task<List<ReceivableDto>> PrepareListDto(List<Receivable> views)
    {
        var sourceIds = views
            .Select(r => r.SourceId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        var partyIds = views
            .Select(r => r.PartyId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        var sourceById = await LoadSourcesByIdsAsync(sourceIds, CancellationToken.None);
        var partyById = await LoadPartiesByIdsAsync(partyIds, CancellationToken.None);

        return views
            .Select(view => MapToDto(view, sourceById, partyById))
            .ToList();
    }

    private ReceivableDto MapToDto(
        Receivable view,
        IReadOnlyDictionary<string, PublicOrderDto> sourceById,
        IReadOnlyDictionary<string, CustomerDto> partyById)
    {
        var settlementDtos = view.Settlements
            .OrderBy(s => s.SettledAt)
            .Select(s => new ReceivableSettlementDto
            {
                Id = s.Id,
                ReceivableId = s.ReceivableId,
                Amount = s.Amount,
                SettledAt = s.SettledAt
            })
            .ToList();

        var dto = new ReceivableDto
        {
            Id = view.Id,
            PartyId = view.PartyId,
            SourceId = view.SourceId,
            Source = sourceById.GetValueOrDefault(view.SourceId),
            Party = partyById.GetValueOrDefault(view.PartyId),
            Amount = view.Amount,
            OutstandingAmount = view.OutstandingAmount,
            Status = view.Status,
            Settlements = settlementDtos
        };

        MapAudited(view, dto);

        return dto;
    }

    private async Task<Dictionary<string, PublicOrderDto>> LoadSourcesByIdsAsync(
        IReadOnlyCollection<string> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return new Dictionary<string, PublicOrderDto>();
        }

        var result = await _orderService.ListOrdersByIdsAsync(ids, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return new Dictionary<string, PublicOrderDto>();
        }

        return result.Value.ToDictionary(o => o.Id);
    }

    private async Task<Dictionary<string, CustomerDto>> LoadPartiesByIdsAsync(
        IReadOnlyCollection<string> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return new Dictionary<string, CustomerDto>();
        }

        var result = await _customerService.ListCustomersByIdsAsync(ids, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return new Dictionary<string, CustomerDto>();
        }

        return result.Value.ToDictionary(c => c.Id);
    }
}
