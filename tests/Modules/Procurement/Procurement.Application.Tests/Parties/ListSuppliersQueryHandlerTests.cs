using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Procurement.Application.Parties.Queries.ListSuppliers;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;
using Invoria.Procurement.Infrastructure.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Application.Tests.Parties;

[TestFixture]
public class ListSuppliersQueryHandlerTests : ProcurementTestFixture
{
    private IProcurementRepository<Supplier> SupplierRepository { get; }
    private IMediator Mediator { get; }

    public ListSuppliersQueryHandlerTests()
    {
        SupplierRepository = ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearSuppliersAsync();
    }

    private async Task ClearSuppliersAsync()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ProcurementDbContext>();
        var suppliers = await db.Set<Supplier>().ToListAsync();
        db.RemoveRange(suppliers);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Should_return_paged_suppliers()
    {
        await CreateSupplierAsync("ACM-001", "Acme Tools");
        await CreateSupplierAsync("ACM-002", "Acme Industrial");
        var third = await CreateSupplierAsync("BET-001", "Beta Metals");

        var query = new ListSupplierQuery { Skip = 1, Length = 2 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Info.Skip.Should().Be(1);
        result.Value.Info.Length.Should().Be(2);
        result.Value.Info.TotalCount.Should().Be(3);
        result.Value.Data.Should().HaveCount(2);
        result.Value.Data.Select(x => x.Id).Should().Contain(third.Id);
    }

    [Test]
    public async Task Should_filter_by_name_contains_case_insensitive()
    {
        var matching = await CreateSupplierAsync("ACM-101", "Acme Supply");
        await CreateSupplierAsync("BET-101", "Beta Supply");

        var query = new ListSupplierQuery
        {
            Skip = 0,
            Length = 10,
            Name = "cMe s"
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Info.TotalCount.Should().Be(1);
        result.Value.Data.Should().ContainSingle();
        result.Value.Data.Single().Id.Should().Be(matching.Id);
    }

    [Test]
    public async Task Should_filter_by_code_contains_case_insensitive()
    {
        var matching = await CreateSupplierAsync("SUP-ALPHA-001", "Gamma");
        await CreateSupplierAsync("SUP-BETA-002", "Delta");

        var query = new ListSupplierQuery
        {
            Skip = 0,
            Length = 10,
            Code = "alpha"
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Info.TotalCount.Should().Be(1);
        result.Value.Data.Should().ContainSingle();
        result.Value.Data.Single().Id.Should().Be(matching.Id);
    }

    [Test]
    public async Task Should_filter_by_name_and_code()
    {
        var matching = await CreateSupplierAsync("SUP-ACME-001", "Acme North");
        await CreateSupplierAsync("SUP-ACME-002", "Acme South");
        await CreateSupplierAsync("SUP-BETA-003", "Beta North");

        var query = new ListSupplierQuery
        {
            Skip = 0,
            Length = 10,
            Name = "north",
            Code = "acme"
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Info.TotalCount.Should().Be(1);
        result.Value.Data.Should().ContainSingle();
        result.Value.Data.Single().Id.Should().Be(matching.Id);
    }

    [Test]
    public async Task Should_ignore_whitespace_only_filters()
    {
        var one = await CreateSupplierAsync("SUP-001", "Acme");
        var two = await CreateSupplierAsync("SUP-002", "Beta");

        var query = new ListSupplierQuery
        {
            Skip = 0,
            Length = 10,
            Name = "   ",
            Code = "\t"
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Info.TotalCount.Should().Be(2);
        result.Value.Data.Should().HaveCount(2);
        result.Value.Data.Select(x => x.Id).Should().Contain([one.Id, two.Id]);
    }

    private async Task<Supplier> CreateSupplierAsync(string supplierCode, string name)
    {
        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: supplierCode,
            name: name,
            contactEmail: null,
            phone: null,
            createdBy: "tests");

        return await SupplierRepository.Add(supplier);
    }
}
