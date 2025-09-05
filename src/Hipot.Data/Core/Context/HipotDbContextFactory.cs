using Hipot.Core.Context;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipot.Data.Core.Context
{
    public class HipotDbContextFactory : IDesignTimeDbContextFactory<HipotDbContext>
    {
        public HipotDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HipotDbContext>();

            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "logs.db"
            );

            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            return new HipotDbContext(optionsBuilder.Options);
        }
    }
}
