using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.CustomerManagement.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.CustomerManagement.Infrastructure.EntityFramework.Configuration
{
    public class CustomerEntityTypeConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.MapId();

            builder.Property(x => x.Id)
                .HasMaxLength(CustomerTableConsts.IdMaxLength);

            builder.Property(x => x.Name)
                .HasMaxLength(CustomerTableConsts.NameMaxLength);

            builder.MapAudited();
        }
    }
}

