namespace TelimenaClient
{
    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        private class StartupData
        {
            public StartupData(ProgramInfo programInfo, UserInfo userInfo, string telimenaVersion)
            {
                this.ProgramInfo = programInfo;
                this.UserInfo = userInfo;
                this.TelimenaVersion = telimenaVersion;
            }

            public ProgramInfo ProgramInfo { get; set; }
            public UserInfo UserInfo { get; set; }
            public  string TelimenaVersion { get; set; }
        }
    }
}