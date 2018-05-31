namespace Telimena.WebApp.Infrastructure.Database
{
    using System;
    using System.Data.Entity;
    using Core.Models;
    using Microsoft.AspNet.Identity.EntityFramework;

    public class TelimenaContext : IdentityDbContext<TelimenaUser>
    {
        public TelimenaContext() : base("name=TestDBContext")
        {
            Database.SetInitializer(new TelimenaDbInitializer());
        }

        Type type = typeof(System.Data.Entity.SqlServer.SqlProviderServices) ?? throw new Exception("Do not remove, ensures static reference to System.Data.Entity.SqlServer");
        public DbSet<Program> Programs { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<ProgramUsage> ProgramUsages { get; set; }
        public DbSet<FunctionUsage> FunctionUsages { get; set; }
        public DbSet<ClientAppUser> AppUsers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Program>()
            //    .HasMany(c => c.Assemblies)
            //    .WithRequired(x=>x.Program)
            //    .WillCascadeOnDelete(true);

            modelBuilder.Entity<Program>()
                .HasOptional(c => c.PrimaryAssembly)
                .WithRequired(x => x.Program)
                .WillCascadeOnDelete(true);
        }

    }


}