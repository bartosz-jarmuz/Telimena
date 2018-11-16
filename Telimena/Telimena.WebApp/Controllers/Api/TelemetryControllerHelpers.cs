using System;
using System.IdentityModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using TelimenaClient;

namespace Telimena.WebApp.Controllers.Api
{
    public static class TelemetryControllerHelpers
    {
        public static TelemetryUpdateResponse PrepareUpdateResponse(TelemetryUpdateRequest updateRequest, TelemetrySummary usageSummary, Program program
            , ClientAppUser clientAppUser)
        {

            TelemetryUpdateResponse response = new TelemetryUpdateResponse
            {
                Count = usageSummary.SummaryCount,
                TelemetryKey = program.TelemetryKey,
                UserId = clientAppUser.Guid,
                ComponentName = updateRequest.ComponentName
            };
            if (usageSummary is ViewTelemetrySummary viewSummary)
            {
                response.ComponentId = viewSummary.ViewId;
            }
            if (usageSummary is EventTelemetrySummary eventSummary)
            {
                response.ComponentId = eventSummary.EventId;
            }

            return response;
        }

        public static async Task<TelemetryUpdateResponse> PerformUpdate(ITelemetryUnitOfWork work, TelemetryUpdateRequest request, Func<string> getClientIp, Func<string, Program, Task<ITelemetryAware>> getTrackedComponent)
        {
            try
            {
                (bool isRequestValid, TelemetryUpdateResponse response, Program program, ClientAppUser clientAppUser) actionItems = await GetTelemetryUpdateActionItems(work, request);
                if (!actionItems.isRequestValid)
                {
                    return actionItems.response;
                }

                ITelemetryAware trackedComponent = await getTrackedComponent(request.ComponentName, actionItems.program);

                TelemetrySummary summary = GetTelemetrySummary(actionItems.clientAppUser, trackedComponent);

                AssemblyVersionInfo versionInfoInfo = GetAssemblyVersionInfoOrAddIfMissing(request.AssemblyVersion, request.FileVersion, actionItems.program);

                summary.UpdateTelemetry(versionInfoInfo, getClientIp(), request.TelemetryData);

                await work.CompleteAsync();
                return PrepareUpdateResponse(request, summary, actionItems.program, actionItems.clientAppUser);
            }
            catch (Exception ex)
            {
                return new TelemetryUpdateResponse { Exception = new InvalidOperationException("Error while processing statistics update request", ex) };
            }
        }

      

        public static async Task<ClientAppUser> GetUserOrAddIfMissing(ITelemetryUnitOfWork work, UserInfo userDto, string ip)
        {
            ClientAppUser user = await work.ClientAppUsers.FirstOrDefaultAsync(x => x.UserName == userDto.UserName);
            if (user == null)
            {
                user = Mapper.Map<ClientAppUser>(userDto);
                user.RegisteredDate = DateTime.UtcNow;
                user.IpAddresses.Add(ip);
                work.ClientAppUsers.Add(user);
            }
            else
            {
                if (!user.IpAddresses.Contains(ip))
                {
                    user.IpAddresses.Add(ip);
                }
            }
            return user;
        }

        public static async Task<ITelemetryAware> RecordVersions(ITelemetryUnitOfWork work, Program program, TelemetryInitializeRequest request)
        {
            AssemblyInfo primaryAss = request.ProgramInfo.PrimaryAssembly;
            program.PrimaryAssembly.AddVersion(primaryAss.AssemblyVersion, primaryAss.FileVersion);

            if (request.ProgramInfo.HelperAssemblies.AnyAndNotNull())
            {
                foreach (AssemblyInfo helperAssembly in request.ProgramInfo.HelperAssemblies)
                {
                    ProgramAssembly existingAssembly = program.ProgramAssemblies.FirstOrDefault(x => x.Name == helperAssembly.Name);
                    if (existingAssembly == null)
                    {
                        existingAssembly = Mapper.Map<ProgramAssembly>(helperAssembly);
                        program.ProgramAssemblies.Add(existingAssembly);
                    }

                    existingAssembly.AddVersion(helperAssembly.AssemblyVersion, helperAssembly.FileVersion);
                }
            }

            await AssignToolkitVersion(work, program.PrimaryAssembly, primaryAss.AssemblyVersion, primaryAss.FileVersion, request.TelimenaVersion);
            return program;
        }

        private static  async Task AssignToolkitVersion(ITelemetryUnitOfWork work, ProgramAssembly programAssembly, string programVersion, string fileVersion, string toolkitVersion)
        {
            TelimenaToolkitData toolkitData = await work.ToolkitData.FirstOrDefaultAsync(x => x.Version == toolkitVersion);
            AssemblyVersionInfo assemblyVersionInfo = programAssembly.GetVersion(programVersion, fileVersion);
            if (toolkitData == null)
            {
                toolkitData = new TelimenaToolkitData(toolkitVersion);
            }

            assemblyVersionInfo.ToolkitData = toolkitData;
        }


        public static AssemblyVersionInfo GetAssemblyVersionInfoOrAddIfMissing(string assemblyVersion, string fileVersion, Program program)
        {
            program.PrimaryAssembly.AddVersion(assemblyVersion, fileVersion);
            AssemblyVersionInfo versionInfo = program.PrimaryAssembly.GetVersion(assemblyVersion, fileVersion);
            return versionInfo;
        }

        public static async Task<(bool isValid, TelemetryUpdateResponse response, Program program, ClientAppUser user)> GetTelemetryUpdateActionItems(ITelemetryUnitOfWork work, TelemetryUpdateRequest request)
        {
            if (!ApiRequestsValidator.IsRequestValid(request))
            {
                return (false, new TelemetryUpdateResponse { Exception = new BadRequestException("Request is not valid") }, null, null);
            }

            Program program = await work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == request.TelemetryKey);
            if (program == null)
            {
                {
                    TelemetryUpdateResponse response = new TelemetryUpdateResponse { Exception = new InvalidOperationException($"Program [{request.TelemetryKey}] is null") };
                    return (false, response, null, null);
                }
            }

            ClientAppUser clientAppUser = await work.ClientAppUsers.FirstOrDefaultAsync(x=> x.Guid == request.UserId);
            if (clientAppUser == null)
            {
                {
                    TelemetryUpdateResponse response = new TelemetryUpdateResponse { Exception = new InvalidOperationException($"User [{request.UserId}] is null") };
                    return (false, response, null, null);
                }
            }

            return (true, null, program, clientAppUser);
        }

        public static async Task<(bool isValid, TelemetryInitializeResponse response, Program program)> GetTelemetryInitializeActionItems(ITelemetryUnitOfWork work, TelemetryInitializeRequest request)
        {
            if (!ApiRequestsValidator.IsRequestValid(request))
            {
                return (false, new TelemetryInitializeResponse() { Exception = new BadRequestException("Request is not valid") }, null);
            }

            Program program = await work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == request.TelemetryKey);
            if (program == null)
            {
                {
                    TelemetryInitializeResponse response = new TelemetryInitializeResponse { Exception = new InvalidOperationException($"Program [{request.TelemetryKey}] is null") };
                    return (false, response, null);
                }
            }

            return (true, null, program);
        }

        public static TelemetrySummary GetTelemetrySummary(ClientAppUser clientAppUser, ITelemetryAware component)
        {
            TelemetrySummary summary = component.GetTelemetrySummary(clientAppUser.Id);
            if (summary == null)
            {
                summary = component.AddTelemetrySummary(clientAppUser.Id);
            }

            return summary;
        }

        public static async Task<ITelemetryAware> GetEventOrAddIfMissing(ITelemetryUnitOfWork work, string componentName, Program program)
        {
            Event obj = await work.Events.FirstOrDefaultAsync(x => x.Name == componentName && x.Program.Name == program.Name);
            if (obj == null)
            {
                obj = new Event() { Name = componentName, Program = program, ProgramId = program.Id };
                work.Events.Add(obj);
            }

            return obj;
        }

        public static async Task<ITelemetryAware> GetViewOrAddIfMissing(ITelemetryUnitOfWork work, string viewName, Program program)
        {
            View view = await work.Views.FirstOrDefaultAsync(x => x.Name == viewName && x.Program.Name == program.Name);
            if (view == null)
            {
                view = new View { Name = viewName, Program = program, ProgramId = program.Id };
                work.Views.Add(view);
            }

            return view;
        }

        public static async Task<ITelemetryAware> GetProgram(ITelemetryUnitOfWork work, Guid telemetryKey)
        {
            return await work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey);
        }

        public static void SetPrimaryAssembly(Program program, TelemetryInitializeRequest request)
        {
            if (program.PrimaryAssembly == null)
            {
                program.PrimaryAssembly = Mapper.Map<ProgramAssembly>(request.ProgramInfo.PrimaryAssembly);
                program.PrimaryAssembly.ProgramId = program.Id;
            }
        }

    }
}