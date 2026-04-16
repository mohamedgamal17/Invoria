using Invoria.Procurement.Infrastructure.EntityFramework;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Application.Tests
{
    [TestFixture]
    public class ProcurementModuleInstallerSmokeTests : ProcurementTestFixture
    {
        [Test]
        public void ServiceProvider_resolves_ProcurementDbContext()
        {
            using var scope = ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProcurementDbContext>();

            Assert.That(db, Is.Not.Null);
        }
    }
}
