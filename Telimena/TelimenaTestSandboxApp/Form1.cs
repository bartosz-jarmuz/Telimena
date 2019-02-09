using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TelimenaClient;
using TelimenaClient.Model;
using TelimenaClient.Model.Internal;

namespace TelimenaTestSandboxApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();

            Random rnd = new Random();
            var randomName = new []{ "Jess", "Anastasia", "Bobby", "Steven", "Bonawentura" }.OrderBy(a => rnd.NextDouble()).First();
            this.userNameTextBox.Text = randomName;
            this.apiUrlTextBox.Text = string.IsNullOrEmpty(Properties.Settings.Default.baseUri)
                ? "http://localhost:7757/"
                : Properties.Settings.Default.baseUri;
            this.apiKeyTextBox.Text = string.IsNullOrEmpty(Properties.Settings.Default.telemetryKey)
                ? ""
                : Properties.Settings.Default.telemetryKey;
            if (Guid.TryParse(this.apiKeyTextBox.Text, out Guid key))
            {
                this.teli =
                    Telimena.Construct(new TelimenaStartupInfo(key, new Uri(this.apiUrlTextBox.Text)){UserInfo = new UserInfo(){UserId = this.userNameTextBox.Text}}) as Telimena;
            }

            this.Text = $"Sandbox v. {TelimenaVersionReader.Read(this.GetType(), VersionTypes.FileVersion)}";
        }


        


        private string PresentResponse(UpdateCheckResult response)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings {ContractResolver = new MyContractResolver(),};
            return JsonConvert.SerializeObject(response, settings);
        }

        private ITelimena teli;
        private TelimenaHammer hammer;

        private void ThrowUnhandledButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException(this.telemetryDataTextBox.Text, new AbandonedMutexException("Mutex soo lonely"));
        }

        private void SendUpdateAppUsageButton_Click(object sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var rand = new Random();
            var numberOfMessages = 1;
            string viewName = string.IsNullOrEmpty(this.telemetryDataTextBox.Text)
                ? "DefaultView"
                : this.telemetryDataTextBox.Text;

                for (int index = 0; index < numberOfMessages; index++)
                {
                        this.teli.Tracking.View(viewName, new Dictionary<string, string>(){{"SomeViewMetric", $"{new Random().Next(100)}"}});
                }
                sw.Stop();
                this.resultTextBox.Text += $@"{sw.ElapsedMilliseconds}ms - Reported {numberOfMessages} occurrences of view [{viewName}] access" + Environment.NewLine;
        }

        private void reportErrorButtonClick(object sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                throw new DivideByZeroException(this.telemetryDataTextBox.Text);
            }
            catch (Exception ex)
            {
                this.teli.Tracking.Exception(ex);
            }

                this.resultTextBox.Text += $@"{sw.ElapsedMilliseconds}ms - Thrown error: {this.telemetryDataTextBox.Text}" + Environment.NewLine;
        }

        private void UpdateText(string text)
        {
            this.resultTextBox.Text = text + "\r\n" + this.resultTextBox.Text;
        }

        private async void checkForUpdateButton_Click(object sender, EventArgs e)
        {
            var response = await this.teli.Updates.CheckForUpdatesAsync().ConfigureAwait(true);
            this.UpdateText(this.PresentResponse(response));
        }

        private void handleUpdatesButton_Click(object sender, EventArgs e)
        {



            throw new InvalidOperationException("I wanted this");
            //this.UpdateText("Handling updates...");
            //var suppressAllErrors = this.teli.Properties.SuppressAllErrors;
            //this.teli.Properties.SuppressAllErrors = false;
            //try
            //{
            //    await this.teli.Updates.Async.HandleUpdatesAsync(false).ConfigureAwait(true);
            //}
            //catch (Exception ex)
            //{
            //    this.UpdateText(ex.ToString());
            //}

            //this.teli.Properties.SuppressAllErrors = suppressAllErrors;
            //this.UpdateText("Finished handling updates...");
        }

        private void setAppButton_Click(object sender, EventArgs e)
        {

            var si = new TelimenaStartupInfo(Guid.Empty, telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text))
            {
                InstrumentationKey = "1a14064b-d326-4ce3-939e-8cba4d08c255"
            };

            if (!string.IsNullOrEmpty(this.userNameTextBox.Text))
            {   
                si.UserInfo = new UserInfo { UserId = this.userNameTextBox.Text };
            }



            if (Guid.TryParse(this.apiKeyTextBox.Text, out Guid key))
            {
                si.TelemetryKey = key;
                Properties.Settings.Default.telemetryKey = this.apiKeyTextBox.Text;
                Properties.Settings.Default.Save();
                this.teli = Telimena.Construct(si) as Telimena;
                ;
            }
            else
            {
                MessageBox.Show("Api key missing, cannot run teli");
            }
            Properties.Settings.Default.baseUri = this.apiUrlTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void useCurrentAppButton_Click(object sender, EventArgs e)
        {
            if (Guid.TryParse(this.apiKeyTextBox.Text, out Guid key))
            {
                Properties.Settings.Default.telemetryKey = this.apiKeyTextBox.Text;
                Properties.Settings.Default.Save();
                this.teli = Telimena.Construct(new TelimenaStartupInfo(key
                    , telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text))) as Telimena;
            }
            else
            {
                MessageBox.Show("Api key missing, cannot run teli");
            }

            Properties.Settings.Default.baseUri = this.apiUrlTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private async void hammer_StartButton_Click(object sender, EventArgs e)
        {
            this.hammer?.Stop();
            if (Guid.TryParse(this.apiKeyTextBox.Text, out Guid key))
            {
                Properties.Settings.Default.telemetryKey = this.apiKeyTextBox.Text;
                Properties.Settings.Default.Save();
                this.teli = Telimena.Construct(new TelimenaStartupInfo(key
                    , telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text))) as Telimena;
            }
            else
            {
                MessageBox.Show("Api key missing, cannot run hammer");
                return;
            }

            this.hammer = new TelimenaHammer(key, this.apiUrlTextBox.Text
                , Convert.ToInt32(this.hammer_AppNumberSeedBox.Text)
                , Convert.ToInt32(this.hammer_numberOfApps_TextBox.Text)
                , Convert.ToInt32(this.hammer_numberOfFuncs_TextBox.Text)
                , Convert.ToInt32(this.hammer_numberOfUsers_TextBox.Text)
                , Convert.ToInt32(this.hammer_delayMinTextBox.Text), Convert.ToInt32(this.hammer_delayMaxTextBox.Text)
                , Convert.ToInt32(this.hammer_DurationTextBox.Text), this.UpdateText);

            await this.hammer.Hit().ConfigureAwait(true);
        }

        private void hammer_StopBtn_Click(object sender, EventArgs e)
        {
            this.hammer?.Stop();
        }

        private void throwErrorButton_Click(object sender, EventArgs e)
        {
            throw new InvalidOperationException("Manual error");
        }

        private void sendEvent_Button_Click(object sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var rand = new Random();
            var numberOfMessages = 1;
            string name = string.IsNullOrEmpty(this.telemetryDataTextBox.Text)
                ? "DefaultEvent"
                : this.telemetryDataTextBox.Text;

                for (int index = 0; index < numberOfMessages; index++)
                {
                    this.teli.Tracking.Event(name, new Dictionary<string, string>()
                    {
                        { "SomeEventMetric", $"{new Random().Next(100)}" },
                        {"WeekDay",$"{DateTime.Today.DayOfWeek}" }
                    });
            }
                sw.Stop();
            this.resultTextBox.Text += $@"{sw.ElapsedMilliseconds}ms - Reported {numberOfMessages} occurrences of event [{name}]" + Environment.NewLine;

        }

        private void sendLog_Button_Click(object sender, EventArgs e)
        {
            this.teli.Tracking.Log(LogLevel.Warn, this.telemetryDataTextBox.Text);
        }
    }

    class MyContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var list = base.CreateProperties(type, memberSerialization);

            foreach (var prop in list)
            {
                prop.Ignored = false; // Don't ignore any property
            }

            return list;
        }
    }
}