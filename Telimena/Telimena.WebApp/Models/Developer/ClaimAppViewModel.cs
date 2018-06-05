using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Telimena.WebApp.Models.Developer
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class ClaimAppViewModel
    {
        [Required]
        [DisplayName("Application name")]
        public string AppName { get; set; }
        public bool IsSuccess { get; set; }
    }
}