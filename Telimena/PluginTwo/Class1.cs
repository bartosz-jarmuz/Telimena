using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginHostApp;
using TelimenaClient;

namespace PluginTwo
{
    public class PluginTwo : IPlugin
    {
        private ITelimena teli;

        public PluginTwo()
        {
            this.teli = Telimena.Construct(new TelimenaStartupInfo(Guid.Parse("aeb21d70-a645-4666-b2f7-c34391997032")));
        }

        public void SendEvent(string eventName)
        {
            this.teli.Track.Event(eventName);
        }
    }
}
