using Invoria.BuildingBlocks.EntityFramework.Hooks;
using Invoria.Reporting.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Invoria.Reporting.Application.Tests.Orders.Queries;

internal static class ReportingQueryTestDbContextFactory
{
    public static async Task<ReportingDbContext> CreateAsync(CancellationToken cancellationToken = default)
    {
        var catalog = $"ReportingQuery_{Guid.NewGuid():N}";
        var connectionString =
            $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={catalog};Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True";

        var options = new DbContextOptionsBuilder<ReportingDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        var hookEngine = new Mock<IDbHookEngine>();
        hookEngine
            .Setup(h => h.RunBeforeSaveAsync(It.IsAny<DbContext>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        hookEngine
            .Setup(h => h.RunAfterSaveAsync(It.IsAny<DbContext>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var db = new ReportingDbContext(options, hookEngine.Object);
        await db.Database.MigrateAsync(cancellationToken);
        return db;
    }
}
