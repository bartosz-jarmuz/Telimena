namespace Telimena.WebApp.Core.Models
{
    using System.Collections.Generic;

    public class Developer
    {
        public string Name { get; set; }
        public string MainEmail { get; set; }
        public int Id { get; set; }

        public TelimenaUser MainUser { get; set; }
        public ICollection<TelimenaUser> AssociatedUsers { get; set; } = new List<TelimenaUser>();

        public ICollection<Program> Programs { get; set; } = new List<Program>();
    }
}