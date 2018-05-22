namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using AutoMapper;
    using Client;
    using Core.Models;
    using Database;
    #endregion

    public partial class ProgramRepository : Repository<Program>, IProgramRepository
    {
        public ProgramRepository(DbContext dbContext) : base(dbContext)
        {
        }

        private TelimenaContext TelimenaContext => this.DbContext as TelimenaContext;

        public override void Add(Program objectToAdd)
        {
            objectToAdd.RegisteredDate = DateTime.UtcNow;
            this.TelimenaContext.Programs.Add(objectToAdd);
        }

        public void AddProgramUsage(ProgramUsage objectToAdd)
        {
            this.TelimenaContext.ProgramUsages.Add(objectToAdd);
        }

        public IEnumerable<Program> GetProgramsByDeveloperName(string developerName)
        {
            return this.TelimenaContext.Programs.Include(x => x.Developer).Where(x => x.Developer != null && x.Developer.Name == developerName);
        }

        public Program GetProgramOrAddIfNotExists(string programName)
        {
            var program = this.TelimenaContext.Programs.FirstOrDefault(x => x.Name == programName);
            if (program == null)
            {
                program = new Program()
                {
                    Name = programName
                };
                this.Add(program);
            }

            return program;
        }

        public Program GetProgramOrAddIfNotExists(ProgramInfo programDto)
        {
            var program = this.TelimenaContext.Programs.FirstOrDefault(x => x.Name == programDto.Name);
            if (program == null)
            {
                program = Mapper.Map<Program>(programDto);
                this.Add(program);
            }

            return program;
        }

        public ProgramUsage GetProgramUsageOrAddIfNotExists(Program program, ClientAppUser clientAppUser)
        {
            var usageData = this.TelimenaContext.ProgramUsages.FirstOrDefault(x => x.Program.Name == program.Name && x.ClientAppUser.UserName == clientAppUser.UserName);
            if (usageData == null)
            {
                usageData = new ProgramUsage()
                {
                    Program = program,
                    ClientAppUser = clientAppUser,
                };
                this.AddProgramUsage(usageData);
            }

            return usageData;
        }
    }
}