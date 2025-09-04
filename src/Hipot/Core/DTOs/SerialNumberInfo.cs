namespace Hipot.Core.DTOs
{
    public class SerialNumberInfo
    {
        public string CardName { get; set; } = default!;
        public string TestScript { get; set; } = default!;
        public string UID { get; set; } = default!;
        public bool IsValid { get; set; }
    }
}
