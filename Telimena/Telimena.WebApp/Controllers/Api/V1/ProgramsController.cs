using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AutoMapper;
using DotNetLittleHelpers;
using Microsoft.ApplicationInsights;
using Microsoft.Web.Http;
using Newtonsoft.Json;
using Telimena.WebApp.Controllers.Api.V1.Helpers;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Utils.VersionComparison;

namespace Telimena.WebApp.Controllers.Api.V1
{
    /// <summary>
    /// Controls program management related endpoints
    /// </summary>
    [TelimenaApiAuthorize(Roles = TelimenaRoles.Developer)]
    [RoutePrefix("api/v{version:apiVersion}/programs")]
    public partial class ProgramsController : ApiController
    {
        private readonly IFileSaver fileSaver;
        private readonly IFileRetriever fileRetriever;
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// New instance
        /// </summary>
        /// <param name="work"></param>
        /// <param name="fileSaver"></param>
        /// <param name="fileRetriever"></param>
        public ProgramsController(IProgramsUnitOfWork work, IFileSaver fileSaver, IFileRetriever fileRetriever)
        {
            this.fileSaver = fileSaver;
            this.fileRetriever = fileRetriever;
            this.telemetryClient = new TelemetryClient();
            this.Work = work;
        }

        private IProgramsUnitOfWork Work { get; }

        /// <summary>
        /// Deletes the program with the specified key
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>

        [HttpDelete, Route("{telemetryKey}", Name = Routes.Delete)]
        public async Task<IHttpActionResult> Delete(Guid telemetryKey)
        {
            var prg = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (prg == null)
            {
                return this.BadRequest($"Program with key {telemetryKey} does not exist");
            }
            try
            {
                this.Work.Programs.Remove(prg);
                await this.Work.CompleteAsync().ConfigureAwait(false);
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

        [HttpPost, Route("{telemetryKey}/packages", Name = Routes.Upload)]
        public async Task<IHttpActionResult> Upload(Guid telemetryKey)
        {
            try
            {
                HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    var program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);

                    if (program == null)
                    {
                        return this.BadRequest("Failed to find corresponding program");
                    }
                    this.telemetryClient.TrackEvent("UploadedPackage", new Dictionary<string, string>()
                    {
                        {$"ProgramName", program.Name },
                        {$"UploadedFileName", uploadedFile?.FileName},
                    });
                    ProgramPackageInfo pkg = await this.Work.ProgramPackages.StorePackageAsync(program, uploadedFile.InputStream, uploadedFile.FileName, this.fileSaver).ConfigureAwait(false);
                    await this.Work.CompleteAsync().ConfigureAwait(false);
                    return this.Ok(pkg.PublicId);
                }

                return this.BadRequest("Empty attachment");
            }
            catch (Exception ex)
            {
                this.telemetryClient.TrackException(ex, new Dictionary<string, string>()
                {
                    {$"Method",  Routes.Upload},
                    {$"{nameof(Program.TelemetryKey)}", telemetryKey.ToString()}
                });
                return this.BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Downloads the latest (newest version) package of the specified program
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        [AllowAnonymous]

        [HttpGet, Route("{telemetryKey}/packages/latest", Name = Routes.DownloadLatestProgramPackage)]
        public async Task<IHttpActionResult> DownloadLatestProgramPackage(Guid telemetryKey)
        {
            Program prg = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (prg == null)
            {
                return this.BadRequest($"Program with key [{telemetryKey}] does not exist");
            }
            return await ProgramsControllerHelpers.GetDownloadLatestProgramPackageResponse(this.Work, prg.Id, this.fileRetriever).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the latest version info for the specified program
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        [HttpGet, Route("{telemetryKey}/versions/latest", Name = Routes.GetLatestVersionInfo)]
        public async Task<string> GetLatestVersionInfo(Guid telemetryKey)
        {
            try
            {
                Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
                if (program == null)
                {
                    return $"Failed to find program by Key: [{telemetryKey}]";
                }

                var packageInfo = await this.Work.UpdatePackages.GetLatestPackage(program.Id).ConfigureAwait(false);

                if (packageInfo == null)
                {
                    return "0.0";
                }

                return packageInfo.Version;
            }
            catch (Exception ex)
            {
                return $"Error while processing registration request: {ex.Message}";
            }
        }

        /// <summary>
        /// Gets the total number of versions of this program
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        [HttpGet, Route("{telemetryKey}/versions/count", Name = Routes.GetVersionsCount)]
        public async Task<IHttpActionResult> GetVersionsCount(Guid telemetryKey)
        {
            Program prg = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (prg == null)
            {
                return this.BadRequest($"Program key [{telemetryKey}] not found");
            }

            List<ProgramUpdatePackageInfo> packages = await this.Work.UpdatePackages.GetAllPackages(prg.Id).ConfigureAwait(false);

            return this.Ok(packages?.Count);
        }

        /// <summary>
        /// Gets the User tracking settings
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet, Route("{telemetryKey}/telemetry-settings", Name = Routes.GetTelemetrySettings)]
        public async Task<UserTrackingSettings> GetTelemetrySettings(Guid telemetryKey)
        {
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (program == null)
            {
                throw new BadRequestException($"Program with Key [{telemetryKey}] does not exist");
            }
            var storedSettings = program.UserTrackingSettings;
            if (storedSettings != null)
            {
                try
                {
                    UserTrackingSettings deserialized = JsonConvert.DeserializeObject<UserTrackingSettings>(storedSettings);
                    return deserialized;
                }
                catch (Exception ex)
                {
                    this.telemetryClient.TrackException(new InvalidOperationException($"ERROR while deserializing {nameof(UserTrackingSettings)}", ex), new Dictionary<string, string>()
                    {
                        {$"Method", Routes.GetTelemetrySettings},
                        {$"{nameof(Program.TelemetryKey)}", telemetryKey.ToString()}
                    });
                }
            }
            return new UserTrackingSettings()
            {
                UserIdentifierMode = UserIdentifierMode.RandomFriendlyName,
                ShareIdentifierWithOtherTelimenaApps = false
            };
        }


        /// <summary>
        /// Gets the User tracking settings
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        [HttpPost, Route("{telemetryKey}/telemetry-settings", Name = Routes.SetTelemetrySettings)]
        public async Task<IHttpActionResult> SetTelemetrySettings(Guid telemetryKey, UserTrackingSettings settings)
        {
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (program == null)
            {
                throw new BadRequestException($"Program with Key [{telemetryKey}] does not exist");
            }

            var serialized = JsonConvert.SerializeObject(settings);

            program.UserTrackingSettings = serialized;

            await this.Work.CompleteAsync().ConfigureAwait(false);
            return this.Ok("Updated user tracking settings");

        }

        /// <summary>
        /// Gets the AppInsights instrumentation key
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet, Route("{telemetryKey}/instrumentation-key", Name = Routes.GetInstrumentationKey)]
        public async Task<IHttpActionResult> GetInstrumentationKey(Guid telemetryKey)
        {
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (program == null)
            {
                throw new BadRequestException($"Program with Key [{telemetryKey}] does not exist");
            }

            if (program.InstrumentationKey != null)
            {
                return this.Ok(program.InstrumentationKey);
            }

            return this.Ok();
        }

        /// <summary>
        /// Gets the AppInsights instrumentation key
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <param name="instrumentationKey"></param>
        /// <returns></returns>
        [HttpPost, Route("{telemetryKey}/instrumentation-key", Name = Routes.SetInstrumentationKey)]
        public async Task<IHttpActionResult> SetInstrumentationKey(Guid telemetryKey, string instrumentationKey)
        {
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (program == null)
            {
                throw new BadRequestException($"Program with Key [{telemetryKey}] does not exist");
            }

            program.InstrumentationKey = instrumentationKey;
            await this.Work.CompleteAsync().ConfigureAwait(false);
            return this.Ok();
        }

        /// <summary>
        /// Gets the name of the updater for the given program
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet, Route("{telemetryKey}/updater/name", Name = Routes.GetProgramUpdaterName)]
        public async Task<HttpResponseMessage> GetProgramUpdaterName(Guid telemetryKey)
        {
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
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

        [HttpPut, Route("{telemetryKey}/updater/set-updater/{updaterId}", Name = Routes.SetUpdater)]
        public async Task<IHttpActionResult> SetUpdater(Guid telemetryKey, Guid updaterId)
        {
            Program prg = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (prg == null)
            {
                return this.BadRequest($"Program with Key {telemetryKey} does not exist");
            }

            var updater = await this.Work.UpdaterRepository.GetUpdater(updaterId).ConfigureAwait(false);
            if (updater == null)
            {
                return this.BadRequest($"Updater with Unique Id {updaterId} does not exist");
            }

            prg.Updater = updater;
            await this.Work.CompleteAsync().ConfigureAwait(false);
            return this.Ok($"Updater set to {updater.InternalName}");
        }

        /// <summary>
        /// Gets the info about whether an update is available for the specified request
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost, Route("update-check", Name = Routes.UpdateCheck)]
        public async Task<UpdateResponse> UpdateCheck(UpdateRequest requestModel)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                Program program = await this.Work.Programs.GetByTelemetryKey(requestModel.TelemetryKey).ConfigureAwait(false);

                if (program == null)
                {
                    return new UpdateResponse { Exception = new BadRequestException($"Failed to find program by Id: [{requestModel.TelemetryKey}]") };
                }



                Trace.TraceInformation($"Program {program.GetNameAndIdString()} checking for updates with request: {requestModel.GetPropertyInfoString()}");
                List<ProgramUpdatePackageInfo> allUpdatePackages =
                    (await this.Work.UpdatePackages.GetAllPackagesNewerThan(requestModel.VersionData, program.Id).ConfigureAwait(false))
                    .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ToList();
                Trace.TraceInformation($"Found {allUpdatePackages.Count} packages");

                List<ProgramUpdatePackageInfo> filteredPackages = ProgramsControllerHelpers.FilterPackagesSet(allUpdatePackages, requestModel);
                //todo - for now updating the package is disabled - update packagaces are expected to contain the toolkit package
                //  string supportedToolkitVersion = await ProgramsControllerHelpers.GetMaximumSupportedToolkitVersion(this.Work, filteredPackages, program, requestModel).ConfigureAwait(false);
                // TelimenaPackageInfo toolkitPackage = await ProgramsControllerHelpers.GetToolkitUpdateInfo(this.Work, program, requestModel, supportedToolkitVersion).ConfigureAwait(false);
                Trace.TraceInformation($"Filtered {filteredPackages.Count} packages");

                List<UpdatePackageData> packageDataSets = new List<UpdatePackageData>();
                foreach (ProgramUpdatePackageInfo programUpdatePackageInfo in filteredPackages)
                {
                    packageDataSets.Add(Mapper.Map<ProgramUpdatePackageInfo, UpdatePackageData>(programUpdatePackageInfo
                        , options => options.AfterMap((info, data) => data.DownloadUrl = Router.Api.DownloadProgramUpdate(info))));
                }

                //if (packageDataSets.Any()) //disabled for now (lets see when I look at this comment next time - it's 18.04.2019...)
                //{
                //    if (toolkitPackage != null)
                //    {
                //        packageDataSets.Add(Mapper.Map<TelimenaPackageInfo, UpdatePackageData>(toolkitPackage
                //            , options => options.AfterMap((info, data) => data.DownloadUrl = Router.Api.DownloadToolkitUpdate(info))));
                //    }
                //}


                UpdateResponse response = new UpdateResponse { UpdatePackages = packageDataSets };
                sw.Stop();
                this.telemetryClient.TrackEvent("UpdateCheck", new Dictionary<string, string>()
                {
                    {$"ProgramName", program.Name },
                    {$"ExecutionTime", sw.ElapsedMilliseconds.ToString()},
                    {$"UpdateRequest", requestModel.GetPropertyInfoString() },
                });
                return response;
            }
            catch (Exception ex)
            {
                this.telemetryClient.TrackException(ex, new Dictionary<string, string>()
                {
                    {$"Method", Routes.UpdateCheck},
                    {$"{nameof(Program.TelemetryKey)}", requestModel.TelemetryKey.ToString()}
                });
                return new UpdateResponse { Exception = new InvalidOperationException("Error while processing registration request", ex) };
            }
        }




        /// <summary>
        /// User friendly URL for downloading latest program version
        /// </summary>
        /// <param name="developerName"></param>
        /// <param name="programName"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [ApiVersionNeutral]

        [HttpGet, Route("~/{developerName}/{programName}/download", Name = ProgramsController.Routes.DownloadApp)]
        public async Task<IHttpActionResult> DownloadApp(string developerName, string programName)
        {
            Program prg = await this.Work.Programs.GetByNames(developerName, programName).ConfigureAwait(false);
            if (prg == null)
            {
                return this.BadRequest($"Program [{programName}] does not exist");
            }
            return await ProgramsControllerHelpers.GetDownloadLatestProgramPackageResponse(this.Work, prg.Id, this.fileRetriever).ConfigureAwait(false);
        }

        /// <summary>
        /// Clears all telemetry data
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        [HttpPost, Route("{telemetryKey}/clear-all-telemetry", Name = Routes.ClearAllTelemetryData)]
        public async Task<IHttpActionResult> ClearAllTelemetryData(Guid telemetryKey)
        {
            return await ClearTelemetryData(telemetryKey, null);
        }

        /// <summary>
        /// Clears all events telemetry data
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        [HttpGet, Route("{telemetryKey}/clear-telemetry", Name = Routes.ClearTelemetryData)]
        public async Task<IHttpActionResult> ClearTelemetryData(Guid telemetryKey, TelemetryItemTypes? dataType)
        {
            var prg = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (prg == null)
            {
                return this.BadRequest($"Program with key {telemetryKey} does not exist");
            }
            try
            {
                this.telemetryClient.TrackEvent("AttemptedToClearTelemetryData", new Dictionary<string, string>()
                    {
                        {$"ProgramName", prg.Name },
                        {$"DataType", dataType?.ToString()??"ALL" },
                    });
                this.Work.Programs.ClearTelemetryData(prg, dataType);

                await this.Work.CompleteAsync(TimeSpan.FromMinutes(5)).ConfigureAwait(false);
                this.telemetryClient.TrackEvent("ClearedTelemetryData", new Dictionary<string, string>()
                    {
                        {$"ProgramName", prg.Name },
                          {$"DataType", dataType?.ToString()??"ALL" },
                    });
            }
            catch (Exception ex)
            {
                var props = new Dictionary<string, string>()
                    {
                        {$"ProgramName", prg.Name },
                        {$"Exception", ex.ToString()},
                          {$"DataType", dataType?.ToString()??"ALL" },

                    };
                var wrapper = new InvalidOperationException($"Error while clearing telemetry data for {prg.Name} (Key: {telemetryKey})", ex);

                this.telemetryClient.TrackEvent($"TelemetryDataClearingError", props);
                this.telemetryClient.TrackException(wrapper, props);
                this.telemetryClient.Flush();
                return this.InternalServerError(wrapper);
            }
            return this.Ok($"{dataType?.ToString() ?? "ALL" } Telemetry data cleared.");
        }



    }
}