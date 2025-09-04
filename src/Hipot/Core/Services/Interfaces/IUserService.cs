using Hipot.Core.DTOs;

namespace Hipot.Core.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserInfo?> ValidateUserAsync(string userEN, string password);
    }
}
