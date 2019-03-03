using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DotNetLittleHelpers;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Core.Models.Telemetry;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using LogLevel = Telimena.WebApp.Core.DTO.MappableToClient.LogLevel;
using UserInfo = Telimena.WebApp.Core.DTO.MappableToClient.UserInfo;

namespace Telimena.WebApp.Controllers.Api.V1
{
    internal static class TelemetryControllerHelpers
    {


        private static async Task<ITelemetryAware> GetTrackedComponent(ITelemetryUnitOfWork work, TelemetryItemTypes itemType, string key, TelemetryRootObject program)
        {
            switch (itemType)
            {
                case TelemetryItemTypes.Event:
                    return await GetEventOrAddIfMissing(work, key, program).ConfigureAwait(false);
                case TelemetryItemTypes.View:
                    return await GetViewOrAddIfMissing(work, key, program).ConfigureAwait(false);
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
            }
        }

        public static async Task<List<TelemetrySummary>> InsertData(ITelemetryUnitOfWork work, List<TelemetryItem> units, TelemetryRootObject program, string ipAddress)
        {
            if (units.Any())
            {

                ClientAppUser clientAppUser =
                    await GetUserOrAddIfMissing(work, units.First(), ipAddress).ConfigureAwait(false);

                IEnumerable<IGrouping<TelemetryItemTypes, TelemetryItem>> typeGroupings =
                    units.GroupBy(x => x.TelemetryItemType);
                List<TelemetrySummary> summaries = new List<TelemetrySummary>();
                foreach (IGrouping<TelemetryItemTypes, TelemetryItem> typeGrouping in typeGroupings)
                {
                    if (typeGrouping.Key == TelemetryItemTypes.Exception)
                    {
                        AddExceptions(work, program, typeGrouping, clientAppUser);
                    }
                    else if (typeGrouping.Key == TelemetryItemTypes.LogMessage)
                    {
                        AddLogs(work, program, typeGrouping);
                    }
                    else
                    {
                        await AddTelemetries(work, program, ipAddress, typeGrouping, clientAppUser, summaries).ConfigureAwait(false);
                    }
                }

                await work.CompleteAsync().ConfigureAwait(false);
                return summaries;

            }

            return null;
        }

        private static async Task AddTelemetries(ITelemetryUnitOfWork work, TelemetryRootObject program, string ipAddress
            , IGrouping<TelemetryItemTypes, TelemetryItem> typeGrouping, ClientAppUser clientAppUser, List<TelemetrySummary> summaries)
        {
            foreach (IGrouping<string, TelemetryItem> keyGroupings in typeGrouping.GroupBy(x => x.EntryKey))
            {
                ITelemetryAware trackedComponent = await GetTrackedComponent(work, typeGrouping.Key, keyGroupings.Key, program)
                    .ConfigureAwait(false);
                TelemetrySummary summary = GetTelemetrySummary(clientAppUser, trackedComponent);
                foreach (TelemetryItem telemetryItem in keyGroupings)
                {
                    summary.UpdateTelemetry(keyGroupings.First().VersionData, ipAddress, telemetryItem);
                }

                summaries.Add(summary);
            }
        }

        private static void AddLogs(ITelemetryUnitOfWork work, TelemetryRootObject program, IGrouping<TelemetryItemTypes, TelemetryItem> typeGrouping)
        {
            foreach (TelemetryItem telemetryItem in typeGrouping)
            {
                var logMsg = new LogMessage()
                {
                    Timestamp = telemetryItem.Timestamp
                    , Id = Guid.NewGuid()
                    , UserName = telemetryItem.UserIdentifier
                    , Sequence = telemetryItem.Sequence
                    , Message = telemetryItem.LogMessage
                    , ProgramId = program.ProgramId
                    , Level = telemetryItem.LogLevel
                };
                work.LogMessages.Add(logMsg);
            }
        }

        private static void AddExceptions(ITelemetryUnitOfWork work, TelemetryRootObject program, IGrouping<TelemetryItemTypes, TelemetryItem> typeGrouping
            , ClientAppUser clientAppUser)
        {
            foreach (TelemetryItem telemetryItem in typeGrouping)
            {
                foreach (TelemetryItem.ExceptionInfo telemetryItemException in telemetryItem.Exceptions)
                {
                    var exception = new ExceptionInfo
                    {
                          Timestamp = telemetryItem.Timestamp
                        , ProgramId = program.ProgramId
                        , ProgramVersion = telemetryItem.VersionData.FileVersion
                        , UserName = clientAppUser.UserIdentifier
                        , Sequence = telemetryItem.Sequence
                        , ExceptionId = telemetryItemException.Id
                        , ExceptionOuterId = telemetryItemException.OuterId
                        , HasFullStack = telemetryItemException.HasFullStack
                        , Message = telemetryItemException.Message
                        , Note = GetExceptionNote(telemetryItem.Properties)
                        , TypeName = telemetryItemException.TypeName
                        , ParsedStack = JsonConvert.SerializeObject(telemetryItemException.ParsedStack)
                    };
                    if (telemetryItem.Properties != null && telemetryItem.Properties.Any())
                    {
                        foreach (KeyValuePair<string, string> unit in telemetryItem.Properties)
                        {
                            ExceptionTelemetryUnit telemetryUnit = new ExceptionTelemetryUnit { Key = unit.Key, ValueString = unit.Value?.ToString(), UnitType = TelemetryUnit.UnitTypes.Property};
                            ((List<ExceptionTelemetryUnit>)exception.TelemetryUnits).Add(telemetryUnit);
                        }
                    }
                    if (telemetryItem.Measurements != null && telemetryItem.Measurements.Any())
                    {
                        foreach (KeyValuePair<string, double> unit in telemetryItem.Measurements)
                        {
                            ExceptionTelemetryUnit telemetryUnit = new ExceptionTelemetryUnit { Key = unit.Key, ValueDouble = unit.Value, UnitType = TelemetryUnit.UnitTypes.Metric };
                            ((List<ExceptionTelemetryUnit>)exception.TelemetryUnits).Add(telemetryUnit);
                        }
                    }

                    work.Exceptions.Add(exception);
                }
            }
        }

        private static string GetExceptionNote(Dictionary<string, string> properties)
        {
            if (properties != null && properties.ContainsKey(DefaultToolkitNames.TelimenaCustomExceptionNoteKey))
            {
                var value = properties[DefaultToolkitNames.TelimenaCustomExceptionNoteKey];
                properties.Remove(DefaultToolkitNames.TelimenaCustomExceptionNoteKey);
                return value;
            }

            return null;

        }

        public static async Task<ClientAppUser> GetUserOrAddIfMissing(ITelemetryUnitOfWork work, UserInfo userDto, string ip)
        {
            ClientAppUser user = await work.ClientAppUsers.FirstOrDefaultAsync(x => x.UserIdentifier == userDto.UserIdentifier).ConfigureAwait(false);
            if (user == null)
            {
                user = Mapper.Map<ClientAppUser>(userDto);
                user.FirstSeenDate = DateTime.UtcNow;
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

        public static async Task<ClientAppUser> GetUserOrAddIfMissing(ITelemetryUnitOfWork work, TelemetryItem item, string ip)
        {
            ClientAppUser user = null;
            if (!string.IsNullOrEmpty(item.AuthenticatedUserIdentifier))
            {
                user = await work.ClientAppUsers.FirstOrDefaultAsync(x => x.AuthenticatedUserIdentifier == item.AuthenticatedUserIdentifier).ConfigureAwait(false);
            }

            if (user == null)
            {
                user = await work.ClientAppUsers.FirstOrDefaultAsync(x => x.UserIdentifier == item.UserIdentifier).ConfigureAwait(false);
            }

            if (user == null)
            {
                user = new ClientAppUser();
                user.UserIdentifier = item.UserIdentifier??"";
                user.AuthenticatedUserIdentifier = item.AuthenticatedUserIdentifier;
                user.FirstSeenDate = DateTime.UtcNow;
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

        public static async Task RecordVersions(ITelemetryUnitOfWork work, Program program, TelemetryInitializeRequest request)
        {
            AssemblyInfo primaryAss = request.ProgramInfo.PrimaryAssembly;
            program.PrimaryAssembly.AddVersion(primaryAss.VersionData);

            await AssignToolkitVersion(work, program.PrimaryAssembly, primaryAss.VersionData , request.TelimenaVersion).ConfigureAwait(false);
        }

        private static  async Task AssignToolkitVersion(ITelemetryUnitOfWork work, ProgramAssembly programAssembly, VersionData versionData, string toolkitVersion)
        {
            TelimenaToolkitData toolkitData = await work.ToolkitData.FirstOrDefaultAsync(x => x.Version == toolkitVersion).ConfigureAwait(false);

            AssemblyVersionInfo assemblyVersionInfo = programAssembly.GetVersion(versionData);
            if (toolkitData == null)
            {
                toolkitData = new TelimenaToolkitData(toolkitVersion);
            }

            assemblyVersionInfo.ToolkitData = toolkitData;
        }

        //todo - decide whether the version data should be stored in telemetry program like that
        //public static AssemblyVersionInfo GetAssemblyVersionInfoOrAddIfMissing(VersionData versionData, Program program)
        //{
        //    program.PrimaryAssembly.AddVersion(versionData);
        //    AssemblyVersionInfo versionInfo = program.PrimaryAssembly.GetVersion(versionData);
        //    return versionInfo;
        //}

       
     

        public static TelemetrySummary GetTelemetrySummary(ClientAppUser clientAppUser, ITelemetryAware component)
        {
            TelemetrySummary summary = component.GetTelemetrySummary(clientAppUser.Id);
            if (summary == null)
            {
                summary = component.AddTelemetrySummary(clientAppUser.Id);
            }

            return summary;
        }

        public static async Task<ITelemetryAware> GetEventOrAddIfMissing(ITelemetryUnitOfWork work, string componentName, TelemetryRootObject root)
        {
            Event obj = await work.Events.FirstOrDefaultAsync(x => x.Name == componentName && x.Program.ProgramId == root.ProgramId).ConfigureAwait(false);
            if (obj == null)
            {
                obj = new Event() { Name = componentName, Program = root, ProgramId = root.ProgramId };
                work.Events.Add(obj);
            }

            return obj;
        }

        public static async Task<ITelemetryAware> GetViewOrAddIfMissing(ITelemetryUnitOfWork work, string viewName, TelemetryRootObject program)
        {
            View view = await work.Views.FirstOrDefaultAsync(x => x.Name == viewName && x.Program.ProgramId == program.ProgramId).ConfigureAwait(false);
            if (view == null)
            {
                view = new View { Name = viewName, Program = program, ProgramId = program.ProgramId };
                work.Views.Add(view);
            }

            return view;
        }

   

    }
}