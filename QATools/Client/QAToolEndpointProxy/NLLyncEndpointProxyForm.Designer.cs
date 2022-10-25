namespace NLLyncEndpointProxy
{
    partial class NLLyncEndpointProxyForm
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
            this.textSendMessage = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textReceivedMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.comboxUsers = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnManulClassify = new System.Windows.Forms.Button();
            this.comboBoxSFBObjUri = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxTestCaseFile = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonOpenTestCase = new System.Windows.Forms.Button();
            this.buttonStartCase = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textSendMessage
            // 
            this.textSendMessage.Location = new System.Drawing.Point(12, 171);
            this.textSendMessage.Multiline = true;
            this.textSendMessage.Name = "textSendMessage";
            this.textSendMessage.Size = new System.Drawing.Size(526, 116);
            this.textSendMessage.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Runtime Status";
            // 
            // textReceivedMessage
            // 
            this.textReceivedMessage.Location = new System.Drawing.Point(12, 49);
            this.textReceivedMessage.Multiline = true;
            this.textReceivedMessage.Name = "textReceivedMessage";
            this.textReceivedMessage.Size = new System.Drawing.Size(526, 116);
            this.textReceivedMessage.TabIndex = 2;
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(428, 293);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(110, 21);
            this.btnSend.TabIndex = 3;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // comboxUsers
            // 
            this.comboxUsers.FormattingEnabled = true;
            this.comboxUsers.Items.AddRange(new object[] {
            "sip:abraham.lincoln@lync.nextlabs.solutions",
            "sip:kim1.yang@lync.nextlabs.solutions"});
            this.comboxUsers.Location = new System.Drawing.Point(78, 293);
            this.comboxUsers.Name = "comboxUsers";
            this.comboxUsers.Size = new System.Drawing.Size(344, 21);
            this.comboxUsers.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 293);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "User Sip Uri";
            // 
            // btnManulClassify
            // 
            this.btnManulClassify.Location = new System.Drawing.Point(428, 320);
            this.btnManulClassify.Name = "btnManulClassify";
            this.btnManulClassify.Size = new System.Drawing.Size(110, 24);
            this.btnManulClassify.TabIndex = 6;
            this.btnManulClassify.Text = "Assistant command";
            this.btnManulClassify.UseVisualStyleBackColor = true;
            // 
            // comboBoxSFBObjUri
            // 
            this.comboBoxSFBObjUri.FormattingEnabled = true;
            this.comboBoxSFBObjUri.Location = new System.Drawing.Point(78, 320);
            this.comboBoxSFBObjUri.Name = "comboBoxSFBObjUri";
            this.comboBoxSFBObjUri.Size = new System.Drawing.Size(344, 21);
            this.comboBoxSFBObjUri.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 323);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "SFB Obj Uri";
            // 
            // comboBoxType
            // 
            this.comboBoxType.FormattingEnabled = true;
            this.comboBoxType.Items.AddRange(new object[] {
            "emClassifyMeeting",
            "emClassifyChatRoom"});
            this.comboBoxType.Location = new System.Drawing.Point(78, 347);
            this.comboBoxType.Name = "comboBoxType";
            this.comboBoxType.Size = new System.Drawing.Size(344, 21);
            this.comboBoxType.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 350);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Type";
            // 
            // textBoxTestCaseFile
            // 
            this.textBoxTestCaseFile.Location = new System.Drawing.Point(78, 376);
            this.textBoxTestCaseFile.Name = "textBoxTestCaseFile";
            this.textBoxTestCaseFile.Size = new System.Drawing.Size(344, 20);
            this.textBoxTestCaseFile.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 380);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "TestCase: ";
            // 
            // buttonOpenTestCase
            // 
            this.buttonOpenTestCase.Location = new System.Drawing.Point(428, 374);
            this.buttonOpenTestCase.Name = "buttonOpenTestCase";
            this.buttonOpenTestCase.Size = new System.Drawing.Size(75, 23);
            this.buttonOpenTestCase.TabIndex = 13;
            this.buttonOpenTestCase.Text = "Open";
            this.buttonOpenTestCase.UseVisualStyleBackColor = true;
            this.buttonOpenTestCase.Click += new System.EventHandler(this.buttonOpenTestCase_Click);
            // 
            // buttonStartCase
            // 
            this.buttonStartCase.Location = new System.Drawing.Point(508, 374);
            this.buttonStartCase.Name = "buttonStartCase";
            this.buttonStartCase.Size = new System.Drawing.Size(75, 23);
            this.buttonStartCase.TabIndex = 14;
            this.buttonStartCase.Text = "Start";
            this.buttonStartCase.UseVisualStyleBackColor = true;
            this.buttonStartCase.Click += new System.EventHandler(this.buttonStartCase_Click);
            // 
            // NLLyncEndpointProxyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(595, 430);
            this.Controls.Add(this.buttonStartCase);
            this.Controls.Add(this.buttonOpenTestCase);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxTestCaseFile);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBoxType);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBoxSFBObjUri);
            this.Controls.Add(this.btnManulClassify);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboxUsers);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.textReceivedMessage);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textSendMessage);
            this.Name = "NLLyncEndpointProxyForm";
            this.Text = "NLLyncEndpointProxyForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textSendMessage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textReceivedMessage;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.ComboBox comboxUsers;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnManulClassify;
        private System.Windows.Forms.ComboBox comboBoxSFBObjUri;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxTestCaseFile;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonOpenTestCase;
        private System.Windows.Forms.Button buttonStartCase;
    }
}

