using Hipot.Core.Models;

namespace Hipot.Core.Services.Interfaces
{
    public interface ILogService
    {
        Task GenerateLog(string lSN, string lRsl, string mainText, string detailText, string channelName);
        Task<IReadOnlyList<LogEntry>> GetAllLogs();
    }
}
