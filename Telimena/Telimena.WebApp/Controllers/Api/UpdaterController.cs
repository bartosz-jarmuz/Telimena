using System;
using System.IdentityModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AutoMapper;
using MvcAuditLogger;
using Newtonsoft.Json;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using TelimenaClient;
using TelimenaClient.Serializer;

namespace Telimena.WebApp.Controllers.Api
{
    [TelimenaApiAuthorize(Roles = TelimenaRoles.Admin)]
    public class UpdaterController : ApiController
    {
        public UpdaterController(IToolkitDataUnitOfWork work, ITelimenaSerializer serializer, IFileSaver fileSaver, IFileRetriever fileRetriever)
        {
            this.work = work;
            this.serializer = serializer;
            this.fileSaver = fileSaver;
            this.fileRetriever = fileRetriever;
        }

        private readonly IToolkitDataUnitOfWork work;
        private readonly ITelimenaSerializer serializer;
        private readonly IFileSaver fileSaver;
        private readonly IFileRetriever fileRetriever;

        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> GetProgramUpdaterName(Guid telemetryKey)
        {
            Program program = await this.work.Programs.FirstOrDefaultAsync(x=>x.TelemetryKey == telemetryKey);
            if (program == null)
            {
                throw new BadRequestException($"Program with Key [{telemetryKey}] does not exist");
            }

            var name = program.Updater?.FileName ?? DefaultToolkitNames.UpdaterFileName;

            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Content = new StringContent(name, System.Text.Encoding.UTF8, "text/plain");
            return resp;
        }

      

        [AllowAnonymous]
        [HttpGet]
        [Audit]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            UpdaterPackageInfo updaterInfo = await this.work.UpdaterRepository.GetPackageInfo(id);
            if (updaterInfo == null)
            {
                return this.BadRequest($"Updater id [{id}] does not exist");
            }

            byte[] bytes = await this.work.UpdaterRepository.GetPackage(updaterInfo.Id, this.fileRetriever);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {FileName = updaterInfo.ZippedFileName };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

            return this.ResponseMessage(result);
        }

        [HttpPut]
        [Audit]
        public async Task<IHttpActionResult> SetIsPublic(Guid id, bool isPublic)
        {
            var updater = await this.work.UpdaterRepository.GetUpdater(id);
            if (updater == null)
            {
                return this.BadRequest($"Updater id [{id}] does not exist");
            }

            if (!isPublic && updater.InternalName == DefaultToolkitNames.UpdaterInternalName)
            {
                return this.BadRequest($"Cannot change default updater");
            }
            updater.IsPublic = isPublic;
            await this.work.CompleteAsync();
            return this.Ok($"Set package with ID: {id} public flag to: {isPublic}");
        }

        

        [AllowAnonymous]
        [HttpGet]
        public async Task<UpdateResponse> GetUpdateInfo(string request)
        {
            UpdateRequest requestModel = Utilities.ReadRequest(request, this.serializer);
            var program = await this.work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == requestModel.TelemetryKey);
            if (program == null)
            {
                return new UpdateResponse()
                {
                    Exception = new BadRequestException($"Program with id [{requestModel.TelemetryKey}] does not exist")
                };
            }
            UpdaterPackageInfo updaterInfo =
                await this.work.UpdaterRepository.GetNewestCompatibleUpdater(program, requestModel.UpdaterVersion, requestModel.ToolkitVersion, false);
            UpdateResponse response = new UpdateResponse();
            if (updaterInfo != null)
            {
                var info = Mapper.Map<UpdatePackageData>(updaterInfo);
                info.DownloadUrl = Router.Api.DownloadUpdaterUpdate(updaterInfo);
                response.UpdatePackages = new[] {info};
            }

            return response;
        }

        [HttpPost]
        [Audit]
        public async Task<IHttpActionResult> Upload()
        {
            try
            {
                string reqString = HttpContext.Current.Request.Form["Model"];
                UploadUpdaterRequest request = JsonConvert.DeserializeObject<UploadUpdaterRequest>(reqString);

                HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    TelimenaUser user = await this.work.Users.GetByPrincipalAsync(this.User);
                    Updater updater = await this.work.UpdaterRepository.GetUpdater(request.UpdaterInternalName);

                    if (updater == null)
                    {
                       updater = this.work.UpdaterRepository.Add(uploadedFile.FileName, request.UpdaterInternalName, user);
                    }

                    if (user.AssociatedDeveloperAccounts.All(x => x.Id != updater.DeveloperAccount.Id))
                    {
                        return this.BadRequest(
                            $"Updater '{updater.InternalName}' is managed by a team that you don't belong to - '{updater.DeveloperAccount.Name}'");
                    }

                    if (uploadedFile.FileName != updater.FileName && !uploadedFile.FileName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return this.BadRequest(
                            $"Incorrect file. Expected {updater.FileName} or a zip package with it");
                    }

                    UpdaterPackageInfo pkg =
                        await this.work.UpdaterRepository.StorePackageAsync(updater, request.MinimumCompatibleToolkitVersion, uploadedFile.InputStream, this.fileSaver);
                    await this.work.CompleteAsync();
                    return this.Ok($"Uploaded package {pkg.Version} with ID {pkg.Id}");
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