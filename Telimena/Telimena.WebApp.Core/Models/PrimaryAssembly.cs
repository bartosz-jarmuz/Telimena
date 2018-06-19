namespace Telimena.WebApp.Core.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class PrimaryAssembly : AssemblyData
    {
        [ForeignKey("Program")]
        public int Id { get; set; }
        [Required]
        public virtual Program Program { get; set; }


    }
}