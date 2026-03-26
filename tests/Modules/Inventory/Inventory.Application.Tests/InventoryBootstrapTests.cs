using Invoria.Inventory.Infrastructure.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Invoria.Inventory.Application.Tests
{
    [TestFixture]
    public class InventoryBootstrapTests : InventoryTestFixture
    {
        [Test]
        public void Should_resolve_inventory_db_context()
        {
            var dbContext = ServiceProvider.GetRequiredService<InventoryDbContext>();

            Assert.NotNull(dbContext);
        }
    }
}

