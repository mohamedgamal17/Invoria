using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Endpoints.Tests;

[SetUpFixture]
public class InventoryTestFixture
{
    protected HttpClient Client { get; private set; } = null!;
    protected IServiceScope Scope { get; private set; } = null!;

    private InventoryModuleWebApplicationFactory _factory = null!;

    public InventoryTestFixture()
    {
        _factory = new InventoryModuleWebApplicationFactory();
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
