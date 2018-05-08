namespace Telimena.WebApp.Models.Account
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterViewModel
    {
        [Display(Name = "Display Name")]    
        public string Name { get; set; }
        [Display(Name = "Email (will be used as login)")]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}