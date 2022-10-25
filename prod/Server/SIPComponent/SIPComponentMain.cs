using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SFBCommon.NLLog;
using SFBCommon.Common;
using System.ServiceProcess;

using Nextlabs.SFBServerEnforcer.SIPComponent.Common;
using Nextlabs.SFBServerEnforcer.SIPComponent.DataManagement;

namespace Nextlabs.SFBServerEnforcer.SIPComponent
{
    class SIPComponentMain
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(SIPComponentMain));
        #endregion

        #region Static public members
        static public readonly string m_strPerformaceLogFolder = SFBCommon.Common.CommonHelper.kstrSFBInstallPath + "Logs\\JoinMeetingLog\\";
        #endregion

        #region Static private members
        static private readonly string m_strManifestFile = SFBCommon.Common.ConfigureFileManager.GetCfgFilePath(SFBCommon.Common.EMSFB_CFGTYPE.emCfgSipMSPL, EMSFB_MODULE.emSFBModule_SIPComponent);
        static private CSessionManager m_sessionManager = new CSessionManager();

        static protected Thread m_threadConnectServer = null;
        static protected Object m_lockThreadConnectServer = new object();
        static protected bool m_bServiceNormalStop = false;
        #endregion

        static SIPComponentMain()
        {
            SFBBaseCommon.AssemblyLoadHelper.InitAssemblyLoaderHelper();
            SFBCommon.Startup.InitSFBCommon(EMSFB_MODULE.emSFBModule_SIPComponent);
        }

        public static void MainX(string[] args)
        {
            // only one instance, for windows service program, when it restart the old Mutex do not released
//             bool createdNew = false;
//             m_Mutex = new Mutex(true, "Global\\Nextlabs.SFBServerEnforcer.SIPComponent", out createdNew);
//             if (!createdNew)
//             {
//                 //already running
//                 CLog.OutputTraceLog("the Application is already running.");
//                 return;
//             }

            // Init
            m_bServiceNormalStop = false;
            CommonCfgMgr.GetInstance(); // Init Common config manager

            //set threadpool
            if(SIPComponentConfig.knMinThreadPoolWorkerThreads>0)
            {
                int nOldCompletePortThread = 0;
                int nOldWorkerThread = 0;
                ThreadPool.GetMinThreads(out nOldWorkerThread, out nOldCompletePortThread);
                ThreadPool.SetMinThreads(SIPComponentConfig.knMinThreadPoolWorkerThreads, nOldCompletePortThread);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "ThreadPool.SetMinThreads, WorkerThread Count={0}", SIPComponentConfig.knMinThreadPoolWorkerThreads);
            }

            //get endpointproxy info
            ContactToEndpointProxy.GetAgentProxyContact().InitEndpointProxyInfo();  // Asynchronous initialize
            ContactToEndpointProxy.GetAssistantProxyContact().InitEndpointProxyInfo(); 

            //Connect to server
            m_sessionManager.DisconnectListeners += new CSessionManager.DisconnectEventListeners(OnServerDisconnect);//listen disconnect event
            StartConnectToServer();

            //start data cache maintain
            CDataCacheMantain.GetInstance().Start();
    
        }

        public static void Exit()
        {
            m_bServiceNormalStop = true;
            try
            {//if ServerAgent.dll can't be load, the following call will throw exception. cause Service can't been stop.
                m_sessionManager.Disconnect();
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Exception on call SessionManager::Disconnect.{0}", ex.ToString());
            }

            ContactToEndpointProxy.GetAgentProxyContact().Stop();
            ContactToEndpointProxy.GetAssistantProxyContact().Stop();
            CDataCacheMantain.GetInstance().Stop();
            SFBCommon.Startup.UninitSFBCommon();

            //if we wait for SFB Service's status, we need to cancel it, otherwise our service can't be stop success.
            lock (m_lockThreadConnectServer)
            {
                if (m_threadConnectServer != null)
                {
                    m_threadConnectServer.Abort();
                    m_threadConnectServer.Join(5 * 1000);
                    m_threadConnectServer = null;
                }
            }
        }

        #region Inner tools
        //Note: this event is triggered when our application disconnected with SFB Service. 
        //1. when we stop our service normal, this event will also triggered.
        //2. this event will triggered more than once during one disconnection.
        private static void OnServerDisconnect(string strReason)
        {
           if(!m_bServiceNormalStop)//stop our service also trigger this event, we need to filter it.
           {
               theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Disconnect from Server, will reconnect. reason:{0}.", strReason);
               StartConnectToServer();
           }
        }
        private static void StartConnectToServer()
        {
            lock(m_lockThreadConnectServer)
            {
                if(m_threadConnectServer==null)
                {
                    m_threadConnectServer = new Thread(SIPComponentMain.ConnectToServer);
                    m_threadConnectServer.Start();
                }
            }
        }
        private static void ConnectToServer()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "Enter ConnectServer.");
            const string strSFBFrontEndSerivceName = "RtcSrv";
            ServiceController sc = new ServiceController(strSFBFrontEndSerivceName);

            while(true)
            {
                //if the status of SFB is not "Running", wait untill the status becomes "Running".
                try
                {
                    sc.Refresh();
                    if (sc.Status != ServiceControllerStatus.Running)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "Wait SFB Service becomes Running...");
                        sc.WaitForStatus(ServiceControllerStatus.Running);
      
                    }
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "SFB Service is Running, Begin Connect...");
                }
                catch(InvalidOperationException ex)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "SFB Service doesn't exist. Please check this machine have Skype for business Front End Service role installed. {0}", ex.ToString() );
                    break; //exit for SFB Service didn't exist.
                }
                catch(ThreadAbortException exAbort)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Thread Abort. {0}", exAbort.ToString());
                    break; //abort
                }
                catch(Exception ex)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Exception on check service status. {0}", ex.ToString());
                    //break; normal exception, didn't exit.
                }
        

                //Connect to server
                try
                {
                    m_sessionManager.ConnectToServer(m_strManifestFile, "SfbEnforcer");
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Connect to server success.");
                    break;
                }
                catch (Exception ex)
                {
                    ///we are unable to connect, print the exception in our UI, restore ///the button state
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Excepion on ConnectToServer, will try again:{0}", ex.Message);
                }

                Thread.Sleep(10 * 1000);
            }

            lock (m_lockThreadConnectServer)
            {
                m_threadConnectServer = null;
            }
        }
        #endregion
    }
}
