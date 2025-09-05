using Hipot.Core.Context;
using Hipot.Core.DTOs;
using Hipot.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipot.Core.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly HipotDbContext _context;

        public UserService(HipotDbContext context)
        {
            _context = context;
        }

        public async Task<UserInfo?> ValidateUserAsync(string userEN, string password)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserEN == userEN && u.Password == password);
        }
    }
}
