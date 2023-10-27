using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AuctionBackEnd.Database
{
    public class SqlDbContext : DbContext
    {
        public DbSet<ClientData> Clients { get; set; }
        public DbSet<AuctionData> Auctions { get; set; }
        

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var server = Env.GetString("DB_HOST");
            var port = Env.GetString("DB_PORT");
            if (port != null)
            {
                server += $":{port}";
            }
            var connString = $"Server={server};" +
                             $"Database={Env.GetString("DB_DATABASE")};" +
                             $"User ID={Env.GetString("DB_USERNAME")};" +
                             $"Password={Env.GetString("DB_PASSWORD")}";
            optionsBuilder.UseMySql(connString, ServerVersion.AutoDetect(connString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuctionData>()
                .Property(e => e.IsEnd)
                .HasConversion(new BoolToStringConverter("false", "true"));
            
            modelBuilder.Entity<AuctionData>()
                .Property(e => e.IsReceived)
                .HasConversion(new BoolToStringConverter("false", "true"));
        }
    }
}