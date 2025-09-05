using Hipot.Core.Services.Interfaces;
using System.Xml;

namespace Hipot.Core.Services.Implementations.XML
{
    public class XmlLoaderService : IXmlLoaderService
    {
        private readonly IFileService _fileService;

        public XmlLoaderService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public XmlDocument LoadXml(string fileName)
        {
            var xmlDoc = new XmlDocument();
            var filePath = Path.Combine(_fileService.ScriptPath, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The required XML file was not found: {filePath}");
            }
            xmlDoc.Load(filePath);
            return xmlDoc;
        }
    }
}