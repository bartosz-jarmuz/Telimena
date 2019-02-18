using System.Collections.Generic;

namespace TelimenaClient
{
    public partial class Telimena 
    {
        /// <summary>
        /// 
        /// </summary>
        public static class Telemetry
        {
            /// <summary>
            ///     Reports an occurence of an event
            ///     <para>This is an ASYNC method which should be awaited</para>
            /// </summary>
            /// <param name="startupInfo">Startup data</param>
            /// <param name="eventName">Name of the event</param>
            /// <param name="telemetryData">Custom telemetry data</param>
            /// <returns></returns>
            public static void Event(ITelimenaStartupInfo startupInfo, string eventName, Dictionary<string, string> telemetryData = null)
            {
                ITelimena teli = TelimenaFactory.Construct(startupInfo);
                teli.Track.Event(eventName, telemetryData);
            }

            /// <summary>
            ///     Report the usage of the application view.
            ///     <para>This is an ASYNC method which should be awaited</para>
            /// </summary>
            /// <param name="startupInfo">Startup data</param>
            /// <param name="viewName">The name of the view. If left blank, it will report the name of the invoked method</param>
            /// <param name="telemetryData"></param>
            /// <returns></returns>
            public static void View(ITelimenaStartupInfo startupInfo, string viewName, Dictionary<string, string> telemetryData = null)
            {
                ITelimena teli = TelimenaFactory.Construct(startupInfo);
                teli.Track.View(viewName, telemetryData);
            }


        }
    }
}