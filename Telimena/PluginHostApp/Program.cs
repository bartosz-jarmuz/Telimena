using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace PluginHostApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var worker = new PluginHostWorker();

            worker.LoadPlugin();

            for (int i = 0; i < worker.Plugins.Count; i++)
            {
                IPlugin workerPlugin = worker.Plugins[i];
                workerPlugin.SendEvent($"Event {i}");
            }

            ConsoleKeyInfo input = Console.ReadKey();
            while (true)
            {

                if (input.Key == ConsoleKey.D1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    var plugin = worker.Plugins.FirstOrDefault(x => x.GetType().Name == "PluginOne");
                  var result = plugin.SendEvent("Plugin 1 event" );
                    Console.WriteLine("Sent plugin 1 event " + result);
                }
                else if (input.Key == ConsoleKey.D2)
               {
                   Console.ForegroundColor = ConsoleColor.Blue;
                    var plugin = worker.Plugins.FirstOrDefault(x => x.GetType().Name == "PluginTwo");
                    var result = plugin.SendEvent("Plugin 2 event");
                   Console.WriteLine("Sent plugin 2 event" + result);

                }
                else
                {
                    break;
                }

                input = Console.ReadKey();
            }

            Console.ReadKey();
        }

        

    }

    public class PluginHostWorker
    {
        public void LoadPlugin()
        {
            var parentFolder = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent;
            var dlls = parentFolder.GetFiles("*", SearchOption.AllDirectories).Where(x=>
              (string.Equals(x.Name, "PluginOne.exe", StringComparison.Ordinal) && x.FullName.Contains("\\bin\\Debug")) 
              || (string.Equals(x.Name, "PluginTwo.dll" , StringComparison.Ordinal) && x.FullName.Contains("\\bin\\Debug")));
            foreach (FileInfo fileInfo in dlls)
            {
                Assembly ass = null;
                try
                {
                    ass = Assembly.LoadFile(fileInfo.FullName);
                    var plg = ass.GetTypes().FirstOrDefault(x => typeof(IPlugin).IsAssignableFrom(x));
                    if (plg != null)
                    {
                        var instance = Activator.CreateInstance(plg);
                        if (instance is IPlugin plgInstance)
                        {
                            this.Plugins.Add(plgInstance);
                        }
                    }
                }
                catch
                {
                }
               
            }

        }

        public List<IPlugin> Plugins { get; set; } = new List<IPlugin>();
    }

    public interface IPlugin
    {
        string SendEvent(string eventName);
    }
}
