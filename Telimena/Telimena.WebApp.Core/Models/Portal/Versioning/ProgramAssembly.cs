using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.DTO.MappableToClient;

namespace Telimena.WebApp.Core.Models.Portal
{
    public class ProgramAssembly
    {
        [Key, ForeignKey(nameof(Program))]
        public int ProgramId { get; set; }
        public virtual Program Program { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        
        public virtual RestrictedAccessList<AssemblyVersionInfo> Versions { get; set; } = new RestrictedAccessList<AssemblyVersionInfo>();

        public string GetFileName()
        {
            return this.Name + this.Extension;
        }

      
     

    }
}