using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Endpoints.Tests;

[SetUpFixture]
public class ProcurementTestFixture
{
    protected HttpClient Client { get; private set; } = null!;
    protected IServiceScope Scope { get; private set; } = null!;

    private ProcurementModuleWebApplicationFactory _factory = null!;

    public ProcurementTestFixture()
    {
        _factory = new ProcurementModuleWebApplicationFactory();
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

