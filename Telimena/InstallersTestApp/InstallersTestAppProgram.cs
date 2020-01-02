using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedLogic;
using Newtonsoft.Json;
using TelimenaClient;
using Console = System.Console;

namespace InstallersTestApp
{
    class InstallersTestAppProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Starting {typeof(InstallersTestAppProgram).Assembly.GetName().Name}");

            if (!SharedUtils.PrintVersionsAndCheckArgs(args, typeof(InstallersTestAppProgram)))
            {
                return;
            }

            var arguments = SharedUtils.LoadArguments(args);

            
            new TestAppWorker(arguments).Work();

        }
    }
}
