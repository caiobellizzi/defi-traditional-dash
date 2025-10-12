using ApiService.Common.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiService.Common.Database.Configurations;

public class ClientAssetAllocationConfiguration : IEntityTypeConfiguration<ClientAssetAllocation>
{
    public void Configure(EntityTypeBuilder<ClientAssetAllocation> builder)
    {
        builder.ToTable("client_asset_allocations");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(c => c.ClientId)
            .HasColumnName("client_id")
            .IsRequired();

        builder.HasIndex(c => c.ClientId)
            .HasDatabaseName("idx_allocations_client");

        builder.Property(c => c.AssetType)
            .HasColumnName("asset_type")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.AssetId)
            .HasColumnName("asset_id")
            .IsRequired();

        builder.HasIndex(c => new { c.AssetType, c.AssetId })
            .HasDatabaseName("idx_allocations_asset");

        builder.Property(c => c.AllocationType)
            .HasColumnName("allocation_type")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.AllocationValue)
            .HasColumnName("allocation_value")
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(c => c.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(c => c.EndDate)
            .HasColumnName("end_date");

        // Unique constraint: no overlapping active allocations
        builder.HasIndex(c => new { c.ClientId, c.AssetType, c.AssetId, c.EndDate })
            .IsUnique()
            .HasDatabaseName("client_asset_allocations_client_id_asset_type_asset_id_end_d_key");

        // Index for active allocations
        builder.HasIndex(c => c.ClientId)
            .HasFilter("end_date IS NULL")
            .HasDatabaseName("idx_allocations_active");

        builder.HasIndex(c => new { c.StartDate, c.EndDate })
            .HasDatabaseName("idx_allocations_date_range");

        builder.Property(c => c.Notes)
            .HasColumnName("notes");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
    }
}
