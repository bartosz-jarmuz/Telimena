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
            this.sendSync_button = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.viewNameTextBox = new System.Windows.Forms.TextBox();
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
            this.static_viewNameTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.static_sendUsageReportButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.handleUpdatesButton = new System.Windows.Forms.Button();
            this.checkForUpdateButton = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.hammer_AppNumberSeedBox = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.hammer_StopBtn = new System.Windows.Forms.Button();
            this.hammer_DurationTextBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.hammer_delayMaxTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.hammer_delayMinTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.hammer_numberOfFuncs_TextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.hammer_numberOfUsers_TextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.hammer_numberOfApps_TextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.hammer_StartButton = new System.Windows.Forms.Button();
            this.useCurrentAppButton = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.apiKeyTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // apiUrlTextBox
            // 
            this.apiUrlTextBox.Location = new System.Drawing.Point(93, 9);
            this.apiUrlTextBox.Name = "apiUrlTextBox";
            this.apiUrlTextBox.Size = new System.Drawing.Size(134, 20);
            this.apiUrlTextBox.TabIndex = 0;
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
            this.groupBox1.Controls.Add(this.sendSync_button);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.viewNameTextBox);
            this.groupBox1.Controls.Add(this.F);
            this.groupBox1.Controls.Add(this.SendUpdateAppUsageButton);
            this.groupBox1.Location = new System.Drawing.Point(20, 46);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(318, 90);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "INSTANCE CLIENT - Update app usage";
            // 
            // sendSync_button
            // 
            this.sendSync_button.Location = new System.Drawing.Point(156, 48);
            this.sendSync_button.Name = "sendSync_button";
            this.sendSync_button.Size = new System.Drawing.Size(75, 23);
            this.sendSync_button.TabIndex = 5;
            this.sendSync_button.Text = "Send sync";
            this.sendSync_button.UseVisualStyleBackColor = true;
            this.sendSync_button.Click += new System.EventHandler(this.sendSync_button_Click);
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
            // viewNameTextBox
            // 
            this.viewNameTextBox.Location = new System.Drawing.Point(100, 22);
            this.viewNameTextBox.Name = "viewNameTextBox";
            this.viewNameTextBox.Size = new System.Drawing.Size(212, 20);
            this.viewNameTextBox.TabIndex = 2;
            // 
            // F
            // 
            this.F.AutoSize = true;
            this.F.Location = new System.Drawing.Point(17, 25);
            this.F.Name = "F";
            this.F.Size = new System.Drawing.Size(59, 13);
            this.F.TabIndex = 1;
            this.F.Text = "View name";
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
            this.resultTextBox.Size = new System.Drawing.Size(1018, 277);
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
            this.groupBox2.Size = new System.Drawing.Size(1024, 296);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Result";
            // 
            // appNameTextBox
            // 
            this.appNameTextBox.Location = new System.Drawing.Point(398, 9);
            this.appNameTextBox.Name = "appNameTextBox";
            this.appNameTextBox.Size = new System.Drawing.Size(84, 20);
            this.appNameTextBox.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(337, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "App name";
            // 
            // userNameTextBox
            // 
            this.userNameTextBox.Location = new System.Drawing.Point(546, 9);
            this.userNameTextBox.Name = "userNameTextBox";
            this.userNameTextBox.Size = new System.Drawing.Size(84, 20);
            this.userNameTextBox.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(482, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "User name";
            // 
            // setAppButton
            // 
            this.setAppButton.Location = new System.Drawing.Point(636, 7);
            this.setAppButton.Name = "setAppButton";
            this.setAppButton.Size = new System.Drawing.Size(54, 23);
            this.setAppButton.TabIndex = 5;
            this.setAppButton.Text = "Set App ";
            this.setAppButton.UseVisualStyleBackColor = true;
            this.setAppButton.Click += new System.EventHandler(this.setAppButton_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.static_viewNameTextBox);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.static_sendUsageReportButton);
            this.groupBox4.Location = new System.Drawing.Point(366, 46);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(318, 90);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "STATIC CLIENT - Update app usage";
            // 
            // static_viewNameTextBox
            // 
            this.static_viewNameTextBox.Location = new System.Drawing.Point(100, 22);
            this.static_viewNameTextBox.Name = "static_viewNameTextBox";
            this.static_viewNameTextBox.Size = new System.Drawing.Size(212, 20);
            this.static_viewNameTextBox.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "View name";
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
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.handleUpdatesButton);
            this.groupBox3.Controls.Add(this.checkForUpdateButton);
            this.groupBox3.Location = new System.Drawing.Point(690, 36);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(90, 100);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "App Updating";
            // 
            // handleUpdatesButton
            // 
            this.handleUpdatesButton.Location = new System.Drawing.Point(6, 58);
            this.handleUpdatesButton.Name = "handleUpdatesButton";
            this.handleUpdatesButton.Size = new System.Drawing.Size(75, 36);
            this.handleUpdatesButton.TabIndex = 1;
            this.handleUpdatesButton.Text = "Handle updates";
            this.handleUpdatesButton.UseVisualStyleBackColor = true;
            this.handleUpdatesButton.Click += new System.EventHandler(this.handleUpdatesButton_Click);
            // 
            // checkForUpdateButton
            // 
            this.checkForUpdateButton.Location = new System.Drawing.Point(6, 19);
            this.checkForUpdateButton.Name = "checkForUpdateButton";
            this.checkForUpdateButton.Size = new System.Drawing.Size(75, 38);
            this.checkForUpdateButton.TabIndex = 0;
            this.checkForUpdateButton.Text = "Check for update";
            this.checkForUpdateButton.UseVisualStyleBackColor = true;
            this.checkForUpdateButton.Click += new System.EventHandler(this.checkForUpdateButton_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.hammer_AppNumberSeedBox);
            this.groupBox5.Controls.Add(this.label12);
            this.groupBox5.Controls.Add(this.hammer_StopBtn);
            this.groupBox5.Controls.Add(this.hammer_DurationTextBox);
            this.groupBox5.Controls.Add(this.label11);
            this.groupBox5.Controls.Add(this.label10);
            this.groupBox5.Controls.Add(this.hammer_delayMaxTextBox);
            this.groupBox5.Controls.Add(this.label9);
            this.groupBox5.Controls.Add(this.hammer_delayMinTextBox);
            this.groupBox5.Controls.Add(this.label8);
            this.groupBox5.Controls.Add(this.hammer_numberOfFuncs_TextBox);
            this.groupBox5.Controls.Add(this.label7);
            this.groupBox5.Controls.Add(this.hammer_numberOfUsers_TextBox);
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Controls.Add(this.hammer_numberOfApps_TextBox);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.hammer_StartButton);
            this.groupBox5.Location = new System.Drawing.Point(786, 13);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(268, 123);
            this.groupBox5.TabIndex = 9;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "HAMMER";
            // 
            // hammer_AppNumberSeedBox
            // 
            this.hammer_AppNumberSeedBox.Location = new System.Drawing.Point(150, 22);
            this.hammer_AppNumberSeedBox.Name = "hammer_AppNumberSeedBox";
            this.hammer_AppNumberSeedBox.Size = new System.Drawing.Size(16, 20);
            this.hammer_AppNumberSeedBox.TabIndex = 16;
            this.hammer_AppNumberSeedBox.Text = "0";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(88, 25);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(56, 13);
            this.label12.TabIndex = 15;
            this.label12.Text = "App#seed";
            // 
            // hammer_StopBtn
            // 
            this.hammer_StopBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.hammer_StopBtn.Location = new System.Drawing.Point(208, 96);
            this.hammer_StopBtn.Name = "hammer_StopBtn";
            this.hammer_StopBtn.Size = new System.Drawing.Size(53, 23);
            this.hammer_StopBtn.TabIndex = 14;
            this.hammer_StopBtn.Text = "Stop";
            this.hammer_StopBtn.UseVisualStyleBackColor = true;
            this.hammer_StopBtn.Click += new System.EventHandler(this.hammer_StopBtn_Click);
            // 
            // hammer_DurationTextBox
            // 
            this.hammer_DurationTextBox.Location = new System.Drawing.Point(187, 48);
            this.hammer_DurationTextBox.Name = "hammer_DurationTextBox";
            this.hammer_DurationTextBox.Size = new System.Drawing.Size(40, 20);
            this.hammer_DurationTextBox.TabIndex = 13;
            this.hammer_DurationTextBox.Text = "1";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label11.Location = new System.Drawing.Point(113, 51);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(72, 13);
            this.label11.TabIndex = 12;
            this.label11.Text = "DURATION";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(207, 73);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(20, 13);
            this.label10.TabIndex = 11;
            this.label10.Text = "ms";
            // 
            // hammer_delayMaxTextBox
            // 
            this.hammer_delayMaxTextBox.Location = new System.Drawing.Point(164, 70);
            this.hammer_delayMaxTextBox.Name = "hammer_delayMaxTextBox";
            this.hammer_delayMaxTextBox.Size = new System.Drawing.Size(38, 20);
            this.hammer_delayMaxTextBox.TabIndex = 10;
            this.hammer_delayMaxTextBox.Text = "5000";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(133, 73);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(25, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "and";
            // 
            // hammer_delayMinTextBox
            // 
            this.hammer_delayMinTextBox.Location = new System.Drawing.Point(91, 70);
            this.hammer_delayMinTextBox.Name = "hammer_delayMinTextBox";
            this.hammer_delayMinTextBox.Size = new System.Drawing.Size(38, 20);
            this.hammer_delayMinTextBox.TabIndex = 8;
            this.hammer_delayMinTextBox.Text = "300";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 73);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(78, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Delay between";
            // 
            // hammer_numberOfFuncs_TextBox
            // 
            this.hammer_numberOfFuncs_TextBox.Location = new System.Drawing.Point(232, 22);
            this.hammer_numberOfFuncs_TextBox.Name = "hammer_numberOfFuncs_TextBox";
            this.hammer_numberOfFuncs_TextBox.Size = new System.Drawing.Size(23, 20);
            this.hammer_numberOfFuncs_TextBox.TabIndex = 6;
            this.hammer_numberOfFuncs_TextBox.Text = "5";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(172, 25);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(58, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "# of Funcs";
            // 
            // hammer_numberOfUsers_TextBox
            // 
            this.hammer_numberOfUsers_TextBox.Location = new System.Drawing.Point(69, 48);
            this.hammer_numberOfUsers_TextBox.Name = "hammer_numberOfUsers_TextBox";
            this.hammer_numberOfUsers_TextBox.Size = new System.Drawing.Size(27, 20);
            this.hammer_numberOfUsers_TextBox.TabIndex = 4;
            this.hammer_numberOfUsers_TextBox.Text = "5";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 51);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "# of Users";
            // 
            // hammer_numberOfApps_TextBox
            // 
            this.hammer_numberOfApps_TextBox.Location = new System.Drawing.Point(57, 22);
            this.hammer_numberOfApps_TextBox.Name = "hammer_numberOfApps_TextBox";
            this.hammer_numberOfApps_TextBox.Size = new System.Drawing.Size(28, 20);
            this.hammer_numberOfApps_TextBox.TabIndex = 2;
            this.hammer_numberOfApps_TextBox.Text = "3";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 25);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "# of Apps ";
            // 
            // hammer_StartButton
            // 
            this.hammer_StartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.hammer_StartButton.Location = new System.Drawing.Point(6, 96);
            this.hammer_StartButton.Name = "hammer_StartButton";
            this.hammer_StartButton.Size = new System.Drawing.Size(196, 23);
            this.hammer_StartButton.TabIndex = 0;
            this.hammer_StartButton.Text = "Hit the API with random calls";
            this.hammer_StartButton.UseVisualStyleBackColor = true;
            this.hammer_StartButton.Click += new System.EventHandler(this.hammer_StartButton_Click);
            // 
            // useCurrentAppButton
            // 
            this.useCurrentAppButton.Location = new System.Drawing.Point(696, 7);
            this.useCurrentAppButton.Name = "useCurrentAppButton";
            this.useCurrentAppButton.Size = new System.Drawing.Size(75, 23);
            this.useCurrentAppButton.TabIndex = 10;
            this.useCurrentAppButton.Text = "Use Current";
            this.useCurrentAppButton.UseVisualStyleBackColor = true;
            this.useCurrentAppButton.Click += new System.EventHandler(this.useCurrentAppButton_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(233, 13);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(25, 13);
            this.label13.TabIndex = 12;
            this.label13.Text = "Key";
            // 
            // apiKeyTextBox
            // 
            this.apiKeyTextBox.Location = new System.Drawing.Point(264, 10);
            this.apiKeyTextBox.Name = "apiKeyTextBox";
            this.apiKeyTextBox.Size = new System.Drawing.Size(67, 20);
            this.apiKeyTextBox.TabIndex = 11;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1056, 441);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.apiKeyTextBox);
            this.Controls.Add(this.useCurrentAppButton);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox3);
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
            this.groupBox3.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
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
        private System.Windows.Forms.TextBox viewNameTextBox;
        private System.Windows.Forms.TextBox appNameTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox userNameTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button setAppButton;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox static_viewNameTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button static_sendUsageReportButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button checkForUpdateButton;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox hammer_delayMinTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox hammer_numberOfFuncs_TextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox hammer_numberOfUsers_TextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox hammer_numberOfApps_TextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button hammer_StartButton;
        private System.Windows.Forms.TextBox hammer_delayMaxTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox hammer_DurationTextBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button hammer_StopBtn;
        private System.Windows.Forms.TextBox hammer_AppNumberSeedBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button handleUpdatesButton;
        private System.Windows.Forms.Button useCurrentAppButton;
        private System.Windows.Forms.Button sendSync_button;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox apiKeyTextBox;
    }
}

