namespace Telimena.WebApp.Core.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class DeveloperAccount
    {
        public string Name { get; set; }
        public string MainEmail { get; set; }
        public int Id { get; set; }

        public string MainUserId { get; set; }
        [ForeignKey("MainUserId")]
        public virtual TelimenaUser MainUser { get; set; }

        public virtual ICollection<TelimenaUser> AssociatedUsers { get; set; } = new List<TelimenaUser>();

        public virtual ICollection<Program> Programs { get; set; } = new List<Program>();
    }
}