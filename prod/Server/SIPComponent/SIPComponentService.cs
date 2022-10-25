using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.NLLog;

namespace Nextlabs.SFBServerEnforcer.SIPComponent
{
    public partial class SIPComponentService : ServiceBase
    {
        protected static CLog theLog;

        public SIPComponentService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                SIPComponentMain.MainX(args);
            }
            catch(Exception)
            {
                // Set the ExitCode property to a non-zero value before stopping the service to indicate an error to the Service Control Manager.
                // The error code is base on testing.
                base.ExitCode =13816;
                Stop();
            }
        }

        protected override void OnStop()
        {
            try
            {
                SIPComponentMain.Exit();
            }
            catch (Exception ex)
            {
                CLog.OutputTraceLog("Exception on OnStop:{0}", ex.ToString());
            }
        }
    }
}
