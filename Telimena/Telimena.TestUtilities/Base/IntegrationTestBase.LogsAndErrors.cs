using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Deployment.WindowsInstaller;
using Newtonsoft.Json;
using NUnit.Framework;
using SharedLogic;
using Telimena.TestUtilities.Base.TestAppInteraction;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Messages;
using TelimenaClient.Model;

namespace Telimena.TestUtilities.Base
{
    [TestFixture]
    public abstract partial class IntegrationTestBase 
    {
        protected IntegrationTestBase()
        {
            try
            {
                Directory.CreateDirectory(LogsOutputFolderPath);
            }
            catch 
            {
                //
            }
        }

        protected List<string> errors = new List<string>();
        protected List<string> outputs = new List<string>();

        public static string LogsOutputFolderPath = Path.Combine(TestOutputFolderPathBase, "Logs");


        protected void CleanupAndLog(Exception ex, [CallerMemberName] string caller = "")
        {
            string msg =
                $"Error when executing test: {caller}" + 
                $"\r\n***Outputs:***\r\n{String.Join("\r\n", this.outputs)}\r\n\r\n***End of Outputs***" + 
                $"\r\n\r\n***Errors:***\r\n{string.Join("\r\n", this.errors)}\r\n\r\n***End of Errors:***." + 
                $"\r\n\r\n{ex}";
            TestAppProvider.KillTestApps();
            Log(msg);
            
        }

        public static void Log(string info, [CallerMemberName] string caller = "")
        {
            //Trace.TraceInformation("trace - UiTestsLogger:" + info);
            //Trace.TraceError("error - UiTestsLogger:" + info);
            //Logger.LogMessage("Logger - UiTestsLogger:" + info);
            //TestContext.Out.WriteLine("Ctx -  UiTestsLogger:" + info);

            Console.WriteLine(caller + " - "+ info);
            LogToFile(caller + " - " + info);
        }

        private static void LogToFile(string message)
        {
            try
            {
                string path = Path.Combine(IntegrationTestBase.LogsOutputFolderPath
                    , IntegrationTestBase.TestRunTimestamp + " Logs.txt");
                File.AppendAllText(path
                    , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " - " + message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when writing error log:" + ex);
            }
        }
        
        protected T ParseOutput<T>() where T : class
        {
            foreach (string output in this.outputs)
            {
                if (!string.IsNullOrWhiteSpace(output))
                {
                    Log(output);
                    try
                    {
                        T obj = JsonConvert.DeserializeObject<T>(output, new JsonSerializerSettings(){TypeNameHandling = TypeNameHandling.Auto});
                        if (obj != null)
                        {
                            IntegrationTestBase.Log($"Output deserialized as {typeof(T).Name}");
                            return obj;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }


            foreach (string output in this.errors)
            {
                if (!string.IsNullOrWhiteSpace(output))
                {
                    Log(output);
                    try
                    {
                        T obj = JsonConvert.DeserializeObject<T>(output, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                        if (obj != null)
                        {
                            IntegrationTestBase.Log($"Error output deserialized as {typeof(T).Name}");
                            return obj;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return null;
        }

    }
}