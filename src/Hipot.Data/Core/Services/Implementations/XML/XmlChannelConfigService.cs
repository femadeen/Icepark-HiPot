using Hipot.Core.DTOs;
using Hipot.Core.Services.Interfaces;
using System.Xml;

namespace Hipot.Core.Services.Implementations.XML
{
    public class XmlChannelConfigService : IChannelConfigService
    {
        private readonly IXmlLoaderService _xmlLoader;

        public XmlChannelConfigService(IXmlLoaderService xmlLoader)
        {
            _xmlLoader = xmlLoader;
        }

        public Task<IReadOnlyList<ChannelConfig>> GetChannelConfigurationsAsync()
        {
            var configs = new List<ChannelConfig>();
            var xmlDoc = _xmlLoader.LoadXml("main_usage.xml");

            var nodeList = xmlDoc.SelectNodes("usage/channel");
            if (nodeList != null)
            {
                foreach (XmlNode node in nodeList)
                {
                    var config = new ChannelConfig
                    {
                        Name = node.Attributes?["name"]?.Value ?? string.Empty
                    };

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        switch (child.Name.ToUpper())
                        {
                            case "SRP":
                                config.SerialPorts.Add(child.InnerText);
                                break;
                            case "SRC":
                                config.SerialResources.Add(child.InnerText);
                                break;
                        }
                    }
                    configs.Add(config);
                }
            }
            return Task.FromResult<IReadOnlyList<ChannelConfig>>(configs);
        }
    }
}
