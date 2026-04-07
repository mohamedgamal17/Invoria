using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Endpoints.Tests;

[SetUpFixture]
public class OrderingTestFixture
{
    protected HttpClient Client { get; private set; } = null!;
    protected IServiceScope Scope { get; private set; } = null!;

    private OrderingModuleWebApplicationFactory _factory = null!;

    /// <summary>Host service provider (use <see cref="IServiceScope"/> for scoped services such as <c>DbContext</c>).</summary>
    protected OrderingModuleWebApplicationFactory Factory => _factory;

    public OrderingTestFixture()
    {
        _factory = new OrderingModuleWebApplicationFactory();

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
