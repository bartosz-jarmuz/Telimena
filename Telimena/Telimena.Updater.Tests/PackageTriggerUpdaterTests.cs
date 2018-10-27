// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using TelimenaClient;
using Telimena.PackageTriggerUpdater;
using Telimena.PackageTriggerUpdater.CommandLineArguments;
using Assert = NUnit.Framework.Assert;

namespace TelimenaUpdaterTests
{
   
    [TestFixture]
    public class PackageTriggerUpdaterTests
    {

        private DirectoryInfo MyAppFolder => new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockDisk", "PackageTriggerUpdater/MyPackageBasedAppFolder.zip_Extracted"));
        private DirectoryInfo Update12Folder => this.MyAppFolder.CreateSubdirectory(Path.Combine("Updates", "1.2"));
        private DirectoryInfo Update11Folder => this.MyAppFolder.CreateSubdirectory(Path.Combine("Updates", "1.1"));


        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        private static string[] CommandLineToArgs(string commandLine)
        {
            int argc;
            IntPtr argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            try
            {
                string[] args = new string[argc];
                for (int i = 0; i < args.Length; i++)
                {
                    IntPtr p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }


        [DeploymentItem("MockDisk")]
        [Test]
        public void Test_Worker_ZippedPkg()
        {
            var sut = new PackageUpdaterWorker();
            var executablePackage = sut.GetExecutablePackage(new FileInfo(Path.Combine(this.Update12Folder.FullName, "MyPackageBasedApp Update v. 1.2.zip")));
            Assert.IsTrue(executablePackage.Exists);
            Assert.IsTrue(executablePackage.FullName.EndsWith(@"\MyPackageBasedAppFolder.zip_Extracted\Updates\1.2 Extracted\UpdatePackage.pkg"));
        }

        [DeploymentItem("MockDisk")]
        [Test]
        public void Test_Worker_Pkg()
        {
            var sut = new PackageUpdaterWorker();
            var executablePackage = sut.GetExecutablePackage(new FileInfo(Path.Combine(this.Update11Folder.FullName, "MyPackageBasedApp Update v. 1.1.pkg")));
            Assert.IsTrue(executablePackage.FullName.EndsWith(@"\MyPackageBasedAppFolder.zip_Extracted\Updates\1.1\MyPackageBasedApp Update v. 1.1.pkg"));
            Assert.IsTrue(executablePackage.Exists);
        }

    }
}