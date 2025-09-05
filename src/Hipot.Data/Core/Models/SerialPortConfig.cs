namespace Hipot.Data.Core.Models
{
    public class SerialPortConfig
    {
        public string Name { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public System.IO.Ports.Parity Parity { get; set; }
        public System.IO.Ports.StopBits StopBits { get; set; }
    }
}
