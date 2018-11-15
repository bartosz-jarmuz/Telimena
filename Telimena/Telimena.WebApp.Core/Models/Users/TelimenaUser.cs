﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DotNetLittleHelpers;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Telimena.WebApp.Core.Models
{
    public class TelimenaUser : IdentityUser
    {
        //EF needs a default constructor
        // ReSharper disable once UnusedMember.Local
        protected TelimenaUser()
        {
        }

        public TelimenaUser(string email, string displayName)
        {
            this.SetAttributes(email, displayName);
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserNumber { get; private set; }

        public DateTime RegisteredDate { get; private set; }
        public DateTime? LastLoginDate { get; set; }
        public string DisplayName { get; private set; }
        public bool IsActivated { get; set; }
        public bool MustChangePassword { get; set; }

        /// <summary>
        ///     Returns developer accounts associated with this user (e.g. dev accounts which main user is set to this user)
        /// </summary>
        public virtual RestrictedAccessList<DeveloperAccount> AssociatedDeveloperAccounts { get; set; } = new RestrictedAccessList<DeveloperAccount>();

        public void AssociateWithDeveloperAccount(DeveloperAccount developer)
        {
            if (!this.AssociatedDeveloperAccounts.Contains(developer))
            {
                ((List<DeveloperAccount>) this.AssociatedDeveloperAccounts).Add(developer);
            }
        }

        /// <summary>
        ///     Returns the developer accounts where the current user is the main user
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DeveloperAccount> GetDeveloperAccountsLedByUser()
        {
            return this.AssociatedDeveloperAccounts.Where(x => x.MainUser == this);
        }

        private void SetAttributes(string email, string displayName)
        {
            this.RegisteredDate = DateTime.UtcNow;
            this.Email = email;
            this.UserName = email;
            this.DisplayName = displayName;
        }
    }
}