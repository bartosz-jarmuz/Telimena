#pragma warning disable 1591
using System.ComponentModel.DataAnnotations;

namespace Telimena.WebApp.Models.Account
{
    public class ChangePasswordViewModel
    {
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; }

        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Display(Name = "New Password Repeated")]
        public string NewPasswordRepeated { get; set; }

        public bool IsSuccess { get; set; }
    }
}