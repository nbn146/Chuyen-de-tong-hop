using ECommerce.Api.Features.AI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Api.Features.AI.Data.Configurations;

public sealed class AiIndexJobConfiguration : IEntityTypeConfiguration<AiIndexJob>
{
    public void Configure(EntityTypeBuilder<AiIndexJob> builder)
    {
        builder.ToTable("AIIndexJobs");
        builder.HasKey(job => job.JobID);
        builder.Property(job => job.JobID).UseIdentityColumn();
        builder.Property(job => job.Status).IsRequired().HasMaxLength(20).IsUnicode(false);
        builder.Property(job => job.Attempts).IsRequired().HasDefaultValue(0);
        builder.Property(job => job.LastError).HasMaxLength(2000);
        builder.Property(job => job.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(job => new { job.Status, job.NextAttemptAt, job.CreatedAt });
        builder.HasIndex(job => new { job.ProductID, job.Status });
        builder.HasOne(job => job.Product)
            .WithMany()
            .HasForeignKey(job => job.ProductID)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_AIIndexJobs_Products");
        builder.ToTable(table => table.HasCheckConstraint("CK_AIIndexJobs_Attempts", "[Attempts] >= 0"));
    }
}
