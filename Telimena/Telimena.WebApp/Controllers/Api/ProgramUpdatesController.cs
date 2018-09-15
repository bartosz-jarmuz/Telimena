﻿using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using AutoMapper;
using DotNetLittleHelpers;
using Newtonsoft.Json;
using Telimena.Client;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;

namespace Telimena.WebApp.Controllers.Api
{
    #region Using

    #endregion

    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class ProgramUpdatesController : ApiController
    {
        public ProgramUpdatesController(IProgramsUnitOfWork work)
        {
            this.Work = work;
        }

        private IProgramsUnitOfWork Work { get; }


        [System.Web.Http.HttpGet]
        public async Task<UpdateResponse> GetUpdateInfo(int programId, string version)
        {
            try
            {
                Program program = await this.Work.Programs.FirstOrDefaultAsync(x => x.Id == programId);
                if (program == null)
                {
                    return new UpdateResponse()
                    {
                        Error = new BadRequestException($"Failed to find program by Id: [{programId}]")
                    };
                }

                return await this.GetUpdatePackagesResponse(program, version);
            }
            catch (Exception ex)
            {
                return new UpdateResponse
                {
                    Error = new InvalidOperationException("Error while processing registration request", ex)
                };
            }
        }

        private List<UpdatePackageData> GetMatchingPackages(List<ProgramUpdatePackageInfo> updatePackages)
        {
            if (updatePackages.IsNullOrEmpty())
            {
                return null;
            }
            ProgramUpdatePackageInfo newestPackage = updatePackages.First();
            if (newestPackage.IsStandalone)
            {
                return new List<UpdatePackageData>(){ Mapper.Map<UpdatePackageData>(newestPackage) };
            }
            else
            {
                List<UpdatePackageData> list = new List<UpdatePackageData>();
                foreach (ProgramUpdatePackageInfo updatePackageInfo in updatePackages)
                {
                    list.Add(Mapper.Map<UpdatePackageData>(updatePackageInfo));
                    if (updatePackageInfo.IsStandalone)
                    {
                        break;
                    }
                }
                return list;
            }
        }

        private async Task<UpdateResponse> GetUpdatePackagesResponse(Program program, string version)
        {
            List<ProgramUpdatePackageInfo> updatePackages = (await this.Work.UpdatePackages.GetAllPackagesNewerThan(program.Id, version)).OrderByDescending(x=>x.Version, new TelimenaVersionStringComparer()).ToList();
            if (!updatePackages.Any())
            {
                return new UpdateResponse();
            }
            List<ProgramUpdatePackageInfo> nonBetaPackages = updatePackages.Where(x => !x.IsBeta).ToList();
            UpdateResponse updatesResponse = new UpdateResponse();

            if (updatePackages.Any(x=>x.IsBeta))
            {
                updatesResponse.UpdatePackagesIncludingBeta = this.GetMatchingPackages(updatePackages);
            }

            updatesResponse.UpdatePackages = this.GetMatchingPackages(nonBetaPackages);

            if (updatesResponse.UpdatePackagesIncludingBeta.IsNullOrEmpty())
            {
                updatesResponse.UpdatePackagesIncludingBeta = new List<UpdatePackageData>(updatesResponse.UpdatePackages);
            }
            

            return updatesResponse;


        }


        [System.Web.Http.HttpPost]
        public IHttpActionResult ValidatePackageVersion(CreateUpdatePackageRequest request)
        {
            if (!ApiRequestsValidator.IsRequestValid(request, out List<string> errors))
            {
                return this.BadRequest(string.Join(", ", errors));
            }

            return this.Ok();
        }

        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> UploadUpdatePackage()
        {
            try
            {
                string reqString = HttpContext.Current.Request.Form["Model"];
                CreateUpdatePackageRequest request = JsonConvert.DeserializeObject<CreateUpdatePackageRequest>(reqString);
                HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    Program program = await this.Work.Programs.FirstOrDefaultAsync(x => x.Id == request.ProgramId);
                    ProgramUpdatePackageInfo pkg = await this.Work.UpdatePackages.StorePackageAsync(program, request.PackageVersion, uploadedFile.InputStream);
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


        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public async Task<IHttpActionResult> GetUpdateInfo(int programId)
        {
            ProgramUpdatePackageInfo packageInfo = await this.Work.UpdatePackages.GetUpdatePackageInfo(programId);

            byte[] bytes = await this.Work.UpdatePackages.GetPackage(packageInfo.Id);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = packageInfo.FileName
                };
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            return this.ResponseMessage(result);
        }


        [System.Web.Http.HttpPost]
        public async Task<bool> ToggleBetaSetting(int updatePackageId, bool isBeta)
        {
            ProgramUpdatePackageInfo pkg = await this.Work.UpdatePackages.FirstOrDefaultAsync(x=> x.Id == updatePackageId);
            pkg.IsBeta = isBeta;
            await this.Work.CompleteAsync();
            return pkg.IsBeta;
        }

        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpGet]
        public async Task<IHttpActionResult> DownloadUpdatePackage(int id)
        {
            ProgramUpdatePackageInfo packageInfo = await this.Work.UpdatePackages.GetUpdatePackageInfo(id);

            byte[] bytes = await this.Work.UpdatePackages.GetPackage(packageInfo.Id);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = packageInfo.FileName
                };
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            return this.ResponseMessage(result);
        }
    }
}