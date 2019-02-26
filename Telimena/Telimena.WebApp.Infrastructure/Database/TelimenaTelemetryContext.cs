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
    public class TelimenaTelemetryContext : DbContext
    {
        public TelimenaTelemetryContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public TelimenaTelemetryContext() : base("name=TelimenaTelemetryDb")
        {
            System.Data.Entity.Database.SetInitializer(new TelimenaTelemetryDbInitializer());
        }

        public TelimenaTelemetryContext(DbConnection conn) : base(conn, true)
        {
            System.Data.Entity.Database.SetInitializer(new TelimenaTelemetryDbInitializer());
        }

        public TelimenaTelemetryContext(SqlConnection conn) : base(conn, true)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.ConnectionString = WebConfigurationManager.ConnectionStrings["TelimenaTelemetryDb"].ConnectionString;
                // DataSource != LocalDB means app is running in Azure with the SQLDB connection string you configured
                if (!conn.DataSource.Equals("(localdb)\\MSSQLLocalDB", StringComparison.InvariantCultureIgnoreCase))
                {
                    conn.AccessToken = new AzureServiceTokenProvider()
                        .GetAccessTokenAsync("https://database.windows.net/").Result;
                }
            }

            System.Data.Entity.Database.SetInitializer(new TelimenaTelemetryDbInitializer());
        }

        private Type type = typeof(SqlProviderServices) ??
                            throw new Exception(
                                "Do not remove, ensures static reference to System.Data.Entity.SqlServer");
        public DbSet<View> Views { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<ViewTelemetrySummary> ViewTelemetrySummaries { get; set; }
        public DbSet<ViewTelemetryDetail> ViewTelemetryDetails { get; set; }
        public DbSet<EventTelemetryDetail> EventTelemetryDetails { get; set; }
        public DbSet<ViewTelemetryUnit> ViewTelemetryUnits { get; set; }
        public DbSet<TelemetryMonitoredProgram> Programs { get; set; }
        public DbSet<ClientAppUser> AppUsers { get; set; }

        public DbSet<ExceptionInfo> Exceptions { get; set; }
        public DbSet<LogMessage> LogMessages { get; set; }

    }
}