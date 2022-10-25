using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QAToolSFBCommon.Common;

namespace NLLyncEndpointProxy
{
    class NLLyncEndpointProxyConfigInfo
    {
        #region Static sington
        static public NLLyncEndpointProxyConfigInfo s_endpointProxyConfigInfo = new NLLyncEndpointProxyConfigInfo();
        #endregion

        #region Wildcards
        public const string kstrConfigWildcardStartFlag = "\\";
        public const string kstrConfigWildcardEndFlag = ";";

        public const string kstrWildcardUserDisplayName = "USERDISPLAYNAME";
        public const string kstrWildcardClassifyUrl = "CLASSIFYURL";
        #endregion

        #region Config default info
        static private readonly Dictionary<EMSFB_ENDPOINTTTYPE, string> kdicEndpointTypeAndDefaultConversationSubject = new Dictionary<EMSFB_ENDPOINTTTYPE, string>()
        {
            {EMSFB_ENDPOINTTTYPE.emTypeUnknown, ""},
            {EMSFB_ENDPOINTTTYPE.emTypePerformanceTester, ""}
        };
        private const string kstrDefaultAgentAutoReply = "Hi \\USERDISPLAYNAME;, I am compliance messenger. Thank you!";
        private const string kstrDefaultAssitantAutoReply = "Hi \\USERDISPLAYNAME;, I am meeting classification assistant. If you want to classify your meeting, please click this link, \\CLASSIFYURL;. Thank you!";
        private const string kstrDefaultAssitantAutoSend = "Hi \\USERDISPLAYNAME;, please click this link, \\CLASSIFYURL;, to classify the meeting you just created. Thank you!";
        #endregion

        #region Const/Read only values
        private static readonly Dictionary<EMSFB_ENDPOINTTTYPE, EMSFB_MODULE> kdicEndpointTypeAndModuleType = new Dictionary<EMSFB_ENDPOINTTTYPE, EMSFB_MODULE>
        {
            {EMSFB_ENDPOINTTTYPE.emTypeUnknown, EMSFB_MODULE.emSFBModule_Unknown},
        };
        #endregion

        #region Static functions
        static public EMSFB_MODULE GetModuleTypeByEndpointType(EMSFB_ENDPOINTTTYPE emEndpointType)
        {
            return CommonHelper.GetValueByKeyFromDir(kdicEndpointTypeAndModuleType, emEndpointType, EMSFB_MODULE.emSFBModule_Unknown);
        }
        #endregion

        #region Fields
        public STUSFB_PROMPTMSG PromptMsg { get { return m_stuPromptMsg; } }
        public STUSFB_ENDPOINTPROXYACCOUNT EndpointProxyAccount { get { return m_stuEndpointProxyAccount; } }
        public STUSFB_ENDPOINTPROXYTCPINFO EndpointProxyTcpIp { get { return m_stuEndpointProxyTcpIp; } }
        
        public string ConversationUserAgent { get { return m_strConversationSubject; } }
        public string AgentAutoReply { get { return m_strAgentAutoReply; } }
        public string AssitantAutoReply { get { return m_strAssitantAutoReply; } }
        public string AssitantAutoSend { get { return m_strAssitantAutoSend; } }
        public STUSFB_ERRORMSG ClassifyAssistantUnknownError { get { return m_stuClassifyAssitantUnknonwnError; } }
        public string ClassifyAssitantWebServiceAddr { get { return m_strClassifyAssitantWebServiceAddr; } }
        public int MinMessageInterval
        {
            get
            {
                if (0 >= m_nMinMessageInterval)
                {
                    const int knDefaultMinMessageInterval = 100;
                    string strMinMessageInterval = GetRuntimeConfigInfoByKeyFlag(ConfigureFileManager.kstrXMLIMFrequentMessageIntervalFlag, knDefaultMinMessageInterval.ToString());
                    if (!int.TryParse(strMinMessageInterval, out m_nMinMessageInterval))
                    {
                        m_nMinMessageInterval = knDefaultMinMessageInterval;
                    }
                }
                return m_nMinMessageInterval;
            }
        }
        #endregion

        #region Members
        private ConfigureFileManager m_obCfgFileMgr = null; 
        private STUSFB_PROMPTMSG m_stuPromptMsg = null;
        private STUSFB_ENDPOINTPROXYACCOUNT m_stuEndpointProxyAccount = null;
        private STUSFB_ENDPOINTPROXYTCPINFO m_stuEndpointProxyTcpIp = null;

        private int m_nMinMessageInterval = -1;
        private string m_strConversationSubject = "";
        private string m_strClassifyAssitantWebServiceAddr = "";
        private string m_strAgentAutoReply = kstrDefaultAgentAutoReply;
        private string m_strAssitantAutoReply = kstrDefaultAssitantAutoReply;
        private string m_strAssitantAutoSend = kstrDefaultAssitantAutoSend;
        private STUSFB_ERRORMSG m_stuClassifyAssitantUnknonwnError = new STUSFB_ERRORMSG("Unknown error, you can ask your IT Admin for help.", 0);
        #endregion

        #region Constructor
        protected NLLyncEndpointProxyConfigInfo() { }
        #endregion

        #region Tools
        public bool LoadConfigInfo(EMSFB_ENDPOINTTTYPE emEndpointType)
        {
            if (EMSFB_ENDPOINTTTYPE.emTypeUnknown != emEndpointType)
            {
                EMSFB_MODULE emCurrentModuleType = GetModuleTypeByEndpointType(emEndpointType);
                m_obCfgFileMgr = new ConfigureFileManager(emCurrentModuleType);
                if (m_obCfgFileMgr.IsLoadSuccess())
                {
                    m_stuPromptMsg = m_obCfgFileMgr.GetPromptMsg();
                    if (null != m_stuPromptMsg)
                    {
                        string strConversationFlag = ConfigureFileManager.GetEndpointConversationFlag(emEndpointType);
                        string strDefaultConversationSubject = CommonHelper.GetValueByKeyFromDir(kdicEndpointTypeAndDefaultConversationSubject, emEndpointType, "");
                        m_strConversationSubject = GetRuntimeConfigInfoByKeyFlag(strConversationFlag, strDefaultConversationSubject);
                        m_strClassifyAssitantWebServiceAddr = GetRuntimeConfigInfoByKeyFlag(ConfigureFileManager.kstrXMLClassifyAssistantServiceAddrFlag, kstrDefaultAgentAutoReply);

                        m_strAgentAutoReply = GetRuntimeConfigInfoByKeyFlag(ConfigureFileManager.kstrXMLAgentAuotReplyFlag, kstrDefaultAgentAutoReply);
                        m_strAssitantAutoReply = GetRuntimeConfigInfoByKeyFlag(ConfigureFileManager.kstrXMLAssitantAutoReplyFlag, kstrDefaultAssitantAutoReply);
                        m_strAssitantAutoSend = GetRuntimeConfigInfoByKeyFlag(ConfigureFileManager.kstrXMLAssitantAutoSendFlag, kstrDefaultAssitantAutoSend);

                        m_stuClassifyAssitantUnknonwnError = GetErrorMsgConfigInfoByKeyFlag(ConfigureFileManager.kstrXMLAssitantUnknonwnErrorFlag, new STUSFB_ERRORMSG("Unknown error, you can ask your IT Admin for help.", 0));
                    }

                    // For endpoint proxy must be contains proxy account and proxy TCP IP info
                    Dictionary<EMSFB_ENDPOINTTTYPE, STUSFB_ENDPOINTPROXYACCOUNT> dicEndpointProxyAccount = m_obCfgFileMgr.GetEndpointProxyAccount();
                    Dictionary<EMSFB_ENDPOINTTTYPE, STUSFB_ENDPOINTPROXYTCPINFO> dicEndpointProxyTcpIp = m_obCfgFileMgr.GetEndpointProxyTcpInfo();
                    if ((null != dicEndpointProxyAccount) && (null != dicEndpointProxyTcpIp))
                    {
                        // Check proxy account and TCP IP info
                        m_stuEndpointProxyAccount = CommonHelper.GetValueByKeyFromDir(dicEndpointProxyAccount, emEndpointType, null);
                        m_stuEndpointProxyTcpIp = CommonHelper.GetValueByKeyFromDir(dicEndpointProxyTcpIp, emEndpointType, null);
                        if ((null != m_stuEndpointProxyAccount) && (null != m_stuEndpointProxyTcpIp))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public string GetLoadStatusInfo()
        {
            if (null != m_obCfgFileMgr)
            {
                return m_obCfgFileMgr.GetLoadStatusInfo();
            }
            return "Load config info failed";
        }
        public string GetRuntimeConfigInfoByKeyFlag(string strKeyFlag, string strDefaultValue)
        {
            if (null != m_stuPromptMsg)
            {
                return CommonHelper.GetValueByKeyFromDir(m_stuPromptMsg.m_dirRuntimeInfo, strKeyFlag, strDefaultValue);
            }
            return strDefaultValue;
        }
        public STUSFB_ERRORMSG GetErrorMsgConfigInfoByKeyFlag(string strKeyFlag, STUSFB_ERRORMSG stuDefaultErrorMsg)
        {
            if (null != m_stuPromptMsg)
            {
                return CommonHelper.GetValueByKeyFromDir(m_stuPromptMsg.m_dirErrorMsg, strKeyFlag, null);
            }
            return stuDefaultErrorMsg;
        }
        #endregion
    }
}
