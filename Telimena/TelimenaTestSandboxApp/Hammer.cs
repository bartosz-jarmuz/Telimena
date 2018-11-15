using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using TelimenaClient;

namespace TelimenaTestSandboxApp
{
    public class TelimenaHammer
    {
        public class CustomData
        {
            public int PageCount { get; set; }
            public int WordCount { get; set; }

            public TimeSpan TimeElapsed { get; set; }
        }

        public TelimenaHammer(Guid telemetryKey, string url, int appIndexSeed, int numberOfApps, int numberOfFuncs, int numberOfUsers, int delayMin, int delayMax, int durationMinutes
            , Action<string> progressReport)
        {
            this.telemetryKey = telemetryKey;
            this.url = url;
            this.appIndexSeed = appIndexSeed;
            this.numberOfApps = numberOfApps;
            this.numberOfFuncs = numberOfFuncs;
            this.numberOfUsers = numberOfUsers;
            this.delayMin = delayMin;
            this.delayMax = delayMax;
            this.progressReport = progressReport;
            this.duration = TimeSpan.FromMinutes(durationMinutes);
        }

        private readonly Guid telemetryKey;
        private readonly string url;
        private readonly int appIndexSeed;
        private readonly int numberOfApps;
        private readonly int numberOfFuncs;
        private readonly int numberOfUsers;
        private readonly int delayMin;
        private readonly int delayMax;
        private readonly Action<string> progressReport;
        private readonly TimeSpan duration;
        private List<ProgramInfo> apps;
        private List<UserInfo> users;
        private Stopwatch timeoutStopwatch;
        private List<string> funcs;

        public CustomData GetRandomData()
        {
            var random = new Random();
            return new CustomData
            {
                PageCount = random.Next(1, 100), WordCount = random.Next(1, 99999), TimeElapsed = TimeSpan.FromMilliseconds(random.Next(1, 99999999))
            };
        }

        public async Task Hit()
        {
            this.progressReport("HAMMER STARTING...\r\n\r\n");
            await this.Initialize();
            this.progressReport("HAMMER Apps Initialized. Start hitting...\r\n\r\n");

            this.timeoutStopwatch = new Stopwatch();
            this.timeoutStopwatch.Start();
            List<Task> tasks = new List<Task>();
            foreach (UserInfo userInfo in this.users)
            {
                tasks.Add(this.StartReporting(userInfo));
            }

            await Task.WhenAll(tasks);
            this.progressReport("\r\nHAMMER FINISHED...\r\n\r\n");
        }

        public void Stop()
        {
            this.timeoutStopwatch?.Stop();
        }

        private string GetRandomName(string objectType, int number)
        {
            string str = Guid.NewGuid().ToString().Remove(5);
            return objectType + "_" + str + "_" + number;
        }

        private async Task Initialize()
        {
            Random rnd = new Random();
            this.users = new List<UserInfo>();
            for (int i = 0; i < this.numberOfUsers; i++)
            {
                UserInfo user = new UserInfo
                {
                    Email = this.GetRandomName("Email@", i), UserName = this.GetRandomName("User", i), MachineName = this.GetRandomName("Machine", i)
                };
                this.users.Add(user);
            }

            this.apps = new List<ProgramInfo>();
            for (int i = this.appIndexSeed; i < this.numberOfApps + this.appIndexSeed; i++)
            {
                ProgramInfo programInfo = new ProgramInfo {Name = "Program_" + i, PrimaryAssembly = new AssemblyInfo {Name = "PrimaryAssembly_Program_" + i, AssemblyVersion = $"{1}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Day}.{rnd.Next(10)}" }};
                this.apps.Add(programInfo);
                Telimena teli = new Telimena(this.telemetryKey, programInfo, new Uri(this.url));
                await teli.RegisterClient();

            }

            this.funcs = new List<string>();

            for (int i = 0; i < this.numberOfFuncs; i++)
            {
                this.funcs.Add(this.GetRandomName("View", i));
            }
        }

        private string PresentResponse(TelimenaResponseBase response)
        {
            if (response.Exception != null)
            {
                return response.Exception.ToString();
            }

            return new JavaScriptSerializer().Serialize(response);
        }

        private async Task StartReporting(UserInfo userInfo)
        {
            Random random = new Random();
            while (this.timeoutStopwatch.IsRunning && this.timeoutStopwatch.ElapsedMilliseconds < this.duration.TotalMilliseconds)
            {
                ProgramInfo prg = this.apps[random.Next(0, this.apps.Count)];
                Telimena teli = new Telimena(this.telemetryKey, prg, new Uri(this.url));

                TelimenaResponseBase result;
                var operation = random.Next(4);
                if (operation == 1)
                {
                    result = await teli.ReportUsageAsync();
                }
                else if (operation == 2)
                {
                    result = await teli.InitializeAsync();
                }
                else
                {
                    if (random.Next(2) == 1)
                    {
                        result = await teli.ReportUsageAsync(this.funcs[random.Next(0, this.funcs.Count)]);
                    }
                    else
                    {
                        result = await teli.ReportUsageWithCustomDataAsync(this.GetRandomData(), this.funcs[random.Next(0, this.funcs.Count)]);
                    }
                }

                this.progressReport(this.PresentResponse(result));
                await Task.Delay(random.Next(this.delayMin, this.delayMax));
            }
        }
    }
}