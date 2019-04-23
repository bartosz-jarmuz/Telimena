#pragma warning disable 1591
using System;
using System.Collections.Generic;

namespace Telimena.WebApp.Models.PortalUsers
{
    public class TelimenaUserViewModel
    {
        public string Id { get; set; }
        public int UserNumber { get; set; }
        public DateTime RegisteredDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool IsActivated { get; set; }
        public bool MustChangePassword { get; set; }
        public ICollection<string> RoleNames { get; set; }
        public IEnumerable<string> DeveloperAccountsLed { get; set; }
    }
}