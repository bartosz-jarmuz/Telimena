using System.Runtime.CompilerServices;

namespace TelimenaClient
{
    /// <summary>
    /// Constructs new instances of Telimena
    /// </summary>
    public static class TelimenaFactory
    {
        /// <summary>
        ///     Creates a new instance of Telimena Client. Equivalent to Telimena.Construct();
        /// </summary>
        /// <param name="startupInfo">Data object which contains startup parameters for Telimena client</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ITelimena Construct(ITelimenaStartupInfo startupInfo)
        {
            try
            {
                Telimena instance = new Telimena(startupInfo);
                return instance;
            }
            catch
            {
                return GetNullObjectTeli(startupInfo);
            }
            
        }

        internal static ITelimena GetNullObjectTeli(ITelimenaStartupInfo startupInfo)
        {
//something went wrong when building telimena
            //return something that will not break client's code
            ITelimenaProperties props;
            try
            {
                //try at least building properties if possible
                props = new TelimenaProperties(startupInfo);
            }
            catch
            {
                //ok, even that failed, return fail safe thing
                props = new NullObjectTelimenaProperties();
            }

            return new NullObjectTelimena(props);
        }
    }
}