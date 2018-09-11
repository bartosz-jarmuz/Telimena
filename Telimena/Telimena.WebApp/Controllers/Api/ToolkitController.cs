using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
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

    public class ToolkitController : ApiController
    {
        private readonly IToolkitDataUnitOfWork work;
        private readonly IFileSaver fileSaver;
        private readonly IFileRetriever fileRetriever;

        public ToolkitController(IToolkitDataUnitOfWork work, IFileSaver fileSaver, IFileRetriever fileRetriever)
        {
            this.work = work;
            this.fileSaver = fileSaver;
            this.fileRetriever = fileRetriever;
        }

      

        [System.Web.Http.HttpGet]
        public async Task<IHttpActionResult> Get(int id)
        {
            TelimenaToolkitData toolkitData = await this.work.ToolkitDataRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (toolkitData == null)
            {
                return this.BadRequest($"Toolkit id [{id}] does not exist");
            }
            var bytes = await this.work.ToolkitDataRepository.GetPackage(toolkitData.Id, this.fileRetriever);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };

            result.Content.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = toolkitData.TelimenaPackageInfo.FileName
                };
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            return this.ResponseMessage(result);
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public async Task<IHttpActionResult> Get(string version)
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

            return await this.Get(updaterInfo.Id);
        }

        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> Upload()
        {
            try
            {
                string reqString = System.Web.HttpContext.Current.Request.Form["Model"];
                CreateUpdatePackageRequest request = JsonConvert.DeserializeObject<CreateUpdatePackageRequest>(reqString);
                HttpPostedFile uploadedFile = System.Web.HttpContext.Current.Request.Files.Count > 0 ? System.Web.HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    var pkg = await this.work.ToolkitDataRepository.StorePackageAsync(request.PackageVersion, uploadedFile.InputStream, this.fileSaver);
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