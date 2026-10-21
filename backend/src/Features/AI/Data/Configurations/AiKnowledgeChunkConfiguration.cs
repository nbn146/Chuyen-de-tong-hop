using ECommerce.Api.Features.AI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Api.Features.AI.Data.Configurations;

public sealed class AiKnowledgeChunkConfiguration : IEntityTypeConfiguration<AiKnowledgeChunk>
{
    public void Configure(EntityTypeBuilder<AiKnowledgeChunk> builder)
    {
        builder.ToTable("AIKnowledgeChunks");
        builder.HasKey(chunk => chunk.ChunkID);
        builder.Property(chunk => chunk.ChunkID).ValueGeneratedNever();
        builder.Property(chunk => chunk.Content).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(chunk => chunk.ContentHash).IsRequired().HasMaxLength(64).IsUnicode(false);
        builder.Property(chunk => chunk.EmbeddingModel).IsRequired().HasMaxLength(100);
        builder.Property(chunk => chunk.Status).IsRequired().HasMaxLength(20).IsUnicode(false);
        builder.Property(chunk => chunk.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(chunk => new { chunk.ProductID, chunk.ChunkOrdinal }).IsUnique();
        builder.HasIndex(chunk => new { chunk.Status, chunk.IndexedAt });
        builder.HasOne(chunk => chunk.Product)
            .WithMany()
            .HasForeignKey(chunk => chunk.ProductID)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_AIKnowledgeChunks_Products");
    }
}
