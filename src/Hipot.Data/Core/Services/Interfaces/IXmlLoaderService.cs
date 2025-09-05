using System.Xml;

namespace Hipot.Core.Services.Interfaces
{
    public interface IXmlLoaderService
    {
        XmlDocument LoadXml(string fileName);
    }
}
