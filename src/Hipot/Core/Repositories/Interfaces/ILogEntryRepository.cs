using Hipot.Core.Models;

namespace Hipot.Core.Repositories.Interfaces
{
    public interface ILogEntryRepository
    {
        Task GenerateLog(LogEntry logEntry);
        Task<IReadOnlyList<LogEntry>> GetAllLogs();
    }
}
