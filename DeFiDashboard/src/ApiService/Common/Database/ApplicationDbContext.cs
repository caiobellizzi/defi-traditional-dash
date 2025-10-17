using ApiService.Common.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Common.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<CustodyWallet> CustodyWallets => Set<CustodyWallet>();
    public DbSet<TraditionalAccount> TraditionalAccounts => Set<TraditionalAccount>();
    public DbSet<ClientAssetAllocation> ClientAssetAllocations => Set<ClientAssetAllocation>();
    public DbSet<WalletBalance> WalletBalances => Set<WalletBalance>();
    public DbSet<AccountBalance> AccountBalances => Set<AccountBalance>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionAudit> TransactionAudits => Set<TransactionAudit>();
    public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();
    public DbSet<PerformanceMetric> PerformanceMetrics => Set<PerformanceMetric>();
    public DbSet<RebalancingAlert> RebalancingAlerts => Set<RebalancingAlert>();
    public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();
    public DbSet<ExportJob> ExportJobs => Set<ExportJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema to 'dash'
        modelBuilder.HasDefaultSchema("dash");

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters, conventions, etc. can be added here

        // Set default values for timestamps
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Client).IsAssignableFrom(entityType.ClrType) ||
                typeof(CustodyWallet).IsAssignableFrom(entityType.ClrType) ||
                typeof(TraditionalAccount).IsAssignableFrom(entityType.ClrType) ||
                typeof(ClientAssetAllocation).IsAssignableFrom(entityType.ClrType) ||
                typeof(Transaction).IsAssignableFrom(entityType.ClrType))
            {
                var createdAtProperty = entityType.FindProperty("CreatedAt");
                if (createdAtProperty != null)
                {
                    createdAtProperty.SetDefaultValueSql("CURRENT_TIMESTAMP");
                }

                var updatedAtProperty = entityType.FindProperty("UpdatedAt");
                if (updatedAtProperty != null)
                {
                    updatedAtProperty.SetDefaultValueSql("CURRENT_TIMESTAMP");
                }
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Added &&
                entry.Entity.GetType().GetProperty("CreatedAt") != null)
            {
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
