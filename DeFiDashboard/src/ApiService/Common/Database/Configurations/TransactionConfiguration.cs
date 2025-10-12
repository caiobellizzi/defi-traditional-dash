using ApiService.Common.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiService.Common.Database.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(t => t.TransactionType).HasColumnName("transaction_type").HasMaxLength(20).IsRequired();
        builder.Property(t => t.AssetId).HasColumnName("asset_id").IsRequired();
        builder.Property(t => t.TransactionHash).HasColumnName("transaction_hash").HasMaxLength(200);
        builder.HasIndex(t => t.TransactionHash).HasDatabaseName("idx_transactions_hash");

        builder.Property(t => t.ExternalId).HasColumnName("external_id").HasMaxLength(200);
        builder.HasIndex(t => t.ExternalId).HasDatabaseName("idx_transactions_external_id");

        builder.Property(t => t.Chain).HasColumnName("chain").HasMaxLength(50);
        builder.Property(t => t.Direction).HasColumnName("direction").HasMaxLength(10).IsRequired();
        builder.Property(t => t.FromAddress).HasColumnName("from_address").HasMaxLength(200);
        builder.Property(t => t.ToAddress).HasColumnName("to_address").HasMaxLength(200);
        builder.Property(t => t.TokenSymbol).HasColumnName("token_symbol").HasMaxLength(20);
        builder.Property(t => t.Amount).HasColumnName("amount").HasPrecision(36, 18).IsRequired();
        builder.Property(t => t.AmountUsd).HasColumnName("amount_usd").HasPrecision(18, 2);
        builder.Property(t => t.Fee).HasColumnName("fee").HasPrecision(36, 18);
        builder.Property(t => t.FeeUsd).HasColumnName("fee_usd").HasPrecision(18, 2);
        builder.Property(t => t.Description).HasColumnName("description");
        builder.Property(t => t.Category).HasColumnName("category").HasMaxLength(100);
        builder.Property(t => t.TransactionDate).HasColumnName("transaction_date").IsRequired();
        builder.HasIndex(t => t.TransactionDate).HasDatabaseName("idx_transactions_date");

        builder.Property(t => t.IsManualEntry).HasColumnName("is_manual_entry").HasDefaultValue(false).IsRequired();
        builder.Property(t => t.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("Confirmed").IsRequired();
        builder.HasIndex(t => t.Status).HasDatabaseName("idx_transactions_status");

        builder.Property(t => t.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

        builder.HasIndex(t => new { t.TransactionType, t.AssetId, t.TransactionDate })
            .HasDatabaseName("idx_transactions_asset");

        builder.HasMany(t => t.AuditTrail)
            .WithOne(a => a.Transaction)
            .HasForeignKey(a => a.TransactionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
