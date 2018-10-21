using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using AutomaticTestsClient;
using Newtonsoft.Json;

namespace Telimena.WebApp.UITests.IntegrationTests.TestAppInteraction
{
    internal static class ProcessCreator
    {
        private static ProcessStartInfo CreateStartInfo(FileInfo testAppFile, Arguments arguments)
        {
            var serialized = JsonConvert.SerializeObject(arguments);
            var encoded = Base64Encode(serialized);
            Log($"Starting app {testAppFile.FullName}");
            Log($"Start info Arguments {serialized}");
            Log($"Start info Encoded arguments{encoded}");
            return new ProcessStartInfo
            {
                FileName = testAppFile.FullName
                , Arguments = encoded
                , RedirectStandardError = true
                , RedirectStandardOutput = true
                , UseShellExecute = false
            };
        }

        private static void Log(string info)
        {
            Debug.WriteLine(info);
        }
      

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static Process Create(FileInfo testAppFile, Arguments arguments, List<string> outputs, List<string> errors)
        {
            var si = CreateStartInfo(testAppFile, arguments);
            Process process = new Process();
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (sender, args) => OutputDataReceived(sender, args, outputs);
            process.ErrorDataReceived += (sender, args) => ErrorDataReceived(sender, args, errors);
            process.StartInfo = si;


            return process;
        }


        static void ErrorDataReceived(object sender, DataReceivedEventArgs e, List<string> errors)
        {
            errors.Add(e.Data);

        }

        static void OutputDataReceived(object sender, DataReceivedEventArgs e, List<string> outputs)
        {
            outputs.Add(e.Data);
        }
    }
}
