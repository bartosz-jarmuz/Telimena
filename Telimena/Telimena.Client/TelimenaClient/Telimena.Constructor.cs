namespace Telimena.Client
{
    #region Using
    using System;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    #endregion

    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        /// <summary>
        ///     Creates a new instance of Telimena Client
        /// </summary>
        /// <param name="telemetryApiBaseUrl">Leave default, unless you want to call different telemetry server</param>
        /// <param name="mainAssembly">
        ///     Leave null, unless you want to use different assembly as the main one for program name,
        ///     version etc
        /// </param>
        public Telimena(Assembly mainAssembly = null, string telemetryApiBaseUrl = "http://localhost:7757/")
        {
            Assembly assembly = mainAssembly ?? Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            this.ProgramInfo = new ProgramInfo()
            {
                PrimaryAssembly = new AssemblyInfo(assembly),
                Name = assembly.GetName().Name,
            };
            this.UserInfo = new UserInfo()
            {
                UserName = Environment.UserName,
                MachineName = Environment.MachineName
            };

            this.TelimenaVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.PrimaryAssemblyVersion = this.ProgramInfo.PrimaryAssembly.Version;
            this.HttpClient = new TelimenaHttpClient(new HttpClient()
            {
                BaseAddress = new Uri(telemetryApiBaseUrl)
            });
            this.Messenger = new Messenger(this.Serializer, this.HttpClient, this.SuppressAllErrors);
        }
    }
}