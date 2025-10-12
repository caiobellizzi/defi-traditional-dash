using ApiService.Common.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiService.Common.Database.Configurations;

public class WalletBalanceConfiguration : IEntityTypeConfiguration<WalletBalance>
{
    public void Configure(EntityTypeBuilder<WalletBalance> builder)
    {
        builder.ToTable("wallet_balances");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(b => b.WalletId)
            .HasColumnName("wallet_id")
            .IsRequired();

        builder.HasIndex(b => b.WalletId)
            .HasDatabaseName("idx_balances_wallet");

        builder.Property(b => b.Chain)
            .HasColumnName("chain")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(b => b.Chain)
            .HasDatabaseName("idx_balances_chain");

        builder.Property(b => b.TokenAddress)
            .HasColumnName("token_address")
            .HasMaxLength(100);

        builder.Property(b => b.TokenSymbol)
            .HasColumnName("token_symbol")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(b => b.TokenSymbol)
            .HasDatabaseName("idx_balances_symbol");

        builder.Property(b => b.TokenName)
            .HasColumnName("token_name")
            .HasMaxLength(100);

        builder.Property(b => b.TokenDecimals)
            .HasColumnName("token_decimals");

        builder.Property(b => b.Balance)
            .HasColumnName("balance")
            .HasPrecision(36, 18)
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(b => b.BalanceUsd)
            .HasColumnName("balance_usd")
            .HasPrecision(18, 2);

        builder.Property(b => b.LastUpdated)
            .HasColumnName("last_updated")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasIndex(b => b.LastUpdated)
            .HasDatabaseName("idx_balances_updated");

        // Unique constraint
        builder.HasIndex(b => new { b.WalletId, b.Chain, b.TokenAddress })
            .IsUnique()
            .HasDatabaseName("wallet_balances_wallet_id_chain_coalesce_key");
    }
}
