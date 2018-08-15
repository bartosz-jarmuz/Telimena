// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Telimena.Updater;

namespace Telimena.Client.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class UpdaterTests
    {

        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        private static string[] CommandLineToArgs(string commandLine)
        {
            int argc;
            var argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

        [Test]
        public void Test_StartInfoParsing()
        {
            var instructions = new FileInfo(@"C:\An app\Updates\3.2\Instructions.xml");
            var updater = new FileInfo(@"C:\An app\Updates\Updater.exe");
            var startInfo = StartInfoCreator.CreateStartInfo(instructions, updater);

            Assert.AreEqual(@"C:\An app\Updates\Updater.exe", startInfo.FileName);
            var settings = CommandLineArgumentParser.GetSettings(CommandLineToArgs(startInfo.Arguments));

            Assert.AreEqual(@"C:\An app\Updates\3.2\Instructions.xml", settings.InstructionsFile.FullName);
        }
    }
}

           

