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
            this.Functions = new FunctionRepository(this.context);
            this.Users = new Repository<TelimenaUser>(this.context);
        }

        private readonly TelimenaContext context;
        public IFunctionRepository Functions { get; }

        public IRepository<TelimenaUser> Users { get; }
        public IProgramRepository Programs { get; }

        public async Task<IEnumerable<ProgramSummary>> GetProgramsSummary(List<Program> programs)
        {
            List<ProgramSummary> returnData = new List<ProgramSummary>();

            foreach (Program program in programs)
            {
                List<Function> functions = await this.Functions.FindAsync(x => x.ProgramId == program.Id);
                ProgramSummary summary = new ProgramSummary
                {
                    ProgramName = program.Name
                    , DeveloperName = program.DeveloperAccount?.Name ?? "N/A"
                    , LatestVersion = program.PrimaryAssembly.GetLatestVersion().Version
                    , AssociatedToolkitVersion = program.PrimaryAssembly.GetLatestVersion().ToolkitData?.Version
                    , ProgramId = program.Id
                    , RegisteredDate = program.RegisteredDate
                    , LastUsage = program.UsageSummaries.MaxOrNull(x => x.LastUsageDateTime)
                    , UsersCount = program.UsageSummaries.Count
                    , TodayUsageCount =
                        program.UsageSummaries.Where(x => (DateTime.UtcNow - x.LastUsageDateTime).TotalHours <= 24).Sum(smr =>
                            smr.UsageDetails.Count(detail => (DateTime.UtcNow - detail.DateTime).TotalHours <= 24))
                    , TotalUsageCount = program.UsageSummaries.Sum(x => x.SummaryCount)
                    , FunctionsCount = functions.Count
                    , TotalFunctionsUsageCount = functions.Sum(f => f.UsageSummaries.Sum(s => s.SummaryCount))
                    , TotalTodayFunctionsUsageCount = functions.Sum(f =>
                        f.UsageSummaries.Where(x => (DateTime.UtcNow - x.LastUsageDateTime).TotalHours <= 24).Sum(smr =>
                            smr.UsageDetails.Count(detail => (DateTime.UtcNow - detail.DateTime).TotalHours <= 24)))
                };
                returnData.Add(summary);
            }

            return returnData;
        }

        private static IOrderedQueryable<T> Order<T>(IQueryable<T> query, string key, bool desc, int index) where T: UsageDetail
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

        private static IOrderedQueryable<T> Order<T>(IQueryable<T> query, Expression<Func<T,string>> key, bool desc, int index) where T : UsageDetail
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

        internal static async Task<List<T>> ApplyOrderingQuery<T>(IEnumerable<Tuple<string, bool>> sortBy, IQueryable<T> query, int skip, int take) where T: UsageDetail
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
                        if (typeof(T) == typeof(ProgramUsageDetail))
                        {
                            query = Order(query, x => (x as ProgramUsageDetail).UsageSummary.ClientAppUser.UserName, rule.Item2, index);
                        }
                        else
                        {
                            query = Order(query, x => (x as FunctionUsageDetail).UsageSummary.ClientAppUser.UserName, rule.Item2, index);
                        }
                    }
                    else if (rule.Item1 == nameof(UsageData.CustomData))
                    {
                        if (typeof(T) == typeof(ProgramUsageDetail))
                        {
                            query = Order(query, x => (x as ProgramUsageDetail).CustomUsageData.Data, rule.Item2, index);
                        }
                        else
                        {
                            query = Order(query, x => (x as FunctionUsageDetail).CustomUsageData.Data, rule.Item2, index);
                        }
                    }
                    else if (rule.Item1 == nameof(UsageData.FunctionName) && typeof(T) == typeof(FunctionUsageDetail))
                    {
                        query = Order(query, x => (x as FunctionUsageDetail).UsageSummary.Function.Name, rule.Item2, index);
                    }
                }

                var orderedQuery = query as IOrderedQueryable<T>;
                if (!OrderingMethodFinder.OrderMethodExists(orderedQuery.Expression))
                {
                    orderedQuery = query.OrderByDescending(x => x.Id);
                }
                return await orderedQuery.Skip(skip).Take(take).ToListAsync();
            }
            catch (Exception ex)
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
            List<Function> functions = programs.SelectMany(x => x.Functions).ToList();
            IEnumerable<int> functionIds = functions.Select(x => x.Id);
            List<ProgramUsageSummary> programUsageSummaries = await this.context.ProgramUsages.Where(usg => programIds.Contains(usg.ProgramId)).ToListAsync();
            List<FunctionUsageSummary> functionUsageSummaries =
                await this.context.FunctionUsages.Where(usg => functionIds.Contains(usg.FunctionId)).ToListAsync();
            List<ClientAppUser> users = programUsageSummaries.DistinctBy(x => x.ClientAppUserId).Select(x => x.ClientAppUser).ToList();
            AllProgramsSummaryData summary = new AllProgramsSummaryData
            {
                TotalProgramsCount = programs.Count()
                , TotalAppUsersCount = users.Count()
                , AppUsersRegisteredLast7DaysCount = users.Count(x => (DateTime.UtcNow - x.RegisteredDate).TotalDays <= 7)
                , TotalFunctionsCount = functions.Count()
            };
            int? value = programUsageSummaries.Sum(x => (int?) x.UsageDetails.Count) ?? 0;
            summary.TotalProgramUsageCount = value ?? 0;
            value = functionUsageSummaries.Sum(x => (int?) x.UsageDetails.Count) ?? 0;
            summary.TotalFunctionsUsageCount = value ?? 0;

            summary.NewestProgram = programs.OrderByDescending(x => x.Id) /*.Include(x=>x.Developer)*/.FirstOrDefault();

            return summary;
        }

        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync();
        }

        public async Task<UsageDataTableResult> GetProgramFunctionsUsageData(int programId, int skip, int take, IEnumerable<Tuple<string, bool>> sortBy = null)
        {
            IQueryable<FunctionUsageDetail> query = this.context.FunctionUsageDetails.Where(x => x.UsageSummary.Function.ProgramId == programId);
            int totalCount = await this.context.FunctionUsageDetails.CountAsync(x => x.UsageSummary.Function.ProgramId == programId);
            if (take == -1)
            {
                take = totalCount;
            }

            List<FunctionUsageDetail> usages = await ApplyOrderingQuery(sortBy, query, skip, take);

            List<UsageData> result = new List<UsageData>();
            foreach (FunctionUsageDetail detail in usages)
            {
                UsageData data = new UsageData
                {
                    CustomData = detail.CustomUsageData?.Data
                    , DateTime = detail.DateTime
                    , UserName = detail.UsageSummary.ClientAppUser.UserName
                    , FunctionName = detail.UsageSummary.Function.Name
                    , ProgramVersion = detail.AssemblyVersion.Version
                };
                result.Add(data);
            }

            return new UsageDataTableResult {TotalCount = totalCount, FilteredCount = totalCount, UsageData = result};
        }
        public async Task<UsageDataTableResult> GetProgramUsageData(int programId, int skip, int take, IEnumerable<Tuple<string, bool>> sortBy = null)
        {


            IQueryable<ProgramUsageDetail> query = this.context.ProgramUsageDetails.Where(x => x.UsageSummary.ProgramId == programId);
            int totalCount = await this.context.ProgramUsageDetails.CountAsync(x => x.UsageSummary.ProgramId == programId);

            if (take == -1)
            {
                take = totalCount;
            }

            List<ProgramUsageDetail> usages = await ApplyOrderingQuery(sortBy, query, skip, take);

            List<UsageData> result = new List<UsageData>();
            foreach (ProgramUsageDetail detail in usages)
            {
                UsageData data = new UsageData
                {
                    CustomData = detail.CustomUsageData?.Data
                    ,
                    DateTime = detail.DateTime
                    ,
                    UserName = detail.UsageSummary.ClientAppUser.UserName
                    ,
                    ProgramVersion = detail.AssemblyVersion.Version

                };
                result.Add(data);
            }

            return new UsageDataTableResult { TotalCount = totalCount, FilteredCount = totalCount, UsageData = result };
        }

        private dynamic GetCustomUsageDataObject(FunctionUsageDetail detail, bool includeGenericData)
        {
            try
            {

                var data = System.Web.Helpers.Json.Decode(detail.CustomUsageData.Data);

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
                obj.genericData.functionName = detail.UsageSummary.Function.Name;
                obj.genericData.userName = detail.UsageSummary.ClientAppUser.UserName;
                obj.genericData.userId = detail.UsageSummary.ClientAppUserId;
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

        public async Task<dynamic> ExportFunctionsUsageCustomData(int programId, bool includeGenericData)
        {
            List<FunctionUsageDetail> usages = await this.context.FunctionUsageDetails.Where(x => x.UsageSummary.Function.ProgramId == programId).ToListAsync();


            dynamic root = new
            {
                data = usages.Where(x=>x.CustomUsageData?.Data != null).Select(x => this.GetCustomUsageDataObject(x, includeGenericData)).ToArray()
            };

            return root;


        }
    }
}