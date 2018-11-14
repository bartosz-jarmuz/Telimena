using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using TelimenaClient;

namespace Telimena.WebApp.Controllers.Api
{

    public class TelemetryHelperService
    {
        public TelemetryHelperService(ITelemetryUnitOfWork work)
        {
            this.work = work;
        }

        private readonly ITelemetryUnitOfWork work;

        public async Task<Event> GetEventOrAddIfNotExists(string componentName, Program program)
        {
            Event obj = await this.work.Events.FirstOrDefaultAsync(x => x.Name == componentName && x.Program.Name == program.Name);
            if (obj == null)
            {
                obj = new Event() { Name = componentName, Program = program, ProgramId = program.Id };
                this.work.Events.Add(obj);
            }

            return obj;
        }

        public async Task<View> GetViewOrAddIfNotExists(string viewName, Program program)
        {
            View view = await this.work.Views.FirstOrDefaultAsync(x => x.Name == viewName && x.Program.Name == program.Name);
            if (view == null)
            {
                view = new View {Name = viewName, Program = program, ProgramId = program.Id};
                this.work.Views.Add(view);
            }

            return view;
        }

        public async Task<Program> GetProgramOrAddIfNotExists(RegistrationRequest requestProgramInfo)
        {
            Program program = await this.work.Programs.FirstOrDefaultAsync(x => x.Name == requestProgramInfo.ProgramInfo.Name);
            if (program == null)
            {
                program = Mapper.Map<Program>(requestProgramInfo.ProgramInfo);
                this.work.Programs.Add(program);
            }

            AssemblyInfo primaryAss = requestProgramInfo.ProgramInfo.PrimaryAssembly;
            program.PrimaryAssembly.AddVersion(primaryAss.Version, primaryAss.FileVersion);

            if (requestProgramInfo.ProgramInfo.HelperAssemblies.AnyAndNotNull())
            {
                foreach (AssemblyInfo helperAssembly in requestProgramInfo.ProgramInfo.HelperAssemblies)
                {
                    ProgramAssembly existingAssembly = program.ProgramAssemblies.FirstOrDefault(x => x.Name == helperAssembly.Name);
                    if (existingAssembly == null)
                    {
                        existingAssembly = Mapper.Map<ProgramAssembly>(helperAssembly);
                        program.ProgramAssemblies.Add(existingAssembly);
                    }

                    existingAssembly.AddVersion(helperAssembly.Version, helperAssembly.FileVersion);
                }
            }

            await this.AssignToolkitVersion(program.PrimaryAssembly, primaryAss.Version, primaryAss.FileVersion, requestProgramInfo.TelimenaVersion);
            await this.EnsureDeveloperSet(requestProgramInfo.ProgramInfo, program);
            return program;
        }

        public async Task<ClientAppUser> GetUserInfoOrAddIfNotExists(UserInfo userDto, string ip)
        {
            ClientAppUser user = await this.work.ClientAppUsers.FirstOrDefaultAsync(x => x.UserName == userDto.UserName);
            if (user == null)
            {
                user = Mapper.Map<ClientAppUser>(userDto);
                user.RegisteredDate = DateTime.UtcNow;
                user.IpAddresses.Add(ip);
                this.work.ClientAppUsers.Add(user);
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

        private async Task AssignToolkitVersion(ProgramAssembly programAssembly, string programVersion, string fileVersion, string toolkitVersion)
        {
            TelimenaToolkitData toolkitData = await this.work.ToolkitData.FirstOrDefaultAsync(x => x.Version == toolkitVersion);
            AssemblyVersionInfo assemblyVersionInfo = programAssembly.GetVersion(programVersion, fileVersion);
            if (toolkitData == null)
            {
                toolkitData = new TelimenaToolkitData(toolkitVersion);
            }

            assemblyVersionInfo.ToolkitData = toolkitData;
        }

        private async Task EnsureDeveloperSet(ProgramInfo info, Program program)
        {
            if (program.DeveloperAccount == null && info.DeveloperId != null)
            {
                DeveloperAccount dev = await this.work.Developers.FirstOrDefaultAsync(x => x.Id == info.DeveloperId);
                if (dev != null)
                {
                    program.DeveloperAccount = dev;
                }
            }
        }
    }
}