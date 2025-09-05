namespace Hipot.Core.DTOs
{
    public class ChannelConfig
    {
        public string Name { get; set; } = default!;
        public List<string> SerialPorts { get; set; } = [];
        public List<string> SerialResources { get; set; } = [];
    }
}
