using System;
using System.Windows.Forms;

namespace TelimenaTestSandboxApp
{
    using System.Diagnostics;
    using System.Web.Script.Serialization;
    using Telimena.Client;

    public partial class Form1 : Form
    {
        private Telimena teli;

        public Form1()
        {
            this.InitializeComponent();
            this.teli = new Telimena(telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text));
        }


        private async void SendUpdateAppUsageButton_Click(object sender, EventArgs e)
        {
            StatisticsUpdateResponse result;
            var sw = Stopwatch.StartNew();

            if (!string.IsNullOrEmpty(this.functionNameTextBox.Text))
            {
                result = await this.teli.ReportUsage(string.IsNullOrEmpty(this.functionNameTextBox.Text)? null : this.functionNameTextBox.Text);
                sw.Stop();
            }
            else
            {
                result = await this.teli.ReportUsage();
                sw.Stop();
            }
            if (result.Error == null)
            {
                this.resultTextBox.Text += $@"INSTANCE: {sw.ElapsedMilliseconds}ms " + this.teli.ProgramInfo.Name + " - " + new JavaScriptSerializer().Serialize(result) + Environment.NewLine;
            }
            else
            {
                MessageBox.Show(result.Error.ToString());
            }
        }

        private async void InitializeButton_Click(object sender, EventArgs e)
        {
            var response = await this.teli.Initialize();
            this.resultTextBox.Text += this.teli.ProgramInfo.Name + " - " + new JavaScriptSerializer().Serialize(response) + Environment.NewLine;
        }

        private void setAppButton_Click(object sender, EventArgs e)
        {
            this.teli = new Telimena(telemetryApiBaseUrl: new Uri(this.apiUrlTextBox.Text));
            if (!string.IsNullOrEmpty(this.appNameTextBox.Text))
            {
                this.teli.ProgramInfo = new ProgramInfo()
                {
                    Name = this.appNameTextBox.Text,
                    PrimaryAssembly = new AssemblyInfo()
                    {
                        Company = "Comp A Ny",
                        Name = this.appNameTextBox.Text + ".dll",
                        Version = "1.0.0.0"
                    }
                };
            }

            if (!string.IsNullOrEmpty(this.userNameTextBox.Text)) 
            {
                this.teli.UserInfo = new UserInfo()
                {
                    UserName = this.userNameTextBox.Text
                };
            }
        }

        private async void static_sendUsageReportButton_Click(object sender, EventArgs e)
        {
            StatisticsUpdateResponse result;
            var sw = Stopwatch.StartNew();

            if (!string.IsNullOrEmpty(this.static_functionNameTextBox.Text))
            {
                result = await Telimena.SendUsageReport(string.IsNullOrEmpty(this.static_functionNameTextBox.Text) ? null : this.static_functionNameTextBox.Text);
                sw.Stop();
            }
            else
            {
                result = await Telimena.SendUsageReport();
                sw.Stop();
            }
            if (result.Error == null)
            {
                this.resultTextBox.Text += $@"STATIC: {sw.ElapsedMilliseconds}ms " +  new JavaScriptSerializer().Serialize(result) + Environment.NewLine;
            }
            else
            {
                MessageBox.Show(result.Error.ToString());
            }
        }
    }
}
