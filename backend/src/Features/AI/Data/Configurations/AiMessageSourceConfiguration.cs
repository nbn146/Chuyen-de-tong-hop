using ECommerce.Api.Entities;
using ECommerce.Api.Features.AI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Api.Features.AI.Data.Configurations;

public sealed class AiMessageSourceConfiguration : IEntityTypeConfiguration<AiMessageSource>
{
    public void Configure(EntityTypeBuilder<AiMessageSource> builder)
    {
        builder.ToTable("AIMessageSources");
        builder.HasKey(source => source.SourceID);
        builder.Property(source => source.SourceID).UseIdentityColumn();
        builder.Property(source => source.SourceVersion).IsRequired().HasMaxLength(64).IsUnicode(false);
        builder.Property(source => source.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(source => new { source.MessageID, source.Rank }).IsUnique();
        builder.HasOne(source => source.Message)
            .WithMany(message => message.Sources)
            .HasForeignKey(source => source.MessageID)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_AIMessageSources_Messages");
        builder.HasOne(source => source.Chunk)
            .WithMany()
            .HasForeignKey(source => source.ChunkID)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_AIMessageSources_Chunks");
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(source => source.ProductID)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_AIMessageSources_Products");
    }
}
