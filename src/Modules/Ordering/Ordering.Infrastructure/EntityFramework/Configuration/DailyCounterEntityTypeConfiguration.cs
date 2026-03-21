using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Invoria.Ordering.Infrastructure.EntityFramework.Configuration
{
    public class DailyCounterEntityTypeConfiguration : IEntityTypeConfiguration<DailyCounter>
    {
        public void Configure(EntityTypeBuilder<DailyCounter> builder)
        {
            builder.ToTable("DailyCounters");
            builder.HasKey(x => x.Date);

            builder.Property(x => x.Date)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(x => x.LastValue)
                .IsRequired();
        }
    }
}
