using Hipot.Core.DTOs;
using Hipot.Core.Services.Interfaces;

namespace Hipot.Core.Services.Implementations.XML
{
    public class XmlTestService : ITestService
    {
        private readonly IXmlLoaderService _xmlLoader;

        public XmlTestService(IXmlLoaderService xmlLoader)
        {
            _xmlLoader = xmlLoader;
        }

        public Task<TestHeaderInfo?> GetTestHeaderAsync(string testScript)
        {
            var xmlDoc = _xmlLoader.LoadXml(testScript);

            var nodeList = xmlDoc.SelectNodes("maintest/theader");
            if (nodeList != null && nodeList.Count > 0)
            {
                var node = nodeList[0];
                var info = new TestHeaderInfo
                {
                    TestName = node.Attributes?["tname"]?.Value ?? "",
                    Revision = node.ChildNodes[0]?.InnerText ?? "",
                    Description = node.ChildNodes[1]?.InnerText ?? "",
                    Owner = node.ChildNodes[2]?.InnerText ?? ""
                };
                return Task.FromResult<TestHeaderInfo?>(info);
            }

            return Task.FromResult<TestHeaderInfo?>(null);
        }
    }
}
