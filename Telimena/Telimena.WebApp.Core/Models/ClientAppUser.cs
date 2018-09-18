using System;

namespace Telimena.WebApp.Core.Models
{
    public class ClientAppUser
    {
        public DateTime RegisteredDate { get; set; }
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string MachineName { get; set; }
        public string IpAddress { get; set; }
    }
}