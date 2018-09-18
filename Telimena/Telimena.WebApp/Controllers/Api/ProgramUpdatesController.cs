using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AutoMapper;
using DotNetLittleHelpers;
using Newtonsoft.Json;
using Telimena.Client;
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
    public class ProgramUpdatesController : ApiController
    {
        public ProgramUpdatesController(IProgramsUnitOfWork work, ITelimenaSerializer serializer)
        {
            this.work = work;
            this.serializer = serializer;
        }

        private readonly IProgramsUnitOfWork work;
        private readonly ITelimenaSerializer serializer;

        [AllowAnonymous]
        [HttpGet]
        public async Task<IHttpActionResult> DownloadUpdatePackage(int id)
        {
            ProgramUpdatePackageInfo packageInfo = await this.work.UpdatePackages.GetUpdatePackageInfo(id);

            byte[] bytes = await this.work.UpdatePackages.GetPackage(packageInfo.Id);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {FileName = packageInfo.FileName};
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return this.ResponseMessage(result);
        }

        public async Task<TelimenaPackageInfo> GetToolkitUpdateInfo(Program program, UpdateRequest request, string maximumSupportedToolkitVersion)
        {
            ObjectValidator.Validate(() => Version.TryParse(request.ToolkitVersion, out _)
                , new ArgumentException($"[{request.ToolkitVersion}] is not a valid version string"));

            List<TelimenaPackageInfo> packages = (await this.work.ToolkitData.GetPackagesNewerThan(request.ToolkitVersion))
                .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ToList();

            if (!request.AcceptBeta)
            {
                packages.RemoveAll(x => x.IsBeta);
            }

            if (packages.Any(x => x.IntroducesBreakingChanges))
            {
                packages.Reverse();
                List<TelimenaPackageInfo> listOfCompatiblePackages = new List<TelimenaPackageInfo>();
                foreach (TelimenaPackageInfo package in packages)
                {
                    if (!package.IntroducesBreakingChanges)
                    {
                        listOfCompatiblePackages.Add(package);
                    }
                    else
                    {
                        if (maximumSupportedToolkitVersion.IsNewerOrEqualVersion(package.Version))
                        {
                            listOfCompatiblePackages.Add(package);
                        }
                    }
                }

                return listOfCompatiblePackages.LastOrDefault();
            }

            return packages.FirstOrDefault();

            return null;
        }

        [HttpGet]
        public async Task<UpdateResponse> GetUpdateInfo(string request)
        {
            try
            {
                UpdateRequest requestModel = ReadRequest(request, this.serializer);
                Program program = await this.work.Programs.FirstOrDefaultAsync(x => x.Id == requestModel.ProgramId);
                if (program == null)
                {
                    return new UpdateResponse {Exception = new BadRequestException($"Failed to find program by Id: [{requestModel.ProgramId}]")};
                }

                List<ProgramUpdatePackageInfo> allUpdatePackages =
                    (await this.work.UpdatePackages.GetAllPackagesNewerThan(requestModel.ProgramVersion, program.Id))
                    .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ToList();

                List<ProgramUpdatePackageInfo> filteredPackages = this.FilterPackagesSet(allUpdatePackages, requestModel);
                string supportedToolkitVersion = await this.GetMaximumSupportedToolkitVersion(filteredPackages, program, requestModel);
                TelimenaPackageInfo toolkitPackage = await this.GetToolkitUpdateInfo(program, requestModel, supportedToolkitVersion);
                List<UpdatePackageData> packageDataSets = new List<UpdatePackageData>(Mapper.Map<IEnumerable<UpdatePackageData>>(filteredPackages));
                if (toolkitPackage != null)
                {
                    packageDataSets.Add(Mapper.Map<UpdatePackageData>(toolkitPackage));
                }


                UpdateResponse response = new UpdateResponse {UpdatePackages = packageDataSets};

                return response;
            }
            catch (Exception ex)
            {
                return new UpdateResponse {Exception = new InvalidOperationException("Error while processing registration request", ex)};
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IHttpActionResult> GetUpdateInfo(int programId)
        {
            ProgramUpdatePackageInfo packageInfo = await this.work.UpdatePackages.GetUpdatePackageInfo(programId);

            byte[] bytes = await this.work.UpdatePackages.GetPackage(packageInfo.Id);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {FileName = packageInfo.FileName};
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return this.ResponseMessage(result);
        }

        [HttpPost]
        public async Task<bool> ToggleBetaSetting(int updatePackageId, bool isBeta)
        {
            ProgramUpdatePackageInfo pkg = await this.work.UpdatePackages.FirstOrDefaultAsync(x => x.Id == updatePackageId);
            pkg.IsBeta = isBeta;
            await this.work.CompleteAsync();
            return pkg.IsBeta;
        }

        [HttpPost]
        public async Task<IHttpActionResult> UploadUpdatePackage()
        {
            try
            {
                string reqString = HttpContext.Current.Request.Form["Model"];
                CreateUpdatePackageRequest request = JsonConvert.DeserializeObject<CreateUpdatePackageRequest>(reqString);
                HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    //todo - pass supported toolkit version
                    Program program = await this.work.Programs.FirstOrDefaultAsync(x => x.Id == request.ProgramId);
                    ProgramUpdatePackageInfo pkg =
                        await this.work.UpdatePackages.StorePackageAsync(program, request.PackageVersion, uploadedFile.InputStream, "0.0.0.0");
                    await this.work.CompleteAsync();
                    return this.Ok(pkg.Id);
                }

                return this.BadRequest("Empty attachment");
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IHttpActionResult ValidatePackageVersion(CreateUpdatePackageRequest request)
        {
            if (!ApiRequestsValidator.IsRequestValid(request, out List<string> errors))
            {
                return this.BadRequest(string.Join(", ", errors));
            }

            return this.Ok();
        }

        internal static UpdateRequest ReadRequest(string escapedJsonString, ITelimenaSerializer serializer)
        {
            string json = serializer.UrlDecodeJson(escapedJsonString);
            UpdateRequest model = serializer.Deserialize<UpdateRequest>(json);
            return model;
        }

        private List<ProgramUpdatePackageInfo> FilterPackagesSet(List<ProgramUpdatePackageInfo> updatePackages, UpdateRequest request)
        {
            if (updatePackages.IsNullOrEmpty())
            {
                return new List<ProgramUpdatePackageInfo>();
            }

            if (!request.AcceptBeta)
            {
                updatePackages.RemoveAll(x => x.IsBeta);
                if (updatePackages.IsNullOrEmpty())
                {
                    return new List<ProgramUpdatePackageInfo>();
                }
            }

            ProgramUpdatePackageInfo newestPackage = updatePackages.First();
            if (newestPackage.IsStandalone)
            {
                return new List<ProgramUpdatePackageInfo> {newestPackage};
            }

            List<ProgramUpdatePackageInfo> list = new List<ProgramUpdatePackageInfo>();
            foreach (ProgramUpdatePackageInfo updatePackageInfo in updatePackages)
            {
                list.Add(updatePackageInfo);
                if (updatePackageInfo.IsStandalone)
                {
                    break;
                }
            }

            return list;
        }

        private async Task<string> GetMaximumSupportedToolkitVersion(List<ProgramUpdatePackageInfo> updatePackages, Program program
            , UpdateRequest updateRequest)
        {
            ProgramUpdatePackageInfo newestPackage = updatePackages.OrderByDescending(x => x.Id).FirstOrDefault();
            if (newestPackage != null)
            {
                return newestPackage.SupportedToolkitVersion;
            }

            //no updates now, so figure out what version is supported by the client already
            ProgramUpdatePackageInfo previousPackage =
                await this.work.UpdatePackages.FirstOrDefaultAsync(x => x.ProgramId == program.Id && x.Version == updateRequest.ProgramVersion);
            if (previousPackage != null)
            {
                return previousPackage.SupportedToolkitVersion;
            }

            return (await this.work.ProgramPackages.FirstOrDefaultAsync(x => x.ProgramId == program.Id)).SupportedToolkitVersion;
        }
    }
}