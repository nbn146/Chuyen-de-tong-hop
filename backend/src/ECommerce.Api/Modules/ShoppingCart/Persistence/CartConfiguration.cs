using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Api.Modules.Identity;

namespace ECommerce.Api.Modules.ShoppingCart;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.CartID);

        builder.Property(c => c.CartID)
            .UseIdentityColumn();

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSDATETIME()");

        // Unique â€” má»—i user chá»‰ cÃ³ 1 cart
        builder.HasIndex(c => c.UserID)
            .IsUnique()
            .HasDatabaseName("UQ_Carts_User");

        // FK â†’ Users (1:1)
        builder.HasOne(c => c.User)
            .WithOne(u => u.Cart)
            .HasForeignKey<Cart>(c => c.UserID)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Carts_Users");
    }
}
