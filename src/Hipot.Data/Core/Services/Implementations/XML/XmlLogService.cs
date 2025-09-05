using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipot.Data.Core.Services.Implementations.XML
{
    public class XmlLogService
    {
        public string LogDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "TestLog");

        public void GenerateLog(string lSN, string lRsl, string mainText, string detailText, string channelName)
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            var logMsg = new System.Text.StringBuilder();
            logMsg.AppendLine($"SN:{lSN}");
            logMsg.AppendLine(mainText);
            logMsg.AppendLine();
            logMsg.AppendLine("**************");
            logMsg.AppendLine("*   Detail   *");
            logMsg.AppendLine("**************");
            logMsg.AppendLine();
            logMsg.AppendLine(detailText);

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = $"{lSN}_{timestamp}_{channelName}_{lRsl}.log";
            string filePath = Path.Combine(LogDirectory, fileName);

            File.WriteAllText(filePath, logMsg.ToString());
        }
    }
}
