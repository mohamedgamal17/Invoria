using Invoria.BuildingBlocks.EntityFramework.Contexts;
using Invoria.Modules.Products.Domain;
using Microsoft.EntityFrameworkCore;

namespace Invoria.Modules.Products.Infrastructure;

public class ProductsDbContext : InvertoDbContext
{
    public ProductsDbContext(
        DbContextOptions<ProductsDbContext> options,
        Invoria.BuildingBlocks.EntityFramework.Hooks.IDbHookEngine dbHookEngine)
        : base(options, dbHookEngine)
    {
    }

    public DbSet<ProductAggregate> Products { get; set; } = null!;
}

