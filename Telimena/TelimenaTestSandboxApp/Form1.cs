using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TelimenaTestSandboxApp
{
    using System.Web.Script.Serialization;
    using Telimena.Client;

    public partial class Form1 : Form
    {
        private Telimena teli;

        public Form1()
        {
            this.InitializeComponent();
            this.teli = new Telimena(this.apiUrlTextBox.Text);
        }


        private async void SendUpdateAppUsageButton_Click(object sender, EventArgs e)
        {
            StatisticsUpdateResponse result;

            if (!string.IsNullOrEmpty(this.functionNameTextBox.Text))
            {
                result = await this.teli.ReportUsage(string.IsNullOrEmpty(this.functionNameTextBox.Text)? null : this.functionNameTextBox.Text);
            }
            else
            {
                result = await this.teli.ReportUsage();
            }
            if (result.Error == null)
            {
                this.resultTextBox.Text += this.teli.ProgramInfo.Name + " - " + new JavaScriptSerializer().Serialize(result) + Environment.NewLine;
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
            this.teli = new Telimena(this.apiUrlTextBox.Text);
            if (!string.IsNullOrEmpty(this.appNameTextBox.Text))
            {
                this.teli.ProgramInfo = new ProgramInfo()
                {
                    Name = this.appNameTextBox.Text,
                    Version = "1.0"
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
    }
}
