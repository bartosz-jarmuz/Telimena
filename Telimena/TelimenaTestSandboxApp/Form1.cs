using System;
using System.Diagnostics;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using TelimenaClient;

namespace TelimenaTestSandboxApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();
            this.apiUrlTextBox.Text = string.IsNullOrEmpty(Properties.Settings.Default.baseUri) ? "http://localhost:7757/" : Properties.Settings.Default.baseUri;
            this.apiKeyTextBox.Text = string.IsNullOrEmpty(Properties.Settings.Default.telemetryKey) ? "" : Properties.Settings.Default.telemetryKey;
            if (Guid.TryParse(this.apiKeyTextBox.Text, out Guid key))
            {
                this.teli = new Telimena(key, telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text));
            }
            this.Text = $"Sandbox v. {TelimenaVersionReader.Read(this.GetType(), VersionTypes.FileVersion)}";
        }

        private string PresentResponse(TelimenaResponseBase response)
        {
            if (response.Exception != null)
            {
                return response.Exception.ToString();
            }
            return new JavaScriptSerializer().Serialize(response);
        }

        private string PresentResponse(UpdateCheckResult response)
        {
            if (response.Exception != null)
            {
                return response.Exception.ToString();
            }
            return new JavaScriptSerializer().Serialize(response);
        }

        private Telimena teli;
        private TelimenaHammer hammer;

        private async void InitializeButton_Click(object sender, EventArgs e)
        {
            if (Guid.TryParse(this.apiKeyTextBox.Text, out Guid key))
            {
                this.teli = new Telimena(key, telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text));
            }
            else
            {
                this.resultTextBox.Text = "Cannot run without telemetry key";
                return;
            }
            TelemetryInitializeResponse response = await this.teli.InitializeAsync_toReDo();

            this.resultTextBox.Text += this.teli.StaticProgramInfo.Name + " - " + this.PresentResponse(response) + Environment.NewLine;
        }

        private async void SendUpdateAppUsageButton_Click(object sender, EventArgs e)
        {
            TelemetryUpdateResponse result;
            Stopwatch sw = Stopwatch.StartNew();

            if (!string.IsNullOrEmpty(this.viewNameTextBox.Text))
            {
                result = await this.teli.ReportViewAccessedAsync(string.IsNullOrEmpty(this.viewNameTextBox.Text) ? null : this.viewNameTextBox.Text);
                sw.Stop();
            }
            else
            {
                result = await this.teli.ReportViewAccessedAsync("DefaultView");
                sw.Stop();
            }

            if (result.Exception == null)
            {
                this.resultTextBox.Text += $@"INSTANCE: {sw.ElapsedMilliseconds}ms " + this.teli.StaticProgramInfo.Name + " - " + this.PresentResponse(result) + Environment.NewLine;
            }
            else
            {
                MessageBox.Show(result.Exception.ToString());
            }
        }

        private void sendSync_button_Click(object sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();

            TelemetryUpdateResponse result;

            if (!string.IsNullOrEmpty(this.viewNameTextBox.Text))
            {
                result = this.teli.ReportViewAccessedBlocking(string.IsNullOrEmpty(this.viewNameTextBox.Text) ? null : this.viewNameTextBox.Text);
                sw.Stop();
            }
            else
            {
                result = this.teli.ReportViewAccessedBlocking("DefaultView");
                sw.Stop();
            }

            if (result.Exception == null)
            {
                this.resultTextBox.Text += $@"BLOCKING INSTANCE: {sw.ElapsedMilliseconds}ms " + this.teli.StaticProgramInfo.Name + " - " + this.PresentResponse(result) + Environment.NewLine;
            }
            else
            {
                MessageBox.Show(result.Exception.ToString());
            }
        }

        private void UpdateText(string text)
        {
            this.resultTextBox.Text = text + "\r\n" + this.resultTextBox.Text;
        }

        private async void checkForUpdateButton_Click(object sender, EventArgs e)
        {
            var response = await this.teli.CheckForUpdatesAsync();
            this.UpdateText(this.PresentResponse(response));
        }

        private async void handleUpdatesButton_Click(object sender, EventArgs e)
        {
            this.UpdateText("Handling updates...");
            var suppressAllErrors = this.teli.SuppressAllErrors;
            this.teli.SuppressAllErrors = false;
            try
            {
                await this.teli.HandleUpdatesAsync(false);
            }
            catch (Exception ex)
            {
                this.UpdateText(ex.ToString());
            }

            this.teli.SuppressAllErrors = suppressAllErrors;
            this.UpdateText("Finished handling updates...");
        }

        private void setAppButton_Click(object sender, EventArgs e)
        {
            if (Guid.TryParse(this.apiKeyTextBox.Text, out Guid key))
            {
                Properties.Settings.Default.telemetryKey = this.apiKeyTextBox.Text;
                Properties.Settings.Default.Save();
                this.teli = new Telimena(key, telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text));
            }
            else
            {
                MessageBox.Show("Api key missing, cannot run teli");
            }
            if (!string.IsNullOrEmpty(this.appNameTextBox.Text))
            {
                this.teli.StaticProgramInfo = new ProgramInfo
                {
                    Name = this.appNameTextBox.Text
                    , PrimaryAssembly = new AssemblyInfo {Company = "Comp A Ny", Name = this.appNameTextBox.Text + ".dll", VersionData = new VersionData("1.0.0.0", "2.0.0.0")}
                };
            }

            if (!string.IsNullOrEmpty(this.userNameTextBox.Text))
            {
                this.teli.UserInfo = new UserInfo {UserName = this.userNameTextBox.Text};
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
                this.teli = new Telimena(key, telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text));
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
            TelemetryUpdateResponse result;
            Stopwatch sw = Stopwatch.StartNew();
            if (Guid.TryParse(this.apiKeyTextBox.Text, out Guid key))
            {
                Properties.Settings.Default.telemetryKey = this.apiKeyTextBox.Text;
                Properties.Settings.Default.Save();
                this.teli = new Telimena(key, telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text));
            }
            else
            {
                MessageBox.Show("Api key missing, cannot run teli");
                return;
            }
            if (!string.IsNullOrEmpty(this.static_viewNameTextBox.Text))
            {
                
                result = await Telimena.ReportUsageStatic(key, string.IsNullOrEmpty(this.static_viewNameTextBox.Text)
                    ? null
                    : this.static_viewNameTextBox.Text);
                sw.Stop();
            }
            else
            {
                result = await Telimena.ReportUsageStatic(key);
                sw.Stop();
            }

            if (result.Exception == null)
            {
                this.resultTextBox.Text += $@"STATIC: {sw.ElapsedMilliseconds}ms " + this.PresentResponse(result) + Environment.NewLine;
            }
            else
            {
                MessageBox.Show(result.Exception.ToString());
            }
        }

        private async void hammer_StartButton_Click(object sender, EventArgs e)
        {
            this.hammer?.Stop();
            if (Guid.TryParse(this.apiKeyTextBox.Text, out Guid key))
            {
                Properties.Settings.Default.telemetryKey = this.apiKeyTextBox.Text;
                Properties.Settings.Default.Save();
                this.teli = new Telimena(key, telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text));
            }
            else
            {
                MessageBox.Show("Api key missing, cannot run hammer");
                return;
            }
            this.hammer = new TelimenaHammer(key, this.apiUrlTextBox.Text,
                Convert.ToInt32(this.hammer_AppNumberSeedBox.Text),
                Convert.ToInt32(this.hammer_numberOfApps_TextBox.Text),
                 Convert.ToInt32(this.hammer_numberOfFuncs_TextBox.Text),
                 Convert.ToInt32(this.hammer_numberOfUsers_TextBox.Text),
                 Convert.ToInt32(this.hammer_delayMinTextBox.Text),
                 Convert.ToInt32(this.hammer_delayMaxTextBox.Text),
                 Convert.ToInt32(this.hammer_DurationTextBox.Text),
                this.UpdateText
                );

           await this.hammer.Hit();
        }

        private void hammer_StopBtn_Click(object sender, EventArgs e)
        {
            this.hammer?.Stop();
        }

       
    }
}