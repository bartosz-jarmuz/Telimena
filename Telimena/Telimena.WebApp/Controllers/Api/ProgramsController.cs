namespace Telimena.WebApi.Controllers
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using Client;
    using Newtonsoft.Json;
    using WebApp.Core.Interfaces;
    using WebApp.Core.Messages;
    using WebApp.Core.Models;
    using WebApp.Infrastructure;
    using WebApp.Infrastructure.Security;
    using WebApp.Infrastructure.UnitOfWork;
    #endregion

    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class ProgramsController : ApiController
    {
        public ProgramsController(IProgramsUnitOfWork work)
        {
            this._work = work;
        }

        [System.Web.Http.HttpGet]
        public async Task<LatestVersionResponse> GetLatestVersionInfo(int programId)
        {
            try
            {
                Program program = await this._work.Programs.FirstOrDefaultAsync(x => x.ProgramId == programId);
                if (program == null)
                {
                    return new LatestVersionResponse()
                    {
                        Error = new InvalidOperationException($"Failed to find program by Id: [{programId}]")
                    };
                }

                return await this.CreateUpdateResponse(program);
            }
            catch (Exception ex)
            {
                return new LatestVersionResponse()
                {
                    Error = new InvalidOperationException("Error while processing registration request", ex)
                };
            }
        }


        private readonly IProgramsUnitOfWork _work;

        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> SetLatestVersion(SetLatestVersionRequest request)
        {
            if (!ApiRequestsValidator.IsRequestValid(request))
            {
                return this.BadRequest($"SetLatestVersionRequest is invalid");
            }

            Program prg = await this._work.Programs.FirstOrDefaultAsync(x => x.ProgramId == request.ProgramId);
            if (prg == null)
            {
                return this.BadRequest($"Program [{request.ProgramId}] not found");
            }

            if (!Version.TryParse(request.Version, out Version _))
            {
                return this.BadRequest($"Version [{request.Version}] is not in valid format. Expected e.g. 1.0.0.0");
            }

            prg.PrimaryAssembly.SetLatestVersion(request.Version);
            await this._work.CompleteAsync();
            return this.Ok();
        }

        [System.Web.Http.HttpGet]
        public async Task<IHttpActionResult> GetVersionsCount(int programId)
        {
            Program prg = await this._work.Programs.FirstOrDefaultAsync(x => x.ProgramId == programId);
            if (prg == null)
            {
                return this.BadRequest($"Program [{programId}] not found");
            }

            return this.Ok(prg.PrimaryAssembly.Versions.Count);
        }

        [System.Web.Http.HttpGet]
        public async Task<IEnumerable<Program>> GetPrograms(int developerId)
        {
            return await this._work.Programs.GetAsync(x => x.DeveloperAccount.Id == developerId);
        }

        private async Task<LatestVersionResponse> CreateUpdateResponse(Program program)
        {
            LatestVersionResponse info = new LatestVersionResponse()
            {
                PrimaryAssemblyVersion = this.GetVersionInfo(program.PrimaryAssembly),
                HelperAssemblyVersions = new List<VersionInfo>()
            };
            foreach (ProgramAssembly programAssembly in program.ProgramAssemblies.Where(x => x.PrimaryOf != program))
            {
                info.HelperAssemblyVersions.Add(this.GetVersionInfo(programAssembly));
            }
            TelimenaToolkitData toolkitData = await this._work.TelimenaToolkitData.GetLatestToolkitData();
            info.LatestTelimenaVersion = toolkitData.Version;
            info.IsTelimenaVersionBeta = toolkitData.IsBetaVersion;
            return info;
        }

        private VersionInfo GetVersionInfo(ProgramAssembly assemblyInfo)
        {
            return new VersionInfo()
            {
                AssemblyId = assemblyInfo.ProgramAssemblyId,
                AssemblyName = assemblyInfo.Name,
                LatestVersion = assemblyInfo.LatestVersion?.Version,
                IsBeta = assemblyInfo.LatestVersion?.IsBeta ?? false,
                LatestVersionId = assemblyInfo.LatestVersion?.Id??0
            };
        }

        public IHttpActionResult ValidatePackageVersion(CreateUpdatePackageRequest request)
        {
            if (!ApiRequestsValidator.IsRequestValid(request, out List<string> errors))
            {
                return this.BadRequest(string.Join(", ", errors));
            }

            return this.Ok();
        }

        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> UploadUpdatePackage()
        {
            try
            {
                var reqString = HttpContext.Current.Request.Form["Model"];
                CreateUpdatePackageRequest request = JsonConvert.DeserializeObject<CreateUpdatePackageRequest>(reqString);
                HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ?
                    HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    byte[] fileByteArray = new byte[uploadedFile.ContentLength];
                    uploadedFile.InputStream.Read(fileByteArray, 0, uploadedFile.ContentLength);
                    Attachment newAttchment = new Attachment(uploadedFile.InputStream, uploadedFile.ContentType)
                    {
                        Name = uploadedFile.FileName
                    };
                    var pkg = await this._work.UpdatePackages.StorePackage(request.ProgramId, request.PackageVersion, null, uploadedFile.FileName);

                    await this._work.CompleteAsync();
                    return this.Ok(pkg.Id);
                }

                return this.BadRequest("Empty attachment");
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }
    }
}