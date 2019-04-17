using System;
using SharedLogic;
using TelimenaClient;

namespace EmbeddedAssemblyTestApp
{
    public class EmbeddedAssemblyTestAppProgram
    {
        static void Main(string[] args)
        {
            SharedUtils.ShowArgsRequiredMessage(args, typeof(EmbeddedAssemblyTestAppProgram), "EmbeddedAssemblyTestApp");

            var arguments = SharedUtils.LoadArguments(args);

            TelimenaStartupInfo si = new TelimenaStartupInfo(arguments.TelemetryKey, new Uri(arguments.ApiUrl))
            {
                ProgramInfo = arguments.ProgramInfo
                ,
                DeliveryInterval = TimeSpan.FromSeconds(3)
            };

            //this is to test that telimena works OK if used as embedded assembly
            var teli = Telimena.Construct(si);

            teli.Track.Event("HelloFromEmbeddedTelimenaClientApp");
            teli.Track.SendAllDataNow();

            Console.WriteLine("Ended with no errors");
        }
    }
}
