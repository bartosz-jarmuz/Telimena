namespace Telimena.Client
{
    public class UpdateResponse : TelimenaResponseBase
    {
        public bool IsNewVersionAvailable { get; protected set; }
    }

    public class RegistrationResponse : TelimenaResponseBase
    {
        public int? UserId { get; protected set; }
    }
}