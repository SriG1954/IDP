using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Helper
{
    public class UserSession
    {
        public bool IsAuthenticated { get; set; } = true;
        public string? UserId { get; set; }
    }
}
