using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        // Tables
        public DbSet<PropertyBasics> PropertyBasics { get; set; }
        public DbSet<PropertyAdmin> PropertyAdmin { get; set; }
        public DbSet<Entity> Entity { get; set; }
        public DbSet<StaffAccount> StaffAccount { get; set; }
        public DbSet<AccountsReceivable> AccountsReceivable { get; set; }
        public DbSet<AccountsPayable> AccountsPayable { get; set; }
        public DbSet<APCapex> APCapex { get; set; }
        public DbSet<ConstructionManager> ConstructionManager { get; set; }

        // Views
        public DbSet<PropertyListingViewModel> PropertyListings { get; set; }
        public DbSet<PersonnelReportModel> PersonnelReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("dbo");

            // Table configurations
            modelBuilder.Entity<PropertyBasics>()
                .ToTable("tblPropertyBasics");
            modelBuilder.Entity<PropertyAdmin>()
                .ToTable("tblPropertyAdmin");
            modelBuilder.Entity<Entity>()
                .ToTable("tblEntity");
            modelBuilder.Entity<StaffAccount>()
                .ToTable("tblSA");
            modelBuilder.Entity<AccountsReceivable>()
                .ToTable("tblAR");
            modelBuilder.Entity<AccountsPayable>()
                .ToTable("tblAP");
            modelBuilder.Entity<APCapex>()
                .ToTable("tblAPCapex");
            modelBuilder.Entity<ConstructionManager>()
                .ToTable("tblConsMgr");

            // View configurations
            modelBuilder.Entity<PropertyListingViewModel>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vwInHousePropertyListing");
            });

            modelBuilder.Entity<PersonnelReportModel>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vwInHousePropertyListing1");
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving changes to the database");
                throw new Exception("Error saving changes to the database. See inner exception for details.", ex);
            }
        }
    }
}