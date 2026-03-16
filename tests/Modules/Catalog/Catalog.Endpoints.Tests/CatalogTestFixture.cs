using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.Endpoints.Tests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
namespace Invoria.Catalog.Endpoints.Tests
{
    [SetUpFixture]
    public class CatalogTestFixture
    {
        protected HttpClient Client { get; private set; }
        protected IServiceScope Scope { get; private set; }

        private CatalogModuleWebApplicationFactory _factory;


        public CatalogTestFixture()
        {
            _factory = new CatalogModuleWebApplicationFactory();

            Client = _factory.CreateClient();

            Scope = _factory.Services.CreateScope();
        }
        [OneTimeSetUp]
        public void Initialize()
        {

        }


        [OneTimeTearDown]
        public async Task OneTimeTearDownAsync()
        {
            await _factory.DisposeAsync();

            Client.Dispose();

            Scope.Dispose();
        }

    }
}
