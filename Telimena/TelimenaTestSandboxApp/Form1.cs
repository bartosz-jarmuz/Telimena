using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TelimenaClient;

namespace TelimenaTestSandboxApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();
            this.apiUrlTextBox.Text = string.IsNullOrEmpty(Properties.Settings.Default.baseUri)
                ? "http://localhost:7757/"
                : Properties.Settings.Default.baseUri;
            this.apiKeyTextBox.Text = string.IsNullOrEmpty(Properties.Settings.Default.telemetryKey)
                ? ""
                : Properties.Settings.Default.telemetryKey;
            if (Guid.TryParse(this.apiKeyTextBox.Text, out Guid key))
            {
                this.teli =
                    Telimena.Construct(new TelimenaStartupInfo(key, new Uri(this.apiUrlTextBox.Text))) as Telimena;
                this.teli2 =
                    Telimena.Construct(new TelimenaStartupInfo(key, new Uri(this.apiUrlTextBox.Text))) as Telimena;
                this.teli3 =
                    Telimena.Construct(new TelimenaStartupInfo(key, new Uri(this.apiUrlTextBox.Text))) as Telimena;
            }

            this.Text = $"Sandbox v. {TelimenaVersionReader.Read(this.GetType(), VersionTypes.FileVersion)}";

           
        }


        private string PresentResponse(TelimenaResponseBase response)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings {ContractResolver = new MyContractResolver(),};
            return JsonConvert.SerializeObject(response, settings);
        }

        private string PresentResponse(UpdateCheckResult response)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings {ContractResolver = new MyContractResolver(),};
            return JsonConvert.SerializeObject(response, settings);
        }

        private ITelimena teli;
        private TelimenaHammer hammer;
        private Telimena teli2;
        private Telimena teli3;

        private async void InitializeButton_Click(object sender, EventArgs e)
        {
            if (Guid.TryParse(this.apiKeyTextBox.Text, out Guid key))
            {
                this.teli = Telimena.Construct(new TelimenaStartupInfo(key
                    , telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text)));
            }
            else
            {
                this.resultTextBox.Text = "Cannot run without telemetry key";
                return;
            }

            TelemetryInitializeResponse response = await this.teli.Telemetry.Initialize().ConfigureAwait(true);

            this.resultTextBox.Text += this.teli.Properties.StaticProgramInfo.Name + " - " +
                                       this.PresentResponse(response) + Environment.NewLine;
        }

        private async void SendUpdateAppUsageButton_Click(object sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();

            if (!string.IsNullOrEmpty(this.viewNameTextBox.Text))
            {
                var rand = new Random();

                for (int index = 0; index < rand.Next(40,140); index++)
                {
                        this.teli.Telemetry.View(string.IsNullOrEmpty(this.viewNameTextBox.Text)
                        ? null
                        : this.viewNameTextBox.Text);
                }
                   
                
                sw.Stop();
            }
            else
            {
                this.teli.Telemetry.View("DefaultView");
                sw.Stop();
            }

                this.resultTextBox.Text += $@"INSTANCE: {sw.ElapsedMilliseconds}ms " +
                                           this.teli.Properties.StaticProgramInfo.Name + " - " +
                                           "SENT" + Environment.NewLine;
        }

        private void sendSync_button_Click(object sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();


            if (!string.IsNullOrEmpty(this.viewNameTextBox.Text))
            {
             this.teli.Telemetry.View(string.IsNullOrEmpty(this.viewNameTextBox.Text)
                    ? null
                    : this.viewNameTextBox.Text);
                sw.Stop();
            }
            else
            {
              this.teli.Telemetry.View("DefaultView");
                sw.Stop();
            }

                this.resultTextBox.Text += $@"BLOCKING INSTANCE: {sw.ElapsedMilliseconds}ms " +
                                           this.teli.Properties.StaticProgramInfo.Name + " - " +
                                           "SENT" + Environment.NewLine;
        }

        private void UpdateText(string text)
        {
            this.resultTextBox.Text = text + "\r\n" + this.resultTextBox.Text;
        }

        private async void checkForUpdateButton_Click(object sender, EventArgs e)
        {
            var response = await this.teli.Updates.Async.CheckForUpdates().ConfigureAwait(true);
            this.UpdateText(this.PresentResponse(response));
        }

        private async void handleUpdatesButton_Click(object sender, EventArgs e)
        {



            throw new InvalidOperationException("I wanted this");
            this.UpdateText("Handling updates...");
            var suppressAllErrors = this.teli.Properties.SuppressAllErrors;
            this.teli.Properties.SuppressAllErrors = false;
            try
            {
                await this.teli.Updates.Async.HandleUpdates(false).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                this.UpdateText(ex.ToString());
            }

            this.teli.Properties.SuppressAllErrors = suppressAllErrors;
            this.UpdateText("Finished handling updates...");
        }

        private void setAppButton_Click(object sender, EventArgs e)
        {
           

            if (Guid.TryParse(this.apiKeyTextBox.Text, out Guid key))
            {
                Properties.Settings.Default.telemetryKey = this.apiKeyTextBox.Text;
                Properties.Settings.Default.Save();
                this.teli = Telimena.Construct(new TelimenaStartupInfo(key
                    , telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text))) as Telimena;
                ;
            }
            else
            {
                MessageBox.Show("Api key missing, cannot run teli");
            }

            if (!string.IsNullOrEmpty(this.userNameTextBox.Text))
            {
                (this.teli.Properties as TelimenaProperties).UserInfo =
                    new UserInfo {UserName = this.userNameTextBox.Text};
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

        private async void static_sendUsageReportButton_Click(object sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();
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
                return;
            }

            if (!string.IsNullOrEmpty(this.static_viewNameTextBox.Text))
            {
                  Telimena.Telemetry.View(new TelimenaStartupInfo(key)
                    , string.IsNullOrEmpty(this.static_viewNameTextBox.Text) ? null : this.static_viewNameTextBox.Text);
                sw.Stop();
            }
            else
            {
                  Telimena.Telemetry.View(new TelimenaStartupInfo(key), "No Name");
                sw.Stop();
            }

                this.resultTextBox.Text += $@"STATIC: {sw.ElapsedMilliseconds}ms "  +
                                           Environment.NewLine;
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