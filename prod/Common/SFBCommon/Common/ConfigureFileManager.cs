using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

using SFBCommon.Database;

namespace SFBCommon.Common
{
    #region CFGTYPE
    public enum EMSFB_CFGTYPE
    {
        emCfgLog,
        emCfgCommon,
        emCfgSipMSPL,       // .am file
        emCfgClassifyInfo
    }
    public enum EMSFB_CFGINFOTYPE
    {
        emCfgInfoSip,
        emCfgInfoHttp,
        emCfgInfoSFBCtlPanel,
        emCfgInfoNLLyncEndpointProxy_Agent,
        emCfgInfoNLLyncEndpointProxy_Assistant,
        emCfgInfoMaintainTool,
        emCfgInfoNLAssistantWebService,
        emCfgInfoClassificationTool
    }
    public enum EMSFB_MODULE
    {
        emSFBModule_Unknown,

        emSFBModule_HTTPComponent,
        emSFBModule_NLLyncEndpointProxy_Agent,
        emSFBModule_NLLyncEndpointProxy_Assistant,
        emSFBModule_SFBControlPanel,
        emSFBModule_SIPComponent,
        
        emSFBModule_MaintainTool,
        emSFBModule_NLAssistantWebService,
        emSFBModule_ClassificationTool,
        emSFBModule_UnitTest,
    }
    public enum EMSFB_ENDPOINTTTYPE
    {
        emTypeUnknown,

        emTypeAgent,
        emTypeAssistant
    }
    #endregion

    #region CFGFILENAME
    public class STUSFB_CFGFILENAME
    {
        private string m_strPrefixName = "";
        private string m_strSpecialName = "";
        private string m_strSuffixName = "";
        private string m_strExtension = "";
        private string m_strParentFolderName = "";
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

        #region Public functions
        public void SetSpecialName(string strSpecialName)
        {
            m_strSpecialName = strSpecialName;
        }
        public string GetEffectiveFilePath(string strRootFolderPath, bool bCombineParentFolder) // return an exist file path, otherwise return "";
        {
            // Make a standard root folder path
            strRootFolderPath = CommonHelper.GetStandardFolderPath(strRootFolderPath);

            string strCfgFile = strRootFolderPath + InnerGetFileFullName(bCombineParentFolder);
            bool bIsEffectiveFilePath = File.Exists(strCfgFile);
            SFBCommon.NLLog.CLog.OutputTraceLog("Get FilePath:[{0}], Effictive:[{1}]\n", strCfgFile, bIsEffectiveFilePath);

            return bIsEffectiveFilePath ? strCfgFile : "" ;
        }
        public string GetFileName()
        {
            return InnerGetFileFullName(false);
        }
        #endregion

        #region Private tools
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
        #endregion
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
                SFBCommon.NLLog.CLog.OutputTraceLog("Exception happened in STUSFB_DBACCOUNT constructor,[{0}]\n", ex.Message);
            }
        }
        #endregion
    }
    public class STUSFB_ENDPOINTPROXYACCOUNT
    {
        #region Logger
        static private SFBCommon.NLLog.CLog theLog = SFBCommon.NLLog.CLog.GetLogger(typeof(STUSFB_ENDPOINTPROXYACCOUNT));
        #endregion

        #region Const/read only values
        public const string kstrAccountTypeAgent = "agent";
        public const string kstrAccountTypeAssistant = "assistant";
        private static readonly Dictionary<EMSFB_ENDPOINTTTYPE, string> kdicEmStrAccountTypeMapping = new Dictionary<EMSFB_ENDPOINTTTYPE,string>()
        {
            {EMSFB_ENDPOINTTTYPE.emTypeUnknown, ""},
            {EMSFB_ENDPOINTTTYPE.emTypeAgent, kstrAccountTypeAgent},
            {EMSFB_ENDPOINTTTYPE.emTypeAssistant, kstrAccountTypeAssistant}
        };
        private static readonly Dictionary<string, EMSFB_ENDPOINTTTYPE> kdicStrEmAccountTypeMapping = new Dictionary<string, EMSFB_ENDPOINTTTYPE>()
        {
            {"", EMSFB_ENDPOINTTTYPE.emTypeUnknown},
            {kstrAccountTypeAgent, EMSFB_ENDPOINTTTYPE.emTypeAgent},
            {kstrAccountTypeAssistant, EMSFB_ENDPOINTTTYPE.emTypeAssistant}
        };
        private static readonly Dictionary<EMSFB_ENDPOINTTTYPE, string> kdicTypeAndUserAgentMapping = new Dictionary<EMSFB_ENDPOINTTTYPE,string>()
        {
            {EMSFB_ENDPOINTTTYPE.emTypeUnknown, ""},
            {EMSFB_ENDPOINTTTYPE.emTypeAgent, "NLAgentUser"},
            {EMSFB_ENDPOINTTTYPE.emTypeAssistant, "NLAssistantUser"}
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
                SFBCommon.NLLog.CLog.OutputTraceLog("Exception happened in STUSFB_DBACCOUNT constructor,[{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Tools
        public void OutputInfo(bool bOutputPassword = false)
        {
            theLog.OutputLog(SFBCommon.NLLog.EMSFB_LOGLEVEL.emLogLevelDebug, "Server FQDNQ: {0}\nUser Sip Uri: {1}\nUser Domain: {2}\nUser Name: {3}\nPassword: {4}\nstrUserAgent: {5}\n", 
                m_strServerFQDN, m_strUserSipURI, m_strUserDomain, m_strUserName, (bOutputPassword?m_strPassword:"******"), m_strUserAgent);
        }
        #endregion
    }
    public class STUSFB_ENDPOINTPROXYTCPINFO
    {
        #region Logger
        static private SFBCommon.NLLog.CLog theLog = SFBCommon.NLLog.CLog.GetLogger(typeof(STUSFB_ENDPOINTPROXYACCOUNT));
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
                SFBCommon.NLLog.CLog.OutputTraceLog("Exception happened in STUSFB_DBACCOUNT constructor,[{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Tools
        public void OutputInfo()
        {
            theLog.OutputLog(SFBCommon.NLLog.EMSFB_LOGLEVEL.emLogLevelDebug, "Addr: {0}\nPort: {1}\nEndpoint type: {2}\n",
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
        static public Dictionary<EMSFB_CFGINFOTYPE, string> s_dirCfgInfoType = new Dictionary<EMSFB_CFGINFOTYPE,string>()
        {
            {EMSFB_CFGINFOTYPE.emCfgInfoHttp, ConfigureFileManager.kstrXMLHTTPComponentFlag},
            {EMSFB_CFGINFOTYPE.emCfgInfoNLLyncEndpointProxy_Agent, ConfigureFileManager.kstrXMLNLLyncEndpointProxyFlag},
            {EMSFB_CFGINFOTYPE.emCfgInfoNLLyncEndpointProxy_Assistant, ConfigureFileManager.kstrXMLNLLyncEndpointProxyFlag},
            {EMSFB_CFGINFOTYPE.emCfgInfoSFBCtlPanel, ConfigureFileManager.kstrXMLSFBControlPanelFlag},
            {EMSFB_CFGINFOTYPE.emCfgInfoSip, ConfigureFileManager.kstrXMLSIPComponentFlag},
            {EMSFB_CFGINFOTYPE.emCfgInfoMaintainTool, ConfigureFileManager.kstrXMLMaintainToolFlag},
            {EMSFB_CFGINFOTYPE.emCfgInfoNLAssistantWebService, ConfigureFileManager.kstrXMLNLAssistantWebServiceFlag},
            {EMSFB_CFGINFOTYPE.emCfgInfoClassificationTool, ConfigureFileManager.kstrXMLClassificationToolFlag}
        };
        #endregion

        #region Members
        public Dictionary<string, string> m_dirRuntimeInfo = new Dictionary<string,string>();
        public Dictionary<string, STUSFB_ERRORMSG> m_dirErrorMsg = new Dictionary<string,STUSFB_ERRORMSG>();
        #endregion

        #region Constructors
        public STUSFB_PROMPTMSG() { }
        public STUSFB_PROMPTMSG(XmlNode obXMLPromptMsg, EMSFB_CFGINFOTYPE emCfgInfoType)
        {
            try
            {
                InitPromptMsg(obXMLPromptMsg, emCfgInfoType);
            }
            catch (Exception ex)
            {
                SFBCommon.NLLog.CLog.OutputTraceLog("Exception happened in STUSFB_DBACCOUNT constructor,[{0}]\n", ex.Message);
            }
        }
        #endregion
        
        #region private tools
        private void InitPromptMsg(XmlNode obXMLPromptMsg, EMSFB_CFGINFOTYPE emCfgInfoType)
        {
            if (null != obXMLPromptMsg)
            {
                XmlNode obXMLSubPromptMsg = obXMLPromptMsg.SelectSingleNode(s_dirCfgInfoType[emCfgInfoType]);
                if (null != obXMLSubPromptMsg)
                {
                    m_dirRuntimeInfo = XMLTools.GetAllSubNodesInfo(obXMLSubPromptMsg.SelectSingleNode(ConfigureFileManager.kstrXMLRuntimeInfoFlag));
                    m_dirErrorMsg = XMLTools.GetAllErrorMsgFromSubNodes(obXMLSubPromptMsg.SelectSingleNode(ConfigureFileManager.kstrXMLErrorMessageFlag));
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
        public const string kstrXMLClassificationToolFlag = "ClassificationTool";

        public const string kstrXMLLyncClientAccountFlag = "LyncClient";
        public const string kstrXMLRuntimeInfoFlag = "RuntimeInfo";
        public const string kstrXMLErrorMessageFlag = "ErrorMessage";
        public const string kstrXMLMeetingInviteMsgDenyBeforeManualClassifyDone = "MeetingInviteMsgDenyBeforeManualClassifyDone";
        public const string kstrXMLMeetingJoinMsgDenyBeforeManualClassifyDone = "MeetingJoinMsgDenyBeforeManualClassifyDone";
        public const string kstrXMLMeetingShareCreateMsgDenyBeforeManualClassifyDone = "MeetingShareCreateMsgDenyBeforeManualClassifyDone";
        public const string kstrXMLMeetingShareJoinMsgDenyBeforeManualClassifyDone = "MeetingShareJoinMsgDenyBeforeManualClassifyDone";

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
        public const string kstrXMLTextEnforcePersistentChatRoomPathHeadFlag = "TextEnforcePersistentChatRoomPathHead";
        public const string kstrXMLSFBClientServiceRestartTimeFlag = "SFBClientServiceRestartTime";
        public const string kstrXMLClassficationWarningInfoFlag = "ClassficationWarningInfo";
        public const string kstrXMLSendMessageRetryTimesFlag = "SendMessageRetryTimes";
        public const string kstrXMLSendMessageRetryIntervalFlag = "SendMessageRetryInterval";

        public const string kstrXMLDBConectionErrorFlag = "DBConectionError";
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
            {EMSFB_CFGTYPE.emCfgCommon, new STUSFB_CFGFILENAME("", "", "", kstrCfgFileExtension, kstrCfgFolderName, kstrCfgFileNameSep)},
            {EMSFB_CFGTYPE.emCfgSipMSPL, new STUSFB_CFGFILENAME("SfbServerEnforcer", "", "", ".am", kstrCfgFolderName, "")},
            {EMSFB_CFGTYPE.emCfgClassifyInfo, new STUSFB_CFGFILENAME("", "", "classifyinfo", kstrCfgFileExtension, kstrCfgFolderName, kstrCfgFileNameSep)}
        };
        static private readonly Dictionary<EMSFB_MODULE, string> s_dicModuleComCfgName = new Dictionary<EMSFB_MODULE, string>()
        {
            /* The module com config name is a special unique name for each module, cannot use a same name for two product */
            {EMSFB_MODULE.emSFBModule_Unknown, "sfbe"},
            {EMSFB_MODULE.emSFBModule_HTTPComponent, "nlwebext"},
            {EMSFB_MODULE.emSFBModule_NLLyncEndpointProxy_Agent, "nlimagent"},
            {EMSFB_MODULE.emSFBModule_NLLyncEndpointProxy_Assistant, "nlimassistant"},
            {EMSFB_MODULE.emSFBModule_SFBControlPanel, "nlcatconfig"},
            {EMSFB_MODULE.emSFBModule_SIPComponent, "nlsipproxy"},
            {EMSFB_MODULE.emSFBModule_MaintainTool, "maintaintool"},
            {EMSFB_MODULE.emSFBModule_NLAssistantWebService, "NLAssistantWebService"},
            {EMSFB_MODULE.emSFBModule_ClassificationTool, "ClassificationTool"},
            {EMSFB_MODULE.emSFBModule_UnitTest, "test"}
        };
        static private readonly Dictionary<EMSFB_ENDPOINTTTYPE, string> s_dicEndpointTypeAndConversationFlagMapping = new Dictionary<EMSFB_ENDPOINTTTYPE,string>()
        {
            {EMSFB_ENDPOINTTTYPE.emTypeUnknown, ""},
            {EMSFB_ENDPOINTTTYPE.emTypeAgent, kstrXMLAgentConversationSubjectFlag},
            {EMSFB_ENDPOINTTTYPE.emTypeAssistant, kstrXMLAssistantConversationSubjectFlag}
        };
        #endregion

        #region static public methods
        static public string GetCfgFilePath(EMSFB_CFGTYPE emCfgType, EMSFB_MODULE emModule)
        {
            string strFilePath = "";
            try
            {
                string strCurModuleSpecifyName = Common.CommonHelper.GetValueByKeyFromDir(s_dicModuleComCfgName, emModule, "");
                string strCommonSpecifyName = Common.CommonHelper.GetValueByKeyFromDir(s_dicModuleComCfgName, EMSFB_MODULE.emSFBModule_Unknown, "");
                STUSFB_CFGFILENAME stuCfgFileName = CommonHelper.GetValueByKeyFromDir(kdirCfgFilesInfo, emCfgType, null);
                stuCfgFileName.SetSpecialName(strCurModuleSpecifyName);
                if (null != stuCfgFileName)
                {
                    switch (emCfgType)
                    {
                    case EMSFB_CFGTYPE.emCfgLog:
                    {
                        if (EMSFB_MODULE.emSFBModule_Unknown != emModule)
                        {
                            strFilePath = InnerGetCfgFilePath(stuCfgFileName, strCurModuleSpecifyName);
                            if (!File.Exists(strFilePath))
                            {
                                strFilePath = InnerGetCfgFilePath(stuCfgFileName, strCommonSpecifyName);
                                if (File.Exists(strFilePath))
                                {
                                    string strLogConfigFileName = stuCfgFileName.GetFileName();
                                    string strLogOutputFileName = strCurModuleSpecifyName + ".log";
                                    strFilePath = CreateLogConfigFile(strFilePath, strLogConfigFileName, strLogOutputFileName);
                                    SFBCommon.NLLog.CLog.OutputTraceLog("End create log config file for Module:[{0}], LogFile:[{1}]. ModuelSpecifyName:[{2}], CommonSpecifyName:[{3}]\n", emModule, strFilePath, strCurModuleSpecifyName, strCommonSpecifyName);
                                }
                                else
                                {
                                    SFBCommon.NLLog.CLog.OutputTraceLog("Get common log config file for Module:[{0}] failed. ModuelSpecifyName:[{1}], CommonSpecifyName:[{2}]\n", emModule, strCurModuleSpecifyName, strCommonSpecifyName);
                                }
                            }
                            else
                            {
                                SFBCommon.NLLog.CLog.OutputTraceLog("Special log config file for current module:[{0}] already exist, ModuelSpecifyName:[{1}], CommonSpecifyName:[{2}]\n", emModule, strCurModuleSpecifyName, strCommonSpecifyName);
                            }
                        }
                        else
                        {
                            SFBCommon.NLLog.CLog.OutputTraceLog("Current module type is unknown but file type is log\n");
                        }
                        break;
                    }
                    case EMSFB_CFGTYPE.emCfgCommon:
                    {
                        strFilePath = InnerGetCfgFilePath(stuCfgFileName, strCurModuleSpecifyName, strCommonSpecifyName);
                        break;
                    }
                    case EMSFB_CFGTYPE.emCfgSipMSPL:
                    {
                        strFilePath = InnerGetCfgFilePath(stuCfgFileName, "");
                        break;
                    }
                    case EMSFB_CFGTYPE.emCfgClassifyInfo:
                    {
                        strFilePath = InnerGetCfgFilePath(stuCfgFileName, strCurModuleSpecifyName);
                        break;
                    }
                    default:
                    {
                        break;
                    }
                    }
                }
            }
            catch (Exception ex)
            {
                strFilePath = "";
                SFBCommon.NLLog.CLog.OutputTraceLog("Exception happened in GetCfgFilePath,[{0}]\n", ex.Message);
            }
            SFBCommon.NLLog.CLog.OutputTraceLog("FilePath: GetCfgFilePath:[{0}]\n", strFilePath);
            return strFilePath;
        }
        static public string GetEndpointConversationFlag(EMSFB_ENDPOINTTTYPE emEndpointType)
        {
            return CommonHelper.GetValueByKeyFromDir(s_dicEndpointTypeAndConversationFlagMapping, emEndpointType, "");
        }
        #endregion

        #region static private tools
        static private string CreateLogConfigFile(string strBaseLogConfigFile, string strLogConfigFileName, string strLogOutputFileName)
        {
            string strCurLogConfigFile = "";
            try
            {
                #region Log4Net XML define
                const string kstrXMLLog4netFlag = "log4net";
                const string kstrXMLAppenderFlag = "appender";
                const string kstrXMLTypeAttr = "type";
                const string kstrXMLFileFlag = "file";
                const string kstrXMLValueAttr = "value";
                const string kstrXMLLog4netAppenderRollingFileAppenderAttrValue = "log4net.Appender.RollingFileAppender";
                #endregion

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(strBaseLogConfigFile);  // Load XML file
                XmlNode ndRootLog4Net = xmlDoc.SelectSingleNode(kstrXMLLog4netFlag);
                XmlNodeList  ndAppenders = ndRootLog4Net.SelectNodes(kstrXMLAppenderFlag);
                for (int i=0; i<ndAppenders.Count; ++i)
                {
                    string strType = XMLTools.GetAttributeValue(ndAppenders[i].Attributes, kstrXMLTypeAttr, 0);
                    if (kstrXMLLog4netAppenderRollingFileAppenderAttrValue.Equals(strType, StringComparison.OrdinalIgnoreCase))
                    {
                        XmlNode ndFileNode = ndAppenders[i].SelectSingleNode(kstrXMLFileFlag);
                        string strFileValue = XMLTools.GetAttributeValue(ndFileNode.Attributes, kstrXMLValueAttr, 0);
                        string strLogOutputFileFullPath = CommonHelper.GetStandardFolderPath(Path.GetDirectoryName(strFileValue)) + strLogOutputFileName;
                        XMLTools.SetAttributeValue(xmlDoc, ndFileNode, kstrXMLValueAttr, strLogOutputFileFullPath);
                    }
                }

                strCurLogConfigFile = CommonHelper.GetStandardFolderPath(Path.GetDirectoryName(strBaseLogConfigFile)) + strLogConfigFileName;
                FileOpHelper.SaveToFile(strCurLogConfigFile, FileMode.CreateNew, xmlDoc.InnerXml);
            }
            catch (Exception ex)
            {
                strCurLogConfigFile = "";
                SFBCommon.NLLog.CLog.OutputTraceLog("Exception in CreateLogConfigFile, [{0}]\n", ex.Message);
            }
            return strCurLogConfigFile;
        }
        static private string GetSFBCfgFileFullPath(STUSFB_CFGFILENAME stuInCfgFileName, string strSpecialName)
        {
            string strCfgFile = "";
            try
            {
                STUSFB_CFGFILENAME stuCfgFileName = new STUSFB_CFGFILENAME(stuInCfgFileName);
                stuCfgFileName.SetSpecialName(strSpecialName);

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
                SFBCommon.NLLog.CLog.OutputTraceLog("Cfg File Path:[{0}]\n", strCfgFile);
            }
            catch (Exception ex)
            {
                strCfgFile = "";
                SFBCommon.NLLog.CLog.OutputTraceLog("Exception happened in GetSFBCfgFileFullPath,[{0}]\n", ex.Message);
            }
            return strCfgFile;
        }
        static private string InnerGetCfgFilePath(STUSFB_CFGFILENAME stuCfgFileName, params string[] szStrSpecifyNames)
        {
            string strFilePath = "";
            szStrSpecifyNames = szStrSpecifyNames.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            foreach (string strSpecifyName in szStrSpecifyNames)
            {
                strFilePath = GetSFBCfgFileFullPath(stuCfgFileName, strSpecifyName);
                if (File.Exists(strFilePath))
                {
                    break;
                }
                else
                {
                    continue;
                }
            }
            return strFilePath;
        }
        #endregion

        #region members
        private bool m_bLoadSuccess = false;
        private string m_strLoadInfo = "";
        private string m_strCfgType = null; // reserve
        private STUSFB_DBACCOUNT m_stuDBAccount = null;
        private Dictionary<EMSFB_ENDPOINTTTYPE,STUSFB_ENDPOINTPROXYACCOUNT> m_dicStuEndpointProxyAccount = null;
        private Dictionary<EMSFB_ENDPOINTTTYPE, STUSFB_ENDPOINTPROXYTCPINFO> m_dicEndpointProxyTcpInfo = null;
        private Dictionary<EMSFB_CFGINFOTYPE, STUSFB_PROMPTMSG> m_dicPromptMsg = null;
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
                            if (null == m_dicPromptMsg)
                            {
                                m_dicPromptMsg = new Dictionary<EMSFB_CFGINFOTYPE,STUSFB_PROMPTMSG>();
                            }
                            foreach (KeyValuePair<EMSFB_CFGINFOTYPE, string> pairCfgInfo in STUSFB_PROMPTMSG.s_dirCfgInfoType)
                            {
                                m_dicPromptMsg.Add(pairCfgInfo.Key, new STUSFB_PROMPTMSG(obXMLPromptMsg, pairCfgInfo.Key));
                            }
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
                SFBCommon.NLLog.CLog.OutputTraceLog("Exception happened in ConfigureFileManager constructor,[{0}]\n", ex.Message);
            }
        }
        #endregion

        #region public method
        public STUSFB_DBACCOUNT GetDBAccount() { return m_stuDBAccount; }
        public Dictionary<EMSFB_ENDPOINTTTYPE,STUSFB_ENDPOINTPROXYACCOUNT> GetEndpointProxyAccount() { return m_dicStuEndpointProxyAccount;}
        public Dictionary<EMSFB_ENDPOINTTTYPE,STUSFB_ENDPOINTPROXYTCPINFO> GetEndpointProxyTcpInfo() { return m_dicEndpointProxyTcpInfo;}
        public STUSFB_PROMPTMSG GetPromptMsg(EMSFB_CFGINFOTYPE emCfgInfoType) { return CommonHelper.GetValueByKeyFromDir(m_dicPromptMsg, emCfgInfoType, null);}
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
