using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Telimena.WebApp.Core.Interfaces;
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

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public async Task<IHttpActionResult> DownloadLatestProgramPackage(int programId)
        {
            var packageInfo = await this.Work.ProgramPackages.GetLatestProgramPackageInfo(programId);

            var bytes = await this.Work.ProgramPackages.GetPackage(packageInfo.Id);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            result.Content.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = packageInfo.FileName
                };
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            return this.ResponseMessage(result);
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("download-app/{name}", Name = "DownloadAppRoute")]
        public async Task<IHttpActionResult> DownloadLatestProgramPackage(string name)
        {
            var prg = await this.Work.Programs.FirstOrDefaultAsync(x => x.Name == name);
            return await this.DownloadLatestProgramPackage(prg.Id);
        }

        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> AddProgramPackage(int id)
        {
            try
            {
                HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ?
                    HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    var pkg = await this.Work.ProgramPackages.StorePackageAsync(id, uploadedFile.InputStream, uploadedFile.FileName);
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
    }
}