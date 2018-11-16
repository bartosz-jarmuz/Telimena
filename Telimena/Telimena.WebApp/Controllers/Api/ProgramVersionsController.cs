using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;

namespace Telimena.WebApp.Controllers.Api
{
    #region Using

    #endregion

    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class ProgramVersionsController : ApiController
    {
        public ProgramVersionsController(IProgramsUnitOfWork work)
        {
            this.Work = work;
        }

        private IProgramsUnitOfWork Work { get; }

        [HttpGet]
        public async Task<LatestVersionResponse> GetLatestVersionInfo(int id)
        {
            try
            {
                Program program = await this.Work.Programs.FirstOrDefaultAsync(x => x.Id == id);
                if (program == null)
                {
                    return new LatestVersionResponse {Error = new InvalidOperationException($"Failed to find program by Id: [{id}]")};
                }

                LatestVersionResponse info = new LatestVersionResponse
                {
                    PrimaryAssemblyVersion = this.ConstructVersionInfo(program.PrimaryAssembly), HelperAssemblyVersions = new List<VersionInfo>()
                };
                foreach (ProgramAssembly programAssembly in program.ProgramAssemblies.Where(x => x.PrimaryOf != program))
                {
                    info.HelperAssemblyVersions.Add(this.ConstructVersionInfo(programAssembly));
                }

                return info;
            }
            catch (Exception ex)
            {
                return new LatestVersionResponse {Error = new InvalidOperationException("Error while processing registration request", ex)};
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetVersionsCount(int id)
        {
            Program prg = await this.Work.Programs.FirstOrDefaultAsync(x => x.Id == id);
            if (prg == null)
            {
                return this.BadRequest($"Program [{id}] not found");
            }

            return this.Ok(prg.PrimaryAssembly.Versions.Count);
        }

        private VersionInfo ConstructVersionInfo(ProgramAssembly assemblyInfo)
        {
            return new VersionInfo
            {
                AssemblyId = assemblyInfo.Id
                , AssemblyName = assemblyInfo.Name
                , LatestVersion = assemblyInfo.GetLatestVersion()?.AssemblyVersion
                , LatestVersionId = assemblyInfo.GetLatestVersion()?.Id ?? 0
            };
        }
    }
}