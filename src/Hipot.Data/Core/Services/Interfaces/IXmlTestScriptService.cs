using System.Threading.Tasks;

namespace Hipot.Core.Services.Interfaces
{
    public interface IXmlTestScriptService
    {
        Task<string> GetTestScript(string serialNumber);
    }
}