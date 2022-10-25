using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceProcess;

// Other project
using SFBCommon.NLLog;
using SFBCommon.Common;

namespace NLLyncEndpointProxy
{
    #region Command info objects
    public class UserStartupCmdInfo
    {
        #region Const values
        public const string kstrCmdHeaderFlag = "-";
        public const string kstrCmdNLDebugFlag = "-NLDebug";
        public const string kstrCmdEndpointTypeFlag = "-EndpointType";
        public const string kstrCmdEndpointTypeAttrAgent = "Agent";
        public const string kstrCmdEndpointTypeAttrAssistant = "Assistant";

        private EMSFB_ENDPOINTTTYPE GetEndpointTypeFlag(string strUserEndpointTypeInput)
        {
            if (kstrCmdEndpointTypeAttrAgent.Equals(strUserEndpointTypeInput, StringComparison.OrdinalIgnoreCase))
            {
                return EMSFB_ENDPOINTTTYPE.emTypeAgent;
            }
            else if (kstrCmdEndpointTypeAttrAssistant.Equals(strUserEndpointTypeInput, StringComparison.OrdinalIgnoreCase))
            {
                return EMSFB_ENDPOINTTTYPE.emTypeAssistant;
            }
            return EMSFB_ENDPOINTTTYPE.emTypeUnknown;
        }
        #endregion

        #region Fields
        public EMSFB_ENDPOINTTTYPE EndpointType { get { return m_emEndpointType; } }
        public bool NLDebugFlag { get { return m_bNLDebugFlag; } }
        #endregion

        #region Members
        private EMSFB_ENDPOINTTTYPE m_emEndpointType = EMSFB_ENDPOINTTTYPE.emTypeUnknown;
        private bool m_bNLDebugFlag = false;
        #endregion

        #region Constructors
        public UserStartupCmdInfo(string[] szArgs)
        {
            if ((null != szArgs) && (0 < szArgs.Length))
            {
                for (int i = 0; i < szArgs.Length; ++i)
                {
                    if (kstrCmdEndpointTypeFlag.Equals(szArgs[i], StringComparison.OrdinalIgnoreCase))
                    {
                        string strEndpointType = CommonHelper.GetArrayValueByIndex(szArgs, (i+1), "");
                        if (!string.IsNullOrEmpty(strEndpointType))
                        {
                            if (!strEndpointType.StartsWith(kstrCmdHeaderFlag))
                            {
                                ++i;
                                m_emEndpointType = GetEndpointTypeFlag(strEndpointType);
                            }
                        }
                    }
                    else if (kstrCmdNLDebugFlag.Equals(szArgs[i], StringComparison.OrdinalIgnoreCase))
                    {
                        m_bNLDebugFlag = true;
                    }
                }
            }
        }
        #endregion
    }
    #endregion

    static class NLLyncEndpointProxyMain
    {
        #region Exception prompt string define
        private static readonly string kstrExpWrongStartupParameters = string.Format("You must specify the endpoint type with startup command [{0} {1}] or [{0} {2}]\n", UserStartupCmdInfo.kstrCmdEndpointTypeFlag, UserStartupCmdInfo.kstrCmdEndpointTypeAttrAgent, UserStartupCmdInfo.kstrCmdEndpointTypeAttrAssistant);
        private static readonly string kstrExpFailedEstablishEndpointObjs = "Start failed, maybe your config file info is wrong.";
        #endregion

        #region Static members
        static public NLLyncEndpointProxyObj s_obNLLyncEndpointProxyObj = null;
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] szArgs)
        {
            SFBBaseCommon.AssemblyLoadHelper.InitAssemblyLoaderHelper();

            if (IsStartWithDebugMode(szArgs))
            {
                // Debug
                WaitForStart();
                MainProxy(szArgs);
                if (null != s_obNLLyncEndpointProxyObj)
                {
                    ShowDebugForm(s_obNLLyncEndpointProxyObj.CurEndpointType.ToString());
                }
                WaitForStop();

                Exit();
            }
            else
            {
                ServiceBase[] szServicesToRun = new ServiceBase[] 
                {
                    new NLLyncEndpointProxyService(szArgs) 
                };
                ServiceBase.Run(szServicesToRun);
            }
        }

        #region Public functions
        static public void MainProxy(string[] szArgs)
        {
            // Analysis start up parameters
            UserStartupCmdInfo obUserStartupCmdInfo = new UserStartupCmdInfo(szArgs);
            if (EMSFB_ENDPOINTTTYPE.emTypeUnknown != obUserStartupCmdInfo.EndpointType)
            {
                // Init
                EMSFB_MODULE emCurrentModuleType = NLLyncEndpointProxyConfigInfo.GetModuleTypeByEndpointType(obUserStartupCmdInfo.EndpointType);
                SFBCommon.Startup.InitSFBCommon(emCurrentModuleType);

                bool bLoadSuccess = NLLyncEndpointProxyConfigInfo.s_endpointProxyConfigInfo.LoadConfigInfo(obUserStartupCmdInfo.EndpointType);
                if (bLoadSuccess)
                {
                    s_obNLLyncEndpointProxyObj = new NLLyncEndpointProxyObj(obUserStartupCmdInfo.EndpointType);
                    if (!s_obNLLyncEndpointProxyObj.IsSucceed())
                    {
                        throw new Exception(kstrExpFailedEstablishEndpointObjs);
                    }
                }
                else
                {
                    throw new Exception(NLLyncEndpointProxyConfigInfo.s_endpointProxyConfigInfo.GetLoadStatusInfo());
                }
            }
            else
            {
                throw new Exception(kstrExpWrongStartupParameters);
            }
        }
        static public void Exit()
        {
            if (null != s_obNLLyncEndpointProxyObj)
            {
                s_obNLLyncEndpointProxyObj.Exit();
            }

            // Uninit
            SFBCommon.Startup.UninitSFBCommon();
        }
        #endregion

        #region Debug
        static private bool WaitForStart()  // Return true, continue
        {
            Console.WriteLine("Debug running, you can enter q/Q to exit, others continue.\n");
            ConsoleKeyInfo obKeyInfo = Console.ReadKey();
            bool bRet = (ConsoleKey.Q != obKeyInfo.Key);
            if (bRet)
            {
                Console.WriteLine("\nRunning\n");
            }
            return bRet;
        }
        static private void WaitForStop()
        {
            Console.WriteLine("If you want to exit, you can entry q/Q to exit.\n");
            while (true)
            {
                ConsoleKeyInfo obKeyInfo = Console.ReadKey();
                if (ConsoleKey.Q == obKeyInfo.Key)
                {
                    break;
                }
            }
            Console.WriteLine("\nStart exit\n");
        }
        static private void ShowDebugForm(string strFormTitle) // obUserStartupCmdInfo.EndpointType.ToString()
        {
            // Establish user endpoint success, start UI and received NLLyncServerListener command
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            NLLyncEndpointProxyForm theNLLyncEndpointProxyForm = new NLLyncEndpointProxyForm(strFormTitle);
            Application.Run(theNLLyncEndpointProxyForm);
        }
        static private bool IsStartWithDebugMode(string[] szArgs)
        {
            return szArgs.Contains(UserStartupCmdInfo.kstrCmdNLDebugFlag);
        }
        #endregion
    }
}