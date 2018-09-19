using System;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Telimena.Client;

namespace TelimenaTestSandboxApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();
            this.teli = new Telimena.Client.Telimena(telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text));
        }

        private Telimena.Client.Telimena teli;

        private async void InitializeButton_Click(object sender, EventArgs e)
        {
            RegistrationResponse response = await this.teli.Initialize();
            this.resultTextBox.Text += this.teli.ProgramInfo.Name + " - " + new JavaScriptSerializer().Serialize(response) + Environment.NewLine;
        }

        private async void SendUpdateAppUsageButton_Click(object sender, EventArgs e)
        {
            StatisticsUpdateResponse result;
            Stopwatch sw = Stopwatch.StartNew();

            if (!string.IsNullOrEmpty(this.functionNameTextBox.Text))
            {
                result = await this.teli.ReportUsage(string.IsNullOrEmpty(this.functionNameTextBox.Text) ? null : this.functionNameTextBox.Text);
                sw.Stop();
            }
            else
            {
                result = await this.teli.ReportUsage();
                sw.Stop();
            }

            if (result.Exception == null)
            {
                this.resultTextBox.Text += $@"INSTANCE: {sw.ElapsedMilliseconds}ms " + this.teli.ProgramInfo.Name + " - " +
                                           new JavaScriptSerializer().Serialize(result) + Environment.NewLine;
            }
            else
            {
                MessageBox.Show(result.Exception.ToString());
            }
        }

        private async void checkForUpdateButton_Click(object sender, EventArgs e)
        {
            await this.teli.HandleUpdates(false);
        }

        private void setAppButton_Click(object sender, EventArgs e)
        {
            this.teli = new Telimena.Client.Telimena(telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text));
            if (!string.IsNullOrEmpty(this.appNameTextBox.Text))
            {
                this.teli.ProgramInfo = new ProgramInfo
                {
                    Name = this.appNameTextBox.Text
                    , PrimaryAssembly = new AssemblyInfo {Company = "Comp A Ny", Name = this.appNameTextBox.Text + ".dll", Version = "1.0.0.0"}
                };
            }

            if (!string.IsNullOrEmpty(this.userNameTextBox.Text))
            {
                this.teli.UserInfo = new UserInfo {UserName = this.userNameTextBox.Text};
            }
        }

        private async void static_sendUsageReportButton_Click(object sender, EventArgs e)
        {
            StatisticsUpdateResponse result;
            Stopwatch sw = Stopwatch.StartNew();

            if (!string.IsNullOrEmpty(this.static_functionNameTextBox.Text))
            {
                result = await Telimena.Client.Telimena.SendUsageReport(string.IsNullOrEmpty(this.static_functionNameTextBox.Text)
                    ? null
                    : this.static_functionNameTextBox.Text);
                sw.Stop();
            }
            else
            {
                result = await Telimena.Client.Telimena.SendUsageReport();
                sw.Stop();
            }

            if (result.Exception == null)
            {
                this.resultTextBox.Text += $@"STATIC: {sw.ElapsedMilliseconds}ms " + new JavaScriptSerializer().Serialize(result) + Environment.NewLine;
            }
            else
            {
                MessageBox.Show(result.Exception.ToString());
            }
        }

       
    }
}