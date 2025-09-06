
using Hipot.Core.DTOs;
using System.Collections.Generic;

namespace Hipot.Data
{
    public class AppState
    {
        public UserInfo? CurrentUser { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
    }
}
