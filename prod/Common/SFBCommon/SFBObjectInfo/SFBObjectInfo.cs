using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

// Current project
using SFBCommon.Common;
using SFBCommon.NLLog;
using SFBCommon.Database;


namespace SFBCommon.SFBObjectInfo
{
    // Because of function UpdateObjInfo,NLKeyValuePair, if you want to add members in parent class, do not forget to provide a method to manager there owner members.
    public abstract class SFBObjectInfo
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(SFBObjectInfo));
        #endregion

        #region Values
        public const string kstrMemberSeparator = ";";
        protected const string kstrLyncPSModuleName = "Lync";
        public const string kstrTimeFormatUTCSecond = "yyyy-MM-dd HH:mm:ss";
        #endregion

        #region Static members
        static private readonly STUSFB_DBACCOUNT s_stuDBAccount = GetDBAccount();
        static private readonly string kstrDeprecatedFieldName = "deprecated";
        // static private readonly string kstrTrueFieldValue = "true";  // True and false, only define one value
        static private readonly string kstrFalseFieldValue = "false";
        static private object s_obLockForPersistantStorageIns = new object();
        static private IPersistentStorage s_IPersistantStorageIns = null;
        #endregion

        #region Static functions
        // Note: 
        //  1. CommonConfigureInfo no need and also do not support this function, don't pass "EMSFB_INFOTYPE.emInfoCommonConfigure" in.
        //  2. This function maybe take a long time if you want to use this function
        //  3. Currently only EMSFB_INFOTYPE.emInfoNLClassificationSchema in classification tool need use this
        static public List<SFBObjectInfo> GetAllObjsFrommPersistentInfo(EMSFB_INFOTYPE emInfoType)
        {
            if (EMSFB_INFOTYPE.emInfoCommonConfigure == emInfoType)
            {
                throw new Exception("!!!Error, pass a wrong info type [emInfoCommonConfigure] in GetObjsFrommPersistentInfo function\n");
            }

            EMSFB_MODULE emCurModuleType = Startup.GetCurModuleType();
            if (!(
                    ((EMSFB_INFOTYPE.emInfoNLClassificationSchema == emInfoType) && ((EMSFB_MODULE.emSFBModule_ClassificationTool == emCurModuleType) || (EMSFB_MODULE.emSFBModule_SFBControlPanel == emCurModuleType))) ||
                    ((EMSFB_INFOTYPE.emInfoNLTagLabel == emInfoType) && (EMSFB_MODULE.emSFBModule_ClassificationTool == emCurModuleType))
                 )
               )
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!This is maybe a logic error. Current module:[{0}], info type:[{1}] maybe no need invoke this function to get all objects info\n", Startup.GetCurModuleType(), emInfoType);
            }

            List<SFBObjectInfo> lsSFBObjectInfo = null;
            IPersistentStorage obPersistantStorage = CreateSFBDBMgr(s_stuDBAccount);
            if (null != obPersistantStorage)
            {
                Dictionary<string, string>[] szDicInfos = null;

                if(emInfoType == EMSFB_INFOTYPE.emInfoNLTagLabel || emInfoType == EMSFB_INFOTYPE.emInfoNLClassificationSchema)
                {
                    szDicInfos = GetAllValidObjsFromPersisitentInfo(emInfoType, obPersistantStorage);
                }
                else
                {
                    szDicInfos = obPersistantStorage.GetAllObjInfo(emInfoType);
                }

                if ((null != szDicInfos))
                {
                     lsSFBObjectInfo = new List<SFBObjectInfo>();
                     for (int i = 0; i < szDicInfos.Length; ++i)
                     {
                         lsSFBObjectInfo.Add(CreateSFBObjByInfoType(emInfoType, szDicInfos[i]));
                     }
                }
            }
            return lsSFBObjectInfo;
        }
        // lsSpecifyOutFields:
        //      1. can be null or empty means return all
        //      2. do not suggest to using null or empty
        //      3. if some filed is same in difference tables only return the last one
        // lsSearchScopes cannot be null or empty
        // lsSearchConditions cannot be null or empty
        static public Dictionary<EMSFB_INFOTYPE, Dictionary<string, string>>[] GetDatasFromPersistentInfoWithFullSearchConditions(List<STUSFB_INFOFIELD> lsSpecifyOutFields, List<EMSFB_INFOTYPE> lsSearchScopes, List<KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>> lsSearchConditions)
        {
            Dictionary<EMSFB_INFOTYPE, Dictionary<string, string>>[] szAllObjInfo = null;
            IPersistentStorage obPersistantStorage = CreateSFBDBMgr(s_stuDBAccount);
            if ((null != obPersistantStorage) && ((null != lsSearchScopes) && (0 < lsSearchScopes.Count)) && ((null != lsSearchConditions) && (0 < lsSearchConditions.Count)))
            {
                szAllObjInfo = obPersistantStorage.GetObjInfoWithFullSearchConditions(lsSpecifyOutFields, lsSearchScopes, lsSearchConditions);
                {
                    // Debug
                    if (null != szAllObjInfo)
                    {
                        for (int i = 0; i < szAllObjInfo.Length; ++i)
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Index:{0}:\n", i);
                            foreach (KeyValuePair<EMSFB_INFOTYPE, Dictionary<string, string>> pairObjInfo in szAllObjInfo[i])
                            {
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\tType:{0}:\n", pairObjInfo.Key);
                                foreach (KeyValuePair<string, string> pairKeyValues in pairObjInfo.Value)
                                {
                                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\t\tKey:[{0}],Value:[{1}]\n", pairKeyValues.Key, pairKeyValues.Value);
                                }
                            }
                        }
                    }
                }
            }
            return szAllObjInfo;
        }
        // Note: CommonConfigureInfo no need and also do not support this function, don't pass "EMSFB_INFOTYPE.emInfoCommonConfigure" in.
        static public List<SFBObjectInfo> GetObjsFrommPersistentInfo(EMSFB_INFOTYPE emInfoType, string strFieldName, params string[] szStrFieldValues)
        {
            if (EMSFB_INFOTYPE.emInfoCommonConfigure == emInfoType)
            {
                throw new Exception("!!!Error, pass a wrong info type [emInfoCommonConfigure] in GetObjsFrommPersistentInfo function\n");
            }

            List<SFBObjectInfo> lsSFBObjectInfo = null;
            IPersistentStorage obPersistantStorage = CreateSFBDBMgr(s_stuDBAccount);
            if ((null != obPersistantStorage) && (null != szStrFieldValues) && (0 < szStrFieldValues.Length))
            {
                Dictionary<string, string>[] szDicInfos = obPersistantStorage.GetObjInfoEx(emInfoType, strFieldName, szStrFieldValues);
                if ((null != szDicInfos))
                {
                    lsSFBObjectInfo = new List<SFBObjectInfo>();
                    for (int i = 0; i < szDicInfos.Length; ++i)
                    {
                        lsSFBObjectInfo.Add(CreateSFBObjByInfoType(emInfoType, szDicInfos[i]));
                    }
                }
            }
            return lsSFBObjectInfo;
        }
        // Note: CommonConfigureInfo no need and also do not support this function, don't pass "EMSFB_INFOTYPE.emInfoCommonConfigure" in.
        static public List<SFBObjectInfo> GetObjsFromPersistentInfoByLikeValue(EMSFB_INFOTYPE emInfoType, string strKeyFieldName, string strKeyLikeValue)
        {
            if (EMSFB_INFOTYPE.emInfoCommonConfigure == emInfoType)
            {
                throw new Exception("!!!Error, pass a wrong info type [emInfoCommonConfigure] in GetObjsFromPersistentInfoByLikeValue function\n");
            }

            List<SFBObjectInfo> lsSFBObjectInfo = null;
            IPersistentStorage obPersistantStorage = CreateSFBDBMgr(s_stuDBAccount);
            if ((null != obPersistantStorage) && (!string.IsNullOrEmpty(strKeyFieldName)) && (!string.IsNullOrEmpty(strKeyLikeValue)))
            {
                Dictionary<string, string>[] szDicInfos = obPersistantStorage.GetObjInfoByLikeValue(emInfoType, strKeyFieldName, strKeyLikeValue);
                if ((null != szDicInfos))
                {
                    lsSFBObjectInfo = new List<SFBObjectInfo>();
                    for (int i = 0; i < szDicInfos.Length; ++i)
                    {
                        lsSFBObjectInfo.Add(CreateSFBObjByInfoType(emInfoType, szDicInfos[i]));
                    }
                }
            }
            return lsSFBObjectInfo;
        }
        // This function is not good, we need find a way to register derive class type.
        static private SFBObjectInfo CreateSFBObjByInfoType(EMSFB_INFOTYPE emInfoType, Dictionary<string, string> dicInfos)
        {
            SFBObjectInfo obSFBObjInfo = null;
            switch (emInfoType)
            {
            case EMSFB_INFOTYPE.emInfoCommonConfigure:
            {
                obSFBObjInfo = new CommonConfigureInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoSFBFileStoreService:
            {
                obSFBObjInfo = new SFBFileStoreServiceInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoSFBPersistentChatServer:
            {
                obSFBObjInfo = new SFBPersistentChatServerInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoNLChatCategory:
            {
                obSFBObjInfo = new NLChatCategoryInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoNLChatRoom:
            {
                obSFBObjInfo = new NLChatRoomInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoNLMeeting:
            {
                obSFBObjInfo = new NLMeetingInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoSFBChatCategory:
            {
                obSFBObjInfo = new SFBChatCategoryInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoSFBChatRoom:
            {
                obSFBObjInfo = new SFBChatRoomInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoSFBChatRoomVariable:
            {
                obSFBObjInfo = new SFBChatRoomVariableInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoSFBMeeting:
            {
                obSFBObjInfo = new SFBMeetingInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoSFBMeetingVariable:
            {
                obSFBObjInfo = new SFBMeetingVariableInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoSFBMeetingShare:
            {
                obSFBObjInfo = new SFBMeetingShareInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoSFBUserInfo:
            {
                obSFBObjInfo = new SFBUserInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoNLAssistantServiceInfo:
            {
                obSFBObjInfo = new NLAssistantServiceInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoNLAssistantQueryPolicyInfo:
            {
                obSFBObjInfo = new NLAssistantQueryPolicyInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoSFBChatRoomAttachmentEntryVariable:
            {
                obSFBObjInfo = new SFBChatRoomAttachmentEntryVariableInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoNLChatRoomAttachment:
            {
                obSFBObjInfo = new NLChatRoomAttachmentInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoNLTagLabel:
            {
                obSFBObjInfo = new NLTagLabelInfo(dicInfos);
                break;
            }
            case EMSFB_INFOTYPE.emInfoNLClassificationSchema:
            {
                obSFBObjInfo = new NLClassificationSchemaInfo(dicInfos);
                break;
            }
            default:
            {
                break;
            }
            }

            // Just check
            if (obSFBObjInfo.GetSFBInfoType() != emInfoType)
            {
                throw new Exception("!!!!!Code error, create a wrong object by info type\n");
            }
            return obSFBObjInfo;
        }
        static private IPersistentStorage GetIPersistentStorageInstance()
        {
            if (null == s_IPersistantStorageIns)
            {
                lock (s_obLockForPersistantStorageIns)
                {
                    if (null == s_IPersistantStorageIns)
                    {
                        s_IPersistantStorageIns = CreateSFBDBMgr(s_stuDBAccount);
                    }
                }
                
            }
            if (null == s_IPersistantStorageIns)
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
            }
            return s_IPersistantStorageIns;
        }
        static private IPersistentStorage CreateSFBDBMgr(STUSFB_DBACCOUNT stuDBAccount)
        {
            IPersistentStorage obIPersistentStorage = null;
            if (null != stuDBAccount)
            {
                SFBDBMgr obSFBDBMgr = new SFBDBMgr(stuDBAccount.m_strAddr, stuDBAccount.m_unPort, stuDBAccount.m_strCatalog, stuDBAccount.m_strUserName, stuDBAccount.m_strPassword, stuDBAccount.m_emDBType);
                if (obSFBDBMgr.GetEstablishSFBMgrFlag())
                {
                    obIPersistentStorage = obSFBDBMgr;
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Establish SFBMgr failed\n");
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "DB Init info is null, current module maybe no need used it or forgot to initialize it\n");
            }
            return obIPersistentStorage;
        }
        static private STUSFB_DBACCOUNT GetDBAccount()
        {
            ConfigureFileManager obCfgFileMgr = Startup.GetConfigureFileManager();
            if (null != obCfgFileMgr)
            {
                return obCfgFileMgr.GetDBAccount();
            }
            return null;
        }

        static private Dictionary<string, string>[] GetAllValidObjsFromPersisitentInfo(EMSFB_INFOTYPE emInfoType, IPersistentStorage dbStorage)
        {
            Dictionary<string, string>[] szDictObjInfo = null;
            Dictionary<EMSFB_INFOTYPE, Dictionary<string, string>>[] szSchemaInfo = null;

            if (dbStorage != null)
            {
                STUSFB_CONDITIONGROUP lsSearchFilters = new STUSFB_CONDITIONGROUP();
                lsSearchFilters.emLogicOp = EMSFB_INFOLOGICOP.emSearchLogicOr;
                lsSearchFilters.lsComditonItems = new List<STUSFB_INFOITEM>() 
                {
                    new STUSFB_INFOITEM(new STUSFB_INFOFIELD(emInfoType, kstrDeprecatedFieldName), new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoUnknown, kstrFalseFieldValue), EMSFB_INFOCOMPAREOP.emSearchOp_Equal)
                };

                List<STUSFB_INFOFIELD> lsOutFields = null;
                List<EMSFB_INFOTYPE> lsTables = new List<EMSFB_INFOTYPE>() { emInfoType };
                List<KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>> lsSearchConditions = new List<KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>>();
                lsSearchFilters.emLogicOp = EMSFB_INFOLOGICOP.emSearchLogicAnd;
                lsSearchConditions.Add(new KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>(EMSFB_INFOLOGICOP.emSearchLogicAnd, lsSearchFilters));

                szSchemaInfo = dbStorage.GetObjInfoWithFullSearchConditions(lsOutFields, lsTables, lsSearchConditions);

                if (szSchemaInfo != null)
                {
                    if(szSchemaInfo.Length == 0)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "No Data Matches Condition: {0}={1}", kstrDeprecatedFieldName, kstrFalseFieldValue);
                    }

                    szDictObjInfo = new Dictionary<string, string>[szSchemaInfo.Length];

                    for (int i = 0; i < szDictObjInfo.Length; i++)
                    {
                        Dictionary<string, string> dataRowDict = szSchemaInfo[i][emInfoType];

                        if (dataRowDict != null)
                        {
                            szDictObjInfo[i] = dataRowDict;
                        }
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "GetObjInfoWithFullSearchConditions() failed, may be caused by FieldNotExist, SqlError or Other logical error");
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "GetAllValidTagsFromPersisitentInfo failed, GetIPersistentStorage() failed");
            }
            return szDictObjInfo;
        }
        #endregion

        #region Members
        // private IPersistentStorage m_IPersistantStorage = CreateSFBDBMgr(s_stuDBAccount);
        private Dictionary<string, string> m_dicInfos = new Dictionary<string, string>(); // all keys using lower case. please using the inner tools functions to operator it: InnerSetItem, InnerGetItemValue, InnerUpdateInfo, InnerGetInfo
        #endregion

        #region Constructors
        public SFBObjectInfo(params string[] szStrKeyAndValus)
        {
            InnerSetItem(szStrKeyAndValus);
        }
        public SFBObjectInfo(params KeyValuePair<string, string>[] szPairKeyAndValus)
        {
            InnerSetItem(szPairKeyAndValus);
        }
        public SFBObjectInfo(Dictionary<string, string> dicInfos)
        {
            InnerUpdateInfo(dicInfos);
        }
        #endregion

        #region Destructor
        ~SFBObjectInfo()
        {
#if false
            if (null != m_IPersistantStorage)
            {
                m_IPersistantStorage.Dispose();
            }
#endif
        }
        #endregion

        #region Public functions
        public void OutputObjInfo()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "\nBegin to output object:[{0}] info\n", GetSFBInfoType().ToString());
            if (null != m_dicInfos)
            {
                int i = 0;
                foreach (KeyValuePair<string, string> obPairItem in m_dicInfos)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "[{0}:{1}={2}]\n", i++, obPairItem.Key, obPairItem.Value);
                }
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "\nEnd output object:[{0}] info\n", GetSFBInfoType().ToString());
        }

        public bool SetItem(params string[] szStrKeyAndValus)
        {
            return InnerSetItem(szStrKeyAndValus);
        }
        public bool SetItem(params KeyValuePair<string,string>[] szPairKeyAndValus)
        {
            return InnerSetItem(szPairKeyAndValus);
        }
        public string GetItemValue(string strKey)
        {
            return InnerGetItemValue(strKey);
        }
        public void UpdateInfo(Dictionary<string, string> dicInfos)
        {
            InnerUpdateInfo(dicInfos);
        }
        public Dictionary<string, string> GetInfo()
        {
            if (null != m_dicInfos)
            {
                return new Dictionary<string, string>(m_dicInfos);   // return a clone
            }
            return null;
        }
        public bool DeleteObject(string strKeyValue)
        {
            if (!String.IsNullOrEmpty(strKeyValue))
            {
                IPersistentStorage obIPersistentStorage = GetIPersistentStorage();
                if (null != obIPersistentStorage)
                {
                    return obIPersistentStorage.DeleteObjInfo(GetSFBInfoType(), strKeyValue);
                }
            }
            return false;
        }
        #endregion

        #region Virtual functions
        virtual public bool PersistantSave()
        {
            IPersistentStorage obIPersistentStorage = GetIPersistentStorage();
            if (null != obIPersistentStorage)
            {
                return obIPersistentStorage.SaveObjInfo(GetSFBInfoType(), m_dicInfos);
            }
            return false;
        }
        virtual public bool EstablishObjFormPersistentInfo(string strKey, string strValue)
        {
            IPersistentStorage obIPersistentStorage = GetIPersistentStorage();
            if (null != obIPersistentStorage)
            {
                Dictionary<string, string> dicTemp = obIPersistentStorage.GetObjInfo(GetSFBInfoType(), strKey, strValue);
                if (dicTemp.ContainsKey(kstrDeprecatedFieldName))
                {
                    string strDeprecatedFieldValue = CommonHelper.GetValueByKeyFromDir(dicTemp, kstrDeprecatedFieldName, kstrFalseFieldValue);
                    if (strDeprecatedFieldValue.Equals(kstrFalseFieldValue, StringComparison.OrdinalIgnoreCase))
                    {
                        m_dicInfos = dicTemp;
                    }
                    else
                    {
                        m_dicInfos = new Dictionary<string, string>();
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "current record was deprecated");
                    }
                }
                else
                {
                    m_dicInfos = dicTemp;
                }
                return (null != m_dicInfos);
            }
            return false;
        }
        #endregion

        #region Abstract functions, must be implement by child
        public abstract EMSFB_INFOTYPE GetSFBInfoType();
        #endregion

        #region Inner tools
        protected IPersistentStorage GetIPersistentStorage()
        {
#if false
            if (null == m_IPersistantStorage)
            {
                m_IPersistantStorage = CreateSFBDBMgr(s_stuDBAccount);
            }
            if (null == m_IPersistantStorage)
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
            }
            return m_IPersistantStorage;
#else
            return GetIPersistentStorageInstance();
#endif
        }
        protected bool InnerSetItem<T>(params T[] szTKeyAndValus)
        {
            bool bRet = false;
            if (null != szTKeyAndValus)
            {
                if (szTKeyAndValus is string[])
                {
                    string[] szStrKeyAndValues = szTKeyAndValus as string[];
                    if (0 == (szStrKeyAndValues.Length % 2))
                    {
                        for (int i = 1; i < szStrKeyAndValues.Length; i += 2)
                        {
                            InnerCoreSetItem(szStrKeyAndValues[i - 1], szStrKeyAndValues[i]);
                        }
                        bRet = true;
                    }
                }
                else if (szTKeyAndValus is KeyValuePair<string, string>[])
                {
                    KeyValuePair<string, string>[] szPairKeyAndValus = szTKeyAndValus as KeyValuePair<string, string>[];
                    for (int i = 0; i < szPairKeyAndValus.Length; ++i)
                    {
                        InnerCoreSetItem(szPairKeyAndValus[i].Key, szPairKeyAndValus[i].Value);
                    }
                    bRet = true;
                }
            }
            return bRet;
        }
        protected void InnerCoreSetItem(string strKey, string strValue)
        {
            CommonHelper.AddKeyValuesToDir(m_dicInfos, strKey.ToLower(), strValue);
        }
        protected string InnerGetItemValue(string strKey)
        {
            if ((null != strKey) && (null != m_dicInfos))
            {
                foreach (var obItem in m_dicInfos)
                {
                    if (obItem.Key.Equals(strKey.ToLower()))
                    {
                        return obItem.Value;
                    }
                }
            }
            return null;
        }
        protected void InnerUpdateInfo(Dictionary<string, string> dicInfos)
        {
            if (null != dicInfos)
            {
                foreach (var obItem in dicInfos)
                {
                    InnerSetItem(obItem.Key, obItem.Value);
                }
            }
        }
        #endregion
    }
}
