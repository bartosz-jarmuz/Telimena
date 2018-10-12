using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TelimenaClient
{
    internal partial class UpdateHandler
    {
        internal class Locator
        {
            private readonly LiveProgramInfo programInfo;
            private readonly string updatesFolderName;


            public Locator(LiveProgramInfo programInfo) : this(programInfo, AppDomain.CurrentDomain.BaseDirectory)
            {
                
            }

            internal Locator(LiveProgramInfo programInfo, string basePath)
            {
                this.programInfo = programInfo;
                this.updatesFolderName = this.programInfo.Program.Name + " Updates";
                this.telimenaWorkingDirectory = new Lazy<DirectoryInfo>(() =>
                {
                    if (!basePath.IsDirectoryWritable())
                    {
                        var dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        var dirInfo = new DirectoryInfo(Path.Combine(dir, "Telimena", this.programInfo.Program.Name));
                        dirInfo.Create();
                        return dirInfo;
                    }
                    else
                    {
                        return new DirectoryInfo(basePath);
                    }
                });
            }


            /// <summary>
            /// Gets the telimena working directory - should be the program path, but it might be not write-accessible, in which case try working from my documents
            /// </summary>
            /// <returns>DirectoryInfo.</returns>
            private readonly Lazy<DirectoryInfo> telimenaWorkingDirectory;

            public DirectoryInfo GetUpdatesParentFolder()
            {
                return new DirectoryInfo(Path.Combine(this.telimenaWorkingDirectory.Value.FullName, this.updatesFolderName));
            }

            public FileInfo GetUpdater()
            {
                return new FileInfo(Path.Combine(this.GetUpdatesParentFolder().FullName, this.programInfo.UpdaterName));
            }

            public DirectoryInfo GetCurrentUpdateSubfolder(IEnumerable<UpdatePackageData> packagesToDownload)
            {
                DirectoryInfo dir = this.GetUpdatesParentFolder();
                UpdatePackageData latestPkg = packagesToDownload.OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).First();
                return new DirectoryInfo(Path.Combine(dir.FullName, latestPkg.Version));
            }
        }

      
    }
}