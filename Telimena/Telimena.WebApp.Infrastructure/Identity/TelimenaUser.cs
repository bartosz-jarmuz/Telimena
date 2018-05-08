using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.Identity
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin;
    using WebApi;

    public class TelimenaUser : IdentityUser
    {
        public bool IsActivated { get; set; }
    }

    public class TelimenaUserManager : UserManager<TelimenaUser>
    {
        public TelimenaUserManager(IUserStore<TelimenaUser> store)
            : base(store)
        {
        }

        public static TelimenaUserManager Create(
            IdentityFactoryOptions<TelimenaUserManager> options, IOwinContext context)
        {
            var manager = new TelimenaUserManager(
                new UserStore<TelimenaUser>(context.Get<TelimenaContext>()));

            return manager;
        }
    }
}
