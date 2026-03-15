using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Invoria.Application.Tests;

/// <summary>
/// Basic container class responsible for registering the services required for tests.
/// Uses Autofac DI with Microsoft.Extensions.DependencyInjection.
/// </summary>

public abstract class TestFixture
{
    private IContainer _container = null!;

    /// <summary>
    /// Gets the service provider built from the container.
    /// </summary>
    protected IServiceProvider ServiceProvider { get; private set; } = null!;

    /// <summary>
    /// Gets the Autofac lifetime scope for the current test.
    /// </summary>
    protected ILifetimeScope Scope { get; private set; } = null!;


    /// <summary>
    /// Gets the Test configuration built from json config file or environment variables.
    /// </summary>
    protected IConfiguration Configuration { get; private set; } = null!;

    protected TestFixture()
    {
        var services = new ServiceCollection();

        Configuration = BuildConfiguration();

        services.AddSingleton<IConfiguration>(Configuration);

        ConfigureServices(services);

        var builder = new ContainerBuilder();

        builder.Populate(services);

        ConfigureContainer(builder);

        _container = builder.Build();

        ServiceProvider = new AutofacServiceProvider(_container);
    }

    [OneTimeSetUp]
    public async Task GlobalSetupAsync()
    {
        
        await BeforeAllTestRunAsync();
    }

    [OneTimeTearDown]
    public async Task GlobalTearDownAsync()
    {
        await AfterAllTestTearDown();

        if (ServiceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    [SetUp]
    public virtual async Task SetupScopeAsync()
    {
        Scope = _container.BeginLifetimeScope();
        
        await BeforeAnyTestRunAsync();
    }

    [TearDown]
    public virtual async Task TearDownScopeAsync()
    {
        // Call the optional hook
        await AfterAnyTestTearDown();

        if (Scope != null)
        {
            await Scope.DisposeAsync();
        }
    }

    /// <summary>
    /// Configures Microsoft.Extensions.DependencyInjection services.
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    /// <summary>
    /// Configures Autofac specific dependencies.
    /// </summary>
    protected virtual void ConfigureContainer(ContainerBuilder builder)
    {
    }

    /// <summary>
    /// Optional hook that is called once before any test run in this fixture.
    /// </summary>
    protected virtual Task BeforeAllTestRunAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Optional hook that is called once after all tests have run in this fixture.
    /// </summary>
    protected virtual Task AfterAllTestTearDown()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Optional hook that is called before each test run.
    /// </summary>
    protected virtual Task BeforeAnyTestRunAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Optional hook that is called after each test run during TearDown.
    /// </summary>
    protected virtual Task AfterAnyTestTearDown()
    {
        return Task.CompletedTask;
    }

    private IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationManager()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", true, false);
      

        return builder.Build();
    }
}
