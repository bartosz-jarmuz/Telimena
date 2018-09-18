using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AutoMapper;
using Newtonsoft.Json;
using Telimena.Client;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Repository.Implementation;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;

namespace Telimena.WebApp.Controllers.Api
{
    [TelimenaAuthorize(Roles = TelimenaRoles.Admin)]
    public class UpdaterController : ApiController
    {
        public UpdaterController(IToolkitDataUnitOfWork work, IFileSaver fileSaver, IFileRetriever fileRetriever)
        {
            this.work = work;
            this.fileSaver = fileSaver;
            this.fileRetriever = fileRetriever;
        }

        private readonly IToolkitDataUnitOfWork work;
        private readonly IFileSaver fileSaver;
        private readonly IFileRetriever fileRetriever;

        [AllowAnonymous]
        [HttpGet]
        public async Task<UpdateResponse> CheckForUpdate(string version, string toolkitVersion)
        {
            if (!Version.TryParse(version, out _))
            {
                return new UpdateResponse {Exception = new ArgumentException($"[{version}] is not a valid version string")};
            }

            if (!Version.TryParse(toolkitVersion, out _))
            {
                return new UpdateResponse {Exception = new ArgumentException($"[{toolkitVersion}] is not a valid version string")};
            }

            UpdaterPackageInfo updaterInfo = await this.work.UpdaterRepository.GetNewestCompatibleUpdater(version, toolkitVersion, false);
            UpdateResponse response = new UpdateResponse();
            if (updaterInfo != null)
            {
                response.UpdatePackages = new[] {Mapper.Map<UpdatePackageData>(updaterInfo)};
            }

            return response;
        }

        //public async Task<UpdaterPackageInfo> GetLatestUpdaterInfo()
        //{

        //}

        [HttpGet]
        public async Task<IHttpActionResult> GetUpdater(int id)
        {
            UpdaterPackageInfo updaterInfo = await this.work.UpdaterRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (updaterInfo == null)
            {
                return this.BadRequest($"Updater id [{id}] does not exist");
            }

            byte[] bytes = await this.work.UpdaterRepository.GetPackage(id, this.fileRetriever);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {FileName = updaterInfo.FileName};
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return this.ResponseMessage(result);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IHttpActionResult> GetUpdater(string version)
        {
            if (!Version.TryParse(version, out _))
            {
                return this.BadRequest($"[{version}] is not a valid version string");
            }

            UpdaterPackageInfo updaterInfo = await this.work.UpdaterRepository.FirstOrDefaultAsync(x => x.Version == version);
            if (updaterInfo == null)
            {
                return this.BadRequest($"Updater version [{version}] does not exist");
            }

            return await this.GetUpdater(updaterInfo.Id);
        }

        [HttpPost]
        public async Task<IHttpActionResult> UploadUpdaterPackage()
        {
            try
            {
                string reqString = HttpContext.Current.Request.Form["Model"];
                CreateUpdatePackageRequest request = JsonConvert.DeserializeObject<CreateUpdatePackageRequest>(reqString);
                HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    UpdaterPackageInfo pkg =
                        await this.work.UpdaterRepository.StorePackageAsync(request.PackageVersion, "0.0.0.0", uploadedFile.InputStream, this.fileSaver);
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
    }
}