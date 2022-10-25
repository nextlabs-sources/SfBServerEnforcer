namespace QAToolControlCenter
{
    partial class ControlCenterMainForm
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Conference");
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabPageBasicInfo = new System.Windows.Forms.TabPage();
            this.btnBasicInfoOK = new System.Windows.Forms.Button();
            this.textControlCenterPort = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textPerformanceResultLogFolder = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textPerformanceOrgLogFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPageUserInfo = new System.Windows.Forms.TabPage();
            this.btnUserInfoOK = new System.Windows.Forms.Button();
            this.groupBoxCheckUserRuntimeInfo = new System.Windows.Forms.GroupBox();
            this.btnUserChecking = new System.Windows.Forms.Button();
            this.textUserCheckedIndex = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.groupBoxUserInfo = new System.Windows.Forms.GroupBox();
            this.textUserMaxIndex = new System.Windows.Forms.TextBox();
            this.textUserMinIndex = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textUserLastName = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboxPassword = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textUserBaseFirstName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textUserDomain = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textServerFQDN = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPageConferenceInfo = new System.Windows.Forms.TabPage();
            this.btnDeleteConferenceInfo = new System.Windows.Forms.Button();
            this.btnAppendConference = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.textJoinedUserMaxIndex = new System.Windows.Forms.TextBox();
            this.textJoinedUserMinIndex = new System.Windows.Forms.TextBox();
            this.textIMInterval = new System.Windows.Forms.TextBox();
            this.textJoinTimes = new System.Windows.Forms.TextBox();
            this.textJoinInterval = new System.Windows.Forms.TextBox();
            this.textConferenceLength = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.textConferenceUri = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.treeConference = new System.Windows.Forms.TreeView();
            this.tabMain.SuspendLayout();
            this.tabPageBasicInfo.SuspendLayout();
            this.tabPageUserInfo.SuspendLayout();
            this.groupBoxCheckUserRuntimeInfo.SuspendLayout();
            this.groupBoxUserInfo.SuspendLayout();
            this.tabPageConferenceInfo.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabPageBasicInfo);
            this.tabMain.Controls.Add(this.tabPageUserInfo);
            this.tabMain.Controls.Add(this.tabPageConferenceInfo);
            this.tabMain.Location = new System.Drawing.Point(-1, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(1092, 535);
            this.tabMain.TabIndex = 0;
            // 
            // tabPageBasicInfo
            // 
            this.tabPageBasicInfo.Controls.Add(this.btnBasicInfoOK);
            this.tabPageBasicInfo.Controls.Add(this.textControlCenterPort);
            this.tabPageBasicInfo.Controls.Add(this.label10);
            this.tabPageBasicInfo.Controls.Add(this.textPerformanceResultLogFolder);
            this.tabPageBasicInfo.Controls.Add(this.label2);
            this.tabPageBasicInfo.Controls.Add(this.textPerformanceOrgLogFolder);
            this.tabPageBasicInfo.Controls.Add(this.label1);
            this.tabPageBasicInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPageBasicInfo.Name = "tabPageBasicInfo";
            this.tabPageBasicInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageBasicInfo.Size = new System.Drawing.Size(1084, 509);
            this.tabPageBasicInfo.TabIndex = 0;
            this.tabPageBasicInfo.Text = "Basic Info";
            this.tabPageBasicInfo.UseVisualStyleBackColor = true;
            // 
            // btnBasicInfoOK
            // 
            this.btnBasicInfoOK.Location = new System.Drawing.Point(938, 470);
            this.btnBasicInfoOK.Name = "btnBasicInfoOK";
            this.btnBasicInfoOK.Size = new System.Drawing.Size(127, 23);
            this.btnBasicInfoOK.TabIndex = 12;
            this.btnBasicInfoOK.Text = "OK";
            this.btnBasicInfoOK.UseVisualStyleBackColor = true;
            this.btnBasicInfoOK.Click += new System.EventHandler(this.btnBasicInfoOK_Click);
            // 
            // textControlCenterPort
            // 
            this.textControlCenterPort.Location = new System.Drawing.Point(161, 72);
            this.textControlCenterPort.Name = "textControlCenterPort";
            this.textControlCenterPort.Size = new System.Drawing.Size(123, 20);
            this.textControlCenterPort.TabIndex = 11;
            this.textControlCenterPort.Text = "50000";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 72);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(26, 13);
            this.label10.TabIndex = 10;
            this.label10.Text = "Port";
            // 
            // textPerformanceResultLogFolder
            // 
            this.textPerformanceResultLogFolder.Location = new System.Drawing.Point(161, 44);
            this.textPerformanceResultLogFolder.Name = "textPerformanceResultLogFolder";
            this.textPerformanceResultLogFolder.Size = new System.Drawing.Size(905, 20);
            this.textPerformanceResultLogFolder.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(144, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "PerformanceResultLogFolder";
            // 
            // textPerformanceOrgLogFolder
            // 
            this.textPerformanceOrgLogFolder.Location = new System.Drawing.Point(161, 15);
            this.textPerformanceOrgLogFolder.Name = "textPerformanceOrgLogFolder";
            this.textPerformanceOrgLogFolder.Size = new System.Drawing.Size(905, 20);
            this.textPerformanceOrgLogFolder.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "PerformanceOrgLogFolder";
            // 
            // tabPageUserInfo
            // 
            this.tabPageUserInfo.Controls.Add(this.btnUserInfoOK);
            this.tabPageUserInfo.Controls.Add(this.groupBoxCheckUserRuntimeInfo);
            this.tabPageUserInfo.Controls.Add(this.groupBoxUserInfo);
            this.tabPageUserInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPageUserInfo.Name = "tabPageUserInfo";
            this.tabPageUserInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageUserInfo.Size = new System.Drawing.Size(1084, 509);
            this.tabPageUserInfo.TabIndex = 1;
            this.tabPageUserInfo.Text = "User Info";
            this.tabPageUserInfo.UseVisualStyleBackColor = true;
            // 
            // btnUserInfoOK
            // 
            this.btnUserInfoOK.Location = new System.Drawing.Point(945, 475);
            this.btnUserInfoOK.Name = "btnUserInfoOK";
            this.btnUserInfoOK.Size = new System.Drawing.Size(127, 23);
            this.btnUserInfoOK.TabIndex = 13;
            this.btnUserInfoOK.Text = "OK";
            this.btnUserInfoOK.UseVisualStyleBackColor = true;
            this.btnUserInfoOK.Click += new System.EventHandler(this.btnUserInfoOK_Click);
            // 
            // groupBoxCheckUserRuntimeInfo
            // 
            this.groupBoxCheckUserRuntimeInfo.Controls.Add(this.btnUserChecking);
            this.groupBoxCheckUserRuntimeInfo.Controls.Add(this.textUserCheckedIndex);
            this.groupBoxCheckUserRuntimeInfo.Controls.Add(this.label11);
            this.groupBoxCheckUserRuntimeInfo.Location = new System.Drawing.Point(6, 117);
            this.groupBoxCheckUserRuntimeInfo.Name = "groupBoxCheckUserRuntimeInfo";
            this.groupBoxCheckUserRuntimeInfo.Size = new System.Drawing.Size(1072, 126);
            this.groupBoxCheckUserRuntimeInfo.TabIndex = 3;
            this.groupBoxCheckUserRuntimeInfo.TabStop = false;
            this.groupBoxCheckUserRuntimeInfo.Text = "Checking";
            // 
            // btnUserChecking
            // 
            this.btnUserChecking.Location = new System.Drawing.Point(10, 58);
            this.btnUserChecking.Name = "btnUserChecking";
            this.btnUserChecking.Size = new System.Drawing.Size(156, 23);
            this.btnUserChecking.TabIndex = 21;
            this.btnUserChecking.Text = "Checking";
            this.btnUserChecking.UseVisualStyleBackColor = true;
            this.btnUserChecking.Click += new System.EventHandler(this.btnUserChecking_Click);
            // 
            // textUserCheckedIndex
            // 
            this.textUserCheckedIndex.Location = new System.Drawing.Point(113, 26);
            this.textUserCheckedIndex.Name = "textUserCheckedIndex";
            this.textUserCheckedIndex.Size = new System.Drawing.Size(53, 20);
            this.textUserCheckedIndex.TabIndex = 20;
            this.textUserCheckedIndex.Text = "1";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 29);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(55, 13);
            this.label11.TabIndex = 19;
            this.label11.Text = "UserIndex";
            // 
            // groupBoxUserInfo
            // 
            this.groupBoxUserInfo.Controls.Add(this.textUserMaxIndex);
            this.groupBoxUserInfo.Controls.Add(this.textUserMinIndex);
            this.groupBoxUserInfo.Controls.Add(this.label9);
            this.groupBoxUserInfo.Controls.Add(this.label8);
            this.groupBoxUserInfo.Controls.Add(this.textUserLastName);
            this.groupBoxUserInfo.Controls.Add(this.label7);
            this.groupBoxUserInfo.Controls.Add(this.comboxPassword);
            this.groupBoxUserInfo.Controls.Add(this.label6);
            this.groupBoxUserInfo.Controls.Add(this.textUserBaseFirstName);
            this.groupBoxUserInfo.Controls.Add(this.label5);
            this.groupBoxUserInfo.Controls.Add(this.textUserDomain);
            this.groupBoxUserInfo.Controls.Add(this.label4);
            this.groupBoxUserInfo.Controls.Add(this.textServerFQDN);
            this.groupBoxUserInfo.Controls.Add(this.label3);
            this.groupBoxUserInfo.Location = new System.Drawing.Point(6, 6);
            this.groupBoxUserInfo.Name = "groupBoxUserInfo";
            this.groupBoxUserInfo.Size = new System.Drawing.Size(1072, 104);
            this.groupBoxUserInfo.TabIndex = 2;
            this.groupBoxUserInfo.TabStop = false;
            this.groupBoxUserInfo.Text = "User info";
            // 
            // textUserMaxIndex
            // 
            this.textUserMaxIndex.Location = new System.Drawing.Point(763, 70);
            this.textUserMaxIndex.Name = "textUserMaxIndex";
            this.textUserMaxIndex.Size = new System.Drawing.Size(53, 20);
            this.textUserMaxIndex.TabIndex = 19;
            this.textUserMaxIndex.Text = "100";
            // 
            // textUserMinIndex
            // 
            this.textUserMinIndex.Location = new System.Drawing.Point(578, 70);
            this.textUserMinIndex.Name = "textUserMinIndex";
            this.textUserMinIndex.Size = new System.Drawing.Size(53, 20);
            this.textUserMinIndex.TabIndex = 18;
            this.textUserMinIndex.Text = "1";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(682, 70);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(75, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "UserMaxIndex";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(472, 70);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(72, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "UserMinIndex";
            // 
            // textUserLastName
            // 
            this.textUserLastName.Location = new System.Drawing.Point(578, 36);
            this.textUserLastName.Name = "textUserLastName";
            this.textUserLastName.Size = new System.Drawing.Size(488, 20);
            this.textUserLastName.TabIndex = 13;
            this.textUserLastName.Text = "lyncDev";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(472, 43);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "UserLastName";
            // 
            // comboxPassword
            // 
            this.comboxPassword.FormattingEnabled = true;
            this.comboxPassword.Items.AddRange(new object[] {
            "SameAsUserName"});
            this.comboxPassword.Location = new System.Drawing.Point(113, 62);
            this.comboxPassword.Name = "comboxPassword";
            this.comboxPassword.Size = new System.Drawing.Size(234, 21);
            this.comboxPassword.TabIndex = 11;
            this.comboxPassword.Text = "123blue!";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 70);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Password";
            // 
            // textUserBaseFirstName
            // 
            this.textUserBaseFirstName.Location = new System.Drawing.Point(113, 36);
            this.textUserBaseFirstName.Name = "textUserBaseFirstName";
            this.textUserBaseFirstName.Size = new System.Drawing.Size(353, 20);
            this.textUserBaseFirstName.TabIndex = 9;
            this.textUserBaseFirstName.Text = "kimTest2";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "UserBaseFirstName";
            // 
            // textUserDomain
            // 
            this.textUserDomain.Location = new System.Drawing.Point(578, 12);
            this.textUserDomain.Name = "textUserDomain";
            this.textUserDomain.Size = new System.Drawing.Size(488, 20);
            this.textUserDomain.TabIndex = 7;
            this.textUserDomain.Text = "lab11.com";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(472, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "UserDomain";
            // 
            // textServerFQDN
            // 
            this.textServerFQDN.Location = new System.Drawing.Point(113, 12);
            this.textServerFQDN.Name = "textServerFQDN";
            this.textServerFQDN.Size = new System.Drawing.Size(353, 20);
            this.textServerFQDN.TabIndex = 5;
            this.textServerFQDN.Text = "LyncDev.lab11.com";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "ServerFQDN";
            // 
            // tabPageConferenceInfo
            // 
            this.tabPageConferenceInfo.Controls.Add(this.btnDeleteConferenceInfo);
            this.tabPageConferenceInfo.Controls.Add(this.btnAppendConference);
            this.tabPageConferenceInfo.Controls.Add(this.groupBox3);
            this.tabPageConferenceInfo.Controls.Add(this.treeConference);
            this.tabPageConferenceInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPageConferenceInfo.Name = "tabPageConferenceInfo";
            this.tabPageConferenceInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageConferenceInfo.Size = new System.Drawing.Size(1084, 509);
            this.tabPageConferenceInfo.TabIndex = 2;
            this.tabPageConferenceInfo.Text = "Conference Info";
            this.tabPageConferenceInfo.UseVisualStyleBackColor = true;
            // 
            // btnDeleteConferenceInfo
            // 
            this.btnDeleteConferenceInfo.Location = new System.Drawing.Point(817, 475);
            this.btnDeleteConferenceInfo.Name = "btnDeleteConferenceInfo";
            this.btnDeleteConferenceInfo.Size = new System.Drawing.Size(127, 23);
            this.btnDeleteConferenceInfo.TabIndex = 16;
            this.btnDeleteConferenceInfo.Text = "Delete";
            this.btnDeleteConferenceInfo.UseVisualStyleBackColor = true;
            this.btnDeleteConferenceInfo.Visible = false;
            this.btnDeleteConferenceInfo.Click += new System.EventHandler(this.btnDeleteConferenceInfo_Click);
            // 
            // btnAppendConference
            // 
            this.btnAppendConference.Location = new System.Drawing.Point(950, 475);
            this.btnAppendConference.Name = "btnAppendConference";
            this.btnAppendConference.Size = new System.Drawing.Size(127, 23);
            this.btnAppendConference.TabIndex = 15;
            this.btnAppendConference.Text = "Append/Change";
            this.btnAppendConference.UseVisualStyleBackColor = true;
            this.btnAppendConference.Click += new System.EventHandler(this.btnAppendConference_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.textJoinedUserMaxIndex);
            this.groupBox3.Controls.Add(this.textJoinedUserMinIndex);
            this.groupBox3.Controls.Add(this.textIMInterval);
            this.groupBox3.Controls.Add(this.textJoinTimes);
            this.groupBox3.Controls.Add(this.textJoinInterval);
            this.groupBox3.Controls.Add(this.textConferenceLength);
            this.groupBox3.Controls.Add(this.label17);
            this.groupBox3.Controls.Add(this.label16);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.textConferenceUri);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.label18);
            this.groupBox3.Location = new System.Drawing.Point(270, 7);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(807, 172);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Conference info";
            // 
            // textJoinedUserMaxIndex
            // 
            this.textJoinedUserMaxIndex.Location = new System.Drawing.Point(347, 84);
            this.textJoinedUserMaxIndex.Name = "textJoinedUserMaxIndex";
            this.textJoinedUserMaxIndex.Size = new System.Drawing.Size(78, 20);
            this.textJoinedUserMaxIndex.TabIndex = 35;
            this.textJoinedUserMaxIndex.Text = "50";
            // 
            // textJoinedUserMinIndex
            // 
            this.textJoinedUserMinIndex.Location = new System.Drawing.Point(116, 84);
            this.textJoinedUserMinIndex.Name = "textJoinedUserMinIndex";
            this.textJoinedUserMinIndex.Size = new System.Drawing.Size(78, 20);
            this.textJoinedUserMinIndex.TabIndex = 34;
            this.textJoinedUserMinIndex.Text = "1";
            // 
            // textIMInterval
            // 
            this.textIMInterval.Location = new System.Drawing.Point(347, 121);
            this.textIMInterval.Name = "textIMInterval";
            this.textIMInterval.Size = new System.Drawing.Size(78, 20);
            this.textIMInterval.TabIndex = 33;
            this.textIMInterval.Text = "1";
            // 
            // textJoinTimes
            // 
            this.textJoinTimes.Location = new System.Drawing.Point(347, 52);
            this.textJoinTimes.Name = "textJoinTimes";
            this.textJoinTimes.Size = new System.Drawing.Size(78, 20);
            this.textJoinTimes.TabIndex = 32;
            this.textJoinTimes.Text = "5";
            // 
            // textJoinInterval
            // 
            this.textJoinInterval.Location = new System.Drawing.Point(116, 121);
            this.textJoinInterval.Name = "textJoinInterval";
            this.textJoinInterval.Size = new System.Drawing.Size(78, 20);
            this.textJoinInterval.TabIndex = 31;
            this.textJoinInterval.Text = "5";
            // 
            // textConferenceLength
            // 
            this.textConferenceLength.Location = new System.Drawing.Point(113, 52);
            this.textConferenceLength.Name = "textConferenceLength";
            this.textConferenceLength.Size = new System.Drawing.Size(78, 20);
            this.textConferenceLength.TabIndex = 30;
            this.textConferenceLength.Text = "30";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(218, 124);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(54, 13);
            this.label17.TabIndex = 29;
            this.label17.Text = "IMInterval";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(218, 59);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(54, 13);
            this.label16.TabIndex = 28;
            this.label16.Text = "JoinTimes";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(7, 124);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(61, 13);
            this.label14.TabIndex = 27;
            this.label14.Text = "JoinInterval";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(7, 59);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(95, 13);
            this.label15.TabIndex = 26;
            this.label15.Text = "ConferenceLength";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 28);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(75, 13);
            this.label13.TabIndex = 24;
            this.label13.Text = "ConferenceUri";
            // 
            // textConferenceUri
            // 
            this.textConferenceUri.Location = new System.Drawing.Point(87, 21);
            this.textConferenceUri.Name = "textConferenceUri";
            this.textConferenceUri.Size = new System.Drawing.Size(379, 20);
            this.textConferenceUri.TabIndex = 23;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(218, 94);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(106, 13);
            this.label12.TabIndex = 21;
            this.label12.Text = "JoinedUserMaxIndex";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(6, 91);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(103, 13);
            this.label18.TabIndex = 20;
            this.label18.Text = "JoinedUserMinIndex";
            // 
            // treeConference
            // 
            this.treeConference.Location = new System.Drawing.Point(10, 7);
            this.treeConference.Name = "treeConference";
            treeNode1.Name = "rootNodeConferences";
            treeNode1.Text = "Conference";
            this.treeConference.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeConference.Size = new System.Drawing.Size(254, 491);
            this.treeConference.TabIndex = 0;
            this.treeConference.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeConference_AfterSelect);
            // 
            // ControlCenterMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1092, 532);
            this.Controls.Add(this.tabMain);
            this.MinimizeBox = false;
            this.Name = "ControlCenterMainForm";
            this.Text = "ControlCenterMainForm";
            this.tabMain.ResumeLayout(false);
            this.tabPageBasicInfo.ResumeLayout(false);
            this.tabPageBasicInfo.PerformLayout();
            this.tabPageUserInfo.ResumeLayout(false);
            this.groupBoxCheckUserRuntimeInfo.ResumeLayout(false);
            this.groupBoxCheckUserRuntimeInfo.PerformLayout();
            this.groupBoxUserInfo.ResumeLayout(false);
            this.groupBoxUserInfo.PerformLayout();
            this.tabPageConferenceInfo.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabPageBasicInfo;
        private System.Windows.Forms.TabPage tabPageUserInfo;
        private System.Windows.Forms.TextBox textControlCenterPort;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textPerformanceResultLogFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textPerformanceOrgLogFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBoxUserInfo;
        private System.Windows.Forms.TextBox textUserMaxIndex;
        private System.Windows.Forms.TextBox textUserMinIndex;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textUserLastName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboxPassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textUserBaseFirstName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textUserDomain;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textServerFQDN;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBoxCheckUserRuntimeInfo;
        private System.Windows.Forms.TextBox textUserCheckedIndex;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnUserChecking;
        private System.Windows.Forms.TabPage tabPageConferenceInfo;
        private System.Windows.Forms.TreeView treeConference;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox textJoinedUserMaxIndex;
        private System.Windows.Forms.TextBox textJoinedUserMinIndex;
        private System.Windows.Forms.TextBox textIMInterval;
        private System.Windows.Forms.TextBox textJoinTimes;
        private System.Windows.Forms.TextBox textJoinInterval;
        private System.Windows.Forms.TextBox textConferenceLength;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textConferenceUri;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Button btnBasicInfoOK;
        private System.Windows.Forms.Button btnUserInfoOK;
        private System.Windows.Forms.Button btnAppendConference;
        private System.Windows.Forms.Button btnDeleteConferenceInfo;
    }
}