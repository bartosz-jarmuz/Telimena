using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using DataTables.AspNet.Core;
using DotNetLittleHelpers;
using Newtonsoft.Json;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Core.Models.Telemetry;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Database.Sql.StoredProcedures;
using Telimena.WebApp.Infrastructure.Repository.Implementation;
using Telimena.WebApp.Utils;

namespace Telimena.WebApp.Infrastructure.Repository
{

    public partial class ProgramsDashboardUnitOfWork : IProgramsDashboardUnitOfWork
    {
        public ProgramsDashboardUnitOfWork(TelimenaPortalContext portalContext, TelimenaTelemetryContext telemetryContext)
        {
            this.portalContext = portalContext;
            this.telemetryContext = telemetryContext;
            this.Programs = new ProgramRepository(this.portalContext, telemetryContext);
            this.Views = new ViewRepository(this.telemetryContext);
            this.Events = new Repository<Event>(this.telemetryContext);
            this.UpdatePackages = new UpdatePackageRepository(this.portalContext, null);
            this.ProgramPackages = new ProgramPackageRepository(this.portalContext, null);
            this.Users = new Repository<TelimenaUser>(this.portalContext);
        }

        public IProgramPackageRepository ProgramPackages { get; set; }

        public IUpdatePackageRepository UpdatePackages { get; set; }

        private readonly TelimenaPortalContext portalContext;
        private readonly TelimenaTelemetryContext telemetryContext;
        public IRepository<View> Views { get; }
        public IRepository<Event> Events { get; }

        public IRepository<TelimenaUser> Users { get; }
        public IProgramRepository Programs { get; }


        public async Task<IEnumerable<ProgramSummary>> GetProgramSummary(List<Program> programs)
        {
            List<ProgramSummary> returnData = new List<ProgramSummary>();

            foreach (Program program in programs)
            {
                ProgramSummary summary;
                try
                {
                    List<Event> events = await this.Events.FindAsync(x => x.ProgramId == program.Id).ConfigureAwait(false);
                    List<EventTelemetrySummary> eventSummaries = events.SelectMany(x => x.TelemetrySummaries).ToList();

                    summary = new ProgramSummary
                    {
                        ProgramName = program.Name
                        ,DeveloperName = program.DeveloperTeam?.Name ?? "N/A"
                        ,TelemetryKey = program.TelemetryKey
                        ,RegisteredDate = program.RegisteredDate
                        ,UsersCount = eventSummaries.DistinctBy(x=>x.ClientAppUserId).Count()
                        
                    };
                    ProgramUpdatePackageInfo latestPkg = await this.UpdatePackages.GetLatestPackage(program.Id).ConfigureAwait(false);

                    if (latestPkg != null)
                    {
                        summary.LastUpdateDate = latestPkg.UploadedDate;
                        summary.LatestVersion = latestPkg.SupportedToolkitVersion ?? "?";
                        summary.ToolkitVersion = latestPkg.SupportedToolkitVersion ?? "?";
                    }
                    else
                    {
                        ProgramPackageInfo programPkg = await this.ProgramPackages.GetLatestProgramPackageInfo(program.Id).ConfigureAwait(false);
                        if (programPkg != null)
                        {
                            summary.LastUpdateDate = programPkg.UploadedDate;
                            summary.LatestVersion = programPkg.Version;
                            summary.ToolkitVersion = programPkg.SupportedToolkitVersion;
                        }
                        else
                        {
                            summary.LatestVersion ="N/A";
                            summary.ToolkitVersion = "N/A";

                        }
                    }

                    summary.NumberOfUpdatePackages = await this.UpdatePackages.CountPackages(program.Id).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    summary = new ProgramSummary();
                    summary.ProgramName = program?.Name ?? "Error while loading summary";
                    summary.DeveloperName = "Error while loading summary";
                }

                returnData.Add(summary);
            }

            return returnData;
        }

        public async Task<DataTable> GetDailyActivityScore(Program program, DateTime startDate, DateTime endDate)
        {
            DataSet set = await this.ExecuteStoredProcedure(program, StoredProcedureNames.p_GetDailySummaryCounts, startDate, endDate).ConfigureAwait(false);

            return PrepareDailyActivityScoreTable(startDate, endDate, set.Tables[0].AsEnumerable().ToList()
                , set.Tables[1].AsEnumerable().ToList(), set.Tables[2].AsEnumerable().ToList());
        }

        public async Task<DataTable> GetVersionDistribution(Program program, DateTime startDate, DateTime endDate)
        {
           return (await this.ExecuteStoredProcedure(program, StoredProcedureNames.p_GetVersionUsage, startDate, endDate).ConfigureAwait(false)).Tables[0];
        }

        public async Task<DataTable> GetDailyUsersCount(Program program, DateTime startDate, DateTime endDate)
        {
            DataTable queryResult = (await this.ExecuteStoredProcedure(program, StoredProcedureNames.p_GetDailyUsersCounts, startDate, endDate).ConfigureAwait(false)).Tables[0];

            DataTable data = new DataTable();
            data.Columns.Add("Date");
            data.Columns.Add("Users", typeof(int));

            DateTime dateRecord = startDate;

            List<DataRow> resultsRows = queryResult.AsEnumerable().ToList();

            while (dateRecord.Date <= endDate.Date) //include empty days
            {
                DataRow row = data.NewRow();
                row["Date"] = ToDashboardDateString(dateRecord);
                row["Users"] = resultsRows.FirstOrDefault(x=> (DateTime)x[0] == dateRecord.Date)?[1]??0;
                data.Rows.Add(row);
                dateRecord = dateRecord.AddDays(1);
            }

            return data;
        }

        public async Task<IEnumerable<ProgramUsageSummary>> GetProgramUsagesSummary(List<Program> programs)
        {
            List<ProgramUsageSummary> returnData = new List<ProgramUsageSummary>();

            DataSet set = await this.ExecuteStoredProcedure(StoredProcedureNames.p_GetProgramUsagesSummary
              , this.GetProgramIdsParam(programs)).ConfigureAwait(false);

            List<DataRow> eventsRows = set.Tables[0].AsEnumerable().ToList();
            List<DataRow> viewsRows = set.Tables[1].AsEnumerable().ToList();

            foreach (Program program in programs)
            {
                
                ProgramUsageSummary usageSummary;
                try
                {
                    DataRow programEventSummary = eventsRows.FirstOrDefault(x => (int) x["ProgramId"] == program.Id);
                    DataRow programViewSummary = viewsRows.FirstOrDefault(x => (int)x["ProgramId"] == program.Id);

                    usageSummary = new ProgramUsageSummary
                    {
                        ProgramName = program.Name 
                        , EventsCount = (int)(programEventSummary?["Types"]??0)
                        , ViewsCount = (int)(programViewSummary?["Types"] ?? 0)

                        , TotalViewsUsageCount = (int)(programViewSummary?["Total"] ?? 0)
                        , TotalTodayViewsUsageCount = (int)(programViewSummary?["Todays"] ?? 0)

                        , TotalEventsUsageCount = (int)(programEventSummary?["Total"] ?? 0)
                        , TotalTodayEventsUsageCount = (int)(programEventSummary?["Todays"] ?? 0)

                      };

                    DateTimeOffset lastView = (DateTimeOffset) (programViewSummary?["Last"] ?? default(DateTimeOffset));
                    DateTimeOffset lastEvent = (DateTimeOffset) (programEventSummary?["Last"] ?? default(DateTimeOffset));
                    usageSummary.LastUsage = lastEvent;
                    if (lastView > lastEvent)
                    {
                        usageSummary.LastUsage = lastView;
                    }


                }
                catch (Exception)
                {
                    usageSummary = new ProgramUsageSummary();
                    usageSummary.ProgramName = program?.Name ?? "Error while loading summary";
                }

                returnData.Add(usageSummary);
            }

            return returnData;
        }

        public async Task<PortalSummaryData> GetPortalSummary()
        {
            PortalSummaryData summary = new PortalSummaryData
            {
                TotalUsersCount = await this.portalContext.Users.CountAsync().ConfigureAwait(false)
                , NewestUser = await this.portalContext.Users.OrderByDescending(x => x.UserNumber).FirstAsync().ConfigureAwait(false)
                , LastActiveUser = await this.portalContext.Users.OrderByDescending(x => x.LastLoginDate).FirstAsync().ConfigureAwait(false)
                , UsersActiveInLast24Hrs = await this.portalContext.Users.CountAsync(x =>
                    x.LastLoginDate != null && DbFunctions.DiffDays(DateTime.UtcNow, x.LastLoginDate.Value) < 1).ConfigureAwait(false)
            };
            return summary;
        }

        public async Task<List<AppUsersSummaryData>> GetAppUsersSummary(List<Program> programs, DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null)
            {
                startDate = new DateTime(1970, 01,01);
            }
            if (endDate== null)
            {
                endDate = DateTime.UtcNow.AddDays(1);
            }

            DataSet set = await this.ExecuteStoredProcedure(StoredProcedureNames.p_GetUserActivitySummary
                , startDate.Value
                , endDate.Value
                , this.GetProgramIdsParam(programs)
                ).ConfigureAwait(false);

            List<AppUsersSummaryData> list = new List<AppUsersSummaryData>();
            DataRowCollection rows = set.Tables[0].Rows;
            foreach (DataRow row in rows)
            {
                AppUsersSummaryData data = new AppUsersSummaryData();
                data.ActivityScore = (int) row["ActivityScore"];
                data.UserName = (string) row["UserId"];
                data.FirstSeenDate= (DateTime) row["FirstSeen"];
                data.LastActiveDate= (DateTime) row["LastActive"];
                data.FileVersion = (string) row["FileVersion"];
                list.Add(data);
            }
            

            return list;
        }

        

        public async Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts(List<Program> programs)
        {
            try
            {
                DataSet set = await this.ExecuteStoredProcedure(StoredProcedureNames.p_GetProgramSummaryCounts
                    , this.GetProgramIdsParam(programs)).ConfigureAwait(false);

                if (set.Tables[0]?.Rows.Count < 1)
                {
                    return new AllProgramsSummaryData();
                }
                DataRow dataRow = set.Tables[0].Rows[0];

                AllProgramsSummaryData summary = new AllProgramsSummaryData
                {
                    TotalProgramsCount = programs.Count()
                    , TotalAppUsersCount = (int) (dataRow["UsersCount"] ?? 0)
                    , AppUsersRegisteredLast7DaysCount = (int) (dataRow["NewUsersCount"] ?? 0)
                    , ViewTypesCount = (int) (dataRow["ViewTypes"] ?? 0)
                    , TotalViewsUsageCount = (int) (dataRow["ViewsTotal"] ?? 0)
                    , EventTypesCount = (int) (dataRow["EventTypes"] ?? 0)
                    , TotalEventUsageCount = (int) (dataRow["EventsTotal"] ?? 0)
                    , TotalExceptionsInLast7Days = (int) (dataRow["RecentExceptionsCount"] ?? 0)
                    , MostPopularExceptionInLast7Days = dataRow["MostPopularRecentException"] as string
                    , NewestProgram = programs.OrderByDescending(x => x.Id) /*.Include(x=>x.Developer)*/
                        .FirstOrDefault()
                };
                return summary;

            }
            catch (Exception ex)
            {
                return new AllProgramsSummaryData()
                {
                    MostPopularExceptionInLast7Days = "Error while loading summary counts"
                };
            }


        }

     
        public async Task CompleteAsync()
        {
            await this.portalContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<UsageDataTableResult> GetExceptions(Guid telemetryKey, TelemetryItemTypes itemType, int skip
            , int take, IEnumerable<Tuple<string, bool>> sortBy , ISearch requestSearch 
            , List<string> searchableColumns)
        {
            Program program = await this.portalContext.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                throw new ArgumentException($"Program with key {telemetryKey} does not exist");
            }

            IQueryable<ExceptionInfo> query = this.telemetryContext.Exceptions.Where(x => x.ProgramId == program.Id);
            int totalCount = await this.telemetryContext.Exceptions.CountAsync(x => x.ProgramId == program.Id).ConfigureAwait(false);

            if (take == -1)
            {
                take = totalCount;
            }



            IQueryable<ExceptionInfo> filteredQuery = EntityFilter.Match(query
                , property => property.Contains(requestSearch.Value)
                , new List<Expression<Func<ExceptionInfo, string>>>()
                {
                    {x=>x.Sequence },
                    {x=>x.Message },
                    { x=>x.TypeName},
                    { x=>x.ProgramVersion},
                    {x=>x.ParsedStack},
                    {x=>x. UserName},
                });


           List<ExceptionInfo> ordered = await ApplyOrderingQuery(sortBy, filteredQuery, skip, take).ConfigureAwait(false);

            List<ExceptionData> result = new List<ExceptionData>();
            foreach (ExceptionInfo exception in ordered)
            {
                ExceptionData data = new ExceptionData
                {
                    Timestamp = exception.Timestamp
                    ,UserName = exception.UserName
                    ,EntryKey = exception.TypeName
                    ,Note = exception.Note
                    ,ProgramVersion = exception.ProgramVersion
                    ,Sequence = exception.Sequence
                    ,ErrorMessage = exception.Message
                    ,StackTrace = GetStackTrace(exception.ParsedStack)
                    ,Properties = exception.TelemetryUnits.Where(x=>x.UnitType == TelemetryUnit.UnitTypes.Property).ToDictionary(x => x.Key, x => x.ValueString)
                    ,Metrics = exception.TelemetryUnits.Where(x => x.UnitType == TelemetryUnit.UnitTypes.Metric).ToDictionary(x => x.Key, x => x.ValueDouble)
                };
                result.Add(data);
            }

            return new UsageDataTableResult { TotalCount = totalCount, FilteredCount = totalCount, UsageData = result };
        }


        public async Task<UsageDataTableResult> GetLogs(Guid telemetryKey, int skip, int take, IEnumerable<Tuple<string, bool>> sortBy, string searchPhrase)
        {
            Program program = await this.portalContext.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                throw new ArgumentException($"Program with key {telemetryKey} does not exist");
            }

            IQueryable<LogMessage> query = this.telemetryContext.LogMessages.Where(x => x.ProgramId == program.Id);
            int totalCount = await this.telemetryContext.LogMessages.CountAsync(x => x.ProgramId == program.Id).ConfigureAwait(false);

            if (take == -1)
            {
                take = totalCount;
            }



            IQueryable<LogMessage> filteredQuery = EntityFilter.Match(query
                , property => property.Contains(searchPhrase)
                , new List<Expression<Func<LogMessage, string>>>()
                {
                    {x=>x.Sequence },
                    {x=>x.Message },
                    { x=>x.ProgramVersion},
                    {x=>x. UserName},
                });


            List<LogMessage> ordered = await ApplyOrderingQuery(sortBy, filteredQuery, skip, take).ConfigureAwait(false);

            List<LogMessageData> result = new List<LogMessageData>();
            foreach (LogMessage item in ordered)
            {
                LogMessageData data = new LogMessageData
                {
                    Timestamp = item.Timestamp,
                    UserName = item.UserName,
                    Message= item.Message
                    ,ProgramVersion = item.ProgramVersion
                    ,Sequence = item.Sequence
                    ,LogLevel= item.Level.ToString()
                };
                result.Add(data);
            }

            return new UsageDataTableResult { TotalCount = totalCount, FilteredCount = totalCount, UsageData = result };
        }

        public async Task<UsageDataTableResult> GetSequenceHistory(Guid telemetryKey, string sequenceId, string searchValue)
        {

            Program program = await this.portalContext.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                throw new ArgumentException($"Program with key {telemetryKey} does not exist");
            }

           string sequencePrefix = SequenceIdParser.GetPrefix(sequenceId);
           if (string.IsNullOrEmpty(sequencePrefix))
           {
               return new UsageDataTableResult { TotalCount = 0, FilteredCount = 0, UsageData = new List<DataTableTelemetryData>()};
           }


           IEnumerable<TelemetryDetail> viewQuery = (await this.telemetryContext.ViewTelemetryDetails.Where(x => x.Sequence.StartsWith(sequencePrefix)).ToListAsync().ConfigureAwait(false)).Cast<TelemetryDetail>();
           IEnumerable<TelemetryDetail> eventQuery = (await this.telemetryContext.EventTelemetryDetails.Where(x => x.Sequence.StartsWith(sequencePrefix)).ToListAsync().ConfigureAwait(false)).Cast<TelemetryDetail>();
           List<LogMessage> logsQuery = (await this.telemetryContext.LogMessages.Where(x =>x.ProgramId == program.Id && x.Sequence.StartsWith(sequencePrefix)).ToListAsync().ConfigureAwait(false));
           List<ExceptionInfo> exceptionsQuery = (await this.telemetryContext.Exceptions.Where(x => x.ProgramId == program.Id && x.Sequence.StartsWith(sequencePrefix)).ToListAsync().ConfigureAwait(false));


            List<object> allResults =  viewQuery.Concat(eventQuery).Cast<object>().ToList();
            allResults.AddRange(logsQuery);
            allResults.AddRange(exceptionsQuery);

            int totalCount = allResults.Count;
            
            List<SequenceHistoryData> result = new List<SequenceHistoryData>();
            foreach (object detail in allResults)
            {

                SequenceHistoryData data = this.BuildSequenceItem(detail);
                result.Add(data);
              
            }

            result = result.OrderByDescending(x => x.Order).ToList();

            return new UsageDataTableResult { TotalCount = totalCount, FilteredCount = totalCount, UsageData = result };
        }


        public async Task<UsageDataTableResult> GetProgramUsageData(Guid telemetryKey, TelemetryItemTypes itemType
            , int skip, int take, IEnumerable<Tuple<string, bool>> sortBy, ISearch requestSearch 
            , List<string> searchableColumns)
        {

            Program program = await this.portalContext.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                throw new ArgumentException($"Program with key {telemetryKey} does not exist");
            }

            IQueryable<TelemetryDetail> query;
            int totalCount;
            if (itemType == TelemetryItemTypes.View)
            {
                 query = this.telemetryContext.ViewTelemetryDetails.Where(x => x.TelemetrySummary.View.ProgramId == program.Id);
                 totalCount = await this.telemetryContext.ViewTelemetryDetails.CountAsync(x => x.TelemetrySummary.View.ProgramId == program.Id).ConfigureAwait(false);

            }
            else
            {
                 query = this.telemetryContext.EventTelemetryDetails.Where(x => x.TelemetrySummary.Event.ProgramId == program.Id && !string.IsNullOrEmpty(x.TelemetrySummary.Event.Name)); //todo remove this empty string check after dealing with heartbeat data
                 totalCount = await this.telemetryContext.EventTelemetryDetails.CountAsync(x => x.TelemetrySummary.Event.ProgramId == program.Id && !string.IsNullOrEmpty(x.TelemetrySummary.Event.Name)).ConfigureAwait(false);
            }



            IQueryable<TelemetryDetail> filteredQuery = EntityFilter.Match(query
                , property => property.Contains(requestSearch.Value)
                , new List<Expression<Func<TelemetryDetail, string>>>()
                {
                    {x=>x.Sequence },
                    {x=>x.IpAddress },
                    {x=>x.UserIdentifier },
                    {x=>x.EntryKey },
                });


            if (take == -1)
            {
                take = totalCount;
            }

            List<TelemetryDetail> usages = await ApplyOrderingQuery(sortBy, filteredQuery, skip, take).ConfigureAwait(false);

            List<DataTableTelemetryData> result = new List<DataTableTelemetryData>();
            foreach (TelemetryDetail detail in usages)
            {
                DataTableTelemetryData data = new DataTableTelemetryData()
                {
                    Timestamp = detail.Timestamp
                    , UserName = detail.UserIdentifier
                    , IpAddress = detail.IpAddress
                    , EntryKey = detail.EntryKey
                    , ProgramVersion = detail.FileVersion
                    , Sequence = detail.Sequence
                    ,Properties = detail.GetTelemetryUnits().Where(x => x.UnitType == TelemetryUnit.UnitTypes.Property).ToDictionary(x => x.Key,x=> x.ValueString)
                    ,Metrics = detail.GetTelemetryUnits().Where(x => x.UnitType == TelemetryUnit.UnitTypes.Metric).ToDictionary(x => x.Key,x=> x.ValueDouble)
                };
                result.Add(data);
            }

            return new UsageDataTableResult {TotalCount = totalCount, FilteredCount = totalCount, UsageData = result};
        }

        public async Task<IEnumerable<RawTelemetryUnit>> GetRawData(Guid telemetryKey, TelemetryItemTypes type, DateTime startDate, DateTime endDate)
        {
            Program program = await this.portalContext.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                throw new ArgumentException($"Program with key {telemetryKey} does not exist");
            }

            DataSet set;
            if (type == TelemetryItemTypes.Event)
            {
                set = await this.ExecuteStoredProcedure(program,
                    StoredProcedureNames.p_GetEventTelemetryUnits
                    , startDate
                    , endDate
                ).ConfigureAwait(false);
            }
            else if (type == TelemetryItemTypes.View)
            {
                set = await this.ExecuteStoredProcedure(program,
                    StoredProcedureNames.p_GetViewTelemetryUnits
                    , startDate
                    , endDate
                ).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException($"{type} is not allowed as raw data type export." + 
                                            $"Supported types: {TelemetryItemTypes.Event}, {TelemetryItemTypes.View}");
            }
                

            List<RawTelemetryUnit> list = new List<RawTelemetryUnit>();
            DataRowCollection rows = set.Tables[0].Rows;
            foreach (DataRow row in rows)
            {
                RawTelemetryUnit data = new RawTelemetryUnit()
                {
                    ComponentName = (string) row["EntryKey"],
                    Timestamp = (DateTimeOffset) row["Timestamp"],
                    User= (string) row["UserIdentifier"],
                    Sequence= (string)row["Sequence"],
                    Key= (string) row["Key"],
                    PropertyValue = (string) row["ValueString"],
                    MetricValue= (double) row["ValueDouble"],
                };
                list.Add(data);
            }
            
            return list;

        }

        public async Task<TelemetryInfoTable> GetPivotTableData(TelemetryItemTypes type, Guid telemetryKey, DateTime startDate, DateTime endDate)
        {
            List<RawTelemetryUnit> rows = (await this.GetRawData(telemetryKey, type, startDate, endDate)).ToList();

            TelemetryInfoTableHeader header = new TelemetryInfoTableHeader();
            return new TelemetryInfoTable {Header = header, Rows = rows.Select(x=>new TelemetryPivotTableRow(x)).ToList()};
        }

    }
}