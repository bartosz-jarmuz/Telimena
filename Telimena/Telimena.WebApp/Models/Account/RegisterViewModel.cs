namespace Telimena.WebApp.Models.Account
{
    using System.ComponentModel.DataAnnotations;

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
    }
}