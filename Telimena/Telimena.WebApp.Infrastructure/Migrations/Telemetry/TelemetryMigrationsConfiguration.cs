using System;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.WebApp.Infrastructure.Migrations.Telemetry
{
    public sealed class TelemetryMigrationsConfiguration : DbMigrationsConfiguration<Telimena.WebApp.Infrastructure.Database.TelimenaTelemetryContext>
    {
        public TelemetryMigrationsConfiguration()
        {
            this.AutomaticMigrationsEnabled = false;
            this.ContextKey = "Telimena.WebApp.Infrastructure.Database.TelimenaTelemetryContext";
        }

        protected override void Seed(TelimenaTelemetryContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
            this.RecreateProcedures(context);
            this.RecreateFunctions(context);
        }

        private void RecreateFunctions(TelimenaTelemetryContext context)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Database\\Sql\\Functions");
            DirectoryInfo dir = new DirectoryInfo(path);
            var spFiles = dir.GetFiles("*.sql");
            foreach (FileInfo file in spFiles)
            {
                string funcName = Path.GetFileNameWithoutExtension(file.Name);
                try
                {
                    context.Database.ExecuteSqlCommand($"DROP FUNCTION {funcName}");
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("because it does not exist or you do not have permission."))
                    {
                        throw;
                    }
                    //otherwise it's fine, the procedure is new
                }

                var funcText = File.ReadAllText(file.FullName);
                try
                {
                    context.Database.ExecuteSqlCommand(funcText);
                    Debug.WriteLine("Adding function");
                    Debug.WriteLine(funcText);
                }
                catch (Exception ex)
                {
                    Debug.Fail($"Failed to add function [{funcName}]", funcText + "\r\n" +ex);
                }
            }
        }

        private void RecreateProcedures(TelimenaTelemetryContext context)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Database\\Sql\\StoredProcedures");
            DirectoryInfo dir = new DirectoryInfo(path);
            var spFiles = dir.GetFiles("*.sql");
            foreach (FileInfo file in spFiles)
            {
                string spName = Path.GetFileNameWithoutExtension(file.Name);
                try
                {
                    context.Database.ExecuteSqlCommand($"DROP PROCEDURE {spName}");
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("because it does not exist or you do not have permission."))
                    {
                        throw;
                    }
                    //otherwise it's fine, the procedure is new
                }

                var text = File.ReadAllText(file.FullName);
                try
                {
                    context.Database.ExecuteSqlCommand(text);
                    Debug.WriteLine("Adding procedure");
                    Debug.WriteLine(text);
                }
                catch (Exception ex)
                {
                    Debug.Fail($"Failed to add procedure [{spName}]", text + "\r\n" + ex);
                }

            }
        }
            
    }
}
