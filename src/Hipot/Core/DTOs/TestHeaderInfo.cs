namespace Hipot.Core.DTOs
{
    public class TestHeaderInfo
    {
        public string TestName { get; set; } = default!;
        public string? Revision { get; set; }
        public string? Description { get; set; }
        public string? Owner { get; set; }
    }
}
