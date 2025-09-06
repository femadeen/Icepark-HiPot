using Hipot.Core.DTOs;
using System.Threading.Tasks;

namespace Hipot.Core.Services.Interfaces
{
    public interface IXmlTestScriptService
    {
        Task<string> GetTestScript(string serialNumber);
        Task<TestHeaderInfo> GetTestHeaderInfo(string testScript);
    }
}