﻿// -----------------------------------------------------------------------
//  <copyright file="FunctionRepository.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using TelimenaClient;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    #region Using

    #endregion

    internal class UpdaterRepository : IUpdaterRepository
    {
        private readonly IAssemblyVersionReader versionReader;

        public UpdaterRepository(DbContext dbContext, IAssemblyVersionReader versionReader) 
        {
            this.versionReader = versionReader;
            this.TelimenaContext = dbContext as TelimenaContext;
        }

        protected TelimenaContext TelimenaContext { get; }
        private readonly string containerName = "toolkit-packages";

        public Task<Updater> GetUpdater(int id)
        {
            return this.TelimenaContext.Updaters.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Updater Add(string fileName, string internalName, TelimenaUser user)
        {
           var updater = new Updater(fileName, internalName);
            this.TelimenaContext.Updaters.Add(updater);
            updater.DeveloperAccount = user.AssociatedDeveloperAccounts.FirstOrDefault(x => x.MainUserId == user.Id);
            return updater;
        }

        public UpdaterPackageInfo GetPackageForVersion(Updater updater, string version)
        {
            ObjectValidator.Validate(() => Version.TryParse(version, out _), new ArgumentException($"[{version}] is not a valid version string"));
            return updater.Packages.FirstOrDefault(x => x.Version == version);
        }

        public Task<UpdaterPackageInfo> GetPackageInfo(int packageId)
        {
            return this.TelimenaContext.UpdaterPackages.FirstOrDefaultAsync(x => x.Id == packageId);
        }

        public async Task<IEnumerable<UpdaterPackageInfo>> GetPackages(string updaterInternalName)
        {
            return (await this.TelimenaContext.UpdaterPackages.Where(x => x.Updater.InternalName == updaterInternalName).ToListAsync()).AsReadOnly();
        }

        public async Task<IEnumerable<UpdaterPackageInfo>> GetAllPackages()
        {
            return (await this.TelimenaContext.UpdaterPackages.ToListAsync()).AsReadOnly();
        }

        public async Task<UpdaterPackageInfo> GetNewestCompatibleUpdater(Program program, string version, string toolkitVersion, bool includingBeta)
        {
            ObjectValidator.Validate(() => Version.TryParse(version, out _), new ArgumentException($"[{version}] is not a valid version string"));
            ObjectValidator.Validate(() => Version.TryParse(toolkitVersion, out _), new ArgumentException($"[{toolkitVersion}] is not a valid version string"));

            var updater = await this.GetUpdaterForProgram(program);

            //UpdaterPackageInfo current = updater.Packages.FirstOrDefault(x => x.Version == version);
            UpdaterPackageInfo current = updater.Packages.FirstOrDefault(x => x.Version == version);

            List<UpdaterPackageInfo> newerOnes;
            if (current != null)
            {
                newerOnes = updater.Packages.Where(x => x.Id > current.Id)
                    .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ToList();
            }
            else
            {
                newerOnes = updater.Packages.Where(x => x.Version.IsNewerVersionThan(version))
                    .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ToList();
            }

            if (newerOnes.Any())
            {
                List<UpdaterPackageInfo> compatibleOnes = newerOnes.Where(x => toolkitVersion.IsNewerOrEqualVersion(x.MinimumRequiredToolkitVersion)).ToList();
                if (includingBeta)
                {
                    return compatibleOnes.FirstOrDefault();
                }

                return compatibleOnes.FirstOrDefault(x => !x.IsBeta);
            }

            return null;
        }

        public async Task<IEnumerable<Updater>> GetPublicUpdaters()
        {
            return await this.TelimenaContext.Updaters.Where(x => x.IsPublic).ToListAsync();
        }

        public async Task<Updater> GetUpdater(string updaterInternalName)
        {
            var updater = await this.TelimenaContext.Updaters.FirstOrDefaultAsync(x => x.InternalName == updaterInternalName);
            if (updater == null && updaterInternalName == DefaultToolkitNames.UpdaterInternalName) //create the default updater
            {
                updater = new Updater(DefaultToolkitNames.UpdaterFileName, DefaultToolkitNames.UpdaterInternalName);
                updater.DeveloperAccount = await this.TelimenaContext.Developers.SingleOrDefaultAsync(x => x.Name == DefaultToolkitNames.TelimenaSystemDevTeam);
            }
            if (updater != null && updater.DeveloperAccount == null)
            {
                updater.DeveloperAccount = await this.TelimenaContext.Developers.SingleOrDefaultAsync(x => x.Name == DefaultToolkitNames.TelimenaSystemDevTeam);
            }

            return updater;

        }


        private async Task<Updater> GetUpdaterForProgram(Program program)
        {
            var updater = program.Updater;
            if (updater == null)
            {
                updater = await this.TelimenaContext.Updaters.FirstOrDefaultAsync(x => x.InternalName == DefaultToolkitNames.UpdaterInternalName);
                if (updater == null)
                {
                    throw new InvalidOperationException("Default updater not found!");
                }

                program.Updater = updater;

            }

            return updater;
        }

        public async Task<UpdaterPackageInfo> StorePackageAsync(Updater updater, string minimumRequiredToolkitVersion, Stream fileStream, IFileSaver fileSaver)
        {
            string actualVersion = await this.versionReader.GetFileVersion(fileStream, updater.FileName, true);
            fileStream.Position = 0;
            fileStream = await Utilities.EnsureStreamIsZipped(updater.FileName, fileStream);

            UpdaterPackageInfo pkg = new UpdaterPackageInfo(actualVersion, updater.FileName, fileStream.Length, minimumRequiredToolkitVersion);

            this.TelimenaContext.UpdaterPackages.Add(pkg);
            pkg.UpdaterId = updater.Id;
            await fileSaver.SaveFile(pkg, fileStream, this.containerName);

            return pkg;
        }

        

        public async Task<byte[]> GetPackage(int packageId, IFileRetriever fileRetriever)
        {
            UpdaterPackageInfo pkg = await this.TelimenaContext.UpdaterPackages.FirstOrDefaultAsync(x => x.Id == packageId);

            if (pkg != null)
            {
                return await fileRetriever.GetFile(pkg, this.containerName);
            }

            return null;
        }
    }
}