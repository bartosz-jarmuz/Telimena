namespace Telimena.WebApp.Infrastructure.Database
{
    using System;
    using System.Data.Common;
    using System.Data.Entity;
    using Core.Models;
    using Microsoft.AspNet.Identity.EntityFramework;

    public class TelimenaContext : IdentityDbContext<TelimenaUser>
    {

        public TelimenaContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public TelimenaContext() : base("name=DevelopmentDBContext")
        {
            Database.SetInitializer(new TelimenaDbInitializer());
        }

        Type type = typeof(System.Data.Entity.SqlServer.SqlProviderServices) ?? throw new Exception("Do not remove, ensures static reference to System.Data.Entity.SqlServer");

        public TelimenaContext(DbConnection conn) : base(conn, true)
        {
           Database.SetInitializer(new TelimenaDbInitializer());
        }

        public DbSet<Program> Programs { get; set; }
        public DbSet<UpdaterPackageInfo> UpdaterInfo { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<ProgramUsageSummary> ProgramUsages { get; set; }
        public DbSet<ProgramUsageDetail> ProgramUsageDetails { get; set; }

        public DbSet<FunctionUsageSummary> FunctionUsages { get; set; }
        public DbSet<FunctionUsageDetail> FunctionUsageDetails { get; set; }
        public DbSet<ClientAppUser> AppUsers { get; set; }
        public DbSet<DeveloperAccount> Developers { get; set; }
        public DbSet<ProgramAssembly> ProgramAssemblies { get; set; }
        public DbSet<AssemblyVersion> Versions { get; set; }
        public DbSet<TelimenaToolkitData> TelimenaToolkitData { get; set; }
        public DbSet<ProgramUpdatePackageInfo> UpdatePackages { get; set; }
        public DbSet<ProgramPackageInfo> ProgramPackages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProgramAssembly>()
                .HasRequired(a => a.Program)
                .WithMany(c => c.ProgramAssemblies)
                .HasForeignKey(a => a.ProgramId);

            modelBuilder.Entity<Program>()
                .HasOptional(a => a.PrimaryAssembly)
                .WithOptionalPrincipal(u => u.PrimaryOf);

            modelBuilder.Entity<AssemblyVersion>()
                .HasRequired(a => a.ProgramAssembly)
                .WithMany(c => c.Versions)
                .HasForeignKey(a => a.ProgramAssemblyId);

            modelBuilder.Entity<ProgramAssembly>()
                .HasOptional(a => a.LatestVersion)
                .WithOptionalPrincipal(u => u.LatestVersionOf);

            modelBuilder.Entity<TelimenaUser>()
                .HasMany<DeveloperAccount>(s => s.AssociatedDeveloperAccounts)
                .WithMany(c => c.AssociatedUsers).Map(cs =>
                {
                    cs.MapLeftKey("TelimenaUser_Id");
                    cs.MapRightKey("DeveloperAccount_Id");
                    cs.ToTable("TelimenaUserDeveloperAccountAssociations");
                });

            modelBuilder.Entity<DeveloperAccount>()
                .HasMany<TelimenaUser>(s => s.AssociatedUsers)
                .WithMany(c => c.AssociatedDeveloperAccounts).Map(cs =>
                {
                    cs.MapRightKey("TelimenaUser_Id");
                    cs.MapLeftKey("DeveloperAccount_Id");
                    cs.ToTable("TelimenaUserDeveloperAccountAssociations");
                });
        }

    }


}