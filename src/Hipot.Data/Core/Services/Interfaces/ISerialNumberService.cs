using Hipot.Core.DTOs;

namespace Hipot.Core.Services.Interfaces
{
    public interface ISerialNumberService
    {
        Task<SerialNumberInfo> GetSerialNumberInfoAsync(string serialNumber);
    }
}
