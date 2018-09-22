using System;
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
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;

namespace Telimena.WebApp.Controllers.Api
{
    [TelimenaAuthorize(Roles = TelimenaRoles.Admin)]
    public class ToolkitController : ApiController
    {
        //The toolkit can evolve independently of the client app - some changes might be breaking the contracts, but most - should not
        //a non-breaking change example - add new functions or overloads, pull more client data or handle changes in the web api
        //a breaking change would be renaming methods, changing the default URI etc - these are the things that would break the app if DLL is replaced
        //in such cases, the app needs to be recompiled against latest toolkit.

        //todo - when a new toolkit version is uploaded and is marked as 'breaking changes'
        //all the programs which were not compiled against that version should get a notification:
        // - at developer portal, a box saying '4 of your apps need to be recompiled to use latest Telimena
        //The dev should be able to open project in VS, update nuget and recompile
        //then go to portal and upload update package - FOOBARER v.4.1
        //When uploading an update, dev is prompted for version of toolkit that was used (later this becomes automatic)

        public ToolkitController(IToolkitDataUnitOfWork work, IFileSaver fileSaver, IFileRetriever fileRetriever)
        {
            this.work = work;
            this.fileSaver = fileSaver;
            this.fileRetriever = fileRetriever;
        }

        private readonly IToolkitDataUnitOfWork work;
        private readonly IFileSaver fileSaver;
        private readonly IFileRetriever fileRetriever;

        [HttpGet]
        public async Task<IHttpActionResult> Get(int id)
        {
            TelimenaToolkitData toolkitData = await this.work.ToolkitDataRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (toolkitData == null)
            {
                return this.BadRequest($"Toolkit id [{id}] does not exist");
            }

            byte[] bytes = await this.work.ToolkitDataRepository.GetPackage(toolkitData.Id, this.fileRetriever);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {FileName = toolkitData.TelimenaPackageInfo.FileName};
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

            return this.ResponseMessage(result);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IHttpActionResult> Get(string version)
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

            return await this.Get(updaterInfo.Id);
        }

        [HttpPost]
        public async Task<IHttpActionResult> Upload()
        {
            try
            {
                string reqString = HttpContext.Current.Request.Form["Model"];
                CreateUpdatePackageRequest request = JsonConvert.DeserializeObject<CreateUpdatePackageRequest>(reqString);
                HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    TelimenaToolkitData pkg =
                        await this.work.ToolkitDataRepository.StorePackageAsync(request.PackageVersion, false, false, uploadedFile.InputStream, this.fileSaver);
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