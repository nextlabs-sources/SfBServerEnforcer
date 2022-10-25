namespace SFBControlPanel
{
    partial class SFBControlPanel
    {
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("ChatCategoryPlaceholder");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Chat Room Categories", new System.Windows.Forms.TreeNode[] {
            treeNode1});
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SFBControlPanel));
            this.trSFBESettings = new System.Windows.Forms.TreeView();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.pgCategorySetting = new System.Windows.Forms.TabPage();
            this.groupClassficationArea = new System.Windows.Forms.GroupBox();
            this.txtClassificationWarning = new System.Windows.Forms.TextBox();
            this.labClassificationName = new System.Windows.Forms.Label();
            this.comboxClassificationSchemas = new System.Windows.Forms.ComboBox();
            this.groupEnforcerArea = new System.Windows.Forms.GroupBox();
            this.checkBoxEnforcer = new System.Windows.Forms.CheckBox();
            this.txtEnforcerExplain = new System.Windows.Forms.TextBox();
            this.groupForceEnforcerArea = new System.Windows.Forms.GroupBox();
            this.checkBoxForceEnforcer = new System.Windows.Forms.CheckBox();
            this.txtForceEnforcerExplain = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.pgMessagePage = new System.Windows.Forms.TabPage();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.tabControl.SuspendLayout();
            this.pgCategorySetting.SuspendLayout();
            this.groupClassficationArea.SuspendLayout();
            this.groupEnforcerArea.SuspendLayout();
            this.groupForceEnforcerArea.SuspendLayout();
            this.pgMessagePage.SuspendLayout();
            this.SuspendLayout();
            // 
            // trSFBESettings
            // 
            this.trSFBESettings.Location = new System.Drawing.Point(12, 12);
            this.trSFBESettings.Name = "trSFBESettings";
            treeNode1.Name = "ndChatCategoryPlaceholder";
            treeNode1.Text = "ChatCategoryPlaceholder";
            treeNode2.Name = "ndRootChatCategories";
            treeNode2.Text = "Chat Room Categories";
            this.trSFBESettings.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode2});
            this.trSFBESettings.Size = new System.Drawing.Size(180, 440);
            this.trSFBESettings.TabIndex = 0;
            this.trSFBESettings.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.trSFBESettings_BeforeExpand);
            this.trSFBESettings.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trSFBESettings_AfterSelect);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.pgCategorySetting);
            this.tabControl.Controls.Add(this.pgMessagePage);
            this.tabControl.Location = new System.Drawing.Point(199, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(470, 440);
            this.tabControl.TabIndex = 2;
            // 
            // pgCategorySetting
            // 
            this.pgCategorySetting.BackColor = System.Drawing.SystemColors.Control;
            this.pgCategorySetting.Controls.Add(this.txtClassificationWarning);
            this.pgCategorySetting.Controls.Add(this.groupClassficationArea);
            this.pgCategorySetting.Controls.Add(this.groupEnforcerArea);
            this.pgCategorySetting.Controls.Add(this.groupForceEnforcerArea);
            this.pgCategorySetting.Controls.Add(this.btnOK);
            this.pgCategorySetting.Controls.Add(this.btnApply);
            this.pgCategorySetting.Location = new System.Drawing.Point(4, 22);
            this.pgCategorySetting.Name = "pgCategorySetting";
            this.pgCategorySetting.Padding = new System.Windows.Forms.Padding(3);
            this.pgCategorySetting.Size = new System.Drawing.Size(462, 414);
            this.pgCategorySetting.TabIndex = 0;
            this.pgCategorySetting.Text = "Category Setting";
            // 
            // groupClassficationArea
            // 
            this.groupClassficationArea.Controls.Add(this.labClassificationName);
            this.groupClassficationArea.Controls.Add(this.comboxClassificationSchemas);
            this.groupClassficationArea.Location = new System.Drawing.Point(9, 240);
            this.groupClassficationArea.Name = "groupClassficationArea";
            this.groupClassficationArea.Size = new System.Drawing.Size(450, 79);
            this.groupClassficationArea.TabIndex = 9;
            this.groupClassficationArea.TabStop = false;
            this.groupClassficationArea.Text = "Classification";
            this.groupClassficationArea.Visible = false;
            // 
            // txtClassificationWarning
            // 
            this.txtClassificationWarning.BackColor = System.Drawing.Color.Yellow;
            this.txtClassificationWarning.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtClassificationWarning.Enabled = false;
            this.txtClassificationWarning.Location = new System.Drawing.Point(28, 325);
            this.txtClassificationWarning.Multiline = true;
            this.txtClassificationWarning.Name = "txtClassificationWarning";
            this.txtClassificationWarning.Size = new System.Drawing.Size(385, 37);
            this.txtClassificationWarning.TabIndex = 6;
            this.txtClassificationWarning.Visible = false;
            // 
            // labClassificationName
            // 
            this.labClassificationName.AutoSize = true;
            this.labClassificationName.Location = new System.Drawing.Point(16, 35);
            this.labClassificationName.Name = "labClassificationName";
            this.labClassificationName.Size = new System.Drawing.Size(49, 13);
            this.labClassificationName.TabIndex = 3;
            this.labClassificationName.Text = "Schema:";
            // 
            // comboxClassificationSchemas
            // 
            this.comboxClassificationSchemas.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboxClassificationSchemas.FormattingEnabled = true;
            this.comboxClassificationSchemas.IntegralHeight = false;
            this.comboxClassificationSchemas.ItemHeight = 13;
            this.comboxClassificationSchemas.Location = new System.Drawing.Point(68, 32);
            this.comboxClassificationSchemas.MaxDropDownItems = 10;
            this.comboxClassificationSchemas.Name = "comboxClassificationSchemas";
            this.comboxClassificationSchemas.Size = new System.Drawing.Size(336, 21);
            this.comboxClassificationSchemas.TabIndex = 2;
            // 
            // groupEnforcerArea
            // 
            this.groupEnforcerArea.Controls.Add(this.checkBoxEnforcer);
            this.groupEnforcerArea.Controls.Add(this.txtEnforcerExplain);
            this.groupEnforcerArea.Location = new System.Drawing.Point(9, 6);
            this.groupEnforcerArea.Name = "groupEnforcerArea";
            this.groupEnforcerArea.Size = new System.Drawing.Size(450, 111);
            this.groupEnforcerArea.TabIndex = 9;
            this.groupEnforcerArea.TabStop = false;
            // 
            // checkBoxEnforcer
            // 
            this.checkBoxEnforcer.AutoSize = true;
            this.checkBoxEnforcer.Location = new System.Drawing.Point(16, 19);
            this.checkBoxEnforcer.Name = "checkBoxEnforcer";
            this.checkBoxEnforcer.Size = new System.Drawing.Size(188, 17);
            this.checkBoxEnforcer.TabIndex = 0;
            this.checkBoxEnforcer.Text = "Safeguard with NextLabs Enforcer";
            this.checkBoxEnforcer.UseVisualStyleBackColor = true;
            this.checkBoxEnforcer.CheckedChanged += new System.EventHandler(this.checkBoxEnforcer_CheckedChanged);
            // 
            // txtEnforcerExplain
            // 
            this.txtEnforcerExplain.BackColor = System.Drawing.SystemColors.Control;
            this.txtEnforcerExplain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtEnforcerExplain.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtEnforcerExplain.Enabled = false;
            this.txtEnforcerExplain.Location = new System.Drawing.Point(39, 42);
            this.txtEnforcerExplain.Multiline = true;
            this.txtEnforcerExplain.Name = "txtEnforcerExplain";
            this.txtEnforcerExplain.Size = new System.Drawing.Size(385, 60);
            this.txtEnforcerExplain.TabIndex = 4;
            this.txtEnforcerExplain.Text = "If selected, chat room created under this category will be enforced by SFB Server" +
    " Enforce.";
            // 
            // groupForceEnforcerArea
            // 
            this.groupForceEnforcerArea.Controls.Add(this.checkBoxForceEnforcer);
            this.groupForceEnforcerArea.Controls.Add(this.txtForceEnforcerExplain);
            this.groupForceEnforcerArea.Location = new System.Drawing.Point(9, 123);
            this.groupForceEnforcerArea.Name = "groupForceEnforcerArea";
            this.groupForceEnforcerArea.Size = new System.Drawing.Size(450, 111);
            this.groupForceEnforcerArea.TabIndex = 8;
            this.groupForceEnforcerArea.TabStop = false;
            this.groupForceEnforcerArea.Visible = false;
            // 
            // checkBoxForceEnforcer
            // 
            this.checkBoxForceEnforcer.AutoSize = true;
            this.checkBoxForceEnforcer.Location = new System.Drawing.Point(16, 19);
            this.checkBoxForceEnforcer.Name = "checkBoxForceEnforcer";
            this.checkBoxForceEnforcer.Size = new System.Drawing.Size(177, 17);
            this.checkBoxForceEnforcer.TabIndex = 1;
            this.checkBoxForceEnforcer.Text = "Chat room manager can\'t modify";
            this.checkBoxForceEnforcer.UseVisualStyleBackColor = true;
            // 
            // txtForceEnforcerExplain
            // 
            this.txtForceEnforcerExplain.BackColor = System.Drawing.SystemColors.Control;
            this.txtForceEnforcerExplain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtForceEnforcerExplain.Enabled = false;
            this.txtForceEnforcerExplain.Location = new System.Drawing.Point(39, 42);
            this.txtForceEnforcerExplain.Multiline = true;
            this.txtForceEnforcerExplain.Name = "txtForceEnforcerExplain";
            this.txtForceEnforcerExplain.Size = new System.Drawing.Size(385, 60);
            this.txtForceEnforcerExplain.TabIndex = 5;
            this.txtForceEnforcerExplain.Text = "Whether or not this chat room is enforced by SFB Server Enforcer is set by catago" +
    "ry and chat room manager can\'t modify it.";
            // 
            // btnOK
            // 
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Location = new System.Drawing.Point(380, 380);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnApply
            // 
            this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnApply.Location = new System.Drawing.Point(292, 380);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(75, 23);
            this.btnApply.TabIndex = 6;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // pgMessagePage
            // 
            this.pgMessagePage.BackColor = System.Drawing.SystemColors.Control;
            this.pgMessagePage.Controls.Add(this.txtMessage);
            this.pgMessagePage.Location = new System.Drawing.Point(4, 22);
            this.pgMessagePage.Name = "pgMessagePage";
            this.pgMessagePage.Padding = new System.Windows.Forms.Padding(3);
            this.pgMessagePage.Size = new System.Drawing.Size(462, 414);
            this.pgMessagePage.TabIndex = 1;
            this.pgMessagePage.Text = "Message";
            // 
            // txtMessage
            // 
            this.txtMessage.BackColor = System.Drawing.SystemColors.Control;
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMessage.Enabled = false;
            this.txtMessage.Location = new System.Drawing.Point(20, 150);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(418, 60);
            this.txtMessage.TabIndex = 5;
            this.txtMessage.Text = "loading chat Category info ...";
            // 
            // SFBControlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(680, 458);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.trSFBESettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SFBControlPanel";
            this.Text = "SFB Server Enforcer Control Panel";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SFBControlPanel_FormClosing);
            this.tabControl.ResumeLayout(false);
            this.pgCategorySetting.ResumeLayout(false);
            this.pgCategorySetting.PerformLayout();
            this.groupClassficationArea.ResumeLayout(false);
            this.groupClassficationArea.PerformLayout();
            this.groupEnforcerArea.ResumeLayout(false);
            this.groupEnforcerArea.PerformLayout();
            this.groupForceEnforcerArea.ResumeLayout(false);
            this.groupForceEnforcerArea.PerformLayout();
            this.pgMessagePage.ResumeLayout(false);
            this.pgMessagePage.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.TreeView trSFBESettings;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage pgCategorySetting;
        private System.Windows.Forms.TabPage pgMessagePage;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.TextBox txtForceEnforcerExplain;
        private System.Windows.Forms.TextBox txtEnforcerExplain;
        private System.Windows.Forms.CheckBox checkBoxEnforcer;
        private System.Windows.Forms.CheckBox checkBoxForceEnforcer;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.GroupBox groupForceEnforcerArea;
        private System.Windows.Forms.GroupBox groupEnforcerArea;
        private System.Windows.Forms.GroupBox groupClassficationArea;
        private System.Windows.Forms.ComboBox comboxClassificationSchemas;
        private System.Windows.Forms.Label labClassificationName;
        private System.Windows.Forms.TextBox txtClassificationWarning;
    }
}

