using System.Collections.Generic;
using System.IO;
using System.Xml;
using Hipot.Data.Core.Models;
using Hipot.Core.Services.Interfaces;
using System;

namespace Hipot.Data.Core.Services
{
    public class XmlConfigService
    {
        private readonly IFileService _fileService;

        public XmlConfigService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public int GetChannelCount()
        {
            var doc = new XmlDocument();
            doc.Load(Path.Combine(_fileService.ScriptPath, "main_usage.xml"));
            var nodes = doc.SelectNodes("usage/channel");
            return nodes.Count;
        }

        public List<SerialPortConfig> GetSerialPortConfigs()
        {
            var configs = new List<SerialPortConfig>();
            var doc = new XmlDocument();
            doc.Load(Path.Combine(_fileService.ScriptPath, "main_usage.xml"));

            var channelNodes = doc.SelectNodes("usage/channel");
            foreach (XmlNode channelNode in channelNodes)
            {
                foreach (XmlNode portNode in channelNode.ChildNodes)
                {
                    if (portNode.Name.ToUpper() == "SRP")
                    {
                        configs.Add(ParsePortConfig(portNode.InnerText));
                    }
                }
            }

            var commonNodes = doc.SelectNodes("usage/common/SRC");
            foreach (XmlNode commonNode in commonNodes)
            {
                configs.Add(ParsePortConfig(commonNode.InnerText));
            }

            return configs;
        }

        private SerialPortConfig ParsePortConfig(string configString)
        {
            var parts = configString.Split(',');
            var stopBits = parts[5] switch
            {
                "1" => System.IO.Ports.StopBits.One,
                "1.5" => System.IO.Ports.StopBits.OnePointFive,
                "2" => System.IO.Ports.StopBits.Two,
                _ => System.IO.Ports.StopBits.None,
            };
            return new SerialPortConfig
            {
                Name = parts[0],
                PortName = "COM" + parts[1],
                BaudRate = int.Parse(parts[2]),
                DataBits = int.Parse(parts[3]),
                Parity = (System.IO.Ports.Parity)Enum.Parse(typeof(System.IO.Ports.Parity), parts[4].ToUpper() == "N" ? "None" : parts[4]),
                StopBits = stopBits
            };
        }
    }
}
