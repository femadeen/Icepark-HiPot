using Hipot.Core.Models;
using Hipot.Core.Repositories.Interfaces;
using Hipot.Core.Services.Interfaces;
using System;
using System.IO;

namespace Hipot.Core.Services.Implementations;

public class LogService : ILogService
{
    private readonly ILogEntryRepository _logEntryRepository;
    public LogService(ILogEntryRepository logEntryRepository)
    {
        _logEntryRepository = logEntryRepository;
    }
    public async Task GenerateLog(string lSN, string lRsl, string mainText, string detailText, string channelName)
    {
        var log = new LogEntry
        {
            Timestamp = DateTime.Now,
            SerialNumber = lSN,
            Result = lRsl,
            MainText = mainText,
            DetailText = detailText,
            ChannelName = channelName
        };
        await _logEntryRepository.GenerateLog(log);
    }

    public async Task<IReadOnlyList<LogEntry>> GetAllLogs()
    {
        return await _logEntryRepository.GetAllLogs();
    }
}