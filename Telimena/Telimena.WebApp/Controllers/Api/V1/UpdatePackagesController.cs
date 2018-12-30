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
using MvcAuditLogger;
using Newtonsoft.Json;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using TelimenaClient;

namespace Telimena.WebApp.Controllers.Api.V1
{
    #region Using

    #endregion

    /// <summary>
    /// Controls the update packages
    /// </summary>
    [TelimenaApiAuthorize(Roles = TelimenaRoles.Developer)]
    [RoutePrefix("api/v{version:apiVersion}/update-packages")]
    public class UpdatePackagesController : ApiController
    {
        /// <summary>
        /// New instance
        /// </summary>
        /// <param name="work"></param>
        /// <param name="fileSaver"></param>
        /// <param name="fileRetriever"></param>
        public UpdatePackagesController(IProgramsUnitOfWork work, IFileSaver fileSaver, IFileRetriever fileRetriever)
        {
            this.FileRetriever = fileRetriever;
            this.work = work;
            this.fileSaver = fileSaver;
        }

        private readonly IProgramsUnitOfWork work;
        private readonly IFileSaver fileSaver;
        private IFileRetriever FileRetriever { get; }

        ///// <summary>
        ///// Downloads the update package with specified ID
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //[AllowAnonymous]
        //[Audit]
        //[HttpGet, Route("{id}")]
        //public async Task<IHttpActionResult> Get(Guid id)
        //{
        //    ProgramUpdatePackageInfo packageInfo = await this.work.UpdatePackages.GetUpdatePackageInfo(id);

        //    if (packageInfo == null)
        //    {
        //        return this.BadRequest($"Program Update Package [{id}] does not exist!");
        //    }

        //    byte[] bytes = await this.work.UpdatePackages.GetPackage(id, this.FileRetriever);
        //    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
        //    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {FileName = packageInfo.FileName};
        //    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

        //    return this.ResponseMessage(result);
        //}

        ///// <summary>
        ///// Uploads an update package
        ///// </summary>
        ///// <returns></returns>
        //[Audit]
        //[HttpPost, Route("")]
        //public async Task<IHttpActionResult> Upload()
        //{
        //    try
        //    {
        //        string reqString = HttpContext.Current.Request.Form["Model"];
        //        CreateUpdatePackageRequest request = JsonConvert.DeserializeObject<CreateUpdatePackageRequest>(reqString);
        //        HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
        //        if (uploadedFile != null && uploadedFile.ContentLength > 0)
        //        {
        //            Program program = await this.work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == request.TelemetryKey);
        //            ProgramUpdatePackageInfo pkg = await this.work.UpdatePackages.StorePackageAsync(program, uploadedFile.FileName, uploadedFile.InputStream
        //                , request.ToolkitVersionUsed, this.fileSaver);
        //            await this.work.CompleteAsync();
        //            return this.Ok(pkg.Id);
        //        }

        //        return this.BadRequest("Empty attachment");
        //    }
        //    catch (Exception ex)
        //    {
        //        return this.BadRequest(ex.Message);
        //    }
        //}

        ///// <summary>
        ///// Sets the 'is-beta' value of the given package to either true or false
        ///// </summary>
        ///// <param name="packageId"></param>
        ///// <param name="isBeta"></param>
        ///// <returns></returns>
        //[Audit]
        //[HttpPut, Route("{packageId}/is-beta/{isBeta}")]
        //public async Task<bool> ToggleBetaSetting(Guid packageId, bool isBeta)
        //{
        //    ProgramUpdatePackageInfo pkg = await this.work.UpdatePackages.FirstOrDefaultAsync(x => x.Guid == packageId);
        //    pkg.IsBeta = isBeta;
        //    await this.work.CompleteAsync();
        //    return pkg.IsBeta;
        //}

        ///// <summary>
        ///// Validates the update package request prior to uploading
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpPost, Route("validate-request/{request}")]
        //public IHttpActionResult ValidateRequest(CreateUpdatePackageRequest request)
        //{
        //    if (!ApiRequestsValidator.IsRequestValid(request, out List<string> errors))
        //    {
        //        return this.BadRequest(string.Join(", ", errors));
        //    }

        //    return this.Ok();
        //}

    }
}