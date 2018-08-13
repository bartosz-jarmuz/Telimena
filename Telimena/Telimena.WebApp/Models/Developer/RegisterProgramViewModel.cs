namespace Telimena.WebApp.Models.Developer
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class RegisterProgramViewModel
    {
        [Required]
        [DisplayName("Name of the application that you want to register as yours")]
        public string ProgramName { get; set; }
        public bool IsSuccess { get; set; }
    }
}