using Genrpg.ServerShared.Accounts;
using Genrpg.ServerShared.Config;
using Microsoft.EntityFrameworkCore;

namespace Genrpg.ServerShared.DataStores
{
    public class MainDbContext : DbContext
    {
        public DbSet<DbAccount> Accounts { get; set; }


        public static MainDbContext Create(ServerConfig config)
        {
            DbContextOptionsBuilder<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>();
            options.UseSqlServer(config.GetConnectionString("SQLPlatform"), opts => opts.EnableRetryOnFailure());
            return new MainDbContext(options.Options);
        }
        public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbAccount>().ToTable("Accounts");

            base.OnModelCreating(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }



    }
}
