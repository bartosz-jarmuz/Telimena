using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelimenaClient
{
    /// <summary>
    /// Simple retry mechanism
    /// </summary>
    public static class Retry
    {

        /// <summary>
        /// The conditions for exception filtering
        /// </summary>
        public class ExceptionFilterSet
        {

            /// <summary>
            /// 
            /// </summary>
            /// <param name="whiteList"></param>
            /// <param name="blackList"></param>
            public ExceptionFilterSet(List<ExceptionFilter> whiteList, List<ExceptionFilter> blackList)
            {
                this.WhiteList = whiteList;
                this.BlackList = blackList;
            }

            /// <summary>
            /// 
            /// </summary>
            public ExceptionFilterSet()
            {
            }

            /// <summary>
            /// The whitelist of exceptions. If any specified, only the exceptions on the whitelist can be retried
            /// </summary>
            public List<ExceptionFilter> WhiteList { get; set; } = new List<ExceptionFilter>();

            /// <summary>
            /// The blacklist of exceptions. If any specified, the exceptions of that type cannot be retried
            /// </summary>
            public List<ExceptionFilter> BlackList { get; set; } = new List<ExceptionFilter>();

            /// <summary>
            /// Determines whether the whitelist allows 'retrying'
            /// </summary>
            /// <param name="exception"></param>
            /// <returns></returns>
            public bool IsWhitelistAllowed(Exception exception)
            {
                if (!this.WhiteList.Any())
                {
                    return true;
                }
                else
                {
                    foreach (ExceptionFilter exceptionFilter in this.WhiteList)
                    {
                        if (exceptionFilter.ExceptionType == exception.GetType())
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary>
            /// Determines whether the blacklist prevents from 'retrying'
            /// </summary>
            /// <param name="exception"></param>
            /// <returns></returns>
            public bool IsBlacklistAllowed(Exception exception)
            {
                if (!this.BlackList.Any())
                {
                    return true;
                }
                else
                {
                    foreach (ExceptionFilter exceptionFilter in this.BlackList)
                    {
                        if (exceptionFilter.ExceptionType == exception.GetType())
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Filter for exception types
        /// </summary>
        public class ExceptionFilter
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="exceptionType"></param>
            public ExceptionFilter(Type exceptionType)
            {
                this.ExceptionType = exceptionType;
            }
            /// <summary>
            /// Type
            /// </summary>
            public Type ExceptionType { get;  }


            public string AllowedRegex { get; set; }

            public string ForbiddenRegex { get; set; }

        }

        /// <summary>
        ///     Performs the action trying several times and awaiting between the calls
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="retryInterval"></param>
        /// <param name="maxAttemptCount"></param>
        /// <param name="retryIntervalMultiplier">Increase the delay between attempts by the specified multiplier on each retry</param>
        /// <param name="exceptionTypeWhitelist">Only retry on these exceptions</param>
        /// <returns></returns>
        public static async Task DoAsync(Func<Task> action, TimeSpan retryInterval, int maxAttemptCount = 3, decimal retryIntervalMultiplier = 1, params Type[] exceptionTypeWhitelist)
        {
            await DoAsync<object>(async () => {
                await action();
                return Task.FromResult(default(object)); 
            }, retryInterval, maxAttemptCount, retryIntervalMultiplier, exceptionTypeWhitelist);
        }

        public static async Task DoAsync(Action action, TimeSpan retryInterval, int maxAttemptCount = 3, decimal retryIntervalMultiplier = 1, params Type[] exceptionTypeWhitelist)
        {
            
            await DoAsync<object>(() => {
                action();
                return Task.FromResult(default(object));
            }, retryInterval, maxAttemptCount, retryIntervalMultiplier, exceptionTypeWhitelist);
        }

        /// <summary>
        /// Performs the action trying several times and awaiting between the calls
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="retryInterval"></param>
        /// <param name="maxAttemptCount"></param>
        /// <param name="retryIntervalMultiplier">Increase the delay between attempts by the specified multiplier on each retry</param>
        /// <param name="exceptionTypeWhitelist">Only retry on these exceptions</param>
        /// <returns></returns>

        public static async Task<T> DoAsync<T>(Func<T> action, TimeSpan retryInterval, int maxAttemptCount = 3, decimal retryIntervalMultiplier = 1
            , params Type[] exceptionTypeWhitelist)
        {
            List<TimeSpan> intervals = BuildIntervals(retryInterval, maxAttemptCount, retryIntervalMultiplier);
            ExceptionFilterSet filters = BuildFilters(exceptionTypeWhitelist, null);
            return await DoAsync(action, intervals, filters);
        }

        /// <summary>
        /// Performs the action trying several times and awaiting between the calls
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="retryInterval"></param>
        /// <param name="maxAttemptCount"></param>
        /// <param name="retryIntervalMultiplier">Increase the delay between attempts by the specified multiplier on each retry</param>
        /// <param name="exceptionTypeWhitelist">Only retry on these exceptions</param>
        /// <returns></returns>
        public static async Task<T> DoAsync<T>(Func<Task<T>> action, TimeSpan retryInterval, int maxAttemptCount = 3, decimal retryIntervalMultiplier = 1
            , params Type[] exceptionTypeWhitelist)
        {
            List<TimeSpan> intervals = BuildIntervals(retryInterval, maxAttemptCount, retryIntervalMultiplier);
            ExceptionFilterSet filters = BuildFilters(exceptionTypeWhitelist, null);
            return await DoAsync(action, intervals, filters);
        }

        /// <summary>
        /// Performs the action trying several times and awaiting between the calls
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="retryIntervals"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static async Task<T> DoAsync<T>(Func<T> action, IEnumerable<TimeSpan> retryIntervals, ExceptionFilterSet filters)
        {
            var exceptions = new List<Exception>();
            bool firstAttemptDone = false;
            foreach (TimeSpan retryInterval in retryIntervals)
            {
                try
                {
                    if (firstAttemptDone)
                    {
                        await Task.Delay(retryInterval);
                    }

                    firstAttemptDone = true;
                    return action();
                }
                catch (Exception ex)
                {
                    if (!filters.IsBlacklistAllowed(ex) || !filters.IsWhitelistAllowed(ex))
                    {
                        throw;
                    }

                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }

        /// <summary>
        /// Performs the action trying several times and awaiting between the calls
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="retryIntervals"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static async Task<T> DoAsync<T>(Func<Task<T>> action, IEnumerable<TimeSpan> retryIntervals, ExceptionFilterSet filters)
        {
            var exceptions = new List<Exception>();
            bool firstAttemptDone = false;
            foreach (TimeSpan retryInterval in retryIntervals)
            {
                try
                {
                    if (firstAttemptDone)
                    {
                        await Task.Delay(retryInterval);
                    }

                    firstAttemptDone = true;
                    return await action();
                }
                catch (Exception ex)
                {
                    if (!filters.IsBlacklistAllowed(ex) || !filters.IsWhitelistAllowed(ex))
                    {
                        throw;
                    }

                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }

        private static ExceptionFilterSet BuildFilters(IEnumerable<Type> whitelist, IEnumerable<Type> blacklist)
        {
            var filters = new ExceptionFilterSet();
            if (whitelist != null)
            {
                filters.WhiteList = new List<ExceptionFilter>(whitelist.Select(x => new ExceptionFilter(x)));
            }
            if (blacklist != null)
            {
                filters.BlackList = new List<ExceptionFilter>(blacklist.Select(x => new ExceptionFilter(x)));

            }
            return filters;

        }

        private static List<TimeSpan> BuildIntervals(TimeSpan retryInterval, int maxAttemptCount, decimal retryIntervalMultiplier)
        {
            List<TimeSpan> intervals = new List<TimeSpan>();
            var initialInterval = retryInterval;
            for (int i = 0; i < maxAttemptCount; i++)
            {
                intervals.Add(initialInterval);
                initialInterval = TimeSpan.FromTicks((long)(initialInterval.Ticks * retryIntervalMultiplier));
            }

            return intervals;
        }
    }
}
