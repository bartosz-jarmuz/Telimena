using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using MvcAuditLogger;
using Newtonsoft.Json;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;

namespace Telimena.WebApp.Controllers.Api
{
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class ProgramPackagesController : ApiController
    {
        private readonly IFileSaver fileSaver;
        private readonly IFileRetriever fileRetriever;

        public ProgramPackagesController(IProgramsUnitOfWork work, IFileSaver fileSaver, IFileRetriever fileRetriever)
        {
            this.fileSaver = fileSaver;
            this.fileRetriever = fileRetriever;
            this.Work = work;
        }

        private IProgramsUnitOfWork Work { get; }

        [HttpPost]
        [Audit]
        public async Task<IHttpActionResult> Upload(Guid id)
        {
            try
            {
                HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    var program = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == id);

                    if (program == null)
                    {
                        return this.BadRequest("Failed to find corresponding program");
                    }

                    ProgramPackageInfo pkg = await this.Work.ProgramPackages.StorePackageAsync(program.Id, uploadedFile.InputStream, uploadedFile.FileName, this.fileSaver);
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
        [Audit]
        [HttpGet]
        public async Task<IHttpActionResult> DownloadLatestProgramPackage(Guid telemetryKey)
        {
            Program prg = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey);
            if (prg == null)
            {
                return this.BadRequest($"Program with key [{telemetryKey}] does not exist");
            }
            return await this.GetDownloadLatestProgramPackageResponse(prg.Id);
        }


        private async Task<IHttpActionResult> GetDownloadLatestProgramPackageResponse(int programId)
        {
            ProgramPackageInfo packageInfo = await this.Work.ProgramPackages.GetLatestProgramPackageInfo(programId);

            byte[] bytes = await this.Work.ProgramPackages.GetPackage(packageInfo.Id, this.fileRetriever);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = packageInfo.FileName };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return this.ResponseMessage(result);
        }

        [AllowAnonymous]
        [Audit]
        [HttpGet]
        [Route("download-app/{name}", Name = "DownloadAppRoute")]
        public async Task<IHttpActionResult> DownloadLatestProgramPackage(string name)
        {
            Program prg = await this.Work.Programs.FirstOrDefaultAsync(x => x.Name == name);
            if (prg == null)
            {
                return this.BadRequest($"Program [{name}] does not exist");
            }
            return await this.GetDownloadLatestProgramPackageResponse(prg.Id);
        }
    }
}