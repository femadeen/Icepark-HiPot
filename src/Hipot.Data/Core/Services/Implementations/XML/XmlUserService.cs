using Hipot.Core.DTOs;
using Hipot.Core.Services.Interfaces;
using System.Xml;

namespace Hipot.Core.Services.Implementations.XML
{
    public class XmlUserService : IUserService
    {
        private readonly IXmlLoaderService _xmlLoader;

        public XmlUserService(IXmlLoaderService xmlLoader)
        {
            _xmlLoader = xmlLoader;
        }

        public Task<UserInfo?> ValidateUserAsync(string userEN, string password)
        {
            var xmlDoc = _xmlLoader.LoadXml("user.xml");
            var nodeList = xmlDoc.SelectNodes("mainuser/user");

            if (nodeList == null) return Task.FromResult<UserInfo?>(null);

            foreach (XmlNode node in nodeList)
            {
                if (node.Attributes?["uen"]?.Value.Equals(userEN, StringComparison.OrdinalIgnoreCase) == true)
                {
                    var user = new UserInfo { UserEN = userEN };
                    string storedPassword = "";

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        switch (child.Name.ToUpper())
                        {
                            case "USERLEVEL":
                                user.UserLevel = int.Parse(child.InnerText);
                                break;
                            case "USERNAME":
                                user.UserName = child.InnerText;
                                break;
                            case "PASSWORD":
                                storedPassword = child.InnerText;
                                break;
                        }
                    }

                    if (password == storedPassword)
                        return Task.FromResult<UserInfo?>(user);
                }
            }

            return Task.FromResult<UserInfo?>(null);
        }
    }
}
