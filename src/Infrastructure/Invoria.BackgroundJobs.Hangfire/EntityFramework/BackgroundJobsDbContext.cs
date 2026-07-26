using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Invoria.BackgroundJobs.Hangfire.EntityFramework;

public class BackgroundJobsDbContext : DbContext
{
    public BackgroundJobsDbContext(
        DbContextOptions<BackgroundJobsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
