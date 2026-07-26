using Invoria.BackgroundJob.Core;
using Invoria.BackgroundJob.Core.Checkpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Invoria.BackgroundJobs.Hangfire.EntityFramework.Configuration;

public class JobCheckpointEntityTypeConfiguration : IEntityTypeConfiguration<JobCheckpoint>
{
    public void Configure(EntityTypeBuilder<JobCheckpoint> builder)
    {
        builder.ToTable("JobCheckpoints");

        builder.HasKey(x => x.JobId);

        builder.Property(x => x.JobId)
            .HasMaxLength(256)
            .HasConversion(new JobIdValueConverter())
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.State)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();
    }

    private sealed class JobIdValueConverter : ValueConverter<JobId, string>
    {
        public JobIdValueConverter()
            : base(
                v => v.Value,
                v => new JobId(v))
        {
        }
    }
}
