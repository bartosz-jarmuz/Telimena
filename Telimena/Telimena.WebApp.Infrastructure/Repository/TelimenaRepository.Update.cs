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
        public async Task<FunctionUsage> IncrementFunctionUsage(FunctionUsage usage)
        {
            FunctionUsage entity = this.DbContext.FunctionUsages.SingleOrDefault(x=>x.Id == usage.Id);
            entity.Count++;
            entity.DateTime = DateTime.Today;
            await this.DbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<ProgramUsage> IncrementProgramUsage(ProgramUsage usage)
        {
            ProgramUsage entity = this.DbContext.ProgramUsages.SingleOrDefault(x => x.Id == usage.Id);
            entity.Count++;
            entity.DateTime = DateTime.Today;
            await this.DbContext.SaveChangesAsync();
            return entity;
        }
    }
}