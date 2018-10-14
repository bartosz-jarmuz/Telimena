using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Telimena.WebApp.Core.Models
{
    public class ClientAppUser
    {
        public DateTime RegisteredDate { get; set; }
        public int Id { get; set; }
        public string UserName { get; set; }
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
            get => String.Join(",", this.ipAddresses);
            set => this.ipAddresses = value.Split(',').ToList();
        }
    }

  
}