using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models.Portal
{
    public class DeveloperTeam
    {
        //EF Required constructor
        //ReSharper disable once UnusedMember.Local
        protected DeveloperTeam()
        {
        }

        public DeveloperTeam(TelimenaUser user)
        {
            this.MainUserId = user.Id;
            this.MainEmail = user.Email;
            this.Name = user.DisplayName;
            user.AssociateWithDeveloperAccount(this);
        }

        [StringLength(75)]
        [Index(IsUnique = true)]
        public string Name { get; set; }

        public string MainEmail { get; set; }

        [Key]
        public int Id { get; set; }

        [Index(IsUnique = true, IsClustered = false)]
        public Guid  PublicId { get; set; } = Guid.NewGuid();
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local - EF required
        public string MainUserId { get; private set; }
        [ForeignKey("MainUserId")]
        public virtual TelimenaUser MainUser { get; private set; }

        public virtual RestrictedAccessList<TelimenaUser> AssociatedUsers { get; set; } = new RestrictedAccessList<TelimenaUser>();

        public virtual RestrictedAccessList<Program> Programs { get; set; } = new RestrictedAccessList<Program>();
        public virtual RestrictedAccessList<Updater> Updaters { get; set; } = new RestrictedAccessList<Updater>();

        public void AddProgram(Program program)
        {
            if (this.Programs.All(x => x.TelemetryKey != program.TelemetryKey))
            {
                if (this.Programs.Any(x => x.Name == program.Name))
                {
                    throw new ArgumentException($"A program with name [{program.Name}] was already registered by {this.Name}");
                }
                ((List<Program>) this.Programs).Add(program);
            }
            else
            {
                throw new ArgumentException($"A program with guid [{program.TelemetryKey}] was already registered");
            }
        }

        public void AssociateUser(TelimenaUser user)
        {
            if (!this.AssociatedUsers.Contains(user))
            {
                ((List<TelimenaUser>) this.AssociatedUsers).Add(user);
            }
        }

        public void RemoveAssociatedUser(TelimenaUser user)
        {
            if (this.AssociatedUsers.Contains(user))
            {
                ((List<TelimenaUser>) this.AssociatedUsers).Remove(user);
            }
        }

        public void SetMainUser(TelimenaUser user)
        {
            this.MainUser = user;
            this.AssociateUser(user);
        }
    }
}