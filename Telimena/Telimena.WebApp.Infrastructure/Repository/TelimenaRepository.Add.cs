namespace Telimena.WebApp.Infrastructure.Repository
{
    #region Using
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Models;
    using WebApi;
    #endregion

    public partial class TelimenaRepository : ITelimenaRepository
    {
        protected readonly TelimenaContext DbContext = new TelimenaContext();

        public async Task AddFunction(Function objectToAdd)
        {
            this.DbContext.Functions.Add(objectToAdd);
            await this.DbContext.SaveChangesAsync();
        }

        public async Task AddFunctionUsage(FunctionUsage objectToAdd)
        {
            this.DbContext.FunctionUsages.Add(objectToAdd);
            await this.DbContext.SaveChangesAsync();
        }

        public async Task AddProgram(Program objectToAdd)
        {
            this.DbContext.Programs.Add(objectToAdd);
            await this.DbContext.SaveChangesAsync();
        }

        public async Task AddProgramUsage(ProgramUsage objectToAdd)
        {
            this.DbContext.ProgramUsages.Add(objectToAdd);
            await this.DbContext.SaveChangesAsync();
        }

        public async Task AddUserInfo(UserInfo objectToAdd)
        {
            this.DbContext.UserInfos.Add(objectToAdd);
            await this.DbContext.SaveChangesAsync();
        }

    
    }
}