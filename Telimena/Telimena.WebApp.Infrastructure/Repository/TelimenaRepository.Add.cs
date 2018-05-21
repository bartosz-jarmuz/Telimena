namespace Telimena.WebApp.Infrastructure.Repository
{
    using System;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Models;
    using Database;

    public partial class TelimenaRepository : ITelimenaRepository
    {
        internal TelimenaRepository()
        {
            this.DbContext = new TelimenaContext();
        }

        public TelimenaRepository(TelimenaContext dbContext)
        {
            this.DbContext = dbContext;
        }

        public async Task AddFunction(Function objectToAdd)
        {
            objectToAdd.RegisteredDate = DateTime.UtcNow;
            this.DbContext.Functions.Add(objectToAdd);
            await this.DbContext.SaveChangesAsync();
        }

        public TelimenaContext DbContext { get; }

        public async Task AddFunctionUsage(FunctionUsage objectToAdd)
        {
            this.DbContext.FunctionUsages.Add(objectToAdd);
            await this.DbContext.SaveChangesAsync();
        }

        public async Task AddProgram(Program objectToAdd)
        {
            objectToAdd.RegisteredDate = DateTime.UtcNow;
            this.DbContext.Programs.Add(objectToAdd);
            await this.DbContext.SaveChangesAsync();
        }

        public async Task AddProgramUsage(ProgramUsage objectToAdd)
        {
            this.DbContext.ProgramUsages.Add(objectToAdd);
            await this.DbContext.SaveChangesAsync();
        }

        public async Task AddUserInfo(ClientAppUser objectToAdd)
        {
            objectToAdd.RegisteredDate = DateTime.UtcNow;

            this.DbContext.AppUsers.Add(objectToAdd);
            await this.DbContext.SaveChangesAsync();
        }


    }
}