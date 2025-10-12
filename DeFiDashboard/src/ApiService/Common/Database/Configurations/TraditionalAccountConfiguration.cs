using ApiService.Common.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiService.Common.Database.Configurations;

public class TraditionalAccountConfiguration : IEntityTypeConfiguration<TraditionalAccount>
{
    public void Configure(EntityTypeBuilder<TraditionalAccount> builder)
    {
        builder.ToTable("traditional_accounts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(a => a.PluggyItemId)
            .HasColumnName("pluggy_item_id")
            .HasMaxLength(100);

        builder.Property(a => a.PluggyAccountId)
            .HasColumnName("pluggy_account_id")
            .HasMaxLength(100);

        builder.HasIndex(a => a.PluggyAccountId)
            .IsUnique()
            .HasDatabaseName("idx_accounts_pluggy_id");

        builder.Property(a => a.AccountType)
            .HasColumnName("account_type")
            .HasMaxLength(50);

        builder.HasIndex(a => a.AccountType)
            .HasDatabaseName("idx_accounts_type");

        builder.Property(a => a.InstitutionName)
            .HasColumnName("institution_name")
            .HasMaxLength(200);

        builder.Property(a => a.AccountNumber)
            .HasColumnName("account_number")
            .HasMaxLength(100);

        builder.Property(a => a.Label)
            .HasColumnName("label")
            .HasMaxLength(200);

        builder.Property(a => a.OpenFinanceProvider)
            .HasColumnName("open_finance_provider")
            .HasMaxLength(50)
            .HasDefaultValue("Pluggy")
            .IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasDefaultValue("Active")
            .IsRequired();

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("idx_accounts_status");

        builder.Property(a => a.LastSyncAt)
            .HasColumnName("last_sync_at");

        builder.Property(a => a.SyncStatus)
            .HasColumnName("sync_status")
            .HasMaxLength(50);

        builder.Property(a => a.SyncErrorMessage)
            .HasColumnName("sync_error_message");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        // Relationships
        builder.HasMany(a => a.Balances)
            .WithOne(b => b.Account)
            .HasForeignKey(b => b.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
