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
        public DbSet<Function> Functions { get; set; }
        public DbSet<ProgramUsageSummary> ProgramUsages { get; set; }
        public DbSet<ProgramUsageDetail> ProgramUsageDetails { get; set; }

        public DbSet<FunctionUsageSummary> FunctionUsages { get; set; }
        public DbSet<FunctionUsageDetail> FunctionUsageDetails { get; set; }
        public DbSet<ClientAppUser> AppUsers { get; set; }
        public DbSet<Developer> Developers { get; set; }
        public DbSet<PrimaryAssembly> PrimaryAssemblies { get; set; }
        public DbSet<ReferencedAssembly> ReferencedAssemblies { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Program>()
                .HasOptional(c => c.PrimaryAssembly)
                .WithRequired(x => x.Program)
                .WillCascadeOnDelete(true);


        }

    }


}