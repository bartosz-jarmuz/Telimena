//using System;
//using System.IdentityModel;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Http;
//using Telimena.WebApp.Core.Models;
//using Telimena.WebApp.Infrastructure;
//using Telimena.WebApp.Infrastructure.Identity;
//using Telimena.WebApp.Infrastructure.UnitOfWork;
//using TelimenaClient;

//namespace Telimena.WebApp.Controllers.Api
//{
//    #region Using

//    #endregion

//    public class StatisticsController : ApiController
//    {
//        public StatisticsController(ITelemetryUnitOfWork work)
//        {
//            this.work = work;
//            this.helper = new TelemetryHelperService(work);
//        }

//        private readonly ITelemetryUnitOfWork work;
//        private readonly TelemetryHelperService helper;

//        [HttpPost]
//        public async Task<TelemetryInitializeResponse> RegisterClient(TelemetryInitializeRequest request)
//        {
//            if (!ApiRequestsValidator.IsRequestValid(request))
//            {
//                return new TelemetryInitializeResponse { Exception = new BadRequestException("Request is not valid") };
//            }

//            try
//            {
//                Program program = await this.helper.GetProgramOrAddIfMissing(request);
//                var ip = this.Request.GetClientIp();
//                ClientAppUser clientAppUser = await this.helper.GetUserInfoOrAddIfNotExists(request.UserInfo, ip);
//                TelemetrySummary usageSummary = await this.GetUsageData(program, clientAppUser);
//                if (!request.SkipUsageIncrementation)
//                {
//                    AssemblyVersionInfo versionInfo = program.PrimaryAssembly.GetVersion(request.ProgramInfo.PrimaryAssembly.Version, request.ProgramInfo.PrimaryAssembly.FileVersion);
//                    usageSummary.UpdateTelemetry(versionInfo, ip);
//                }

//                await this.work.CompleteAsync();
//                return new TelemetryInitializeResponse { Count = usageSummary.SummaryCount, TelemetryKey = program.Id, UserId = clientAppUser.Id };
//            }
//            catch (Exception ex)
//            {
//                return new TelemetryInitializeResponse { Exception = new InvalidOperationException("Error while processing registration request", ex) };
//            }
//        }

//        //[HttpPost]
//        //public async Task<StatisticsUpdateResponse> Update(TelemetryUpdateRequest updateRequest)
//        //{
//        //    if (!ApiRequestsValidator.IsRequestValid(updateRequest))
//        //    {
//        //        return new StatisticsUpdateResponse {Exception = new BadRequestException("Request is not valid")};
//        //    }

//        //    try
//        //    {
//        //        Program program = await this.work.Programs.FirstOrDefaultAsync(x=>x.Id == updateRequest.TelemetryKey);
//        //        if (program == null)
//        //        {
//        //            return new StatisticsUpdateResponse {Exception = new InvalidOperationException($"Program [{updateRequest.TelemetryKey}] is null")};
//        //        }

//        //        ClientAppUser clientAppUser = this.work.ClientAppUsers.GetById(updateRequest.UserId);
//        //        if (clientAppUser == null)
//        //        {
//        //            return new StatisticsUpdateResponse {Exception = new InvalidOperationException($"User [{updateRequest.UserId}] is null")};
//        //        }

//        //        var usageSummary = await this.GetUsageData(program, clientAppUser, updateRequest.ComponentName);
//        //        program.PrimaryAssembly.AddVersion(updateRequest.AssemblyVersion, updateRequest.FileVersion);
//        //        AssemblyVersionInfo versionInfoInfo = program.PrimaryAssembly.GetVersion(updateRequest.AssemblyVersion, updateRequest.FileVersion);

//        //        var ip = this.Request.GetClientIp();
//        //        usageSummary.UpdateTelemetry(versionInfoInfo, ip, updateRequest.TelemetryData);

//        //        await this.work.CompleteAsync();
//        //        return PrepareResponse(updateRequest, (usageSummary as TelemetrySummary), program, clientAppUser);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        return new StatisticsUpdateResponse {Exception = new InvalidOperationException("Error while processing statistics update request", ex)};
//        //    }
//        //}


//    }
//}