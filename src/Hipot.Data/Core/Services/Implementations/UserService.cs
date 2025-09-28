using Hipot.Core.DTOs;
using Hipot.Core.Services.Interfaces;
using Hipot.Data.Core.Context;
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

        public async Task<UserInfo> ValidateUserAsync(string userEN, string password)
        {
            return null;
            //return await _context.Users
            //    .FirstOrDefaultAsync(u => u.Username == userEN && u.PasswordHash == password);
        }
    }
}
