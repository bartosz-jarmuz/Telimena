using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using TelimenaClient;

namespace SharedLogic
{
    class TestClientProgram
    {

        public static string GetFileVersion(Type type)
        {
            return FileVersionInfo.GetVersionInfo(type.Assembly.Location).FileVersion;
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"Starting {typeof(TestClientProgram).Assembly.GetName().Name}");

            if (!SharedUtils.PrintVersionsAndCheckArgs(args, typeof(TestClientProgram)))
            {
                return;
            }


            var arguments = SharedUtils.LoadArguments(args);
            new TestAppWorker(arguments).Work();
        }

    }
}
