using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using TelimenaClient;
using TelimenaClient.Model;

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

        public TelimenaHammer(Guid telemetryKey, string url, int appIndexSeed, int numberOfApps, int numberOfFuncs, int numberOfUsers, int delayMin
            , int delayMax, int durationMinutes, Action<string> progressReport)
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

        public Dictionary<string, string> GetRandomData()
        {
            Random random = new Random();

            return new Dictionary<string, string>
            {
                {"PageCount", random.Next(1, 100).ToString()}
                , {"WordCount", random.Next(1, 100).ToString()}
                , {"TimeElapsed", random.Next(1, 100).ToString()}
            };
        }

        public async Task Hit()
        {
            this.progressReport("HAMMER STARTING...\r\n\r\n");
            await this.Initialize().ConfigureAwait(false);
            this.progressReport("HAMMER Apps Initialized. Start hitting...\r\n\r\n");

            this.timeoutStopwatch = new Stopwatch();
            this.timeoutStopwatch.Start();
            List<Task> tasks = new List<Task>();
            foreach (UserInfo userInfo in this.users)
            {
                tasks.Add(this.StartReporting(userInfo));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
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
                    Email = this.GetRandomName("Email@", i), UserIdentifier = this.GetRandomName("User", i), MachineName = this.GetRandomName("Machine", i)
                };
                this.users.Add(user);
            }

            this.apps = new List<ProgramInfo>();
            for (int i = this.appIndexSeed; i < this.numberOfApps + this.appIndexSeed; i++)
            {
                ProgramInfo programInfo = new ProgramInfo
                {
                    Name = "Program_" + i
                    , PrimaryAssembly = new AssemblyInfo
                    {
                        Name = "PrimaryAssembly_Program_" + i
                        , VersionData = new VersionData($"{1}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Day}.{rnd.Next(10)}"
                            , $"{1 + 1}.{DateTime.UtcNow.Month}.{DateTime.UtcNow.Day}.{rnd.Next(10)}")
                    }
                };
                this.apps.Add(programInfo);
                ITelimena teli = TelimenaFactory.Construct(new TelimenaStartupInfo(this.telemetryKey, new Uri(this.url)) {ProgramInfo = programInfo});

                await (teli as Telimena).Initialize().ConfigureAwait(false);
            }

            this.funcs = new List<string>();

            for (int i = 0; i < this.numberOfFuncs; i++)
            {
                this.funcs.Add(this.GetRandomName("View", i));
            }
        }

        private string PresentResponse(TelemetryInitializeResponse response)
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
                ITelimena teli = (Telimena) TelimenaFactory.Construct(new TelimenaStartupInfo(this.telemetryKey, new Uri(this.url)) {ProgramInfo = prg});
                int operation = random.Next(4);
                if (operation == 1)
                {
                     teli.Track.Event("SomeEvent");
                    this.progressReport("Done");

                }
                else if (operation == 2)
                {
                    var result = await (teli as Telimena).Initialize().ConfigureAwait(false);
                    this.progressReport(this.PresentResponse(result));
                }
                else
                {
                    if (random.Next(2) == 1)
                    {
                          teli.Track. View(this.funcs[random.Next(0, this.funcs.Count)]) ;
                        this.progressReport("Done");
                    }
                    else
                    {
                          teli.Track. View(this.funcs[random.Next(0, this.funcs.Count)], this.GetRandomData());
                        this.progressReport("Done");
                    }
                }

                await Task.Delay(random.Next(this.delayMin, this.delayMax)).ConfigureAwait(false);
            }
        }
    }
}