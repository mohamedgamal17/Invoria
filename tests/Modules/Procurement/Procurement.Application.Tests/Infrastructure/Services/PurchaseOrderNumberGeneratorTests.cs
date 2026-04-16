using Invoria.Procurement.Application.Services;
using Invoria.Procurement.Infrastructure.EntityFramework;
using Invoria.Procurement.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Application.Tests.Infrastructure.Services;

[TestFixture]
public class PurchaseOrderNumberGeneratorTests : ProcurementTestFixture
{
    [Test]
    public async Task GenerateNextAsync_returns_date_prefixed_and_incrementing_number()
    {
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProcurementDbContext>();
        IPurchaseOrderNumberGenerator generator = new PurchaseOrderNumberGenerator(db);

        var first = await generator.GenerateNextAsync();
        var second = await generator.GenerateNextAsync();

        Assert.That(first, Has.Length.EqualTo(13));
        Assert.That(second, Has.Length.EqualTo(13));
        Assert.That(second[..8], Is.EqualTo(first[..8]));
        Assert.That(int.Parse(second[8..]), Is.EqualTo(int.Parse(first[8..]) + 1));
    }
}
