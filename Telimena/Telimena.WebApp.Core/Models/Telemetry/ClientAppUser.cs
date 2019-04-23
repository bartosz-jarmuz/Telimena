using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Telimena.WebApp.Core.Models.Telemetry
{
    public class ClientAppUser
    {
        public int Id { get; set; } 

        public Guid PublicId { get; set; } = Guid.NewGuid();

        public DateTimeOffset FirstSeenDate { get; set; } = DateTimeOffset.UtcNow;
        /// <summary>
        /// Gets or sets the user identifier - name, email etc. This is not the internal user entity ID
        /// </summary>
        /// <value>The user identifier.</value>
        public string UserIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the authenticated user identifier if the user was authenticated within an app
        /// </summary>
        /// <value>The authenticated user identifier.</value>
        public string AuthenticatedUserIdentifier { get; set; }
        public string Email { get; set; }
        public string MachineName { get; set; }

        private List<String> ipAddresses;

        [NotMapped]
        public List<string> IpAddresses
        {
            get
            {
                if (this.ipAddresses == null)
                {
                    this.ipAddresses = new List<string>();
                }
                return this.ipAddresses;
            }
            set => this.ipAddresses = value;
        }

        public string IpAddressesString
        {
            get => String.Join(",", this.ipAddresses??new List<string>());
            set => this.ipAddresses = value?.Split(',')?.ToList()?? new List<string>();
        }
    }

  
}