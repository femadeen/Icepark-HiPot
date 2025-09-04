namespace Hipot.Core.Models
{
    public class LogEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string MainText { get; set; } = string.Empty;
        public string DetailText { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
    }
}
