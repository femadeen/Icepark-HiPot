
namespace Hipot.Core.DTOs
{
    public class UserInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserName { get; set; } = default!;
        public string UserEN { get; set; } = default!;
        public int UserLevel { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}
