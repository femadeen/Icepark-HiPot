using Hipot.Core.DTOs;
using Hipot.Core.Services.Interfaces;
using System.Xml;

namespace Hipot.Core.Services.Implementations.XML
{
    public class XmlSerialNumberService : ISerialNumberService
    {
        private readonly IXmlLoaderService _xmlLoader;

        public XmlSerialNumberService(IXmlLoaderService xmlLoader)
        {
            _xmlLoader = xmlLoader;
        }

        public Task<SerialNumberInfo> GetSerialNumberInfoAsync(string serialNumber)
        {
            var info = new SerialNumberInfo { IsValid = false };
            var xmlDoc = _xmlLoader.LoadXml("main.xml");

            var nodeList = xmlDoc.SelectNodes("maintest/card");
            if (nodeList == null) return Task.FromResult(info);

            foreach (XmlNode node in nodeList)
            {
                var snFormat = node.Attributes?["snformat"]?.Value;
                if (!string.IsNullOrEmpty(snFormat) &&
                    serialNumber.StartsWith(snFormat, StringComparison.OrdinalIgnoreCase))
                {
                    info.IsValid = true;
                    var uidBuilder = new System.Text.StringBuilder();

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        switch (child.Name.ToUpper())
                        {
                            case "CARDNAME":
                                info.CardName = child.InnerText;
                                break;
                            case "TESTSCRIPT":
                                info.TestScript = child.InnerText;
                                break;
                            case "UID":
                                if (uidBuilder.Length > 0) uidBuilder.AppendLine();
                                uidBuilder.Append(child.InnerText);
                                break;
                        }
                    }
                    info.UID = uidBuilder.ToString();
                    return Task.FromResult(info);
                }
            }

            return Task.FromResult(info);
        }
    }
}
