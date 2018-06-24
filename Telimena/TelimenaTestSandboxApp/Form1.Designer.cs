namespace TelimenaTestSandboxApp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.apiUrlTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.functionNameTextBox = new System.Windows.Forms.TextBox();
            this.F = new System.Windows.Forms.Label();
            this.SendUpdateAppUsageButton = new System.Windows.Forms.Button();
            this.resultTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.appNameTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.userNameTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.setAppButton = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.static_functionNameTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.static_sendUsageReportButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // apiUrlTextBox
            // 
            this.apiUrlTextBox.Location = new System.Drawing.Point(93, 9);
            this.apiUrlTextBox.Name = "apiUrlTextBox";
            this.apiUrlTextBox.Size = new System.Drawing.Size(120, 20);
            this.apiUrlTextBox.TabIndex = 0;
            this.apiUrlTextBox.Text = "http://localhost:7757/";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Api Base URL";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.functionNameTextBox);
            this.groupBox1.Controls.Add(this.F);
            this.groupBox1.Controls.Add(this.SendUpdateAppUsageButton);
            this.groupBox1.Location = new System.Drawing.Point(20, 46);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(318, 90);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "INSTANCE CLIENT - Update app usage";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(14, 46);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(114, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Call \'Initialize\'";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.InitializeButton_Click);
            // 
            // functionNameTextBox
            // 
            this.functionNameTextBox.Location = new System.Drawing.Point(100, 22);
            this.functionNameTextBox.Name = "functionNameTextBox";
            this.functionNameTextBox.Size = new System.Drawing.Size(212, 20);
            this.functionNameTextBox.TabIndex = 2;
            // 
            // F
            // 
            this.F.AutoSize = true;
            this.F.Location = new System.Drawing.Point(17, 25);
            this.F.Name = "F";
            this.F.Size = new System.Drawing.Size(77, 13);
            this.F.TabIndex = 1;
            this.F.Text = "Function name";
            // 
            // SendUpdateAppUsageButton
            // 
            this.SendUpdateAppUsageButton.Location = new System.Drawing.Point(237, 48);
            this.SendUpdateAppUsageButton.Name = "SendUpdateAppUsageButton";
            this.SendUpdateAppUsageButton.Size = new System.Drawing.Size(75, 23);
            this.SendUpdateAppUsageButton.TabIndex = 0;
            this.SendUpdateAppUsageButton.Text = "Send";
            this.SendUpdateAppUsageButton.UseVisualStyleBackColor = true;
            this.SendUpdateAppUsageButton.Click += new System.EventHandler(this.SendUpdateAppUsageButton_Click);
            // 
            // resultTextBox
            // 
            this.resultTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultTextBox.Location = new System.Drawing.Point(3, 16);
            this.resultTextBox.Multiline = true;
            this.resultTextBox.Name = "resultTextBox";
            this.resultTextBox.ReadOnly = true;
            this.resultTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.resultTextBox.Size = new System.Drawing.Size(715, 277);
            this.resultTextBox.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.resultTextBox);
            this.groupBox2.Location = new System.Drawing.Point(20, 142);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(721, 296);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Result";
            // 
            // appNameTextBox
            // 
            this.appNameTextBox.Location = new System.Drawing.Point(286, 9);
            this.appNameTextBox.Name = "appNameTextBox";
            this.appNameTextBox.Size = new System.Drawing.Size(166, 20);
            this.appNameTextBox.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(225, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "App name";
            // 
            // userNameTextBox
            // 
            this.userNameTextBox.Location = new System.Drawing.Point(520, 9);
            this.userNameTextBox.Name = "userNameTextBox";
            this.userNameTextBox.Size = new System.Drawing.Size(110, 20);
            this.userNameTextBox.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(459, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "User name";
            // 
            // setAppButton
            // 
            this.setAppButton.Location = new System.Drawing.Point(645, 7);
            this.setAppButton.Name = "setAppButton";
            this.setAppButton.Size = new System.Drawing.Size(75, 23);
            this.setAppButton.TabIndex = 5;
            this.setAppButton.Text = "Set App ";
            this.setAppButton.UseVisualStyleBackColor = true;
            this.setAppButton.Click += new System.EventHandler(this.setAppButton_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.static_functionNameTextBox);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.static_sendUsageReportButton);
            this.groupBox4.Location = new System.Drawing.Point(366, 46);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(318, 90);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "STATIC CLIENT - Update app usage";
            // 
            // static_functionNameTextBox
            // 
            this.static_functionNameTextBox.Location = new System.Drawing.Point(100, 22);
            this.static_functionNameTextBox.Name = "static_functionNameTextBox";
            this.static_functionNameTextBox.Size = new System.Drawing.Size(212, 20);
            this.static_functionNameTextBox.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Function name";
            // 
            // static_sendUsageReportButton
            // 
            this.static_sendUsageReportButton.Location = new System.Drawing.Point(237, 48);
            this.static_sendUsageReportButton.Name = "static_sendUsageReportButton";
            this.static_sendUsageReportButton.Size = new System.Drawing.Size(75, 23);
            this.static_sendUsageReportButton.TabIndex = 0;
            this.static_sendUsageReportButton.Text = "Send Usage Report";
            this.static_sendUsageReportButton.UseVisualStyleBackColor = true;
            this.static_sendUsageReportButton.Click += new System.EventHandler(this.static_sendUsageReportButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(753, 441);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.setAppButton);
            this.Controls.Add(this.userNameTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.appNameTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.apiUrlTextBox);
            this.Name = "Form1";
            this.Text = "Telimena sandbox";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox apiUrlTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label F;
        private System.Windows.Forms.Button SendUpdateAppUsageButton;
        private System.Windows.Forms.TextBox resultTextBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox functionNameTextBox;
        private System.Windows.Forms.TextBox appNameTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox userNameTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button setAppButton;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox static_functionNameTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button static_sendUsageReportButton;
    }
}

