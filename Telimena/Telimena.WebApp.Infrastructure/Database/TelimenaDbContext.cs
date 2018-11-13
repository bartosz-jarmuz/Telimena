using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Web.Configuration;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Azure.Services.AppAuthentication;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Infrastructure.Database
{
    public class TelimenaContext : IdentityDbContext<TelimenaUser>
    {

        public TelimenaContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public TelimenaContext() : base("name=DevelopmentDBContext")
        {
            System.Data.Entity.Database.SetInitializer(new TelimenaDbInitializer());
        }

        public TelimenaContext(DbConnection conn) : base(conn, true)
        {
            System.Data.Entity.Database.SetInitializer(new TelimenaDbInitializer());
        }

        public TelimenaContext(SqlConnection conn) : base(conn, true)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.ConnectionString = WebConfigurationManager.ConnectionStrings["DevelopmentDBContext"].ConnectionString;
                // DataSource != LocalDB means app is running in Azure with the SQLDB connection string you configured
                if (!conn.DataSource.Equals("(localdb)\\MSSQLLocalDB", StringComparison.InvariantCultureIgnoreCase))
                {
                    conn.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
                }
            }

            System.Data.Entity.Database.SetInitializer(new TelimenaDbInitializer());
        }

        private Type type = typeof(SqlProviderServices) ?? throw new Exception("Do not remove, ensures static reference to System.Data.Entity.SqlServer");

        public DbSet<Program> Programs { get; set; }
        public DbSet<View> Views { get; set; }
        public DbSet<ProgramTelemetrySummary> ProgramUsages { get; set; }
        public DbSet<ProgramTelemetryDetail> ProgramUsageDetails { get; set; }
        public DbSet<ProgramTelemetryUnit> ProgramTelemetryUnits { get; set; }
        public DbSet<ViewTelemetrySummary> ViewUsages { get; set; }
        public DbSet<ViewTelemetryDetail> ViewUsageDetails { get; set; }
        public DbSet<ViewTelemetryUnit> ViewTelemetryUnits { get; set; }
        public DbSet<ClientAppUser> AppUsers { get; set; }
        public DbSet<DeveloperAccount> Developers { get; set; }
        public DbSet<ProgramAssembly> ProgramAssemblies { get; set; }
        public DbSet<AssemblyVersionInfo> Versions { get; set; }
        public DbSet<TelimenaToolkitData> TelimenaToolkitData { get; set; }
        public DbSet<TelimenaPackageInfo> ToolkitPackages { get; set; }
        public DbSet<ProgramUpdatePackageInfo> UpdatePackages { get; set; }
        public DbSet<UpdaterPackageInfo> UpdaterPackages { get; set; }
        public DbSet<Updater> Updaters { get; set; }

        public DbSet<ProgramPackageInfo> ProgramPackages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProgramAssembly>().HasRequired(a => a.Program).WithMany(c => c.ProgramAssemblies).HasForeignKey(a => a.ProgramId);

            modelBuilder.Entity<Program>().HasOptional(a => a.PrimaryAssembly).WithOptionalPrincipal(u => u.PrimaryOf);

            modelBuilder.Entity<AssemblyVersionInfo>().HasRequired(a => a.ProgramAssembly).WithMany(c => c.Versions).HasForeignKey(a => a.ProgramAssemblyId);

            modelBuilder.Entity<ProgramAssembly>().HasOptional(a => a.LatestVersion).WithOptionalPrincipal(u => u.LatestVersionOf);

            modelBuilder.Entity<TelimenaUser>().HasMany(s => s.AssociatedDeveloperAccounts).WithMany(c => c.AssociatedUsers).Map(cs =>
            {
                cs.MapLeftKey("TelimenaUser_Id");
                cs.MapRightKey("DeveloperAccount_Id");
                cs.ToTable("TelimenaUserDeveloperAccountAssociations");
            });

            modelBuilder.Entity<DeveloperAccount>().HasMany(s => s.AssociatedUsers).WithMany(c => c.AssociatedDeveloperAccounts).Map(cs =>
            {
                cs.MapRightKey("TelimenaUser_Id");
                cs.MapLeftKey("DeveloperAccount_Id");
                cs.ToTable("TelimenaUserDeveloperAccountAssociations");
            });
        }
    }
}