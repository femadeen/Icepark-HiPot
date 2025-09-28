using Hipot.Core.Models;
using Hipot.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hipot.Data.Core.Context
{
    public class HipotDbContext : DbContext
    {
        public HipotDbContext(DbContextOptions<HipotDbContext> options) : base(options)
        {
        }
        public DbSet<LogEntry> Logs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Users)
                .WithOne(u => u.Project)
                .HasForeignKey(u => u.ProjectId);
        }
    }
}
