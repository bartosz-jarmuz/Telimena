using System;
using System.Linq;
using System.Web;

namespace Telimena.WebApi
{
    using System.Data.Entity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Identity;

    public class TelimenaContext : IdentityDbContext<TelimenaUser>
    {
        public TelimenaContext() : base("name=TestDBContext")
        {
            Database.SetInitializer(new TelimenaDbInitializer());
        }

        Type type = typeof(System.Data.Entity.SqlServer.SqlProviderServices) ?? throw new Exception("Do not remove, ensures static reference to System.Data.Entity.SqlServer");
        public DbSet<Developer> Developers { get; set; }
        public DbSet<Program> Programs { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<ProgramUsage> ProgramUsages { get; set; }
        public DbSet<FunctionUsage> FunctionUsages { get; set; }
        public DbSet<UserInfo> UserInfos { get; set; }
    }
}