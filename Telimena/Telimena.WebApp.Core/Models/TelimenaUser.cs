namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.AspNet.Identity.EntityFramework;

    public class TelimenaUser : IdentityUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string DisplayName { get; set; }
        public bool IsActivated { get; set; }
        public bool MustChangePassword { get; set; }

        public IList<string> RoleNames { get; set; } = new List<string>();
    }
}
