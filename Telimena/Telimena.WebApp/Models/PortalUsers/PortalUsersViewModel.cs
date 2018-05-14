using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Telimena.WebApp.Models.PortalUsers
{
    using Core.Models;
    using Infrastructure.Identity;

    public class PortalUsersViewModel
    {
        public List<TelimenaUser> Users { get; set; }
    }
}