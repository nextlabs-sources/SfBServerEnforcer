using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

// Other projects
using QAToolSFBCommon.NLLog;

namespace NLLyncEndpointProxy
{
    public partial class NLLyncEndpointProxyService : ServiceBase
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(NLLyncEndpointProxyService));
        #endregion

        #region member
        string[] m_szArgs = null;
        #endregion

        public NLLyncEndpointProxyService(string[] szArgs)
        {
            m_szArgs = szArgs;
            InitializeComponent();
        }

        protected override void OnStart(string[] szArgs)
        {
            try
            {
                NLLyncEndpointProxyMain.MainProxy(m_szArgs);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception happened in EndpointProxy: OnStart, {0}\n", ex.Message);
                base.ExitCode = 13816; //Set the ExitCode property to a non-zero value before stopping the service to indicate an error to the Service Control Manager.
                Stop();
            }
        }

        protected override void OnStop()
        {
            try
            {
                NLLyncEndpointProxyMain.Exit();
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception happened in EndpointProxy: OnStop, {0}\n", ex.Message);
            }
        }
    }
}
