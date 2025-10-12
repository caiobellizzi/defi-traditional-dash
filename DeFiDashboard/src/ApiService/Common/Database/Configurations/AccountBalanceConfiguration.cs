using ApiService.Common.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiService.Common.Database.Configurations;

public class AccountBalanceConfiguration : IEntityTypeConfiguration<AccountBalance>
{
    public void Configure(EntityTypeBuilder<AccountBalance> builder)
    {
        builder.ToTable("account_balances");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(b => b.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.HasIndex(b => b.AccountId)
            .HasDatabaseName("idx_account_balances_account");

        builder.Property(b => b.BalanceType)
            .HasColumnName("balance_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(b => b.BalanceType)
            .HasDatabaseName("idx_account_balances_type");

        builder.Property(b => b.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("BRL")
            .IsRequired();

        builder.Property(b => b.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(b => b.LastUpdated)
            .HasColumnName("last_updated")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Unique constraint
        builder.HasIndex(b => new { b.AccountId, b.BalanceType })
            .IsUnique()
            .HasDatabaseName("account_balances_account_id_balance_type_key");
    }
}
