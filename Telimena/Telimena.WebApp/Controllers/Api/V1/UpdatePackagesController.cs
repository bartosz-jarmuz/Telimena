﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using Telimena.Portal.Api.Models.RequestMessages;
using Telimena.Portal.Utils;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;

namespace Telimena.WebApp.Controllers.Api.V1
{
    #region Using

    #endregion

    /// <summary>
    /// Controls the update packages
    /// </summary>
    [TelimenaApiAuthorize(Roles = TelimenaRoles.Developer)]
    [RoutePrefix("api/v{version:apiVersion}/update-packages")]
    public partial class UpdatePackagesController : ApiController
    {
        /// <summary>
        /// New instance
        /// </summary>
        /// <param name="work"></param>
        /// <param name="fileSaver"></param>
        /// <param name="fileRetriever"></param>
        public UpdatePackagesController(IProgramsUnitOfWork work, IFileSaver fileSaver, IFileRetriever fileRetriever, IFileRemover fileRemover, TelemetryClient telemetryClient)
        {
            this.FileRetriever = fileRetriever;
            this.work = work;
            this.fileSaver = fileSaver;
            this.fileRemover = fileRemover;
            this.telemetryClient = telemetryClient;
        }

        private readonly IProgramsUnitOfWork work;
        private readonly IFileSaver fileSaver;
        private readonly IFileRemover fileRemover;
        private readonly TelemetryClient telemetryClient;
        private IFileRetriever FileRetriever { get; }

        /// <summary>
        /// Downloads the update package with specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        
        [HttpGet, Route("{id}", Name = Routes.Get)]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            ProgramUpdatePackageInfo packageInfo = await this.work.UpdatePackages.GetUpdatePackageInfo( id).ConfigureAwait(false);

            if (packageInfo == null)
            {
                return this.BadRequest($"Program Update Package [{id}] does not exist!");
            }

            try
            {
                this.telemetryClient.TrackEvent("DownloadUpdatePackage", new Dictionary<string, string>()
                {
                    {$"PackageId",id.ToString()},
                    {$"ProgramId", packageInfo.ProgramId.ToString()},
                    {$"FileName", packageInfo.FileName},
                });
                byte[] bytes = await this.work.UpdatePackages.GetPackage(id, this.FileRetriever).ConfigureAwait(false);
                HttpResponseMessage result =
                    new HttpResponseMessage(HttpStatusCode.OK) {Content = new ByteArrayContent(bytes)};
                result.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attachment") {FileName = packageInfo.FileName};
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

                return this.ResponseMessage(result);
            }
            catch (Exception ex)
            {
                this.telemetryClient.TrackException(ex, new Dictionary<string, string>()
                {
                    {$"Method", Routes.Get},
                    {$"PackageId",id.ToString()}
                });
                Trace.TraceError($"Error while trying to download update package {packageInfo.GetPropertyInfoString()}.\r\n\r\n{ex}");
                
                return this.InternalServerError(new InvalidOperationException("Error while trying to download update package"));
            }
        }

        /// <summary>
        /// Deletes the package
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        
        [HttpDelete, Route("{id}", Name = Routes.Delete)]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            ProgramUpdatePackageInfo packageInfo = await this.work.UpdatePackages.GetUpdatePackageInfo(id).ConfigureAwait(false);

            if (packageInfo == null)
            {
                return this.BadRequest($"Program Update Package [{id}] does not exist!");
            }

            await this.work.UpdatePackages.DeletePackage(packageInfo, this.fileRemover).ConfigureAwait(false);
            await this.work.CompleteAsync().ConfigureAwait(false);
            return this.Ok();
        }

        /// <summary>
        /// Uploads an update package
        /// </summary>
        /// <returns></returns>
        
        [HttpPost, Route("", Name = Routes.Upload)]
        public async Task<IHttpActionResult> Upload()
        {
            try
            {
                Trace.TraceInformation(LoggerHelper.FormatMessage(this.RequestContext, "Starting update package upload"));
                string reqString = HttpContext.Current.Request.Form["Model"];
                CreateUpdatePackageRequest request = JsonConvert.DeserializeObject<CreateUpdatePackageRequest>(reqString);
                HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    Program program = await this.work.Programs.GetByTelemetryKey(request.TelemetryKey).ConfigureAwait(false);
                    ProgramUpdatePackageInfo pkg = await this.work.UpdatePackages.StorePackageAsync(program, uploadedFile.FileName, uploadedFile.InputStream
                        , request.ToolkitVersionUsed, request.IsBeta, request.ReleaseNotes, this.fileSaver).ConfigureAwait(false);
                    await this.work.CompleteAsync().ConfigureAwait(false);
#pragma warning disable 618
                    return this.Ok(pkg.Id);
#pragma warning restore 618
                }

                return this.BadRequest("Empty attachment");
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Sets release notes for package
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        
        [HttpPut, Route("{packageId}/release-notes", Name = Routes.SetReleaseNotes)]
        public async Task<IHttpActionResult> SetReleaseNotes(Guid packageId, ReleaseNotesRequest request)
        {
            ProgramUpdatePackageInfo pkg = await this.work.UpdatePackages.FirstOrDefaultAsync(x => x.PublicId == packageId).ConfigureAwait(false);
            Trace.TraceInformation(LoggerHelper.FormatMessage(this.RequestContext, $"Setting release notes on package {pkg.Id}"));

            pkg.ReleaseNotes = request.Notes;
            await this.work.CompleteAsync().ConfigureAwait(false);
            return this.Ok("");
        }

        /// <summary>
        /// Gets release notes for package
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        
        [HttpGet, Route("{packageId}/release-notes/", Name = Routes.GetReleaseNotes)]
        public async Task<string> GetReleaseNotes(Guid packageId)
        {
            ProgramUpdatePackageInfo pkg = await this.work.UpdatePackages.FirstOrDefaultAsync(x => x.PublicId == packageId).ConfigureAwait(false);
            return pkg.ReleaseNotes;
        }

        /// <summary>
        /// Sets the 'is-beta' value of the given package to either true or false
        /// </summary>
        /// <param name = "packageId" ></param>
        /// <param name="isBeta"></param>
        /// <returns></returns>
        
        [HttpPut, Route("{packageId}/is-beta/{isBeta}", Name = Routes.ToggleBetaSetting)]
        public async Task<bool> ToggleBetaSetting(Guid packageId, bool isBeta)
        {
            ProgramUpdatePackageInfo pkg = await this.work.UpdatePackages.FirstOrDefaultAsync(x => x.PublicId == packageId).ConfigureAwait(false);
            Trace.TraceInformation(LoggerHelper.FormatMessage(this.RequestContext, $"Setting IsBeta on package {packageId} to {isBeta}"));
            pkg.IsBeta = isBeta;
           
            await this.work.CompleteAsync().ConfigureAwait(false);

            pkg = await this.work.UpdatePackages.FirstOrDefaultAsync(x => x.PublicId == packageId).ConfigureAwait(false);

            if (pkg.IsBeta != isBeta)
            {
                throw new InvalidOperationException($"Error occurred while setting package 'beta' status to {isBeta}. Value was not saved correctly.");
            }

            Trace.TraceInformation(LoggerHelper.FormatMessage(this.RequestContext, $"Successfully set IsBeta on package {packageId} to {pkg.IsBeta}"));

            return pkg.IsBeta;
        }

        /// <summary>
        /// Validates the update package request prior to uploading
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost, Route("validate-request", Name = Routes.ValidateRequest)]
        public IHttpActionResult ValidateRequest(CreateUpdatePackageRequest request)
        {
            if (!ApiRequestsValidator.IsRequestValid(request, out List<string> errors))
            {
                return this.BadRequest(string.Join(", ", errors));
            }

            return this.Ok();
        }

    }
}