using ECommerce.Api.Features.AI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Api.Features.AI.Data.Configurations;

public sealed class AiChatMessageConfiguration : IEntityTypeConfiguration<AiChatMessage>
{
    public void Configure(EntityTypeBuilder<AiChatMessage> builder)
    {
        builder.ToTable("AIChatMessages");
        builder.HasKey(message => message.MessageID);
        builder.Property(message => message.MessageID).UseIdentityColumn();
        builder.Property(message => message.Role).IsRequired().HasMaxLength(16).IsUnicode(false);
        builder.Property(message => message.Content).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(message => message.AnswerType).HasMaxLength(32).IsUnicode(false);
        builder.Property(message => message.ModelName).HasMaxLength(128);
        builder.Property(message => message.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(message => new { message.SessionID, message.CreatedAt });
        builder.HasOne(message => message.Session)
            .WithMany(session => session.Messages)
            .HasForeignKey(message => message.SessionID)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_AIChatMessages_Sessions");
    }
}
