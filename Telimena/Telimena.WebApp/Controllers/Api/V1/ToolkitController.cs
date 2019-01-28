using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using MvcAuditLogger;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using TelimenaClient;

namespace Telimena.WebApp.Controllers.Api.V1
{
    /// <summary>
    /// Controls the Telimena toolkit operations
    /// </summary>
    [TelimenaApiAuthorize(Roles = TelimenaRoles.Admin)]
    [RoutePrefix("api/v{version:apiVersion}/toolkit")]
    public partial class ToolkitController : ApiController
    {
        //The toolkit can evolve independently of the client app - some changes might be breaking the contracts, but most - should not
        //a non-breaking change example - add new features or overloads, pull more client data or handle changes in the web api
        //a breaking change would be renaming methods, changing the default URI etc - these are the things that would break the app if DLL is replaced
        //in such cases, the app needs to be recompiled against latest toolkit.

        //todo - when a new toolkit version is uploaded and is marked as 'breaking changes'
        //all the programs which were not compiled against that version should get a notification:
        // - at developer portal, a box saying '4 of your apps need to be recompiled to use latest Telimena
        //The dev should be able to open project in VS, update nuget and recompile
        //then go to portal and upload update package - FOOBARER v.4.1
        //When uploading an update, dev is prompted for version of toolkit that was used (later this becomes automatic)

        /// <summary>
        /// New instance
        /// </summary>
        /// <param name="work"></param>
        /// <param name="fileSaver"></param>
        /// <param name="fileRetriever"></param>
        public ToolkitController(IToolkitDataUnitOfWork work, IFileSaver fileSaver, IFileRetriever fileRetriever)
        {
            this.work = work;
            this.fileSaver = fileSaver;
            this.fileRetriever = fileRetriever;
        }

        private readonly IToolkitDataUnitOfWork work;
        private readonly IFileSaver fileSaver;
        private readonly IFileRetriever fileRetriever;

        /// <summary>
        /// Download the Telimena toolkit assembly by specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Audit]
        [HttpGet, Route("{id}", Name = Routes.Get)]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            TelimenaToolkitData toolkitData = await this.work.ToolkitDataRepository.FirstOrDefaultAsync(x => x.Guid == id).ConfigureAwait(false);
            if (toolkitData == null)
            {
                return this.BadRequest($"Toolkit id [{id}] does not exist");
            }

            byte[] bytes = await this.work.ToolkitDataRepository.GetPackage(toolkitData.Id, this.fileRetriever).ConfigureAwait(false);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = toolkitData.TelimenaPackageInfo.ZippedFileName };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

            return this.ResponseMessage(result);
        }

        /// <summary>
        /// Upload a new Telimena toolkit assembly file
        /// </summary>
        /// <returns></returns>
        [Audit]
        [HttpPost, Route("", Name = Routes.Upload)]
        public async Task<IHttpActionResult> Upload()
        {
            try
            {
                HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    if (uploadedFile.FileName != DefaultToolkitNames.TelimenaAssemblyName && !uploadedFile.FileName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return this.BadRequest(
                            $"Incorrect file. Expected {DefaultToolkitNames.TelimenaAssemblyName} or {DefaultToolkitNames.ZippedPackageName}");
                    }
                    TelimenaToolkitData pkg =
                        await this.work.ToolkitDataRepository.StorePackageAsync(false, false, uploadedFile.InputStream, this.fileSaver).ConfigureAwait(false);
                    await this.work.CompleteAsync().ConfigureAwait(false);
                    return this.Ok($"Uploaded package {pkg.Version} with ID {pkg.Id}");
                }

                return this.BadRequest("Empty attachment");
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }
#pragma warning restore 1591

    }
}