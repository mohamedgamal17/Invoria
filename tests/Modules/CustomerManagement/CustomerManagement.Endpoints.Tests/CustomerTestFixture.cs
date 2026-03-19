using Invoria.BuildingBlocks.Core.Extensions;
using Invoria.Endpoints.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.CustomerManagement.Endpoints.Tests
{
    [SetUpFixture]
    public class CustomerTestFixture
    {
        protected HttpClient Client { get; private set; } = null!;
        protected IServiceScope Scope { get; private set; } = null!;

        private CustomerModuleWebApplicationFactory _factory = null!;

        public CustomerTestFixture()
        {
            _factory = new CustomerModuleWebApplicationFactory();

            Client = _factory.CreateClient();
            Scope = _factory.Services.CreateScope();
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

