using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.Identity
{
    using Microsoft.AspNet.Identity.EntityFramework;

    public class TelimenaUser : IdentityUser
    {
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string DisplayName { get; set; }
        public bool IsActivated { get; set; }
        public bool MustChangePassword { get; set; }

        public IList<string> RoleNames { get; set; } = new List<string>();
    }
}
