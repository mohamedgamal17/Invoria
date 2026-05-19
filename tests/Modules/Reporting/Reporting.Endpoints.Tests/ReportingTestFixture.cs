using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Reporting.Endpoints.Tests;

[SetUpFixture]
public class ReportingTestFixture
{
    protected HttpClient Client { get; private set; } = null!;

    protected IServiceProvider Services { get; private set; } = null!;

    protected IServiceScope Scope { get; private set; } = null!;

    private ReportingModuleWebApplicationFactory _factory = null!;

    public ReportingTestFixture()
    {
        _factory = new ReportingModuleWebApplicationFactory();
        Client = _factory.CreateClient();
        Services = _factory.Services;
        Scope = Services.CreateScope();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _factory.DisposeAsync();

        Client.Dispose();
        Scope.Dispose();
    }
}
