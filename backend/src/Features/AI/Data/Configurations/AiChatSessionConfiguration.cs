using ECommerce.Api.Entities;
using ECommerce.Api.Features.AI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Api.Features.AI.Data.Configurations;

public sealed class AiChatSessionConfiguration : IEntityTypeConfiguration<AiChatSession>
{
    public void Configure(EntityTypeBuilder<AiChatSession> builder)
    {
        builder.ToTable("AIChatSessions", table => table.HasCheckConstraint(
            "CK_AIChatSessions_Owner",
            "([UserID] IS NOT NULL AND [GuestTokenHash] IS NULL) OR ([UserID] IS NULL AND [GuestTokenHash] IS NOT NULL)"));
        builder.HasKey(session => session.SessionID);
        builder.Property(session => session.SessionID).ValueGeneratedNever();
        builder.Property(session => session.GuestTokenHash).HasMaxLength(64).IsUnicode(false);
        builder.Property(session => session.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(session => session.UpdatedAt).IsRequired();
        builder.Property(session => session.ExpiresAt).IsRequired();
        builder.HasIndex(session => new { session.UserID, session.UpdatedAt });
        builder.HasIndex(session => session.ExpiresAt);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(session => session.UserID)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_AIChatSessions_Users");
    }
}
