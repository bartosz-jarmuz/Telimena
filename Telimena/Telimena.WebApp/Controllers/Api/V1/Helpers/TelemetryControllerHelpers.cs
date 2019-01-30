﻿using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DotNetLittleHelpers;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using UserInfo = Telimena.WebApp.Core.DTO.MappableToClient.UserInfo;

namespace Telimena.WebApp.Controllers.Api.V1
{
    internal static class TelemetryControllerHelpers
    {


        private static async Task<ITelemetryAware> GetTrackedComponent(ITelemetryUnitOfWork work, TelemetryItemTypes itemType, string key, Program program)
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

        public static async Task<List<TelemetrySummary>> InsertData(ITelemetryUnitOfWork work, List<TelemetryItem> units, Guid telemetryKey, string ipAddress)
        {

            (Program program, ClientAppUser clientAppUser) actionItems = await GetTelemetryUpdateActionItems(work, telemetryKey, units.First().UserId, ipAddress).ConfigureAwait(false);
          

            IEnumerable<IGrouping<TelemetryItemTypes, TelemetryItem>> typeGroupings = units.GroupBy(x => x.TelemetryItemType);
            List<TelemetrySummary> summaries = new List<TelemetrySummary>();
            foreach (IGrouping<TelemetryItemTypes, TelemetryItem> typeGrouping in typeGroupings)
            {
                foreach (IGrouping<string, TelemetryItem> keyGroupings in typeGrouping.GroupBy(x=>x.EntryKey))
                {
                    ITelemetryAware trackedComponent = await GetTrackedComponent(work, typeGrouping.Key, keyGroupings.Key, actionItems.program).ConfigureAwait(false);
                    TelemetrySummary summary = GetTelemetrySummary(actionItems.clientAppUser, trackedComponent);
                    AssemblyVersionInfo versionInfoInfo = GetAssemblyVersionInfoOrAddIfMissing(keyGroupings.First().VersionData, actionItems.program);
                    foreach (TelemetryItem telemetryItem in keyGroupings)
                    {
                        summary.UpdateTelemetry(versionInfoInfo, ipAddress, telemetryItem); 
                    }
                    summaries.Add(summary);

                }
            }

            await work.CompleteAsync().ConfigureAwait(false);
            return summaries;
        }

        public static async Task<ClientAppUser> GetUserOrAddIfMissing(ITelemetryUnitOfWork work, UserInfo userDto, string ip)
        {
            ClientAppUser user = await work.ClientAppUsers.FirstOrDefaultAsync(x => x.UserId == userDto.UserId).ConfigureAwait(false);
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

        public static async Task<ClientAppUser> GetUserOrAddIfMissing(ITelemetryUnitOfWork work, string userId, string ip)
        {
            ClientAppUser user = await work.ClientAppUsers.FirstOrDefaultAsync(x => x.UserId == userId).ConfigureAwait(false);
            if (user == null)
            {
                user = new ClientAppUser();
                user.UserId = userId;
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
        private static void UpdateAssemblyInfo(ProgramAssembly existing, AssemblyInfo assemblyInfo)
        {
            existing.Company = assemblyInfo.Company;
            existing.Copyright = assemblyInfo.Copyright;
            existing.Description = assemblyInfo.Description;
            existing.FullName = assemblyInfo.FullName;
            existing.Product = assemblyInfo.Product;
            existing.Title= assemblyInfo.Title;
            existing.Trademark= assemblyInfo.Trademark;

        }

        public static async Task RecordVersions(ITelemetryUnitOfWork work, Program program, TelemetryInitializeRequest request)
        {
            AssemblyInfo primaryAss = request.ProgramInfo.PrimaryAssembly;
            program.PrimaryAssembly.AddVersion(primaryAss.VersionData);

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
                    else
                    {
                        UpdateAssemblyInfo(existingAssembly, helperAssembly);
                    }

                    existingAssembly.AddVersion(helperAssembly.VersionData);
                }
            }

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


        public static AssemblyVersionInfo GetAssemblyVersionInfoOrAddIfMissing(VersionData versionData, Program program)
        {
            program.PrimaryAssembly.AddVersion(versionData);
            AssemblyVersionInfo versionInfo = program.PrimaryAssembly.GetVersion(versionData);
            return versionInfo;
        }

        public static async Task<(Program program, ClientAppUser user)> GetTelemetryUpdateActionItems(ITelemetryUnitOfWork work, Guid telemetryKey, string userId, string ip)
        {

            Program program = await work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);
            if (program == null)
            {
                throw new InvalidOperationException($"Program [{telemetryKey}] is null") ;
            }

            ClientAppUser clientAppUser = await GetUserOrAddIfMissing(work, userId, ip).ConfigureAwait(false);

            return (program, clientAppUser);
        }

        public static async Task<(bool isValid, TelemetryInitializeResponse response, Program program)> GetTelemetryInitializeActionItems(ITelemetryUnitOfWork work, TelemetryInitializeRequest request)
        {
            if (!ApiRequestsValidator.IsRequestValid(request))
            {
                return (false, new TelemetryInitializeResponse() { Exception = new BadRequestException("Request is not valid") }, null);
            }

            Program program = await work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == request.TelemetryKey).ConfigureAwait(false);
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
            Event obj = await work.Events.FirstOrDefaultAsync(x => x.Name == componentName && x.Program.Name == program.Name).ConfigureAwait(false);
            if (obj == null)
            {
                obj = new Event() { Name = componentName, Program = program, ProgramId = program.Id };
                work.Events.Add(obj);
            }

            return obj;
        }

        public static async Task<ITelemetryAware> GetViewOrAddIfMissing(ITelemetryUnitOfWork work, string viewName, Program program)
        {
            View view = await work.Views.FirstOrDefaultAsync(x => x.Name == viewName && x.Program.Name == program.Name).ConfigureAwait(false);
            if (view == null)
            {
                view = new View { Name = viewName, Program = program, ProgramId = program.Id };
                work.Views.Add(view);
            }

            return view;
        }

        public static void SetPrimaryAssembly(Program program, TelemetryInitializeRequest request)
        {
            if (program.PrimaryAssembly == null)
            {
                program.PrimaryAssembly = Mapper.Map<ProgramAssembly>(request.ProgramInfo.PrimaryAssembly);
                program.PrimaryAssembly.ProgramId = program.Id;
            }
            else
            {
                UpdateAssemblyInfo(program.PrimaryAssembly, request.ProgramInfo.PrimaryAssembly);
            }
        }

    }
}