using System.Windows.Forms;
using System.Drawing;

namespace ClassificationTool
{
    partial class ClassificationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region members
        private WebBrowser classificationBrowser;
        private WebBrowser schemaBrowser;
        private TabControl tabContainer;
        private TabPage classificationTab;
        private TabPage schemaTab;
        #endregion

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClassificationForm));
            this.classificationBrowser = new System.Windows.Forms.WebBrowser();
            this.schemaBrowser = new System.Windows.Forms.WebBrowser();
            this.tabContainer = new System.Windows.Forms.TabControl();
            this.classificationTab = new System.Windows.Forms.TabPage();
            this.schemaTab = new System.Windows.Forms.TabPage();
            this.tabContainer.SuspendLayout();
            this.classificationTab.SuspendLayout();
            this.schemaTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // classificationBrowser
            // 
            this.classificationBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.classificationBrowser.Location = new System.Drawing.Point(3, 3);
            this.classificationBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.classificationBrowser.Name = "classificationBrowser";
            this.classificationBrowser.Size = new System.Drawing.Size(1010, 681);
            this.classificationBrowser.TabIndex = 0;
            // 
            // schemaBrowser
            // 
            this.schemaBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.schemaBrowser.Location = new System.Drawing.Point(3, 3);
            this.schemaBrowser.MinimumSize = new System.Drawing.Size(1280, 720);
            this.schemaBrowser.Name = "schemaBrowser";
            this.schemaBrowser.Size = new System.Drawing.Size(1280, 720);
            this.schemaBrowser.TabIndex = 0;
            // 
            // tabContainer
            // 
            this.tabContainer.Controls.Add(this.classificationTab);
            this.tabContainer.Controls.Add(this.schemaTab);
            this.tabContainer.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(134)));
            this.tabContainer.Location = new System.Drawing.Point(0, 0);
            this.tabContainer.Name = "tabContainer";
            this.tabContainer.SelectedIndex = 0;
            this.tabContainer.Size = new System.Drawing.Size(1024, 720);
            this.tabContainer.TabIndex = 0;
            // 
            // classificationTab
            // 
            this.classificationTab.Controls.Add(this.classificationBrowser);
            this.classificationTab.Location = new System.Drawing.Point(4, 29);
            this.classificationTab.Name = "classificationTab";
            this.classificationTab.Padding = new System.Windows.Forms.Padding(3);
            this.classificationTab.Size = new System.Drawing.Size(1016, 687);
            this.classificationTab.TabIndex = 0;
            this.classificationTab.Text = "Classifiation Elements";
            this.classificationTab.UseVisualStyleBackColor = true;
            // 
            // schemaTab
            // 
            this.schemaTab.Controls.Add(this.schemaBrowser);
            this.schemaTab.Location = new System.Drawing.Point(4, 29);
            this.schemaTab.Name = "schemaTab";
            this.schemaTab.Padding = new System.Windows.Forms.Padding(3);
            this.schemaTab.Size = new System.Drawing.Size(1016, 687);
            this.schemaTab.TabIndex = 1;
            this.schemaTab.Text = "Classification Schema";
            this.schemaTab.UseVisualStyleBackColor = true;
            // 
            // ClassificationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 720);
            this.Controls.Add(this.tabContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ClassificationForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "NextLabs Classification Tool";
            this.tabContainer.ResumeLayout(false);
            this.classificationTab.ResumeLayout(false);
            this.schemaTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

