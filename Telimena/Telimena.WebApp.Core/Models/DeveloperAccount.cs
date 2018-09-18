using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models
{
    public class DeveloperAccount
    {
        //EF Required constructor
        //ReSharper disable once UnusedMember.Local
        protected DeveloperAccount()
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

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local - EF required
        public string MainUserId { get; private set; }

        [ForeignKey("MainUserId")]
        public virtual TelimenaUser MainUser { get; private set; }

        public virtual RestrictedAccessCollection<TelimenaUser> AssociatedUsers { get; set; } = new List<TelimenaUser>();

        public virtual RestrictedAccessCollection<Program> Programs { get; set; } = new List<Program>();

        public void AddProgram(Program program)
        {
            if (this.Programs.All(x => x.Id != this.Id))
            {
                ((Collection<Program>) this.Programs).Add(program);
            }
        }

        public void AssociateUser(TelimenaUser user)
        {
            if (!this.AssociatedUsers.Contains(user))
            {
                ((Collection<TelimenaUser>) this.AssociatedUsers).Add(user);
            }
        }

        public void RemoveAssociatedUser(TelimenaUser user)
        {
            if (this.AssociatedUsers.Contains(user))
            {
                ((Collection<TelimenaUser>) this.AssociatedUsers).Remove(user);
            }
        }

        public void SetMainUser(TelimenaUser user)
        {
            this.MainUser = user;
            this.AssociateUser(user);
        }
    }
}