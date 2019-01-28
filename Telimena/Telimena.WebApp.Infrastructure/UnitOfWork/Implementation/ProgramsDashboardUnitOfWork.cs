using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.Implementation;
using TelimenaClient;

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
                //, AppUsersRegisteredLast7DaysCount = users.Count(x => (DateTime.UtcNow - x.RegisteredDate).TotalDays <= 7)
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

        public async Task<UsageDataTableResult> GetProgramViewsUsageData(Guid telemetryKey, int skip, int take, IEnumerable<Tuple<string, bool>> sortBy = null)
        {
            Program program = await this.context.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                throw new ArgumentException($"Program with key {telemetryKey} does not exist");
            }

            IQueryable<ViewTelemetryDetail> query = this.context.ViewTelemetryDetails.Where(x => x.TelemetrySummary.View.ProgramId == program.Id);
            int totalCount = await this.context.ViewTelemetryDetails.CountAsync(x => x.TelemetrySummary.View.ProgramId == program.Id).ConfigureAwait(false);
            if (take == -1)
            {
                take = totalCount;
            }

            List<ViewTelemetryDetail> usages = await ApplyOrderingQuery(sortBy, query, skip, take).ConfigureAwait(false);

            List<UsageData> result = new List<UsageData>();
            foreach (ViewTelemetryDetail detail in usages)
            {
                UsageData data = new UsageData
                {
                    DateTime = detail.Timestamp
                    , UserName = detail.TelemetrySummary.ClientAppUser.UserName
                    , ViewName = detail.TelemetrySummary.View.Name
                    , ProgramVersion = detail.AssemblyVersion.AssemblyVersion
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
                foreach (TelemetryUnit detailTelemetryUnit in detail.GetTelemetryUnits())
                {
                    TelemetryPivotTableRow row = new TelemetryPivotTableRow
                    {
                        ComponentName = detail.GetTelemetrySummary().GetComponent().Name
                        , Date = detail.Timestamp.Date.ToString("yyyy-MM-dd")
                        , Time = detail.Timestamp.TimeOfDay.TotalSeconds
                        , Value = detailTelemetryUnit.ValueString
                        , Key = detailTelemetryUnit.Key
                        , TelemetryDetailId = detail.Id
                        , UserName = detail.GetTelemetrySummary().ClientAppUser.UserName
                    };
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

            //   query = query.OrderByDescending(x => x.Id);
            try
            {
                for (int index = 0; index < rules.Count; index++)
                {
                    Tuple<string, bool> rule = rules[index];
                    if (rule.Item1 == nameof(UsageData.DateTime))
                    {
                        query = Order(query, rule.Item1, rule.Item2, index);
                    }
                    else if (rule.Item1 == nameof(UsageData.ProgramVersion))
                    {
                        query = Order(query, x => x.AssemblyVersion.AssemblyVersion, rule.Item2, index);
                    }
                    else if (rule.Item1 == nameof(UsageData.UserName))
                    {
                        //todo - verify    - what about events?
                        query = Order(query, x => (x as ViewTelemetryDetail).TelemetrySummary.ClientAppUser.UserName, rule.Item2, index);
                    }
                    else if (rule.Item1 == nameof(UsageData.ViewName) && typeof(T) == typeof(ViewTelemetryDetail))
                    {
                        query = Order(query, x => (x as ViewTelemetryDetail).TelemetrySummary.View.Name, rule.Item2, index);
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