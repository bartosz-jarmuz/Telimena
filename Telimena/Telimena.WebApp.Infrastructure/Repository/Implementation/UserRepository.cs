using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    #region Using

    #endregion

    internal class UserRepository : Repository<TelimenaUser>, IUserRepository
    {
        public UserRepository(DbContext dbContext) : base(dbContext)
        {
        }

        private TelimenaContext TelimenaContext => this.DbContext as TelimenaContext;

        public async Task<TelimenaUser> GetByPrincipalAsync(IPrincipal userPrincipal)
        {
            return await this.TelimenaContext.Users.FirstOrDefaultAsync(x => x.UserName == userPrincipal.Identity.Name).ConfigureAwait(false);
        }

        public TelimenaUser GetByPrincipal(IPrincipal userPrincipal)
        {
            return this.TelimenaContext.Users.FirstOrDefault(x => x.UserName == userPrincipal.Identity.Name);
        }
    }
}