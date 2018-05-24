namespace Telimena.Client
{
    public class RegistrationRequest
    {
        public UserInfo UserInfo { get; set; }
        public ProgramInfo ProgramInfo { get; set; }
        public string TelimenaVersion { get; set; } 
    }
}