using System;
using System.IO;
using System.Text;

namespace Hipot.Data.Core.Services
{
    public class VpdService
    {
        public byte[] LilacMakeVPD(string fPath, int snChrCnt, string startAdr, string vSN, string cSumAdr, string startSumAdr)
        {
            if (vSN.Length == snChrCnt)
            {
                int wrStartAdr = Convert.ToInt32(startAdr.ToUpper().Replace("0X", ""), 16);
                int calStartAdr = Convert.ToInt32(startSumAdr.ToUpper().Replace("0X", ""), 16);
                int sumAdr = Convert.ToInt32(cSumAdr.ToUpper().Replace("0X", ""), 16);
                byte[] datArr = GetBinFile(fPath);
                
                for (int i = wrStartAdr; i < wrStartAdr + snChrCnt; i++)
                {
                    datArr[i] = (byte)vSN[i - wrStartAdr];
                }

                byte[] tArr = new byte[sumAdr - calStartAdr];
                for (int i = 0; i < sumAdr - calStartAdr; i++)
                {
                    tArr[i] = datArr[calStartAdr + i];
                }

                string midSum = GetSum(tArr, 2);
                int sum = (Convert.ToInt32(midSum, 16) % 256) ^ 255;
                sum += 1;
                midSum = sum.ToString("X2");
                
                datArr[sumAdr] = Convert.ToByte(midSum, 16);
                
                string aaa = ConvBin2IntelHex8(datArr);
                Console.WriteLine(aaa);
                
                return datArr;
            }
            else
            {
                return null;
            }
        }

        public string ConvBin2IntelHex8(byte[] inBin)
        {
            StringBuilder tmpStr = new StringBuilder();
            byte[] subDat = new byte[12];

            for (int i = 0; i < inBin.Length; i++)
            {
                int lineBegin = i % 8;
                subDat[lineBegin + 4] = inBin[i];
                string adrs = ((i / 8) * 8).ToString("X4");

                if (lineBegin == 0)
                {
                    if (tmpStr.Length == 0)
                    {
                        tmpStr.Append($":08{adrs}00{inBin[i]:X2}");
                    }
                    else
                    {
                        tmpStr.Append($"\r\n:08{adrs}00{inBin[i]:X2}");
                    }
                    subDat[0] = 8;
                    subDat[1] = Convert.ToByte(adrs.Substring(0, 2), 16);
                    subDat[2] = Convert.ToByte(adrs.Substring(2, 2), 16);
                    subDat[3] = 0;
                }
                else if (lineBegin == 7)
                {
                    string lineSum = GetSum(subDat, 2);
                    int sum = (Convert.ToInt32(lineSum, 16) % 256) ^ 255;
                    sum += 1;
                    lineSum = sum.ToString("X2");
                    tmpStr.Append($"{inBin[i]:X2}{lineSum}");
                }
                else
                {
                    tmpStr.Append($"{inBin[i]:X2}");
                }
            }
            return tmpStr.ToString();
        }

        public byte[] GetBinFile(string fPath)
        {
            return File.ReadAllBytes(fPath);
        }

        public string GetSum(byte[] bufByte, int dig)
        {
            long aSum = 0;
            foreach (byte tmpByte in bufByte)
            {
                aSum += tmpByte;
            }
            return aSum.ToString($"X{dig}");
        }
    }
}
