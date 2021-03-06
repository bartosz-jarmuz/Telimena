﻿using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Web.Configuration;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Azure.Services.AppAuthentication;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;

namespace Telimena.WebApp.Infrastructure.Database
{
    public class TelimenaPortalContext : IdentityDbContext<TelimenaUser>
    {
        public TelimenaPortalContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public TelimenaPortalContext() : base("name=TelimenaPortalDb")
        {
            System.Data.Entity.Database.SetInitializer(new TelimenaPortalDbInitializer());
        }

        public TelimenaPortalContext(DbConnection conn) : base(conn, true)
        {
            System.Data.Entity.Database.SetInitializer(new TelimenaPortalDbInitializer());
        }

        public TelimenaPortalContext(SqlConnection conn) : base(conn, true)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.ConnectionString = WebConfigurationManager.ConnectionStrings["TelimenaPortalDb"].ConnectionString;
                // DataSource != LocalDB means app is running in Azure with the SQLDB connection string you configured
                if (!conn.DataSource.Equals("(localdb)\\MSSQLLocalDB", StringComparison.InvariantCultureIgnoreCase))
                {
                    conn.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
                }
            }

            System.Data.Entity.Database.SetInitializer(new TelimenaPortalDbInitializer());
        }

        private Type type = typeof(SqlProviderServices) ?? throw new Exception("Do not remove, ensures static reference to System.Data.Entity.SqlServer");

        public DbSet<Program> Programs { get; set; }
        
        public DbSet<DeveloperTeam> Developers { get; set; }
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


            modelBuilder.Entity<AssemblyVersionInfo>().HasRequired(a => a.ProgramAssembly).WithMany(c => c.Versions).HasForeignKey(a => a.ProgramAssemblyId);

            modelBuilder.Entity<TelimenaUser>().HasMany(s => s.AssociatedDeveloperAccounts).WithMany(c => c.AssociatedUsers).Map(cs =>
            {
                cs.MapLeftKey("TelimenaUser_Id");
                cs.MapRightKey("DeveloperAccount_Id");
                cs.ToTable("TelimenaUserDeveloperAccountAssociations");
            });

            modelBuilder.Entity<DeveloperTeam>().HasMany(s => s.AssociatedUsers).WithMany(c => c.AssociatedDeveloperAccounts).Map(cs =>
            {
                cs.MapRightKey("TelimenaUser_Id");
                cs.MapLeftKey("DeveloperAccount_Id");
                cs.ToTable("TelimenaUserDeveloperAccountAssociations");
            });
        }
    }
}