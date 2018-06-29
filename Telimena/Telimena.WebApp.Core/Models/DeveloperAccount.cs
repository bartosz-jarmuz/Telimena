namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using DotNetLittleHelpers;

    public class DeveloperAccount
    {
        //EF Required constructor
        // ReSharper disable once UnusedMember.Local
        private DeveloperAccount()
        {
        }

        public DeveloperAccount(TelimenaUser user)
        {
            this.MainUserId = user.Id;
            this.MainEmail = user.Email;
            this.Name = user.DisplayName;
            user.AssociateWithDeveloperAccount(this);
        }

        public string Name { get; set; }
        public string MainEmail { get; set; }
        public int Id { get; set; }

        public string MainUserId { get; private set; }
        [ForeignKey("MainUserId")]
        public virtual TelimenaUser MainUser { get; private set; }

        public void SetMainUser(TelimenaUser user)
        {
            this.MainUser = user;
            this.AssociateUser(user);
        }

        public void AssociateUser(TelimenaUser user)
        {
            if (!this.AssociatedUsers.Contains(user))
            {
                ((Collection<TelimenaUser>)this.AssociatedUsers).Add(user);
            }
        }

        public void RemoveAssociatedUser(TelimenaUser user)
        {
            if (this.AssociatedUsers.Contains(user))
            {
                ((Collection<TelimenaUser>)this.AssociatedUsers).Remove(user);
            }
        }

        public void AddProgram(Program program)
        {
            if (this.Programs.All(x => x.ProgramId != this.Id))
            {
                ((Collection<Program>)this.Programs).Add(program);
            }
        }


        public virtual RestrictedAccessCollection<TelimenaUser> AssociatedUsers { get; set; } = new List<TelimenaUser>();

        public virtual RestrictedAccessCollection<Program> Programs { get; set; } = new List<Program>();
    }
}