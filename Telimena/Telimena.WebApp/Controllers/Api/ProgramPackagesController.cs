using System;
using System.Collections.Generic;
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
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;

namespace Telimena.WebApp.Controllers.Api
{
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class ProgramPackagesController : ApiController
    {
        public ProgramPackagesController(IProgramsUnitOfWork work)
        {
            this.Work = work;
        }

        private IProgramsUnitOfWork Work { get; }

        [HttpPost]
        public IHttpActionResult Validate(CreateProgramPackageRequest request)
        {
            if (!ApiRequestsValidator.IsRequestValid(request, out List<string> errors))
            {
                return this.BadRequest(string.Join(", ", errors));
            }

            return this.Ok();
        }

        [HttpPost]
        public async Task<IHttpActionResult> Upload(int id)
        {
            try
            {
                string reqString = HttpContext.Current.Request.Form["Model"];
                CreateUpdatePackageRequest request = JsonConvert.DeserializeObject<CreateUpdatePackageRequest>(reqString);
                HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    ProgramPackageInfo pkg = await this.Work.ProgramPackages.StorePackageAsync(id, uploadedFile.InputStream, uploadedFile.FileName,request.ToolkitVersionUsed);
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

        [AllowAnonymous]
        [HttpGet]
        public async Task<IHttpActionResult> DownloadLatestProgramPackage(int programId)
        {
            ProgramPackageInfo packageInfo = await this.Work.ProgramPackages.GetLatestProgramPackageInfo(programId);

            byte[] bytes = await this.Work.ProgramPackages.GetPackage(packageInfo.Id);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {FileName = packageInfo.FileName};
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return this.ResponseMessage(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("download-app/{name}", Name = "DownloadAppRoute")]
        public async Task<IHttpActionResult> DownloadLatestProgramPackage(string name)
        {
            Program prg = await this.Work.Programs.FirstOrDefaultAsync(x => x.Name == name);
            return await this.DownloadLatestProgramPackage(prg.Id);
        }
    }
}