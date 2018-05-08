namespace Telimena.WebApp.Infrastructure.Repository
{
    #region Using
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Models;
    using WebApi;
    #endregion

    public partial class TelimenaRepository : ITelimenaRepository
    {
        public async Task<IEnumerable<Program>> GetDeveloperPrograms(string developerName)
        {
            return (await this.DbContext.Programs.Where(x => x.Developer != null && x.Developer.Name == developerName).ToListAsync());
        }
    }
}