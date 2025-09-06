using System.Diagnostics;
using System.Xml;
using Hipot.Core.Services.Interfaces;
using System.Threading.Tasks;

namespace Hipot.Core.Services.Implementations.XML
{
    public class XmlTestScriptService : IXmlTestScriptService
    {
        private readonly IFileService _fileService;

        public XmlTestScriptService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<string> GetTestScript(string serialNumber)
        {
            Debug.WriteLine($"Getting test script for serial number: {serialNumber}");
            var mainXmlContent = await _fileService.GetScriptContent("main.xml");
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(mainXmlContent);

            var nodeList = xmlDoc.SelectNodes("maintest/card");
            if (nodeList == null) return null;

            Debug.WriteLine($"Found {nodeList.Count} card nodes.");

            foreach (XmlNode node in nodeList)
            {
                var snFormat = node.Attributes?["snformat"]?.Value;
                Debug.WriteLine($"Checking format: {snFormat}");
                if (snFormat != null && IsSnFormatMatch(serialNumber, snFormat))
                {
                    Debug.WriteLine("Format match found.");
                    var testScriptNode = node.SelectSingleNode("testscript");
                    return testScriptNode?.InnerText;
                }
            }

            Debug.WriteLine("No matching test script found.");
            return null;
        }

        private bool IsSnFormatMatch(string sn, string format)
        {
            Debug.WriteLine($"Comparing SN '{sn}' with format '{format}'");
            var upperSn = sn.ToUpper();
            var upperFormat = format.ToUpper();

            if (upperSn.Length != upperFormat.Length) 
            {
                Debug.WriteLine("Length mismatch.");
                return false;
            }

            for (int i = 0; i < upperSn.Length; i++)
            {
                if (upperFormat[i] != '?' && upperFormat[i] != upperSn[i])
                {
                    Debug.WriteLine($"Mismatch at index {i}: format has {upperFormat[i]}, SN has {upperSn[i]}");
                    return false;
                }
            }

            Debug.WriteLine("SN format matches.");
            return true;
        }
    }
}