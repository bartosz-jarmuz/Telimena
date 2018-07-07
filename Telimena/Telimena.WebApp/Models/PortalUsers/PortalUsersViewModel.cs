using System.Collections.Generic;

namespace Telimena.WebApp.Models.PortalUsers
{
    public class PortalUsersViewModel
    {
        public List<TelimenaUserViewModel> Users { get; set; } = new List<TelimenaUserViewModel>();
    }
}