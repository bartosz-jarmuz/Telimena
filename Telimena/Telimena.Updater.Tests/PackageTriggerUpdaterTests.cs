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
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using TelimenaClient;
using Telimena.PackageTriggerUpdater;
using Telimena.PackageTriggerUpdater.CommandLineArguments;
using Assert = NUnit.Framework.Assert;
using StringAssert = NUnit.Framework.StringAssert;
using UpdateInstructions = Telimena.PackageTriggerUpdater.UpdateInstructions;

namespace TelimenaUpdaterTests
{
   
    [TestFixture]
    public class PackageTriggerUpdaterTests
    {

        private DirectoryInfo MyAppFolder => new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockDisk", "PackageTriggerUpdater/PackagedApp"));
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
        public void Test_Worker_EnsureProperPackageIsPicked()
        {
            List<UpdatePackageData> packages = new List<UpdatePackageData>
            {
                new UpdatePackageData   { Version = "1.2",   FileName="MyPackageBasedApp Update v. 1.2.zip"}
                , new UpdatePackageData { Version = "1.1.2", FileName="MyPackageBasedApp Update v. 1.1.2.zip"}
                , new UpdatePackageData { Version = "1.1.3", FileName="MyPackageBasedApp Update v. 1.1.3.zip"}
            };
            packages.ForEach(x => x.StoredFilePath = Locator.Static.BuildUpdatePackagePath(this.Update12Folder, x).FullName);


            Tuple<XDocument, FileInfo> tuple = UpdateInstructionCreator.CreateXDoc(packages
                , new ProgramInfo { PrimaryAssembly = new AssemblyInfo { Location = @"C:\AppFolder\MyApp.exe" } });
            XDocument xDoc = tuple.Item1;
            FileInfo file = tuple.Item2;
            Assert.AreEqual($@"{this.Update12Folder.FullName}\UpdateInstructions.xml", file.FullName);

            UpdateInstructions instructions = UpdateInstructionsReader.DeserializeDocument(xDoc);
            Assert.That(()=> instructions.Packages.First().Version, Is.EqualTo("1.1.2"));
            Assert.That(()=> instructions.Packages.Last().Version, Is.EqualTo("1.2"));
            var worker = new PackageUpdaterWorker();
            string expected = worker.GetProperPackagePath(instructions);
            Assert.That(expected, Is.EqualTo($@"{this.Update12Folder.FullName}\1.2\MyPackageBasedApp Update v. 1.2.zip"));
        }



        [DeploymentItem("MockDisk")]
        [Test]
        public void Test_Worker_ZippedPkg()
        {
            //package which is donwloaded needs to be extracted, hence the expected executable package is in _Extracted folder
            var sut = new PackageUpdaterWorker();
            var executablePackage = sut.GetExecutablePackage(new FileInfo(Path.Combine(this.Update12Folder.FullName, "1.2", "MyPackageBasedApp Update v. 1.2.zip")));
            Assert.IsTrue(executablePackage.Exists);
            StringAssert.EndsWith( @"\PackagedApp\Updates\1.2\1.2 Extracted\UpdatePackage.pkg", executablePackage.FullName);

        }



        [DeploymentItem("MockDisk")]
        [Test]
        public void Test_Worker_Pkg()
        {
            var sut = new PackageUpdaterWorker();
            var executablePackage = sut.GetExecutablePackage(new FileInfo(Path.Combine(this.Update11Folder.FullName, "MyPackageBasedApp Update v. 1.1.pkg")));
            StringAssert.EndsWith( @"\PackagedApp\Updates\1.1\MyPackageBasedApp Update v. 1.1.pkg", executablePackage.FullName);
            Assert.IsTrue(executablePackage.Exists);
        }


     
    }


}