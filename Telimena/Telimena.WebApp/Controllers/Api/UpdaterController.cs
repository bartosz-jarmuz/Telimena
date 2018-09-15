using System;
using System.Collections.Generic;
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
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Repository.Implementation;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;

namespace Telimena.WebApp.Controllers.Api
{
    [TelimenaAuthorize(Roles = TelimenaRoles.Admin)]

    public class UpdaterController : ApiController
    {
        private readonly IToolkitDataUnitOfWork work;
        private readonly IFileSaver fileSaver;
        private readonly IFileRetriever fileRetriever;

        public UpdaterController(IToolkitDataUnitOfWork work, IFileSaver fileSaver, IFileRetriever fileRetriever)
        {
            this.work = work;
            this.fileSaver = fileSaver;
            this.fileRetriever = fileRetriever;
        }

        //public async Task<UpdaterPackageInfo> GetLatestUpdaterInfo()
        //{

        //}

        [System.Web.Http.HttpGet]
        public async Task<IHttpActionResult> GetUpdater(int id)
        {
            var updaterInfo = await this.work.UpdaterRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (updaterInfo == null)
            {
                return this.BadRequest($"Updater id [{id}] does not exist");
            }
            var bytes = await this.work.UpdaterRepository.GetPackage(id,this.fileRetriever );
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            result.Content.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = updaterInfo.FileName
                };
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            return this.ResponseMessage(result);
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public async Task<IHttpActionResult> GetUpdater(string version)
        {
            if (!Version.TryParse(version, out _))
            {
                return this.BadRequest($"[{version}] is not a valid version string");
            }
            var updaterInfo = await this.work.UpdaterRepository.FirstOrDefaultAsync(x=>x.Version == version);
            if (updaterInfo == null)
            {
                return this.BadRequest($"Updater version [{version}] does not exist");
            }

            return await this.GetUpdater(updaterInfo.Id);
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public async Task<UpdateResponse> CheckForUpdate(string version, string toolkitVersion)
        {
           
            if (!Version.TryParse(version, out _))
            {
                return new UpdateResponse() {Error = new ArgumentException($"[{version}] is not a valid version string")};
            }
            if (!Version.TryParse(toolkitVersion, out _))
            {
                return new UpdateResponse() {Error = new ArgumentException($"[{toolkitVersion}] is not a valid version string")};
            }

            UpdaterPackageInfo updaterInfoMaybeBeta = await this.work.UpdaterRepository.GetNewestCompatibleUpdater(version, toolkitVersion, true);
            UpdaterPackageInfo updaterInfo = await this.work.UpdaterRepository.GetNewestCompatibleUpdater(version, toolkitVersion, false);
            var response = new UpdateResponse();
            if (updaterInfo != null)
            {
                response.UpdatePackages = new[] {Mapper.Map<UpdatePackageData>(updaterInfo)};
                response.UpdatePackagesIncludingBeta = new[] {Mapper.Map<UpdatePackageData>(updaterInfo)};
            }

            if (updaterInfoMaybeBeta != null)
            {
                response.UpdatePackagesIncludingBeta = new[] { Mapper.Map<UpdatePackageData>(updaterInfoMaybeBeta) };
            }

            return response;

        }

       


        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> UploadUpdaterPackage()
        {
            try
            {
                string reqString = System.Web.HttpContext.Current.Request.Form["Model"];
                CreateUpdatePackageRequest request = JsonConvert.DeserializeObject<CreateUpdatePackageRequest>(reqString);
                HttpPostedFile uploadedFile = System.Web.HttpContext.Current.Request.Files.Count > 0 ? System.Web.HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    var pkg = await this.work.UpdaterRepository.StorePackageAsync(request.PackageVersion,"0.0.0.0", uploadedFile.InputStream, this.fileSaver);
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