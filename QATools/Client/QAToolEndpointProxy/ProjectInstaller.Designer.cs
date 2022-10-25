namespace NLLyncEndpointProxy
{
    partial class ProjectInstaller
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.NLLyncEndpointProxyServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.NLLyncEndpointProxyServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // NLLyncEndpointProxyServiceProcessInstaller
            // 
            this.NLLyncEndpointProxyServiceProcessInstaller.Password = null;
            this.NLLyncEndpointProxyServiceProcessInstaller.Username = null;
            this.NLLyncEndpointProxyServiceProcessInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceProcessInstaller1_AfterInstall);
            // 
            // NLLyncEndpointProxyServiceInstaller
            // 
            this.NLLyncEndpointProxyServiceInstaller.Description = "NextLabs IM Agent";
            this.NLLyncEndpointProxyServiceInstaller.DisplayName = "NextLabs IM Agent";
            this.NLLyncEndpointProxyServiceInstaller.ServiceName = "NLIMAgent";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.NLLyncEndpointProxyServiceProcessInstaller,
            this.NLLyncEndpointProxyServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller NLLyncEndpointProxyServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller NLLyncEndpointProxyServiceInstaller;
    }
}