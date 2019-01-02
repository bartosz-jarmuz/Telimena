using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AutoMapper;
using Microsoft.Web.Http;
using MvcAuditLogger;
using Telimena.WebApp.Controllers.Api.V1.Helpers;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using TelimenaClient;
using TelimenaClient.Serializer;

namespace Telimena.WebApp.Controllers.Api.V1
{
    #region Using

    #endregion

    /// <summary>
    /// Controls program management related endpoints
    /// </summary>
    [TelimenaApiAuthorize(Roles = TelimenaRoles.Developer)]
    [RoutePrefix("api/v1/programs")]
    public partial class ProgramsController : ApiController
    {
        private readonly ITelimenaSerializer serializer;
        private readonly IFileSaver fileSaver;
        private readonly IFileRetriever fileRetriever;

        /// <summary>
        /// New instance
        /// </summary>
        /// <param name="work"></param>
        /// <param name="serializer"></param>
        /// <param name="fileSaver"></param>
        /// <param name="fileRetriever"></param>
        public ProgramsController(IProgramsUnitOfWork work, ITelimenaSerializer serializer, IFileSaver fileSaver, IFileRetriever fileRetriever)
        {
            this.serializer = serializer;
            this.fileSaver = fileSaver;
            this.fileRetriever = fileRetriever;
            this.Work = work;
        }

        private IProgramsUnitOfWork Work { get; }

        /// <summary>
        /// Register a new program in Telimena
        /// </summary>
        /// <param name = "request" ></param>
        /// <returns></returns>
        [Audit]
        [HttpPost, Route("", Name = Routes.Register)]
        public async Task<RegisterProgramResponse> Register(RegisterProgramRequest request)
        {
            try
            {
                if (!ApiRequestsValidator.IsRequestValid(request, out List<string> errors))
                {
                    return new RegisterProgramResponse(new BadRequestException(string.Join(", ", errors)));
                }

                if (await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == request.TelemetryKey) != null)
                {
                    return new RegisterProgramResponse(new BadRequestException($"Use different telemetry key"));
                }

                TelimenaUser user = await this.Work.Users.FirstOrDefaultAsync(x => x.UserName == this.User.Identity.Name);
                DeveloperAccount developerAccount = user.GetDeveloperAccountsLedByUser().FirstOrDefault();
                if (developerAccount == null)
                {
                    return new RegisterProgramResponse(new BadRequestException($"Cannot find developer account associated with user [{user.UserName}]"));
                }

                Program program = new Program(request.Name, request.TelemetryKey)
                {
                    Description = request.Description
                };
                developerAccount.AddProgram(program);

                var primaryAss = new ProgramAssembly()
                {
                    Name = Path.GetFileNameWithoutExtension(request.PrimaryAssemblyFileName),
                    Extension = Path.GetExtension(request.PrimaryAssemblyFileName)
                };
                program.PrimaryAssembly = primaryAss;

                this.Work.Programs.Add(program);

                await this.Work.CompleteAsync();

                program = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == request.TelemetryKey);
                var url = this.Url?.Link("Default", new { Controller = "ProgramManagement", Action = "Index", telemetryKey = program.TelemetryKey });
                return new RegisterProgramResponse(program.TelemetryKey, program.DeveloperAccount.Id, url);
            }
            catch (Exception ex)
            {
                return new RegisterProgramResponse(ex);
            }
        }


        /// <summary>
        /// Deletes the program with the specified key
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        [Audit]
        [HttpDelete, Route("{telemetryKey}", Name = Routes.Delete)]
        public async Task<IHttpActionResult> Delete(Guid telemetryKey)
        {
            var prg = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey);
            if (prg == null)
            {
                return this.BadRequest($"Program with key {telemetryKey} does not exist");
            }
            try
            {
                this.Work.Programs.Remove(prg);
                await this.Work.CompleteAsync();
            }
            catch (Exception ex)
            {
                return this.InternalServerError(new InvalidOperationException($"Error while deleting program {prg.Name} (Key: {telemetryKey})", ex));
            }
            return this.Ok($"Program {prg.Name} (Key: {telemetryKey}) deleted successfully");
        }

        /// <summary>
        /// Uploads a new program package
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        [Audit]
        [HttpPost, Route("{telemetryKey}/packages", Name = Routes.Upload)]
        public async Task<IHttpActionResult> Upload(Guid telemetryKey)
        {
            try
            {
                HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    var program = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey);

                    if (program == null)
                    {
                        return this.BadRequest("Failed to find corresponding program");
                    }

                    ProgramPackageInfo pkg = await this.Work.ProgramPackages.StorePackageAsync(program.Id, uploadedFile.InputStream, uploadedFile.FileName, this.fileSaver);
                    await this.Work.CompleteAsync();
                    return this.Ok(pkg.Id);
                }

                return this.BadRequest("Empty attachment");
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Downloads the latest (newest version) package of the specified program
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Audit]
        [HttpGet, Route("{telemetryKey}/packages/latest", Name = Routes.DownloadLatestProgramPackage)]
        public async Task<IHttpActionResult> DownloadLatestProgramPackage(Guid telemetryKey)
        {
            Program prg = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey);
            if (prg == null)
            {
                return this.BadRequest($"Program with key [{telemetryKey}] does not exist");
            }
            return await ProgramsControllerHelpers.GetDownloadLatestProgramPackageResponse(this.Work, prg.Id, this.fileRetriever);
        }

        /// <summary>
        /// User friendly URL for downloading latest program version
        /// </summary>
        /// <param name="developerName"></param>
        /// <param name="programName"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Audit]
        [HttpGet, Route("{developerName}/download/{programName}", Name =  Routes.DownloadApp)]
        public async Task<IHttpActionResult> DownloadApp(string developerName, string programName)
        {
            Program prg = await this.Work.Programs.FirstOrDefaultAsync(x => x.Name == programName);
            if (prg == null)
            {
                return this.BadRequest($"Program [{programName}] does not exist");
            }
            return await ProgramsControllerHelpers.GetDownloadLatestProgramPackageResponse(this.Work, prg.Id, this.fileRetriever);
        }

        /// <summary>
        /// Gets the latest version info for the specified program
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        [HttpGet, Route("{telemetryKey}/versions/latest", Name = Routes.GetLatestVersionInfo)]
        public async Task<LatestVersionResponse> GetLatestVersionInfo(Guid telemetryKey)
        {
            try
            {
                Program program = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey);
                if (program == null)
                {
                    return new LatestVersionResponse { Error = new InvalidOperationException($"Failed to find program by Key: [{telemetryKey}]") };
                }

                LatestVersionResponse info = new LatestVersionResponse
                {
                    PrimaryAssemblyVersion = ProgramsControllerHelpers.ConstructVersionInfo(program.PrimaryAssembly),
                    HelperAssemblyVersions = new List<VersionInfo>()
                };
                foreach (ProgramAssembly programAssembly in program.ProgramAssemblies.Where(x => x.PrimaryOf != program))
                {
                    info.HelperAssemblyVersions.Add(ProgramsControllerHelpers.ConstructVersionInfo(programAssembly));
                }

                return info;
            }
            catch (Exception ex)
            {
                return new LatestVersionResponse { Error = new InvalidOperationException("Error while processing registration request", ex) };
            }
        }

        /// <summary>
        /// Gets the total number of versions of this program
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, Route("{telemetryKey}/versions/count", Name = Routes.GetVersionsCount)]
        public async Task<IHttpActionResult> GetVersionsCount(Guid id)
        {
            Program prg = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == id);
            if (prg == null)
            {
                return this.BadRequest($"Program key [{id}] not found");
            }

            return this.Ok(prg.PrimaryAssembly?.Versions.Count);
        }

        /// <summary>
        /// Gets the name of the updater for the given program
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet, Route("{telemetryKey}/updater/name", Name= Routes.GetProgramUpdaterName)]
        public async Task<HttpResponseMessage> GetProgramUpdaterName(Guid telemetryKey)
        {
            Program program = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey);
            if (program == null)
            {
                throw new BadRequestException($"Program with Key [{telemetryKey}] does not exist");
            }

            var name = program.Updater?.FileName ?? DefaultToolkitNames.UpdaterFileName;

            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Content = new StringContent(name, System.Text.Encoding.UTF8, "text/plain");
            return resp;
        }

        /// <summary>
        /// Sets the updater to be used for updates of the specified program
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <param name="updaterId"></param>
        /// <returns></returns>
        [Audit]
        [HttpPut, Route("{telemetryKey}/updater/set-updater/{updaterId}", Name = Routes.SetUpdater)]
        public async Task<IHttpActionResult> SetUpdater(Guid telemetryKey, Guid updaterId)
        {
            Program prg = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey);
            if (prg == null)
            {
                return this.BadRequest($"Program with Key {telemetryKey} does not exist");
            }

            var updater = await this.Work.UpdaterRepository.GetUpdater(updaterId);
            if (updater == null)
            {
                return this.BadRequest($"Updater with Unique Id {updaterId} does not exist");
            }

            prg.Updater = updater;
            await this.Work.CompleteAsync();
            return this.Ok($"Updater set to {updater.InternalName}");
        }

        /// <summary>
        /// Gets the info about whether an update is available for the specified request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost, Route("update-check/{request}", Name = Routes.GetUpdateInfo)]
        public async Task<UpdateResponse> GetUpdateInfo(string request)
        {
            try
            {
                UpdateRequest requestModel = Utilities.ReadRequest(request, this.serializer);

                Program program = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == requestModel.TelemetryKey);
                if (program == null)
                {
                    return new UpdateResponse { Exception = new BadRequestException($"Failed to find program by Id: [{requestModel.TelemetryKey}]") };
                }

                List<ProgramUpdatePackageInfo> allUpdatePackages =
                    (await this.Work.UpdatePackages.GetAllPackagesNewerThan(requestModel.VersionData.Map(), program.Id))
                    .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ToList();

                List<ProgramUpdatePackageInfo> filteredPackages = ProgramsControllerHelpers.FilterPackagesSet(allUpdatePackages, requestModel);
                string supportedToolkitVersion = await ProgramsControllerHelpers.GetMaximumSupportedToolkitVersion(this.Work, filteredPackages, program, requestModel);
                TelimenaPackageInfo toolkitPackage = await ProgramsControllerHelpers.GetToolkitUpdateInfo(this.Work, program, requestModel, supportedToolkitVersion);

                List<UpdatePackageData> packageDataSets = new List<UpdatePackageData>();
                foreach (ProgramUpdatePackageInfo programUpdatePackageInfo in filteredPackages)
                {
                    packageDataSets.Add(Mapper.Map<ProgramUpdatePackageInfo, UpdatePackageData>(programUpdatePackageInfo
                        , options => options.AfterMap((info, data) => data.DownloadUrl = Router.Api.DownloadProgramUpdate(info))));
                }

                if (packageDataSets.Any())
                {
                    if (toolkitPackage != null)
                    {
                        packageDataSets.Add(Mapper.Map<TelimenaPackageInfo, UpdatePackageData>(toolkitPackage
                            , options => options.AfterMap((info, data) => data.DownloadUrl = Router.Api.DownloadToolkitUpdate(info))));
                    }
                }


                UpdateResponse response = new UpdateResponse { UpdatePackages = packageDataSets };

                return response;
            }
            catch (Exception ex)
            {
                return new UpdateResponse { Exception = new InvalidOperationException("Error while processing registration request", ex) };
            }
        }

    }
}