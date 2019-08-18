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
    #region Using

    #endregion

    public partial class ProgramsDashboardUnitOfWork : IProgramsDashboardUnitOfWork
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

    

        private  Task<DataSet> ExecuteStoredProcedure(Program program,  string procedureName, DateTime startDate, DateTime endDate)
        {
            SqlParameter programIdParam = new SqlParameter("programId", SqlDbType.Int) { Value = program.Id };
            SqlParameter startDateParam = new SqlParameter("startDate", SqlDbType.DateTime) { Value = startDate };
            SqlParameter endDateParam = new SqlParameter("endDate", SqlDbType.DateTime) { Value = endDate };

            return this.ExecuteStoredProcedure(procedureName, programIdParam, startDateParam
                , endDateParam);
        }

        private Task<DataSet> ExecuteStoredProcedure(string procedureName, DateTime startDate, DateTime endDate, params SqlParameter[] parameters)
        {
            SqlParameter startDateParam = new SqlParameter("startDate", SqlDbType.DateTime) { Value = startDate };
            SqlParameter endDateParam = new SqlParameter("endDate", SqlDbType.DateTime) { Value = endDate };
            var allParams = new List<SqlParameter>()
            {
                startDateParam, endDateParam
            };
            allParams.AddRange(parameters);

            return this.ExecuteStoredProcedure(procedureName,allParams.ToArray());
        }



        private async Task<DataSet> ExecuteStoredProcedure(string procedureName, params SqlParameter[] parameters)
        {
            DataSet set = new DataSet();

            using (DbConnection conn = this.telemetryContext.Database.Connection)
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = procedureName;
                    cmd.CommandType = CommandType.StoredProcedure;
                    foreach (SqlParameter sqlParameter in parameters)
                    {
                        cmd.Parameters.Add(sqlParameter);
                    }

                    var adapter = DbProviderFactories.GetFactory(conn).CreateDataAdapter();
                    if (adapter != null)
                    {
                        adapter.SelectCommand = cmd;
                        await Task.Run(() => adapter.Fill(set)).ConfigureAwait(false);
                    }
                }
            }

            return set;
        }
        private SqlParameter GetProgramIdsParam(IEnumerable<Program> programs)
        {
            return new SqlParameter("programIds", SqlDbType.NVarChar, 500)
            {
                Value = string.Join(",", programs.Select(x => x.Id))
            };
        }


        internal static DataTable PrepareDailyActivityScoreTable(DateTime startDate, DateTime endDate
            , List<DataRow> eventDataSummaries, List<DataRow> viewDataSummaries, List<DataRow> errorData)
        {
            DataTable data = new DataTable();

            data.Columns.Add("Date");
            data.Columns.Add("Events", typeof(int));
            data.Columns.Add("Views", typeof(int));
            data.Columns.Add("Errors", typeof(int));

            DateTime dateRecord = startDate;

            while (dateRecord.Date <= endDate.Date) //include empty days
            {
                DataRow row = data.NewRow();
                row["Date"] = ToDashboardDateString(dateRecord);

                row["Events"] = eventDataSummaries.FirstOrDefault(x => (DateTime)x[0] == dateRecord.Date)?[1] ?? 0;
                row["Views"] = viewDataSummaries.FirstOrDefault(x => (DateTime)x[0] == dateRecord.Date)?[1] ?? 0;
                row["Errors"] = errorData.FirstOrDefault(x => (DateTime)x[0] == dateRecord.Date)?[1] ?? 0;
                    

                data.Rows.Add(row);
                dateRecord = dateRecord.AddDays(1);
            }

            return data;
        }


        internal static string ToDashboardDateString(DateTime date)
        {
            return date.ToString("ddd, dd.MM", new CultureInfo("en-US"));
        }
        

   
        private async Task<List<TelemetryRootObject>> GetTelemetryRootObjects(List<Program> programs)
        {
            List<TelemetryRootObject> telemetryRootObjects = new List<TelemetryRootObject>();
            foreach (Program program in programs)
            {
                TelemetryRootObject telePrg = await this.telemetryContext.TelemetryRootObjects
                    .FirstOrDefaultAsync(x => x.ProgramId == program.Id).ConfigureAwait(false);
                if (telePrg != null)
                {
                    telemetryRootObjects.Add(telePrg);
                }
            }

            return telemetryRootObjects;
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

        internal static async Task<List<T>> ApplyOrderingQuery<T>(IEnumerable<Tuple<string, bool>> sortBy, IQueryable<T> query, int skip, int take)
            where T : TelemetryDetail
        {
            List<Tuple<string, bool>> rules = sortBy.ToList();
            try
            {
                for (int index = 0; index < rules.Count; index++)
                {
                    Tuple<string, bool> rule = rules[index];
                    if (rule.Item1 == nameof(DataTableTelemetryData.Timestamp))
                    {
                        query = Order(query, rule.Item1, rule.Item2, index);
                    }
                    else if (rule.Item1 == nameof(DataTableTelemetryData.ProgramVersion))
                    {
                        query = Order(query, x => x.FileVersion, rule.Item2, index);
                    }
                    else if (rule.Item1 == nameof(DataTableTelemetryData.UserName))
                    {
                        query = Order(query, x => x.UserIdentifier, rule.Item2, index);
                    }
                    else if (rule.Item1 == nameof(DataTableTelemetryData.EntryKey) )
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
                    if (rule.Item1 == nameof(DataTableTelemetryData.Timestamp) || 
                        rule.Item1 == nameof(DataTableTelemetryData.ProgramVersion) || 
                        rule.Item1 == nameof(DataTableTelemetryData.UserName))
                    {
                        query = GenericOrder(query, rule.Item1, rule.Item2, index);
                    }
                    else if (rule.Item1 == nameof(DataTableTelemetryData.EntryKey))
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
                    if (rule.Item1 == nameof(DataTableTelemetryData.Timestamp) ||
                        rule.Item1 == nameof(DataTableTelemetryData.ProgramVersion) ||
                        rule.Item1 == nameof(DataTableTelemetryData.UserName))
                    {
                        query = GenericOrder(query, rule.Item1, rule.Item2, index);
                    }
                    else if (rule.Item1 == nameof(DataTableTelemetryData.EntryKey))
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

        private static List<ClientAppUser> GetClientAppUsers(List<ViewTelemetrySummary> viewSummaries, List<EventTelemetrySummary> eventSummaries)
        {
            List<ClientAppUser> viewUsers =
                viewSummaries.DistinctBy(x => x.ClientAppUserId).Select(x => x.ClientAppUser).ToList();
            List<ClientAppUser> eventUsers =
                eventSummaries.DistinctBy(x => x.ClientAppUserId).Select(x => x.ClientAppUser).ToList();
            List<ClientAppUser> allUsers = viewUsers.Concat(eventUsers).DistinctBy(x => x.Id).ToList();
            return allUsers;
        }

        private async Task<(List<View> views, List<ViewTelemetrySummary> summaries)> GetViewsAndSummaries(List<TelemetryRootObject> telemetryRootObjects)
        {
            List<View> views = telemetryRootObjects.SelectMany(x => x.Views).ToList();
            IEnumerable<Guid> viewIds = views.Select(x => x.Id);
            List<ViewTelemetrySummary> viewTelemetrySummaries = await this.telemetryContext.ViewTelemetrySummaries
                .Where(usg => viewIds.Contains(usg.ViewId)).ToListAsync().ConfigureAwait(false);
            return (views, viewTelemetrySummaries);
        }

        private async Task<(List<Event> events, List<EventTelemetrySummary> summaries)> GetEventsAndSummaries(List<TelemetryRootObject> telemetryRootObjects)
        {
            List<Event> events = telemetryRootObjects.SelectMany(x => x.Events).ToList();
            IEnumerable<Guid> ids = events.Select(x => x.Id);
            List<EventTelemetrySummary> telemetrySummaries = await this.telemetryContext.EventTelemetrySummaries
                .Where(usg => ids.Contains(usg.EventId)).ToListAsync().ConfigureAwait(false);
            return (events, telemetrySummaries);
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