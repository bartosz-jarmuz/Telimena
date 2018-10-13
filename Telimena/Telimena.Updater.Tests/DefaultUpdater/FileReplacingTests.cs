// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Telimena.Updater;
using Assert = NUnit.Framework.Assert;

namespace TelimenaUpdaterTests.DefaultUpdater
{
    #region Using

    #endregion

    [TestFixture]
    [DeploymentItem("MockDisk")]
    public class FileReplacingTests
    {
        private DirectoryInfo MyAppFolder => new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockDisk", "DefaultUpdater/MyAppFolder"));
        private DirectoryInfo Update12Folder => this.MyAppFolder.CreateSubdirectory(Path.Combine("Updates", "1.2"));

        private void CreateBackup()
        {
            string backupPath = this.MyAppFolder.FullName + "_Backup";
            if (!Directory.Exists(backupPath))
            {
                DirectoryCopy.Copy(this.MyAppFolder.FullName, backupPath);
            }
        }

        private void Cleanup()
        {
            string backupPath = this.MyAppFolder.FullName + "_Backup";
            if (!Directory.Exists(backupPath))
            {
                throw new DirectoryNotFoundException("Backup folder missing: " + backupPath);
            }

            Directory.Delete(this.MyAppFolder.FullName, true);

            DirectoryCopy.Copy(backupPath, this.MyAppFolder.FullName);
        }

        private void PreTestAsserts()
        {
            Assert.IsFalse(File.Exists(Path.Combine(this.MyAppFolder.FullName, "SomeLibSubfolder", "SomeOtherSub", "SomeLibFromSomeSubfolder.dll")));
            Assert.IsFalse(File.Exists(Path.Combine(this.MyAppFolder.FullName, "SomeLib2.dll")));

            Assert.AreNotEqual("SomeLib1NEW", File.ReadAllText(Path.Combine(this.MyAppFolder.FullName, "SomeLib1.dll")));
            Assert.AreNotEqual("SomeLib3NEW", File.ReadAllText(Path.Combine(this.MyAppFolder.FullName, "SomeLib3.dll")));
            Assert.AreNotEqual("SomeLib3NEW", File.ReadAllText(Path.Combine(this.MyAppFolder.FullName, "Libs", "SomeLib3.dll")));
            Assert.AreNotEqual("MyAppNEW", File.ReadAllText(Path.Combine(this.MyAppFolder.FullName, "MyApp.exe")));
            Assert.AreNotEqual("Some Nested LibNEW", File.ReadAllText(Path.Combine(this.MyAppFolder.FullName, "Libs", "Some Nested Lib.dll")));
        }

        private void PostTestAsserts()
        {
            Assert.AreEqual("SomeLibFromSomeSubfolderNEW"
                , File.ReadAllText(Path.Combine(this.MyAppFolder.FullName, "SomeLibSubfolder", "SomeOtherSub", "SomeLibFromSomeSubfolder.dll")));
            Assert.AreEqual("SomeLib1NEW", File.ReadAllText(Path.Combine(this.MyAppFolder.FullName, "SomeLib1.dll")));
            Assert.AreEqual("SomeLib2NEW", File.ReadAllText(Path.Combine(this.MyAppFolder.FullName, "SomeLib2.dll")));

            Assert.AreEqual("SomeLib3NEW\r\n", File.ReadAllText(Path.Combine(this.MyAppFolder.FullName, "SomeLib3.dll")));
            Assert.AreEqual("SomeLib3NEW\r\n", File.ReadAllText(Path.Combine(this.MyAppFolder.FullName, "Libs", "SomeLib3.dll")));

            Assert.AreEqual("MyAppNEW", File.ReadAllText(Path.Combine(this.MyAppFolder.FullName, "MyApp.exe")));
            Assert.AreEqual("Some Nested LibNEW", File.ReadAllText(Path.Combine(this.MyAppFolder.FullName, "Libs", "Some Nested Lib.dll")));
        }

        [Test]
        public void Test_Worker()
        {
            this.CreateBackup();
            try
            {
                this.PreTestAsserts();

                UpdateWorker worker = new UpdateWorker();
                worker.PerformUpdate(new UpdateInstructions
                {
                    PackagePaths = new List<string>
                    {
                        Path.Combine(this.Update12Folder.FullName, "MyApp Update v. 1.1.zip")
                        , Path.Combine(this.Update12Folder.FullName, "MyApp Update v. 1.2.zip")
                    }
                });
                this.PostTestAsserts();
            }

            finally
            {
                this.Cleanup();
            }
        }
    }
}