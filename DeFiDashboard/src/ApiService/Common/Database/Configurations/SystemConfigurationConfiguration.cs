using ApiService.Common.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiService.Common.Database.Configurations;

public class SystemConfigurationConfiguration : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.ToTable("system_configuration");

        builder.HasKey(s => s.Key);
        builder.Property(s => s.Key).HasColumnName("key").HasMaxLength(100);
        builder.Property(s => s.Value).HasColumnName("value").IsRequired();
        builder.Property(s => s.Description).HasColumnName("description");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
    }
}
