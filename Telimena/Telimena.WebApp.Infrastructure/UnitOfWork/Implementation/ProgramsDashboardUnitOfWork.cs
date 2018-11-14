using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.Repository
{
    #region Using

    #endregion

    public class ProgramsDashboardUnitOfWork : IProgramsDashboardUnitOfWork
    {
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
                List<View> views = await this.Views.FindAsync(x => x.ProgramId == program.Id);
                ProgramSummary summary = new ProgramSummary
                {
                    ProgramName = program.Name
                    , DeveloperName = program.DeveloperAccount?.Name ?? "N/A"
                    , LatestVersion = program.PrimaryAssembly.GetLatestVersion().Version
                    , AssociatedToolkitVersion = program.PrimaryAssembly.GetLatestVersion().ToolkitData?.Version
                    , ProgramId = program.Id
                    , RegisteredDate = program.RegisteredDate
                    , LastUsage = program.TelemetrySummaries.MaxOrNull(x => x.LastReportedDateTime)
                    , UsersCount = program.TelemetrySummaries.Count
                    , TodayUsageCount =
                        program.TelemetrySummaries.Where(x => (DateTime.UtcNow - x.LastReportedDateTime).TotalHours <= 24).Sum(smr =>
                            smr.GetTelemetryDetails().Count(detail => (DateTime.UtcNow - detail.DateTime).TotalHours <= 24))
                    , TotalUsageCount = program.TelemetrySummaries.Sum(x => x.SummaryCount)
                    , ViewsCount = views.Count
                    , TotalViewsUsageCount = views.Sum(f => f.TelemetrySummaries.Sum(s => s.SummaryCount))
                    , TotalTodayViewsUsageCount = views.Sum(f =>
                        f.TelemetrySummaries.Where(x => (DateTime.UtcNow - x.LastReportedDateTime).TotalHours <= 24).Sum(smr =>
                            smr.TelemetryDetails.Count(detail => (DateTime.UtcNow - detail.DateTime).TotalHours <= 24)))
                };
                returnData.Add(summary);
            }

            return returnData;
        }

        private static IOrderedQueryable<T> Order<T>(IQueryable<T> query, string key, bool desc, int index) where T: TelemetryDetail
        {
            if (index == 0)
            {
                return query.OrderBy(key, desc);
            }
            else
            {
                return (query as IOrderedQueryable<T>).ThenBy(key, desc);
            }
        }

        private static IOrderedQueryable<T> Order<T>(IQueryable<T> query, Expression<Func<T,string>> key, bool desc, int index) where T : TelemetryDetail
        {
            if (index == 0)
            {
                return query.OrderBy(key, desc);
            }
            else
            {
                if (desc)
                {
                    return (query as IOrderedQueryable<T>).ThenByDescending(key);
                }
                return (query as IOrderedQueryable<T>).ThenBy(key);
            }
        }

        internal static async Task<List<T>> ApplyOrderingQuery<T>
            (IEnumerable<Tuple<string, bool>> sortBy, IQueryable<T> query, int skip, int take) where T: TelemetryDetail
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
                        query = Order(query, x=>x.AssemblyVersion.Version, rule.Item2, index);
                    }
                    else if (rule.Item1 == nameof(UsageData.UserName))
                    {
                        if (typeof(T) == typeof(ProgramTelemetryDetail))
                        {
                            query = Order(query, x => (x as ProgramTelemetryDetail).TelemetrySummary.ClientAppUser.UserName, rule.Item2, index);
                        }
                        else
                        {
                            query = Order(query, x => (x as ViewTelemetryDetail).TelemetrySummary.ClientAppUser.UserName, rule.Item2, index);
                        }
                    }
                    //else if (rule.Item1 == nameof(UsageData.CustomData))
                    //{
                    //    if (typeof(T) == typeof(ProgramTelemetryDetail))
                    //    {
                    //        query = Order(query, x => (x as ProgramTelemetryDetail).CustomUsageData.Data, rule.Item2, index);
                    //    }
                    //    //else
                    //    //{
                    //    //    query = Order(query, x => (x as ViewTelemetryDetail).CustomUsageData.Data, rule.Item2, index);
                    //    //}
                    //}
                    else if (rule.Item1 == nameof(UsageData.ViewName) && typeof(T) == typeof(ViewTelemetryDetail))
                    {
                        query = Order(query, x => (x as ViewTelemetryDetail).TelemetrySummary.View.Name, rule.Item2, index);
                    }
                }

                var orderedQuery = query as IOrderedQueryable<T>;
                if (!OrderingMethodFinder.OrderMethodExists(orderedQuery.Expression))
                {
                    orderedQuery = query.OrderByDescending(x => x.Id);
                }
                return await orderedQuery.Skip(skip).Take(take).ToListAsync();
            }
            catch (Exception)
            {
                return await query.OrderByDescending(x=>x.Id).Skip(skip).Take(take).ToListAsync();
            }
        }

        class OrderingMethodFinder : ExpressionVisitor
        {
            bool _orderingMethodFound = false;

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var name = node.Method.Name;

                if (node.Method.DeclaringType == typeof(Queryable) && (
                        name.StartsWith("OrderBy", StringComparison.Ordinal) ||
                        name.StartsWith("ThenBy", StringComparison.Ordinal)))
                {
                    _orderingMethodFound = true;
                }

                return base.VisitMethodCall(node);
            }

            public static bool OrderMethodExists(Expression expression)
            {
                var visitor = new OrderingMethodFinder();
                visitor.Visit(expression);
                return visitor._orderingMethodFound;
            }
        }

        public async Task<PortalSummaryData> GetPortalSummary()
        {
            PortalSummaryData summary = new PortalSummaryData
            {
                TotalUsersCount = await this.context.Users.CountAsync()
                , NewestUser = await this.context.Users.OrderByDescending(x => x.UserNumber).FirstAsync()
                , LastActiveUser = await this.context.Users.OrderByDescending(x => x.LastLoginDate).FirstAsync()
                , UsersActiveInLast24Hrs = await this.context.Users.CountAsync(x =>
                    x.LastLoginDate != null && DbFunctions.DiffDays(DateTime.UtcNow, x.LastLoginDate.Value) < 1)
            };
            return summary;
        }

        public async Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts(List<Program> programs)
        {
            IEnumerable<int> programIds = programs.Select(x => x.Id);
            List<View> views = programs.SelectMany(x => x.Views).ToList();
            IEnumerable<int> viewIds = views.Select(x => x.Id);
            List<ProgramTelemetrySummary> programUsageSummaries = await this.context.ProgramTelemetrySummaries.Where(usg => programIds.Contains(usg.ProgramId)).ToListAsync();
            List<ViewTelemetrySummary> viewTelemetrySummaries =
                await this.context.ViewTelemetrySummaries.Where(usg => viewIds.Contains(usg.ViewId)).ToListAsync();
            List<ClientAppUser> users = programUsageSummaries.DistinctBy(x => x.ClientAppUserId).Select(x => x.ClientAppUser).ToList();
            AllProgramsSummaryData summary = new AllProgramsSummaryData
            {
                TotalProgramsCount = programs.Count()
                , TotalAppUsersCount = users.Count()
                , AppUsersRegisteredLast7DaysCount = users.Count(x => (DateTime.UtcNow - x.RegisteredDate).TotalDays <= 7)
                , TotalViewsCount = views.Count()
            };
            int? value = programUsageSummaries.Sum(x => (int?) x.GetTelemetryDetails().Count()) ?? 0;
            summary.TotalProgramUsageCount = value ?? 0;
            value = viewTelemetrySummaries.Sum(x => (int?) x.TelemetryDetails.Count) ?? 0;
            summary.TotalViewsUsageCount = value ?? 0;

            summary.NewestProgram = programs.OrderByDescending(x => x.Id) /*.Include(x=>x.Developer)*/.FirstOrDefault();

            return summary;
        }

        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync();
        }

        public async Task<UsageDataTableResult> GetProgramViewsUsageData(int programId, int skip, int take, IEnumerable<Tuple<string, bool>> sortBy = null)
        {
            IQueryable<ViewTelemetryDetail> query = this.context.ViewTelemetryDetails.Where(x => x.TelemetrySummary.View.ProgramId == programId);
            int totalCount = await this.context.ViewTelemetryDetails.CountAsync(x => x.TelemetrySummary.View.ProgramId == programId);
            if (take == -1)
            {
                take = totalCount;
            }

            List<ViewTelemetryDetail> usages = await ApplyOrderingQuery(sortBy, query, skip, take);

            List<UsageData> result = new List<UsageData>();
            foreach (ViewTelemetryDetail detail in usages)
            {
                UsageData data = new UsageData
                {
                    //CustomData = detail.CustomUsageData?.Data
                    DateTime = detail.DateTime
                    , UserName = detail.TelemetrySummary.ClientAppUser.UserName
                    , ViewName = detail.TelemetrySummary.View.Name
                    , ProgramVersion = detail.AssemblyVersion.Version
                };
                result.Add(data);
            }

            return new UsageDataTableResult {TotalCount = totalCount, FilteredCount = totalCount, UsageData = result};
        }
        public async Task<UsageDataTableResult> GetProgramUsageData(int programId, int skip, int take, IEnumerable<Tuple<string, bool>> sortBy = null)
        {


            IQueryable<ProgramTelemetryDetail> query = this.context.ProgramTelemetryDetails.Where(x => x.TelemetrySummary.ProgramId == programId);
            int totalCount = await this.context.ProgramTelemetryDetails.CountAsync(x => x.TelemetrySummary.ProgramId == programId);

            if (take == -1)
            {
                take = totalCount;
            }

            var usages = await ApplyOrderingQuery(sortBy, query, skip, take);

            List<UsageData> result = new List<UsageData>();
            foreach (ProgramTelemetryDetail detail in usages)
            {
                UsageData data = new UsageData
                {
                    //CustomData = detail.CustomUsageData?.Data
                    //,
                    DateTime = detail.DateTime
                    ,
                    UserName = detail.TelemetrySummary.ClientAppUser.UserName
                    ,
                    ProgramVersion = detail.AssemblyVersion.Version

                };
                result.Add(data);
            }

            return new UsageDataTableResult { TotalCount = totalCount, FilteredCount = totalCount, UsageData = result };
        }

        private dynamic GetCustomUsageDataObject(ViewTelemetryDetail detail, bool includeGenericData)
        {
            try
            {

                var data = System.Web.Helpers.Json.Decode("");

                if (!includeGenericData)
                {
                    return data;
                }
                dynamic obj = new ExpandoObject();
                obj.customData = data;
                obj.genericData = new ExpandoObject();
                obj.genericData.DateTime = detail.DateTime;
                obj.genericData.detailId = detail.Id;
                obj.genericData.programVersion = detail.AssemblyVersion.Version;
                obj.genericData.viewName = detail.TelemetrySummary.View.Name;
                obj.genericData.userName = detail.TelemetrySummary.ClientAppUser.UserName;
                obj.genericData.userId = detail.TelemetrySummary.ClientAppUserId;
                return obj;
            }
            catch (Exception ex)
            {
                dynamic obj = new ExpandoObject();
                obj.Status = "Invalid CustomUsageData";
                obj.Exception = ex.Message;
                obj.UsageDetailId = detail.Id;
                return obj;
            }

        }

        public async Task<dynamic> ExportViewsUsageCustomData(int programId, bool includeGenericData)
        {
            List<ViewTelemetryDetail> usages = await this.context.ViewTelemetryDetails.Where(x => x.TelemetrySummary.View.ProgramId == programId).ToListAsync();


            //dynamic root = new
            //{
            //    data = usages.Where(x=>x.CustomUsageData?.Data != null).Select(x => this.GetCustomUsageDataObject(x, includeGenericData)).ToArray()
            //};

            //return root;
            return null;

        }
    }
}