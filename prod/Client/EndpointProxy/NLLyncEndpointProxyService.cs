using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

// Other projects
using SFBCommon.NLLog;
using SFBCommon.Common;

namespace NLLyncEndpointProxy
{
    public partial class NLLyncEndpointProxyService : ServiceBase
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(NLLyncEndpointProxyService));
        #endregion

        #region Static members
        static private AutoResetEvent s_eventStop = new AutoResetEvent(false);
        #endregion

        #region Static function for maintian service
        static public void ServiceMaintainThread()
        {
            bool bNeedStop = false;
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Service maintain thread start\n");
            try
            {
                int knSFBClientServiceRestartTime = NLLyncEndpointProxyConfigInfo.s_endpointProxyConfigInfo.SFBClientServiceRestartTime;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "SFBClientServiceRestartTime is:[{0}]\n", knSFBClientServiceRestartTime);
                while (true)
                {
                    const int knCheckFrequency = 1 * 60 * 60 * 1000;
                    bool bWaitResult = s_eventStop.WaitOne(knCheckFrequency); // one hour
                    if (bWaitResult)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Received event to exit service maintain thread. User stop the service, break and exit\n");
                        break;
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Checking if need exit current environment for auto restart\n");
                        DateTime dateTime = DateTime.Now;
                        if (knSFBClientServiceRestartTime == dateTime.Hour)
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Restart time is ready, stop service and wait for auto restart\n");
                            bNeedStop = true;
                            NLLyncEndpointProxyMain.Exit();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in service maintain thread, [{0}]\n", ex.Message);
            }
            if (bNeedStop)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Environment exit\n");
                Environment.Exit(1);
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Service maintain thread exit\n");
        }
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
                ThreadHelper.AsynchronousInvokeHelper(true, ServiceMaintainThread);
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
                s_eventStop.Set();
                NLLyncEndpointProxyMain.Exit();
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception happened in EndpointProxy: OnStop, {0}\n", ex.Message);
            }
        }
    }
}
