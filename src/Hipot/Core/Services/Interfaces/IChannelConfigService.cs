using Hipot.Core.DTOs;

namespace Hipot.Core.Services.Interfaces
{
    public interface IChannelConfigService
    {
        Task<IReadOnlyList<ChannelConfig>> GetChannelConfigurationsAsync();
    }
}
