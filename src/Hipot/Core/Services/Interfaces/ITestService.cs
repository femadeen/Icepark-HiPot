using Hipot.Core.DTOs;

namespace Hipot.Core.Services.Interfaces
{
    public interface ITestService
    {
        Task<TestHeaderInfo?> GetTestHeaderAsync(string testScript);
    }
}
