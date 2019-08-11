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
        protected List<string> errors = new List<string>();
        protected List<string> outputs = new List<string>();

        protected Exception CleanupAndRethrow(Exception ex, [CallerMemberName] string caller = "")
        {
            string msg =
                $"Error when executing test: {caller}" + 
                $"\r\nOutputs:\r\n{String.Join("\r\n", this.outputs)}" + 
                $"\r\n\r\nErrors:\r\n{string.Join("\r\n", this.errors)}\r\n. See inner exception.";
            TestAppProvider.KillTestApps();
            return new InvalidOperationException(msg, ex);
        }

        public static void Log(string info, [CallerMemberName] string caller = "")
        {
            //Trace.TraceInformation("trace - UiTestsLogger:" + info);
            //Trace.TraceError("error - UiTestsLogger:" + info);
            //Logger.LogMessage("Logger - UiTestsLogger:" + info);
            //TestContext.Out.WriteLine("Ctx -  UiTestsLogger:" + info);

            Console.WriteLine(SharedTestHelpers.GetMethodName(caller) + " - "+ info);
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