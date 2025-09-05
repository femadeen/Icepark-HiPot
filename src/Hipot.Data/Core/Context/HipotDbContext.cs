using Hipot.Core.DTOs;
using Hipot.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hipot.Core.Context
{
    public class HipotDbContext : DbContext
    {
        public HipotDbContext(DbContextOptions<HipotDbContext> options) : base(options)
        {
        }
        public DbSet<LogEntry> Logs { get; set; }
        public DbSet<UserInfo> Users { get; set; }
    }
}
