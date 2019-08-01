using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Core.Models.Telemetry;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.UnitOfWork.Implementation
{
    public class RegisterProgramUnitOfWork : IRegisterProgramUnitOfWork
    {
        private readonly TelimenaPortalContext portalContext;
        private readonly TelimenaTelemetryContext telemetryContext;

        public RegisterProgramUnitOfWork(TelimenaTelemetryContext telemetryContext, TelimenaPortalContext portalContext)
        {
            this.telemetryContext = telemetryContext;
            this.portalContext = portalContext;
            this.Users = new UserRepository(portalContext);
            this.Programs = new ProgramRepository(portalContext, telemetryContext);
        }

        public IUserRepository Users { get; }
        public IProgramRepository Programs { get; }

        /// <summary>
        /// Handles adding program to both databases and commits transaction
        /// </summary>
        /// <param name="developerTeam">The developer team.</param>
        /// <param name="program">The program.</param>
        /// <param name="primaryAss">The primary ass.</param>
        /// <returns>Task.</returns>
        public async Task RegisterProgram(DeveloperTeam developerTeam, Program program, ProgramAssembly primaryAss)
        {
            try
            {
                await this.RegisterProgramInternal(developerTeam, program, primaryAss).ConfigureAwait(false);
            }
            catch (Exception)
            {
               
                await this.RegisterProgramInternalFallback(developerTeam, program, primaryAss)
                    .ConfigureAwait(false);
               
            }

        }




        private async Task RegisterProgramInternal(DeveloperTeam developerTeam, Program program, ProgramAssembly primaryAss)
        {

            using (var dbContextTransaction = this.portalContext.Database.BeginTransaction())
            {
                try
                {
                    developerTeam.AddProgram(program);

                    program.PrimaryAssembly = primaryAss;
                    this.portalContext.Programs.Add(program);

                    await this.portalContext.SaveChangesAsync().ConfigureAwait(false);

                    var savedPrg = await this.portalContext.Programs.FirstOrDefaultAsync(x => x.PublicId == program.PublicId).ConfigureAwait(false);

                    var teleRoot = new TelemetryRootObject()
                    {
                        ProgramId = savedPrg.Id,
                        TelemetryKey = savedPrg.TelemetryKey
                    };


                    this.telemetryContext.TelemetryRootObjects.Add(teleRoot);

                    await this.telemetryContext.SaveChangesAsync().ConfigureAwait(false);

                    dbContextTransaction.Commit();
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    throw;
                }
            }
        }

        private async Task RegisterProgramInternalFallback(DeveloperTeam developerTeam, Program program, ProgramAssembly primaryAss)
        {
            using (DbContextTransaction dbContextTransaction = this.portalContext.Database.BeginTransaction())
            {
                List<DbEntityEntry> changedEntities = this.portalContext.ChangeTracker.Entries().ToList();
                changedEntities.AddRange(this.telemetryContext.ChangeTracker.Entries().ToList());
                foreach (DbEntityEntry dbEntityEntry in changedEntities)
                {
                    if (dbEntityEntry.State == EntityState.Added)
                    {
                        dbEntityEntry.State = EntityState.Detached;
                    }
                    else
                    {
                        await dbEntityEntry.ReloadAsync().ConfigureAwait(false);
                    }
                }

                int topPrgId = this.telemetryContext.TelemetryRootObjects.OrderByDescending(x => x.ProgramId)
                                   .FirstOrDefault()?.ProgramId??0;
                topPrgId++;
                program.Id = topPrgId;



                try
                {
                    developerTeam.AddProgram(program);

                    string tblName = nameof(Program) + "s";

                    program.PrimaryAssembly = primaryAss;

                    this.portalContext.Database.ExecuteSqlCommand($"DBCC CHECKIDENT('[dbo].[{tblName}]', RESEED, {topPrgId})");
                    this.portalContext.Programs.Add(program);
                    await this.portalContext.SaveChangesAsync().ConfigureAwait(false);

                    Program savedPrg = await this.portalContext.Programs.FirstOrDefaultAsync(x => x.PublicId == program.PublicId).ConfigureAwait(false);

                    TelemetryRootObject teleRoot = new TelemetryRootObject()
                    {
                        ProgramId = savedPrg.Id,
                        TelemetryKey = savedPrg.TelemetryKey
                    };


                    this.telemetryContext.TelemetryRootObjects.Add(teleRoot);

                    await this.telemetryContext.SaveChangesAsync().ConfigureAwait(false);

                    dbContextTransaction.Commit();
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    throw;
                }
            }
        }
    }
}