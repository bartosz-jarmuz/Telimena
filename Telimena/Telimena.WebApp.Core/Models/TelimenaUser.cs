namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using DotNetLittleHelpers;
    using Microsoft.AspNet.Identity.EntityFramework;

    public class TelimenaUser : IdentityUser
    {

        //EF needs a default constructor
        // ReSharper disable once UnusedMember.Local
        protected TelimenaUser() { }

        public TelimenaUser(string email, string displayName)
        {
            this.SetAttributes(email, displayName);
        }

        private void SetAttributes(string email, string displayName)
        {
            this.CreatedDate = DateTime.UtcNow;
            this.Email = email;
            this.UserName = email;
            this.DisplayName = displayName;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserNumber { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime? LastLoginDate { get; set; }
        public string DisplayName { get; private set; }
        public bool IsActivated { get; set; }
        public bool MustChangePassword { get; set; }

        public IList<string> RoleNames { get; set; } = new List<string>();

        /// <summary>
        /// Returns developer accounts associated with this user (e.g. dev accounts which main user is set to this user)
        /// </summary>
        public virtual RestrictedAccessCollection<DeveloperAccount> AssociatedDeveloperAccounts { get; set; } = new List<DeveloperAccount>();

        public void AssociateWithDeveloperAccount(DeveloperAccount developer)
        {
            if (!this.AssociatedDeveloperAccounts.Contains(developer))
            {
                ((Collection<DeveloperAccount>)this.AssociatedDeveloperAccounts).Add(developer);
            }
        }

        /// <summary>
        /// Returns the developer accounts where the current user is the main user
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DeveloperAccount> GetDeveloperAccountsLedByUser()
        {
            return this.AssociatedDeveloperAccounts.Where(x => x.MainUser == this);
        }
    }
}
