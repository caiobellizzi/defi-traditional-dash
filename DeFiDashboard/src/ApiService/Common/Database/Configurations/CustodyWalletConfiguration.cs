using ApiService.Common.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiService.Common.Database.Configurations;

public class CustodyWalletConfiguration : IEntityTypeConfiguration<CustodyWallet>
{
    public void Configure(EntityTypeBuilder<CustodyWallet> builder)
    {
        builder.ToTable("custody_wallets");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(w => w.WalletAddress)
            .HasColumnName("wallet_address")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(w => w.WalletAddress)
            .IsUnique()
            .HasDatabaseName("idx_wallets_address");

        builder.Property(w => w.Label)
            .HasColumnName("label")
            .HasMaxLength(200);

        builder.Property(w => w.BlockchainProvider)
            .HasColumnName("blockchain_provider")
            .HasMaxLength(50)
            .HasDefaultValue("Moralis")
            .IsRequired();

        builder.HasIndex(w => w.BlockchainProvider)
            .HasDatabaseName("idx_wallets_provider");

        builder.Property(w => w.SupportedChains)
            .HasColumnName("supported_chains");

        builder.Property(w => w.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasDefaultValue("Active")
            .IsRequired();

        builder.HasIndex(w => w.Status)
            .HasDatabaseName("idx_wallets_status");

        builder.Property(w => w.Notes)
            .HasColumnName("notes");

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Relationships
        builder.HasMany(w => w.Balances)
            .WithOne(b => b.Wallet)
            .HasForeignKey(b => b.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
