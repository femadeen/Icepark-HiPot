using Hipot.Core.Context;
using Hipot.Core.Models;
using Hipot.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hipot.Core.Repositories.Implementations
{
    public class LogEntryRepository : ILogEntryRepository
    {
        private readonly HipotDbContext _context;
        public LogEntryRepository(HipotDbContext context)
        {
            _context = context;
        }

        public async Task GenerateLog(LogEntry logEntry)
        {
            await _context.Database.EnsureCreatedAsync();
            await _context.Logs.AddAsync(logEntry);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<LogEntry>> GetAllLogs()
        {
            return await _context.Logs.OrderByDescending(l => l.Timestamp).ToListAsync();
        }
    }
}
