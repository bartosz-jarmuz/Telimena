using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using DotNetLittleHelpers;
using Newtonsoft.Json;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.Implementation;
using Telimena.WebApp.Utils;

namespace Telimena.WebApp.Infrastructure.Repository
{
    #region Using

    #endregion

    public class ProgramsDashboardUnitOfWork : IProgramsDashboardUnitOfWork
    {
        private class OrderingMethodFinder : ExpressionVisitor
        {
            private bool _orderingMethodFound;

            public static bool OrderMethodExists(Expression expression)
            {
                OrderingMethodFinder visitor = new OrderingMethodFinder();
                visitor.Visit(expression);
                return visitor._orderingMethodFound;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                string name = node.Method.Name;

                if (node.Method.DeclaringType == typeof(Queryable) &&
                    (name.StartsWith("OrderBy", StringComparison.Ordinal) || name.StartsWith("ThenBy", StringComparison.Ordinal)))
                {
                    this._orderingMethodFound = true;
                }

                return base.VisitMethodCall(node);
            }
        }

        public ProgramsDashboardUnitOfWork(TelimenaContext context)
        {
            this.context = context;
            this.Programs = new ProgramRepository(this.context);
            this.Views = new ViewRepository(this.context);
            this.Users = new Repository<TelimenaUser>(this.context);
        }

        private readonly TelimenaContext context;
        public IRepository<View> Views { get; }

        public IRepository<TelimenaUser> Users { get; }
        public IProgramRepository Programs { get; }

        public async Task<IEnumerable<ProgramSummary>> GetProgramsSummary(List<Program> programs)
        {
            List<ProgramSummary> returnData = new List<ProgramSummary>();

            foreach (Program program in programs)
            {
                List<View> views = await this.Views.FindAsync(x => x.ProgramId == program.Id).ConfigureAwait(false);
                ProgramSummary summary;
                try
                {
                    summary = new ProgramSummary
                    {
                        ProgramName = program.Name
                        , DeveloperName = program.DeveloperAccount?.Name ?? "N/A"
                        , LatestVersion = program.PrimaryAssembly?.GetLatestVersion()?.AssemblyVersion
                        , AssociatedToolkitVersion = program.PrimaryAssembly?.GetLatestVersion()?.ToolkitData?.Version
                        , TelemetryKey = program.TelemetryKey
                        , RegisteredDate = program.RegisteredDate
                        //todo - program usage remoed, this should then be caluclated some other way
                        //, LastUsage = program.TelemetrySummaries.MaxOrNull(x => x.LastTelemetryUpdateTimestamp)
                        //, UsersCount = program.TelemetrySummaries.Count
                        //, TodayUsageCount =
                        //    program.TelemetrySummaries.Where(x => (DateTime.UtcNow - x.LastTelemetryUpdateTimestamp).TotalHours <= 24).Sum(smr =>
                        //        smr.GetTelemetryDetails().Count(detail => (DateTime.UtcNow - detail.Timestamp).TotalHours <= 24))
                        //, TotalUsageCount = program.TelemetrySummaries.Sum(x => x.SummaryCount)
                        , ViewsCount = views.Count
                        , TotalViewsUsageCount = views.Sum(f => f.TelemetrySummaries.Sum(s => s.SummaryCount))
                        , TotalTodayViewsUsageCount = views.Sum(f =>
                            f.TelemetrySummaries.Where(x => (DateTime.UtcNow - x.LastTelemetryUpdateTimestamp).TotalHours <= 24).Sum(smr =>
                                smr.TelemetryDetails.Count(detail => (DateTime.UtcNow - detail.Timestamp).TotalHours <= 24)))
                    };
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

        public async Task<PortalSummaryData> GetPortalSummary()
        {
            PortalSummaryData summary = new PortalSummaryData
            {
                TotalUsersCount = await this.context.Users.CountAsync().ConfigureAwait(false)
                , NewestUser = await this.context.Users.OrderByDescending(x => x.UserNumber).FirstAsync().ConfigureAwait(false)
                , LastActiveUser = await this.context.Users.OrderByDescending(x => x.LastLoginDate).FirstAsync().ConfigureAwait(false)
                , UsersActiveInLast24Hrs = await this.context.Users.CountAsync(x =>
                    x.LastLoginDate != null && DbFunctions.DiffDays(DateTime.UtcNow, x.LastLoginDate.Value) < 1).ConfigureAwait(false)
            };
            return summary;
        }

        public async Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts(List<Program> programs)
        {
            IEnumerable<int> programIds = programs.Select(x => x.Id);
            List<View> views = programs.SelectMany(x => x.Views).ToList();
            IEnumerable<int> viewIds = views.Select(x => x.Id);
            List<ViewTelemetrySummary> viewTelemetrySummaries =
                await this.context.ViewTelemetrySummaries.Where(usg => viewIds.Contains(usg.ViewId)).ToListAsync().ConfigureAwait(false);
            //List<ClientAppUser> users = programUsageSummaries.DistinctBy(x => x.ClientAppUserId).Select(x => x.ClientAppUser).ToList();
            AllProgramsSummaryData summary = new AllProgramsSummaryData
            {
                TotalProgramsCount = programs.Count()
                //, TotalAppUsersCount = users.Count()
                //, AppUsersRegisteredLast7DaysCount = users.Count(x => (DateTime.UtcNow - x.FirstSeenDate).TotalDays <= 7)
                , TotalViewsCount = views.Count()
            };
            //int? value = programUsageSummaries.Sum(x => (int?) x.GetTelemetryDetails().Count()) ?? 0;
            //summary.TotalProgramUsageCount = value ?? 0;
            //value = viewTelemetrySummaries.Sum(x => (int?) x.TelemetryDetails.Count) ?? 0;
            //summary.TotalViewsUsageCount = value ?? 0;

            summary.NewestProgram = programs.OrderByDescending(x => x.Id) /*.Include(x=>x.Developer)*/.FirstOrDefault();

            return summary;
        }

        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<UsageDataTableResult> GetExceptions(Guid telemetryKey, TelemetryItemTypes itemType, int skip
            , int take, IEnumerable<Tuple<string, bool>> sortBy , ISearch requestSearch 
            , List<string> searchableColumns)
        {
            Program program = await this.context.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                throw new ArgumentException($"Program with key {telemetryKey} does not exist");
            }

            IQueryable<ExceptionInfo> query = this.context.Exceptions.Where(x => x.ProgramId == program.Id);
            int totalCount = await this.context.Exceptions.CountAsync(x => x.ProgramId == program.Id).ConfigureAwait(false);

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
                    ,ProgramVersion = exception.ProgramVersion
                    ,Sequence = exception.Sequence
                    ,ErrorMessage = exception.Message
                    ,StackTrace = GetStackTrace(exception.ParsedStack)
                    ,
                };
                result.Add(data);
            }

            return new UsageDataTableResult { TotalCount = totalCount, FilteredCount = totalCount, UsageData = result };
        }

        public async Task<UsageDataTableResult> GetLogs(Guid telemetryKey, int skip, int take, IEnumerable<Tuple<string, bool>> sortBy, string searchPhrase)
        {
            Program program = await this.context.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                throw new ArgumentException($"Program with key {telemetryKey} does not exist");
            }

            IQueryable<LogMessage> query = this.context.LogMessages.Where(x => x.ProgramId == program.Id);
            int totalCount = await this.context.LogMessages.CountAsync(x => x.ProgramId == program.Id).ConfigureAwait(false);

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

            Program program = await this.context.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                throw new ArgumentException($"Program with key {telemetryKey} does not exist");
            }

           string sequencePrefix = SequenceIdParser.GetPrefix(sequenceId);
           if (string.IsNullOrEmpty(sequencePrefix))
           {
               return new UsageDataTableResult { TotalCount = 0, FilteredCount = 0, UsageData = new List<DataTableTelemetryDataBase>()};
            }
            IEnumerable<TelemetryDetail> viewQuery = (await this.context.ViewTelemetryDetails.Where(x => x.Sequence.StartsWith(sequencePrefix)).ToListAsync().ConfigureAwait(false)).Cast<TelemetryDetail>();
           IEnumerable<TelemetryDetail> eventQuery = (await this.context.EventTelemetryDetails.Where(x => x.Sequence.StartsWith(sequencePrefix)).ToListAsync().ConfigureAwait(false)).Cast<TelemetryDetail>();
           List<LogMessage> logsQuery = (await this.context.LogMessages.Where(x => x.Sequence.StartsWith(sequencePrefix)).ToListAsync().ConfigureAwait(false));
           List<ExceptionInfo> exceptionsQuery = (await this.context.Exceptions.Where(x => x.Sequence.StartsWith(sequencePrefix)).ToListAsync().ConfigureAwait(false));


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


        private SequenceHistoryData BuildSequenceItem(object item)
        {
            SequenceHistoryData data= null;
            if (item is TelemetryDetail telemetryDetail)
            {
                data= new SequenceHistoryData(telemetryDetail);
                data.Order = SequenceIdParser.GetOrder(telemetryDetail.Sequence);
            }
            else if (item is LogMessage logMessage)
            {
                data = new SequenceHistoryData(logMessage);
                data.Order = SequenceIdParser.GetOrder(logMessage.Sequence);

            }
            else if (item is ExceptionInfo error)
            {
                data = new SequenceHistoryData(error);
                data.Order = SequenceIdParser.GetOrder(error.Sequence);

            }

            return data;
        }

        private List<TelemetryItem.ExceptionInfo.ParsedStackTrace> GetStackTrace(string input)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<TelemetryItem.ExceptionInfo.ParsedStackTrace>>(input);
            }
            catch
            {
                //failsafe approach
                return new List<TelemetryItem.ExceptionInfo.ParsedStackTrace>(){new TelemetryItem.ExceptionInfo.ParsedStackTrace(){Method = input}};
            }
        }

        public async Task<UsageDataTableResult> GetProgramUsageData(Guid telemetryKey, TelemetryItemTypes itemType
            , int skip, int take, IEnumerable<Tuple<string, bool>> sortBy, ISearch requestSearch 
            , List<string> searchableColumns)
        {

            Program program = await this.context.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                throw new ArgumentException($"Program with key {telemetryKey} does not exist");
            }

            IQueryable<TelemetryDetail> query;
            int totalCount;
            if (itemType == TelemetryItemTypes.View)
            {
                 query = this.context.ViewTelemetryDetails.Where(x => x.TelemetrySummary.View.ProgramId == program.Id);
                 totalCount = await this.context.ViewTelemetryDetails.CountAsync(x => x.TelemetrySummary.View.ProgramId == program.Id).ConfigureAwait(false);

            }
            else
            {
                 query = this.context.EventTelemetryDetails.Where(x => x.TelemetrySummary.Event.ProgramId == program.Id);
                 totalCount = await this.context.EventTelemetryDetails.CountAsync(x => x.TelemetrySummary.Event.ProgramId == program.Id).ConfigureAwait(false);
            }



            IQueryable<TelemetryDetail> filteredQuery = EntityFilter.Match(query
                , property => property.Contains(requestSearch.Value)
                , new List<Expression<Func<TelemetryDetail, string>>>()
                {
                    {x=>x.Sequence },
                    {x=>x.IpAddress },
                    {x=>x.UserId },
                    {x=>x.EntryKey },
                });


            if (take == -1)
            {
                take = totalCount;
            }

            List<TelemetryDetail> usages = await ApplyOrderingQuery(sortBy, filteredQuery, skip, take).ConfigureAwait(false);

            List<DataTableTelemetryDataBase> result = new List<DataTableTelemetryDataBase>();
            foreach (TelemetryDetail detail in usages)
            {
                DataTableTelemetryData data = new DataTableTelemetryData
                {
                    Timestamp = detail.Timestamp
                    , UserName = detail.UserId
                    , IpAddress = detail.IpAddress
                    , EntryKey = detail.EntryKey
                    , ProgramVersion = detail.AssemblyVersion.AssemblyVersion
                    , Sequence = detail.Sequence
                    ,Values = detail.GetTelemetryUnits().ToDictionary(x => x.Key,x=> x.ValueString)
                };
                result.Add(data);
            }

            return new UsageDataTableResult {TotalCount = totalCount, FilteredCount = totalCount, UsageData = result};
        }

        public async Task<TelemetryInfoTable> GetPivotTableData(TelemetryItemTypes type, Guid telemetryKey)
        {
            Program program = await this.context.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                throw new ArgumentException($"Program with key {telemetryKey} does not exist");
            }

            List<TelemetryDetail> details = program.GetTelemetryDetails(this.context, type).ToList();
            List<TelemetryPivotTableRow> rows = new List<TelemetryPivotTableRow>();

            foreach (TelemetryDetail detail in details)
            {
                List<TelemetryUnit> units = detail.GetTelemetryUnits().ToList();
                if (units.Any())
                {
                    foreach (TelemetryUnit detailTelemetryUnit in units)
                    {
                        TelemetryPivotTableRow row = new TelemetryPivotTableRow(detail)
                        {
                            Value = detailTelemetryUnit.ValueString,
                            Key = detailTelemetryUnit.Key,
                        };
                        rows.Add(row);
                    }
                }
                else
                {
                    TelemetryPivotTableRow row = new TelemetryPivotTableRow(detail);
                    rows.Add(row);
                }

                
            }

            TelemetryInfoTableHeader header = new TelemetryInfoTableHeader();
            return new TelemetryInfoTable {Header = header, Rows = rows};
        }

        internal static async Task<List<T>> ApplyOrderingQuery<T>(IEnumerable<Tuple<string, bool>> sortBy, IQueryable<T> query, int skip, int take)
            where T : TelemetryDetail
        {
            List<Tuple<string, bool>> rules = sortBy.ToList();
            try
            {
                for (int index = 0; index < rules.Count; index++)
                {
                    Tuple<string, bool> rule = rules[index];
                    if (rule.Item1 == nameof(DataTableTelemetryDataBase.Timestamp))
                    {
                        query = Order(query, rule.Item1, rule.Item2, index);
                    }
                    else if (rule.Item1 == nameof(DataTableTelemetryDataBase.ProgramVersion))
                    {
                        query = Order(query, x => x.AssemblyVersion.AssemblyVersion, rule.Item2, index);
                    }
                    else if (rule.Item1 == nameof(DataTableTelemetryDataBase.UserName))
                    {
                        query = Order(query, x => x.UserId, rule.Item2, index);
                    }
                    else if (rule.Item1 == nameof(DataTableTelemetryDataBase.EntryKey) )
                    {
                        query = Order(query, x=> x.EntryKey, rule.Item2, index);
                    }
                }

                IOrderedQueryable<T> orderedQuery = query as IOrderedQueryable<T>;
                if (!OrderingMethodFinder.OrderMethodExists(orderedQuery.Expression))
                {
                    orderedQuery = query.OrderByDescending(x => x.Timestamp);
                }

                return await orderedQuery.Skip(skip).Take(take).ToListAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                return await query.OrderByDescending(x => x.Timestamp).Skip(skip).Take(take).ToListAsync().ConfigureAwait(false);
            }
        }

        internal static async Task<List<ExceptionInfo>> ApplyOrderingQuery(IEnumerable<Tuple<string, bool>> sortBy, IQueryable<ExceptionInfo> query, int skip, int take)
        {
            List<Tuple<string, bool>> rules = sortBy.ToList();
            try
            {
                for (int index = 0; index < rules.Count; index++)
                {
                    Tuple<string, bool> rule = rules[index];
                    if (rule.Item1 == nameof(DataTableTelemetryDataBase.Timestamp) || 
                        rule.Item1 == nameof(DataTableTelemetryDataBase.ProgramVersion) || 
                        rule.Item1 == nameof(DataTableTelemetryDataBase.UserName))
                    {
                        query = GenericOrder(query, rule.Item1, rule.Item2, index);
                    }
                    else if (rule.Item1 == nameof(DataTableTelemetryDataBase.EntryKey))
                    {
                        query = GenericOrder(query, nameof(ExceptionInfo.TypeName), rule.Item2, index);
                    }
                }

                IOrderedQueryable<ExceptionInfo> orderedQuery = query as IOrderedQueryable<ExceptionInfo>;
                if (!OrderingMethodFinder.OrderMethodExists(orderedQuery.Expression))
                {
                    orderedQuery = query.OrderByDescending(x => x.Timestamp);
                }

                return await orderedQuery.Skip(skip).Take(take).ToListAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                return await query.OrderByDescending(x => x.Timestamp).Skip(skip).Take(take).ToListAsync().ConfigureAwait(false);
            }
        }


        internal static async Task<List<LogMessage>> ApplyOrderingQuery(IEnumerable<Tuple<string, bool>> sortBy, IQueryable<LogMessage> query, int skip, int take)
        {
            List<Tuple<string, bool>> rules = sortBy.ToList();
            try
            {
                for (int index = 0; index < rules.Count; index++)
                {
                    Tuple<string, bool> rule = rules[index];
                    if (rule.Item1 == nameof(DataTableTelemetryDataBase.Timestamp) ||
                        rule.Item1 == nameof(DataTableTelemetryDataBase.ProgramVersion) ||
                        rule.Item1 == nameof(DataTableTelemetryDataBase.UserName))
                    {
                        query = GenericOrder(query, rule.Item1, rule.Item2, index);
                    }
                    else if (rule.Item1 == nameof(DataTableTelemetryDataBase.EntryKey))
                    {
                        query = GenericOrder(query, nameof(ExceptionInfo.TypeName), rule.Item2, index);
                    }
                }

                IOrderedQueryable<LogMessage> orderedQuery = query as IOrderedQueryable<LogMessage>;
                if (!OrderingMethodFinder.OrderMethodExists(orderedQuery.Expression))
                {
                    orderedQuery = query.OrderByDescending(x => x.Timestamp);
                }

                return await orderedQuery.Skip(skip).Take(take).ToListAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                return await query.OrderByDescending(x => x.Timestamp).Skip(skip).Take(take).ToListAsync().ConfigureAwait(false);
            }
        }

        private static IOrderedQueryable<T> GenericOrder<T>(IQueryable<T> query, string key, bool desc, int index)
        {
            if (index == 0)
            {
                return query.OrderBy(key, desc);
            }

            return (query as IOrderedQueryable<T>).ThenBy(key, desc);
        }



        private static IOrderedQueryable<T> Order<T>(IQueryable<T> query, string key, bool desc, int index) where T : TelemetryDetail
        {
            if (index == 0)
            {
                return query.OrderBy(key, desc);
            }

            return (query as IOrderedQueryable<T>).ThenBy(key, desc);
        }

        private static IOrderedQueryable<T> Order<T>(IQueryable<T> query, Expression<Func<T, string>> key, bool desc, int index) where T : TelemetryDetail
        {
            if (index == 0)
            {
                return query.OrderBy(key, desc);
            }

            if (desc)
            {
                return (query as IOrderedQueryable<T>).ThenByDescending(key);
            }

            return (query as IOrderedQueryable<T>).ThenBy(key);
        }
    }



}