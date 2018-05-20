namespace Telimena.WebApp.Core.Models
{
    using System;

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