using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Utils.VersionComparison;

namespace Telimena.WebApp.Controllers.Api.V1.Helpers
{
    internal static class ProgramsControllerHelpers
    {
        public static List<ProgramUpdatePackageInfo> FilterPackagesSet(List<ProgramUpdatePackageInfo> updatePackages, UpdateRequest request)
        {
            if (updatePackages.IsNullOrEmpty())
            {
                return new List<ProgramUpdatePackageInfo>();
            }

            if (!request.AcceptBeta)
            {
                updatePackages.RemoveAll(x => x.IsBeta);
                if (updatePackages.IsNullOrEmpty())
                {
                    return new List<ProgramUpdatePackageInfo>();
                }
            }

            ProgramUpdatePackageInfo newestPackage = updatePackages.First();
            if (newestPackage.IsStandalone)
            {
                return new List<ProgramUpdatePackageInfo> { newestPackage };
            }

            List<ProgramUpdatePackageInfo> list = new List<ProgramUpdatePackageInfo>();
            foreach (ProgramUpdatePackageInfo updatePackageInfo in updatePackages)
            {
                list.Add(updatePackageInfo);
                if (updatePackageInfo.IsStandalone)
                {
                    break;
                }
            }

            return list;
        }



        public static async Task<string> GetMaximumSupportedToolkitVersion(IProgramsUnitOfWork unitOfWork, List<ProgramUpdatePackageInfo> updatePackages, Program program
            , UpdateRequest updateRequest)
        {
            string maxVersionInPackages = null;
#pragma warning disable 618
            ProgramUpdatePackageInfo newestPackage = updatePackages.OrderByDescending(x => x.Id).FirstOrDefault();
#pragma warning restore 618
            if (newestPackage != null)
            {
                maxVersionInPackages = newestPackage.SupportedToolkitVersion;
            }
            else
            {
                //no updates now, so figure out what version is supported by the client already
                var version = program.DetermineProgramVersion(updateRequest.VersionData );
                ProgramUpdatePackageInfo previousPackage =
                    await unitOfWork.UpdatePackages.FirstOrDefaultAsync(x => x.ProgramId == program.Id && x.Version == version).ConfigureAwait(false);
                if (previousPackage != null)
                {
                    maxVersionInPackages = previousPackage.SupportedToolkitVersion;
                }
                else
                {
                    var pkg = (await unitOfWork.ProgramPackages.FirstOrDefaultAsync(x => x.ProgramId == program.Id)
                        .ConfigureAwait(false));

                    maxVersionInPackages = pkg?.SupportedToolkitVersion??"0.0.0.0";
                }
            }

            if (updateRequest.ToolkitVersion.IsNewerOrEqualVersion(maxVersionInPackages))
            {
                return updateRequest.ToolkitVersion;
            }

            return maxVersionInPackages;
        }

        public static async Task<TelimenaPackageInfo> GetToolkitUpdateInfo(IProgramsUnitOfWork unitOfWork, Program program, UpdateRequest request, string maximumSupportedToolkitVersion)
        {
            ObjectValidator.Validate(() => Version.TryParse(request.ToolkitVersion, out _)
                , new ArgumentException($"[{request.ToolkitVersion}] is not a valid version string"));

            List<TelimenaPackageInfo> packages = (await unitOfWork.ToolkitData.GetPackagesNewerThan(request.ToolkitVersion).ConfigureAwait(false))
                .OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).ToList();

            if (!request.AcceptBeta)
            {
                packages.RemoveAll(x => x.IsBeta);
            }

            if (packages.Any(x => x.IntroducesBreakingChanges))
            {
                packages.Reverse();
                List<TelimenaPackageInfo> listOfCompatiblePackages = new List<TelimenaPackageInfo>();
                foreach (TelimenaPackageInfo package in packages)
                {
                    if (!package.IntroducesBreakingChanges)
                    {
                        listOfCompatiblePackages.Add(package);
                    }
                    else
                    {
                        if (maximumSupportedToolkitVersion.IsNewerOrEqualVersion(package.Version))
                        {
                            listOfCompatiblePackages.Add(package);
                        }
                        else //at this point a breaking package is not supported by the program, so time to break the loop - no point checking even newer ones
                        {
                            break;
                        }
                    }
                }

                return listOfCompatiblePackages.LastOrDefault();
            }

            return packages.FirstOrDefault();
        }

        public static async Task<IHttpActionResult> GetDownloadLatestProgramPackageResponse(IProgramsUnitOfWork unitOfWork, int programId, IFileRetriever fileRetriever)
        {
            ProgramPackageInfo packageInfo = await unitOfWork.ProgramPackages.GetLatestProgramPackageInfo(programId).ConfigureAwait(false);

            byte[] bytes = await unitOfWork.ProgramPackages.GetPackage(packageInfo.Id, fileRetriever).ConfigureAwait(false);
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = packageInfo.FileName };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return new ResponseMessageResult(result);
        }
    }
}