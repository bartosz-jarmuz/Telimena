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
        public Form1()
        {
            InitializeComponent();
        }


        private async void SendUpdateAppUsageButton_Click(object sender, EventArgs e)
        {
            var teli = new Telimena(this.apiUrlTextBox.Text);
            var result = await teli.ReportUsage(this.functionNameTextBox.Text);
            if (result.IsMessageSuccessful)
            {

                this.resultTextBox.Text = new JavaScriptSerializer().Serialize(result);
            }
            else
            {
                MessageBox.Show(result.Exception.ToString());
            }
        }

     
    }
}
