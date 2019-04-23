#pragma warning disable 1591
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Telimena.WebApp.Core.Interfaces;

namespace Telimena.WebApp.Models.Account
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Display Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Email (will be used as login)")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Role { get; set; }

        public IEnumerable<SelectListItem> RoleList { get; set; } = new List<SelectListItem>
        {
            new SelectListItem {Text = TelimenaRoles.Developer, Value = TelimenaRoles.Developer}
            , new SelectListItem {Text = TelimenaRoles.Viewer, Value = TelimenaRoles.Viewer}
        };
    }
}