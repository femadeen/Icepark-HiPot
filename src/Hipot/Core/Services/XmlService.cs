using System.Xml;
using System.Collections.Generic;
using System.IO;
using System;
using Hipot.Core.DTOs;

namespace Hipot.Core.Services;

public static class XmlService
{
    private static readonly string _scriptPath;
    static XmlService()
    {
        _scriptPath = Path.Combine(FileSystem.AppDataDirectory, "wwwroot", "tscripts");

        if (!Directory.Exists(_scriptPath))
        {
            Directory.CreateDirectory(_scriptPath);
        }

#if DEBUG
        // Development source folder (go up 3 levels from bin\Debug\net8.0-…)
        var projectRoot = GetProjectRoot();
        var devScriptPath = Path.Combine(projectRoot, "wwwroot", "tscripts");

        if (Directory.Exists(devScriptPath))
        {
            // Copy files to AppDataDirectory
            foreach (var file in Directory.GetFiles(devScriptPath))
            {
                var destFile = Path.Combine(_scriptPath, Path.GetFileName(file));
                if (!File.Exists(destFile)) // only copy if missing
                {
                    File.Copy(file, destFile);
                }
            }
        }
#endif
    }
    public static string ScriptPath { get; set; } = GetScriptPath();
    public static string GetScriptPath() => _scriptPath;
    private static string GetProjectRoot()
    {
        var dir = AppContext.BaseDirectory;

        while (!string.IsNullOrEmpty(dir) && !File.Exists(Path.Combine(dir, "Pekasuz3.Maui.csproj")))
        {
            dir = Path.GetDirectoryName(dir);
        }

        if (dir == null)
            throw new DirectoryNotFoundException("Could not locate project root.");

        return dir;
    }
    public static List<ChannelConfig> GetChannelConfigurations()
    {
        var configs = new List<ChannelConfig>();
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(Path.Combine(ScriptPath, "main_usage.xml"));

        var nodeList = xmlDoc.SelectNodes("usage/channel");
        if (nodeList == null) return configs;

        foreach (XmlNode node in nodeList)
        {
            var config = new ChannelConfig
            {
                Name = node.Attributes?["name"]?.Value
            };

            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name.ToUpper())
                {
                    case "SRP":
                        config.SerialPorts.Add(childNode.InnerText);
                        break;
                    case "SRC":
                        config.SerialResources.Add(childNode.InnerText);
                        break;
                }
            }
            configs.Add(config);
        }

        return configs;
    }

    public static string GetTesterId()
    {
        // In a real application, this should not be hardcoded.
        // It should be read from a configuration file.
        return System.Net.Dns.GetHostName();
    }

    public static UserInfo? ValidateUser(string userEN, string password)
    {
        if (string.IsNullOrWhiteSpace(ScriptPath))
            throw new InvalidOperationException("XmlService.ScriptPath is not set.");
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(Path.Combine(ScriptPath, "user.xml"));

        var nodeList = xmlDoc.SelectNodes("mainuser/user");
        if (nodeList == null) return null;

        foreach (XmlNode node in nodeList)
        {
            if (node.Attributes?["uen"]?.Value.Equals(userEN, StringComparison.OrdinalIgnoreCase) == true)
            {
                var userInfo = new UserInfo { UserEN = userEN };
                string storedPassword = "";

                foreach (XmlNode childNode in node.ChildNodes)
                {
                    switch (childNode.Name.ToUpper())
                    {
                        case "USERLEVEL":
                            userInfo.UserLevel = int.Parse(childNode.InnerText);
                            break;
                        case "USERNAME":
                            userInfo.UserName = childNode.InnerText;
                            break;
                        case "PASSWORD":
                            storedPassword = childNode.InnerText;
                            break;
                    }
                }

                if (password == storedPassword)
                {
                    return userInfo;
                }
            }
        }

        return null;
    }

    public static SerialNumberInfo GetSerialNumberInfo(string serialNumber)
    {
        var info = new SerialNumberInfo { IsValid = false };
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(Path.Combine(ScriptPath, "main.xml"));

        var nodeList = xmlDoc.SelectNodes("maintest/card");
        if (nodeList == null) return info;

        foreach (XmlNode node in nodeList)
        {
            var snFormat = node.Attributes?["snformat"]?.Value;
            // The logic for ChkSNForm is missing, so we'll assume a simple check for now.
            if (serialNumber.StartsWith(snFormat, StringComparison.OrdinalIgnoreCase))
            {
                info.IsValid = true;
                var uidBuilder = new System.Text.StringBuilder();

                foreach (XmlNode childNode in node.ChildNodes)
                {
                    switch (childNode.Name.ToUpper())
                    {
                        case "CARDNAME":
                            info.CardName = childNode.InnerText;
                            break;
                        case "TESTSCRIPT":
                            info.TestScript = childNode.InnerText;
                            break;
                        case "UID":
                            if (uidBuilder.Length > 0) uidBuilder.AppendLine();
                            uidBuilder.Append(childNode.InnerText);
                            break;
                    }
                }
                info.UID = uidBuilder.ToString();
                return info;
            }
        }

        return info;
    }

    public static TestHeaderInfo GetTestHeader(string testScript)
    {
        var info = new TestHeaderInfo();
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(Path.Combine(ScriptPath, testScript));

        var nodeList = xmlDoc.SelectNodes("maintest/theader");
        if (nodeList != null && nodeList.Count > 0)
        {
            var node = nodeList[0];
            info.TestName = node.Attributes?["tname"]?.Value;
            info.Revision = node.ChildNodes[0]?.InnerText;
            info.Description = node.ChildNodes[1]?.InnerText;
            info.Owner = node.ChildNodes[2]?.InnerText;
        }

        return info;
    }

    // The complex GetCHCnt function is being refactored and its responsibilities will be split.
    // This service is now only responsible for reading the XML.
    // A higher-level service will be responsible for initializing the application state.
}