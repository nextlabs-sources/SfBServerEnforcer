using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

using QAToolSFBCommon.Database;

namespace QAToolSFBCommon.Common
{
    #region CFGTYPE
    public enum EMSFB_CFGTYPE
    {
        emCfgLog,
        emCfgCommon,
    }
    public enum EMSFB_MODULE
    {
        emSFBModule_Unknown,

    }
    public enum EMSFB_ENDPOINTTTYPE
    {
        emTypeUnknown,

        emTypePerformanceTester,
    }
    #endregion

    #region CFGFILENAME
    public class STUSFB_CFGFILENAME
    {
        private string m_strParentFolderName = "";
        private string m_strPrefixName = "";
        private string m_strSpecialName = "";
        private string m_strSuffixName = "";
        private string m_strExtension = "";
        private string m_strNameSep = "";

        public STUSFB_CFGFILENAME(string strPrefixName, string strSpecialName, string strSuffixName, string strExtension, string strParentFolderName, string strNameSep)
        {
            m_strPrefixName = strPrefixName;
            m_strSpecialName = strSpecialName;
            m_strSuffixName = strSuffixName;
            m_strExtension = strExtension;
            m_strParentFolderName = strParentFolderName;
            m_strNameSep = strNameSep;
        }
        public STUSFB_CFGFILENAME(STUSFB_CFGFILENAME stuCfgFileName)
        {
            m_strPrefixName = stuCfgFileName.m_strPrefixName;
            m_strSpecialName = stuCfgFileName.m_strSpecialName;
            m_strSuffixName = stuCfgFileName.m_strSuffixName;
            m_strExtension = stuCfgFileName.m_strExtension;
            m_strParentFolderName = stuCfgFileName.m_strParentFolderName;
            m_strNameSep = stuCfgFileName.m_strNameSep;
        }

        public void SetSpecialName(string strSpecialName)
        {
            m_strSpecialName = strSpecialName;
        }
        public string GetEffectiveFilePath(string strRootFolderPath, bool bCombineParentFolder) // return an exist file path, otherwise return "";
        {
            // Make a standard root folder path
            strRootFolderPath = CommonHelper.GetStandardFolderPath(strRootFolderPath);

            string strCfgFile = strRootFolderPath + InnerGetFileFullName(bCombineParentFolder);
            QAToolSFBCommon.NLLog.CLog.OutputTraceLog("FilePath:[{0}]\n", strCfgFile);

            return (File.Exists(strCfgFile)) ? strCfgFile : "" ;
        }
        private string InnerGetFileFullName(bool bCombineParentFolder)
        {
            string strFileFullName = ConnectStringWithSeprator(m_strPrefixName, m_strSpecialName, m_strNameSep);
            strFileFullName = ConnectStringWithSeprator(strFileFullName, m_strSuffixName, m_strNameSep);
            if (bCombineParentFolder)
            {
                strFileFullName = m_strParentFolderName + "\\" + strFileFullName;
            }
            strFileFullName += m_strExtension;
            return strFileFullName;
        }
        private string ConnectStringWithSeprator(string strPart1, string strPart2, string strSep)
        {
            if ((!string.IsNullOrEmpty(strPart1)) && (!string.IsNullOrEmpty(strPart2)))
            {
                return (strPart1 + strSep + strPart2);
            }
            else
            {
                return (strPart1 + strPart2);
            }
        }
    }
    #endregion

    #region Configure information structures
    public class STUSFB_DBACCOUNT
    {
        #region Values
        private const string kstrDBTypeMYSQL = "MYSQL";
        private const string kstrDBTypeMSSQL = "MSSQL";
        #endregion

        #region Members
        public string m_strAddr = "";
        public uint m_unPort = 0;
        public string m_strCatalog = "";
        public string m_strUserName = "";
        public string m_strPassword = "";
        public EMSFB_DBTYPE m_emDBType = EMSFB_DBTYPE.emDBTypeUnknown;
        #endregion

        #region Construnctors
        public STUSFB_DBACCOUNT() { }
        public STUSFB_DBACCOUNT(XmlNode obXMLDBAccount)
        {
            try
            {
                // eg: <Database type="MYSQL" addr="10.23.60.242" port="3306" catalog="sfb" username="kim" password="123blue!"></Database>
                if (null != obXMLDBAccount)
                {
                    m_strAddr = XMLTools.GetAttributeValue(obXMLDBAccount.Attributes, ConfigureFileManager.kstrXMLAddrAttr);
                    m_unPort = UInt32.Parse(XMLTools.GetAttributeValue(obXMLDBAccount.Attributes, ConfigureFileManager.kstrXMLPortAttr));
                    m_strCatalog = XMLTools.GetAttributeValue(obXMLDBAccount.Attributes, ConfigureFileManager.kstrXMLCatalogAttr);
                    m_strUserName = XMLTools.GetAttributeValue(obXMLDBAccount.Attributes, ConfigureFileManager.kstrXMLUserNameAttr);
                    m_strPassword = XMLTools.GetAttributeValue(obXMLDBAccount.Attributes, ConfigureFileManager.kstrXMLPasswordAttr);
                    string strDBType = XMLTools.GetAttributeValue(obXMLDBAccount.Attributes, ConfigureFileManager.kstrXMLTypeAttr);  // "MYSQL"| "MSSQL"
                    if (strDBType.Equals(kstrDBTypeMYSQL, StringComparison.OrdinalIgnoreCase))
                    {
                        m_emDBType = EMSFB_DBTYPE.emDBTypeMYSQL;
                    }
                    else if (strDBType.Equals(kstrDBTypeMSSQL, StringComparison.OrdinalIgnoreCase))
                    {
                        m_emDBType = EMSFB_DBTYPE.emDBTypeMSSQL;
                    }
                    else
                    {
                        m_emDBType = EMSFB_DBTYPE.emDBTypeUnknown;
                    }
                }
            }
            catch (Exception ex)
            {
                QAToolSFBCommon.NLLog.CLog.OutputTraceLog("Exception happened in STUSFB_DBACCOUNT constructor,[{0}]\n", ex.Message);
            }
        }
        #endregion
    }
    public class STUSFB_ENDPOINTPROXYACCOUNT
    {
        #region Logger
        static private QAToolSFBCommon.NLLog.CLog theLog = QAToolSFBCommon.NLLog.CLog.GetLogger(typeof(STUSFB_ENDPOINTPROXYACCOUNT));
        #endregion

        #region Const/read only values
        public const string kstrAccountTypePerformanceTester = "PerformanceTester";
        private static readonly Dictionary<EMSFB_ENDPOINTTTYPE, string> kdicEmStrAccountTypeMapping = new Dictionary<EMSFB_ENDPOINTTTYPE,string>()
        {
            {EMSFB_ENDPOINTTTYPE.emTypeUnknown, ""},
            {EMSFB_ENDPOINTTTYPE.emTypePerformanceTester, kstrAccountTypePerformanceTester},
        };
        private static readonly Dictionary<string, EMSFB_ENDPOINTTTYPE> kdicStrEmAccountTypeMapping = new Dictionary<string, EMSFB_ENDPOINTTTYPE>()
        {
            {"", EMSFB_ENDPOINTTTYPE.emTypeUnknown},
            {kstrAccountTypePerformanceTester, EMSFB_ENDPOINTTTYPE.emTypePerformanceTester}
        };
        private static readonly Dictionary<EMSFB_ENDPOINTTTYPE, string> kdicTypeAndUserAgentMapping = new Dictionary<EMSFB_ENDPOINTTTYPE,string>()
        {
            {EMSFB_ENDPOINTTTYPE.emTypeUnknown, ""},
            {EMSFB_ENDPOINTTTYPE.emTypePerformanceTester, ""},
        };
        #endregion

        #region Static functions
        public static EMSFB_ENDPOINTTTYPE GetEndpointEnumTypeByString(string strEndpointType) 
        { 
            return CommonHelper.GetValueByKeyFromDir(STUSFB_ENDPOINTPROXYACCOUNT.kdicStrEmAccountTypeMapping, strEndpointType, EMSFB_ENDPOINTTTYPE.emTypeUnknown); 
        }
        public static string GetEndpointStringTypeByEnum(EMSFB_ENDPOINTTTYPE emEndpointType)
        {
            return CommonHelper.GetValueByKeyFromDir(STUSFB_ENDPOINTPROXYACCOUNT.kdicEmStrAccountTypeMapping, emEndpointType, "");
        }
        private static string GetEndpointUserAgentByEnum(EMSFB_ENDPOINTTTYPE emEndpointType)
        {
            return CommonHelper.GetValueByKeyFromDir(STUSFB_ENDPOINTPROXYACCOUNT.kdicTypeAndUserAgentMapping, emEndpointType, "");
        }
        #endregion

        #region Members
        public string m_strServerFQDN = "";
        public string m_strUserDomain = "";
        public string m_strUserName = "";
        public string m_strPassword = "";
        public string m_strUserSipURI = "";
        public string m_strUserAgent = "";
        #endregion

        #region Construnctors
        public STUSFB_ENDPOINTPROXYACCOUNT(EMSFB_ENDPOINTTTYPE emType, string strServerFQDN, string strUserDomain, string strUserName, string strPassword, string strUserSipURI)
        {
            m_strServerFQDN = strServerFQDN;
            m_strUserDomain = strUserDomain;
            m_strUserName = strUserName;
            m_strPassword = strPassword;
            m_strUserSipURI = strUserSipURI;
            m_strUserAgent = GetEndpointUserAgentByEnum(emType);
        }
        public STUSFB_ENDPOINTPROXYACCOUNT(EMSFB_ENDPOINTTTYPE emType, XmlNode obXMLLyncClientAccount)
        {
            try
            {
                // eg: <NLLyncEndpointProxy serverfqdn="lync-server.lync.nextlabs.solutions" username="EndpointProxy.NLLync" userdomain="lync.nextlabs.solutions" password="123blue!" useruri="EndpointProxy.NLLync@lync.nextlabs.solutions"></NLLyncEndpointProxy>
                if (null != obXMLLyncClientAccount)
                {
                    m_strServerFQDN = XMLTools.GetAttributeValue(obXMLLyncClientAccount.Attributes, ConfigureFileManager.kstrXMLServerFQDNAttr);
                    m_strUserDomain = XMLTools.GetAttributeValue(obXMLLyncClientAccount.Attributes, ConfigureFileManager.kstrXMLUserDomainAttr);
                    m_strUserName = XMLTools.GetAttributeValue(obXMLLyncClientAccount.Attributes, ConfigureFileManager.kstrXMLUserNameAttr);
                    m_strPassword = XMLTools.GetAttributeValue(obXMLLyncClientAccount.Attributes, ConfigureFileManager.kstrXMLPasswordAttr);
                    m_strUserSipURI = XMLTools.GetAttributeValue(obXMLLyncClientAccount.Attributes, ConfigureFileManager.kstrXMLUserURIAttr);
                    m_strUserAgent = GetEndpointUserAgentByEnum(emType);
                }
            }
            catch (Exception ex)
            {
                QAToolSFBCommon.NLLog.CLog.OutputTraceLog("Exception happened in STUSFB_DBACCOUNT constructor,[{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Tools
        public void OutputInfo(bool bOutputPassword = false)
        {
            theLog.OutputLog(QAToolSFBCommon.NLLog.EMSFB_LOGLEVEL.emLogLevelDebug, "Server FQDNQ: {0}\nUser Sip Uri: {1}\nUser Domain: {2}\nUser Name: {3}\nPassword: {4}\nstrUserAgent: {5}\n", 
                m_strServerFQDN, m_strUserSipURI, m_strUserDomain, m_strUserName, (bOutputPassword?m_strPassword:"******"), m_strUserAgent);
        }
        #endregion
    }
    public class STUSFB_ENDPOINTPROXYTCPINFO
    {
        #region Logger
        static private QAToolSFBCommon.NLLog.CLog theLog = QAToolSFBCommon.NLLog.CLog.GetLogger(typeof(STUSFB_ENDPOINTPROXYACCOUNT));
        #endregion

        #region Members
        public EMSFB_ENDPOINTTTYPE m_emEndpointType = EMSFB_ENDPOINTTTYPE.emTypeUnknown;
        public string m_strAddr = "";
        public uint m_unPort = 0;
        #endregion

        #region Construnctors
        public STUSFB_ENDPOINTPROXYTCPINFO() { }
        public STUSFB_ENDPOINTPROXYTCPINFO(EMSFB_ENDPOINTTTYPE emEndpointType, XmlNode obXMLLyncClientTcpInfo)
        {
            try
            {
                // eg: <NLLyncEndpointProxy addr="10.23.60.205" port="8001"/>
                if (null != obXMLLyncClientTcpInfo)
                {
                    m_strAddr = XMLTools.GetAttributeValue(obXMLLyncClientTcpInfo.Attributes, ConfigureFileManager.kstrXMLAddrAttr);
                    m_unPort = UInt32.Parse(XMLTools.GetAttributeValue(obXMLLyncClientTcpInfo.Attributes, ConfigureFileManager.kstrXMLPortAttr));
                    m_emEndpointType = emEndpointType;
                }
            }
            catch (Exception ex)
            {
                QAToolSFBCommon.NLLog.CLog.OutputTraceLog("Exception happened in STUSFB_DBACCOUNT constructor,[{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Tools
        public void OutputInfo()
        {
            theLog.OutputLog(QAToolSFBCommon.NLLog.EMSFB_LOGLEVEL.emLogLevelDebug, "Addr: {0}\nPort: {1}\nEndpoint type: {2}\n",
                                                                m_strAddr, m_unPort, m_emEndpointType);
        }
        #endregion
    }
    public class STUSFB_ERRORMSG
    {
        #region Members
        public int m_nErrorCode = 0;
        public string m_strErrorMsg = "";
        #endregion

        #region Constructors
        public STUSFB_ERRORMSG(string strErrorMsg, int nErrorCode)
        {
            m_strErrorMsg = strErrorMsg;
            m_nErrorCode = nErrorCode;
        }
        public STUSFB_ERRORMSG(STUSFB_ERRORMSG stuErrorMsg)
        {
            m_strErrorMsg = stuErrorMsg.m_strErrorMsg;
            m_nErrorCode = stuErrorMsg.m_nErrorCode;
        }
        #endregion
    }
    public class STUSFB_PROMPTMSG
    {
        #region Values
        static public Dictionary<EMSFB_MODULE, string> s_dirCfgInfoType = new Dictionary<EMSFB_MODULE,string>()
        {
            {EMSFB_MODULE.emSFBModule_Unknown, ""}
        };
        #endregion

        #region Members
        public Dictionary<string, string> m_dirRuntimeInfo = new Dictionary<string,string>();
        public Dictionary<string, STUSFB_ERRORMSG> m_dirErrorMsg = new Dictionary<string,STUSFB_ERRORMSG>();
        #endregion

        #region Constructors
        public STUSFB_PROMPTMSG() { }
        public STUSFB_PROMPTMSG(XmlNode obXMLPromptMsg, EMSFB_MODULE emModuleType)
        {
            try
            {
                InitPromptMsg(obXMLPromptMsg, emModuleType);
            }
            catch (Exception ex)
            {
                QAToolSFBCommon.NLLog.CLog.OutputTraceLog("Exception happened in STUSFB_DBACCOUNT constructor,[{0}]\n", ex.Message);
            }
        }
        #endregion
        
        #region private tools
        private void InitPromptMsg(XmlNode obXMLPromptMsg, EMSFB_MODULE emModuleType)
        {
            if (null != obXMLPromptMsg)
            {
                string strCurModuleInfoFlag = CommonHelper.GetValueByKeyFromDir(STUSFB_PROMPTMSG.s_dirCfgInfoType, emModuleType, "");
                if (!string.IsNullOrEmpty(strCurModuleInfoFlag))
                {
                    XmlNode obXMLSubPromptMsg = obXMLPromptMsg.SelectSingleNode(s_dirCfgInfoType[emModuleType]);
                    if (null != obXMLSubPromptMsg)
                    {
                        m_dirRuntimeInfo = XMLTools.GetAllSubNodesInfo(obXMLSubPromptMsg.SelectSingleNode(ConfigureFileManager.kstrXMLRuntimeInfoFlag));
                        m_dirErrorMsg = XMLTools.GetAllErrorMsgFromSubNodes(obXMLSubPromptMsg.SelectSingleNode(ConfigureFileManager.kstrXMLErrorMessageFlag));
                    }
                }
            }
        }
        #endregion
    }
    #endregion

    public class ConfigureFileManager
    {
        #region configure file node define
        private const string kstrXMLConfigureFlag = "Configure";
        private const string kstrXMLAccountFlag = "Account";
        private const string kstrXMLPromptMsgFlag = "PromptMessage";
        private const string kstrXMLTCPConmunicationFlag = "TCPConmunication";

        private const string kstrXMLDatabaseFlag = "Database";
        public const string kstrXMLNLLyncEndpointProxyFlag = "NLLyncEndpointProxy";
        public const string kstrXMLSIPComponentFlag = "SIPComponent";
        public const string kstrXMLSFBControlPanelFlag = "SFBControlPanel";
        public const string kstrXMLHTTPComponentFlag = "HTTPComponent";
        public const string kstrXMLMaintainToolFlag = "MaintainTool";
        public const string kstrXMLNLAssistantWebServiceFlag = "NLAssistantWebService";

        public const string kstrXMLLyncClientAccountFlag = "LyncClient";
        public const string kstrXMLRuntimeInfoFlag = "RuntimeInfo";
        public const string kstrXMLErrorMessageFlag = "ErrorMessage";
        public const string KstrXMLMsgDenyInviteBeforeManualClassifyDone = "MsgDenyInviteBeforeManualClassifyDone";
        public const string KstrXMLMsgDenyJoinBeforeManualClassifyDone = "MsgDenyJoinBeforeManualClassifyDone";

        public const string kstrXMLTextSetEnforcerFlag = "TextSetEnforcer";
        public const string kstrXMLTextEnforceStatusYesFlag = "TextEnforceStatusYes";
        public const string kstrXMLTextEnforceStatusNoFlag = "TextEnforceStatusNo";
        public const string kstrXMLDenyInviteFlag = "DenyInvite";
        public const string kstrXMLReadPersistentValueErrorFlag = "ReadPersistentValueError";
        public const string kstrXMLPersistentSaveErrorFlag = "PersistentSaveError";
        public const string kstrXMLNoPermissionFlag = "NoPermission";
        public const string kstrXMLUnknownErrorFlag = "UnknownError";
        public const string kstrXMLStartLoadingFlag = "StartLoading";
        public const string kstrXMLEndLoadingFlag = "EndLoading";
        public const string kstrXMLUserAgentFlag = "UserAgent";
        public const string kstrXMLAgentConversationSubjectFlag = "AgentConversationSubject";
        public const string kstrXMLAssistantConversationSubjectFlag = "AssistantConversationSubject";
        public const string kstrXMLPerformanceTesterConversationSubjectFlag = "PerformanceTesterConversationSubject";
        public const string kstrXMLForceEnforcerExplainFlag = "ForceEnforcerExplain";
        public const string kstrXMLEnforcerExplainFlag = "EnforcerExplain";
        public const string KstrXMLClassficationAreaTitleFlag = "ClassficationAreaTitle";
        public const string kstrXMLSupportForceEnforcerOptionFlag = "SupportForceEnforcerOption";
        public const string kstrXMLSubmitSuccessedFlag = "SubmitSuccessed";
        public const string kstrXMLSubmitFailedFlag = "SubmitFailed";
        public const string kstrXMLFormTitleFlag = "FormTitle";
        public const string kstrXMLRecordPerformanceLogFlag = "RecordPerformanceLog";
        public const string kstrXMLThreadPoolMinThreadCountFlag = "ThreadPoolMinThreadCount";
        public const string kstrXMLAgentAuotReplyFlag = "AgentAuotReply";
        public const string kstrXMLAssitantAutoReplyFlag = "AssitantAutoReply";
        public const string kstrXMLAssitantAutoSendFlag = "AssitantAutoSend";
        public const string kstrXMLNLAssistantClassifyTokenExpiryTimeFlag = "NLAssistantClassifyTokenExpiryTime";
        public const string kstrXMLClassifyAssistantServiceAddrFlag = "ClassifyAssistantServiceAddr";
        public const string kstrXMLIMFrequentMessageIntervalFlag = "IMFrequentMessageInterval";

        public const string kstrXMLDBConectionErrorFlag = "DBConectionError";
        public const string ksrtXMLClassficationFormatError = "ClassficationFormatError";
        public const string kstrXMLAssitantUnknonwnErrorFlag = "AssitantUnknonwnError";
        public const string kstrXMLAssitantInvalidRequestErrorFlag = "AssitantInvalidRequestError";
        public const string kstrXMLAssitantNoClassifyPermissionErrorFlag = "AssitantNoClassifyPermissionError";

        public const string kstrXMLCodeAttr = "code";
        public const string kstrXMLTypeAttr = "type";
        public const string kstrXMLAddrAttr = "addr";
        public const string kstrXMLPortAttr = "port";
        public const string kstrXMLCatalogAttr = "catalog";
        public const string kstrXMLUserNameAttr = "username";
        public const string kstrXMLPasswordAttr = "password";
        public const string kstrXMLServerFQDNAttr = "serverfqdn";
        public const string kstrXMLUserDomainAttr = "userdomain";
        public const string kstrXMLUserURIAttr = "useruri";
        #endregion

        #region configure file path define
        private const string kstrCfgFolderName = "config";
        private const string kstrCfgFileExtension = ".xml";
        private const string kstrCfgFileNameSep = "_";
        static private readonly Dictionary<EMSFB_CFGTYPE, STUSFB_CFGFILENAME> kdirCfgFilesInfo = new Dictionary<EMSFB_CFGTYPE, STUSFB_CFGFILENAME>()
        {
            {EMSFB_CFGTYPE.emCfgLog, new STUSFB_CFGFILENAME("", "", "log", kstrCfgFileExtension, kstrCfgFolderName, kstrCfgFileNameSep)},
            {EMSFB_CFGTYPE.emCfgCommon, new STUSFB_CFGFILENAME("", "", "", kstrCfgFileExtension, kstrCfgFolderName, kstrCfgFileNameSep)}
        };
        static private readonly Dictionary<EMSFB_MODULE, string> s_dicModuleComCfgName = new Dictionary<EMSFB_MODULE, string>()
        {
            {EMSFB_MODULE.emSFBModule_Unknown, "test"}
        };
        static private readonly Dictionary<EMSFB_ENDPOINTTTYPE, string> s_dicEndpointTypeAndConversationFlagMapping = new Dictionary<EMSFB_ENDPOINTTTYPE,string>()
        {
            {EMSFB_ENDPOINTTTYPE.emTypeUnknown, ""},
            {EMSFB_ENDPOINTTTYPE.emTypePerformanceTester, kstrXMLPerformanceTesterConversationSubjectFlag},
        };
        #endregion

        #region static public methods
        static public string GetCfgFilePath(EMSFB_CFGTYPE emCfgType, EMSFB_MODULE emModule)
        {
            string strFilePath = "";
            try
            {
                STUSFB_CFGFILENAME stuCfgFileName = CommonHelper.GetValueByKeyFromDir(kdirCfgFilesInfo, emCfgType, null);
                if (null != stuCfgFileName)
                {
                    if ((EMSFB_CFGTYPE.emCfgCommon == emCfgType) || (EMSFB_CFGTYPE.emCfgLog == emCfgType))
                    {
                        string strSpecifyName = Common.CommonHelper.GetValueByKeyFromDir(s_dicModuleComCfgName, emModule, "");
                        stuCfgFileName.SetSpecialName(strSpecifyName);
                    }
                    strFilePath = GetSFBCfgFileFullPath(stuCfgFileName);
                }
            }
            catch (Exception ex)
            {
                strFilePath = "";
                QAToolSFBCommon.NLLog.CLog.OutputTraceLog("Exception happened in GetCfgFilePath,[{0}]\n", ex.Message);
            }
            QAToolSFBCommon.NLLog.CLog.OutputTraceLog("FilePath: GetCfgFilePath:[{0}]\n", strFilePath);
            return strFilePath;
        }
        static public string GetEndpointConversationFlag(EMSFB_ENDPOINTTTYPE emEndpointType)
        {
            return CommonHelper.GetValueByKeyFromDir(s_dicEndpointTypeAndConversationFlagMapping, emEndpointType, "");
        }
        #endregion

        #region static private tools
        static public string GetSFBCfgFileFullPath(STUSFB_CFGFILENAME stuCfgFileName)
        {
            string strCfgFile = "";
            try
            {
                // First find in current folder
                strCfgFile = stuCfgFileName.GetEffectiveFilePath("", false);

                // Second find in specify relative folder base on current folder
                if (string.IsNullOrEmpty(strCfgFile))
                {
                    strCfgFile = stuCfgFileName.GetEffectiveFilePath("", true);
                }

                // Third find in specify relative folder base on SFBInstall folder
                if (string.IsNullOrEmpty(strCfgFile))
                {
                    if (!string.IsNullOrEmpty(CommonHelper.kstrSFBInstallPath))
                    {
                        strCfgFile = stuCfgFileName.GetEffectiveFilePath(CommonHelper.kstrSFBInstallPath, true);
                    }
                }
                QAToolSFBCommon.NLLog.CLog.OutputTraceLog("Cfg File Path:[{0}]\n", strCfgFile);
            }
            catch (Exception ex)
            {
                strCfgFile = "";
                QAToolSFBCommon.NLLog.CLog.OutputTraceLog("Exception happened in GetSFBCfgFileFullPath,[{0}]\n", ex.Message);
            }
            return strCfgFile;
        }
        #endregion

        #region members
        private bool m_bLoadSuccess = false;
        private string m_strLoadInfo = "";
        private string m_strCfgType = null; // reserve
        private STUSFB_DBACCOUNT m_stuDBAccount = null;
        private Dictionary<EMSFB_ENDPOINTTTYPE,STUSFB_ENDPOINTPROXYACCOUNT> m_dicStuEndpointProxyAccount = null;
        private Dictionary<EMSFB_ENDPOINTTTYPE, STUSFB_ENDPOINTPROXYTCPINFO> m_dicEndpointProxyTcpInfo = null;
        private STUSFB_PROMPTMSG m_stuPromptMsg = null;
        #endregion

        #region constructor
        public ConfigureFileManager(EMSFB_MODULE emModule)   // Configure used to manager emCfgCommon configure files
        {
            try
            {
                SetLoadStatusInfo(false, "Unknown error");
                string strCfgCommonFilePath = GetCfgFilePath(EMSFB_CFGTYPE.emCfgCommon, emModule);
                if (!string.IsNullOrEmpty(strCfgCommonFilePath) && (File.Exists(strCfgCommonFilePath)))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(strCfgCommonFilePath);

                    // Select Configure node
                    XmlNode obXMLConfigure = xmlDoc.SelectSingleNode(kstrXMLConfigureFlag);
                    if (null != obXMLConfigure)
                    {
                        // Configure file type
                        m_strCfgType = XMLTools.GetAttributeValue(obXMLConfigure.Attributes, kstrXMLTypeAttr);

                        // Account: Database, NLLyncEndpointProxy
                        XmlNode obXMLAccount = obXMLConfigure.SelectSingleNode(kstrXMLAccountFlag);
                        if (null != obXMLAccount)
                        {
                            m_stuDBAccount = new STUSFB_DBACCOUNT(obXMLAccount.SelectSingleNode(kstrXMLDatabaseFlag));
                            XmlNodeList obXmlLyncClientAccounts = GetXmlLyncClientInfos(obXMLAccount);
                            if (null != obXmlLyncClientAccounts)
                            {
                                if (null == m_dicStuEndpointProxyAccount)
                                {
                                    m_dicStuEndpointProxyAccount = new Dictionary<EMSFB_ENDPOINTTTYPE, STUSFB_ENDPOINTPROXYACCOUNT>();
                                }
                                foreach (XmlNode obXmlLyncClientAccount in obXmlLyncClientAccounts)
                                {
                                    EMSFB_ENDPOINTTTYPE emEndpointType = STUSFB_ENDPOINTPROXYACCOUNT.GetEndpointEnumTypeByString(XMLTools.GetAttributeValue(obXmlLyncClientAccount.Attributes, ConfigureFileManager.kstrXMLTypeAttr));
                                    CommonHelper.AddKeyValuesToDir(m_dicStuEndpointProxyAccount, emEndpointType, new STUSFB_ENDPOINTPROXYACCOUNT(emEndpointType, obXmlLyncClientAccount));
                                }
                            }
                        }

                        // TCPConmunication: NLLyncEndpointProxy
                        XmlNode obXMLTCPConmunication = obXMLConfigure.SelectSingleNode(kstrXMLTCPConmunicationFlag);
                        if (null != obXMLTCPConmunication)
                        {
                            XmlNodeList obXmlLyncClientTcpIps = GetXmlLyncClientInfos(obXMLTCPConmunication);
                            if (null != obXmlLyncClientTcpIps)
                            {
                                if (null == m_dicEndpointProxyTcpInfo)
                                {
                                    m_dicEndpointProxyTcpInfo = new Dictionary<EMSFB_ENDPOINTTTYPE,STUSFB_ENDPOINTPROXYTCPINFO>();
                                }
                                foreach (XmlNode obXmlLyncClientTcpIp in obXmlLyncClientTcpIps)
                                {
                                    EMSFB_ENDPOINTTTYPE emEndpointType = STUSFB_ENDPOINTPROXYACCOUNT.GetEndpointEnumTypeByString(XMLTools.GetAttributeValue(obXmlLyncClientTcpIp.Attributes, ConfigureFileManager.kstrXMLTypeAttr));
                                    CommonHelper.AddKeyValuesToDir(m_dicEndpointProxyTcpInfo, emEndpointType, new STUSFB_ENDPOINTPROXYTCPINFO(emEndpointType, obXmlLyncClientTcpIp));
                                }
                            }
                        }

                        // PromptMessage: SIPComponent[ErrorCode], SFBControlPanel[DBError,PSError,UnknownError]
                        XmlNode obXMLPromptMsg = obXMLConfigure.SelectSingleNode(kstrXMLPromptMsgFlag);
                        if (null != obXMLPromptMsg)
                        {
                            m_stuPromptMsg = new STUSFB_PROMPTMSG(obXMLPromptMsg, emModule);
                        }
                        SetLoadStatusInfo(true, "Load Success");
                    }
                    else
                    {
                        SetLoadStatusInfo(false, string.Format("Donot found the XML node:[{0}]", kstrXMLConfigureFlag));
                    }
                }
                else
                {
                    SetLoadStatusInfo(false, "Configure file not exist");
                }
            }
            catch (Exception ex)
            {
                SetLoadStatusInfo(true, ex.Message);
                QAToolSFBCommon.NLLog.CLog.OutputTraceLog("Exception happened in ConfigureFileManager constructor,[{0}]\n", ex.Message);
            }
        }
        #endregion

        #region public method
        public STUSFB_DBACCOUNT GetDBAccount() { return m_stuDBAccount; }
        public Dictionary<EMSFB_ENDPOINTTTYPE,STUSFB_ENDPOINTPROXYACCOUNT> GetEndpointProxyAccount() { return m_dicStuEndpointProxyAccount;}
        public Dictionary<EMSFB_ENDPOINTTTYPE,STUSFB_ENDPOINTPROXYTCPINFO> GetEndpointProxyTcpInfo() { return m_dicEndpointProxyTcpInfo;}
        public STUSFB_PROMPTMSG GetPromptMsg() { return m_stuPromptMsg; }
        public bool IsLoadSuccess() { return m_bLoadSuccess; }
        public string GetLoadStatusInfo() { return m_strLoadInfo; }
        #endregion

        #region private tools
        private XmlNodeList GetXmlLyncClientInfos(XmlNode obXmlParent)
        {
            XmlNode obXmlEndpointProxy = obXmlParent.SelectSingleNode(kstrXMLNLLyncEndpointProxyFlag);
            if (null != obXmlEndpointProxy)
            {
                return obXmlEndpointProxy.SelectNodes(kstrXMLLyncClientAccountFlag);
            }
            return null;
        }
        private void SetLoadStatusInfo(bool bSuccess, string strLoadInfo)
        {
            m_bLoadSuccess = bSuccess;
            m_strLoadInfo = strLoadInfo;
        }
        #endregion
    }


}
