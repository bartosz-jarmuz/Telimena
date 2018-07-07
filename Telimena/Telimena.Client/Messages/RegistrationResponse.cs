namespace Telimena.Client
{
    using System;

    public class RegistrationResponse : TelimenaResponseBase
    {
        public UpdateInfo UpdateInfo { get; set; }
        public int UserId { get; set; }
        public int ProgramId { get; set; }
        public int Count { get; set; }
    }
}