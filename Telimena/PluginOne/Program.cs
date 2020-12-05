using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginHostApp;
using TelimenaClient;

namespace PluginOne
{
    class Program
    {
        static void Main(string[] args)
        {

        }
    }

    public class PluginOne : IPlugin
    {
        private readonly ITelimena teli;

        public PluginOne()
        {
            this.teli = Telimena.Construct(new TelimenaStartupInfo(Guid.Parse("b1ad5308-43f2-4600-a851-d7cbd0f7b374"), new Uri("http://localhost:7757/")));
        }

        public string SendEvent(string eventName)
        {
            this.teli.Track.Event(eventName + $" {this.teli.Properties.UserInfo.UserIdentifier}");
            return $" {this.teli.Properties.UserInfo.UserIdentifier}";

        }
    }
}
