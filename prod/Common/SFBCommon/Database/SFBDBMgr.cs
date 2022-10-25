using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using SFBCommon.NLLog;
using SFBCommon.Common;
using SFBCommon.SFBObjectInfo;

namespace SFBCommon.Database
{
    public enum EMSFB_DBTYPE
    {
        emDBTypeUnknown,

        emDBTypeMSSQL,
        emDBTypeMYSQL
    }

    public struct StuTableInfo
    {
        public string strTableName;
        public string strKeyFieldName;
        public string strKeyFieldType;
        public string[] szStrFieldNameAndTypes;
        public List<string> lsIndexFieldNames;      // The extra index without PRIMARY KEY
        public bool bKeyNotNull;

        public StuTableInfo(string strParamTableName, string strParamKeyFieldName, string strParamKeyFieldType, List<string> lsParamIndexFieldNames, bool bParamKeyNotNull, params string[] szStrParamFieldNameAndTypes)
        {
            strTableName = strParamTableName;
            strKeyFieldName = strParamKeyFieldName;
            strKeyFieldType = strParamKeyFieldType;
            szStrFieldNameAndTypes = szStrParamFieldNameAndTypes;

            lsIndexFieldNames = null;
            if (null != lsParamIndexFieldNames)
            {
                lsIndexFieldNames = lsParamIndexFieldNames;
            }
            bKeyNotNull = bParamKeyNotNull;
        }
        public StuTableInfo(StuTableInfo obStuTableInfo)
        {
            strTableName = obStuTableInfo.strTableName;
            strKeyFieldName = obStuTableInfo.strKeyFieldName;
            strKeyFieldType = obStuTableInfo.strKeyFieldType;
            szStrFieldNameAndTypes = obStuTableInfo.szStrFieldNameAndTypes;
            lsIndexFieldNames = obStuTableInfo.lsIndexFieldNames;
            bKeyNotNull = obStuTableInfo.bKeyNotNull;
        }
    }

    public class SFBDBMgr : IDisposable, IPersistentStorage
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger("SFBDBMgr");
        #endregion

        #region SFB Table define
        private const string kstrVarcharFieldType = "varchar(255)";
        private const string kstrLongTxtFieldType = "text";     // MSSQL do not support longtext, using text for both, for MYSQL using longtext is better.

        #region  Common Configure Table
        private const string kstrTableNameCommonConfigure = "commonconfiguretable";
        private static readonly string[] kszStrCommonConfigureFieldNameAndTypes = new string[]
        {
            CommonConfigureInfo.kstrValueFieldName, kstrLongTxtFieldType,
        };
        private static readonly StuTableInfo kstuCommonConfigureTable = new StuTableInfo(kstrTableNameCommonConfigure, CommonConfigureInfo.kstrNameFieldName, kstrVarcharFieldType, null, true, kszStrCommonConfigureFieldNameAndTypes);
        #endregion

        #region SFB File Store Service table
        private const string kstrTableNameSFBFileStoreService = "sfbfilestoreservicetable";
        private static readonly string[] kszStrSFBFileStoreServiceFieldNameAndTypes = new string[]
        {
            SFBFileStoreServiceInfo.kstrShareNameFieldName, kstrVarcharFieldType,
            SFBFileStoreServiceInfo.kstrUNCPathFieldName, kstrLongTxtFieldType,
            SFBFileStoreServiceInfo.kstrIsDfsShareFieldName, kstrVarcharFieldType,
            SFBFileStoreServiceInfo.kstrDependentServiceListFieldName, kstrLongTxtFieldType,
            SFBFileStoreServiceInfo.kstrServiceIDFieldName, kstrVarcharFieldType,
            SFBFileStoreServiceInfo.kstrSiteIDFieldName, kstrVarcharFieldType,
            SFBFileStoreServiceInfo.kstrVersionFieldName, kstrVarcharFieldType,
            SFBFileStoreServiceInfo.kstrRoleFieldName, kstrVarcharFieldType,
        };
        private static readonly StuTableInfo kstuSFBFileStoreServiceTable = new StuTableInfo(kstrTableNameSFBFileStoreService, SFBFileStoreServiceInfo.kstrPoolFqdnFieldName, kstrVarcharFieldType, null, true, kszStrSFBFileStoreServiceFieldNameAndTypes);
        #endregion

        #region SFB Persistent chat server table
        private const string kstrTableNameSFBPersistentChatServer = "sfbpersistentchatservertable";
        private static readonly string[] kszStrSFBPersistentChatServerFieldNameAndTypes = new string[]
        {
            SFBPersistentChatServerInfo.kstrRegistrarFieldName, kstrVarcharFieldType,
            SFBPersistentChatServerInfo.kstrFileStoreFieldName, kstrVarcharFieldType,
            SFBPersistentChatServerInfo.kstrFileStoreUncPathFieldName, kstrVarcharFieldType,
            SFBPersistentChatServerInfo.kstrMtlsPortFieldName, kstrVarcharFieldType,
            SFBPersistentChatServerInfo.kstrDisplayNameFieldName, kstrVarcharFieldType,
            SFBPersistentChatServerInfo.kstrActiveServersFieldName, kstrLongTxtFieldType,
            SFBPersistentChatServerInfo.kstrInactiveServersFieldName, kstrLongTxtFieldType,
            SFBPersistentChatServerInfo.kstrDependentServiceListFieldName, kstrLongTxtFieldType,
            SFBPersistentChatServerInfo.kstrServiceIdFieldName, kstrVarcharFieldType,
            SFBPersistentChatServerInfo.kstrSiteIdFieldName, kstrVarcharFieldType,
            SFBPersistentChatServerInfo.kstrVersionFieldName, kstrVarcharFieldType,
            SFBPersistentChatServerInfo.kstrRoleFieldName, kstrVarcharFieldType
        };
        private static readonly StuTableInfo kstuSFBPersistentChatServerTable = new StuTableInfo(kstrTableNameSFBPersistentChatServer, SFBPersistentChatServerInfo.kstrPoolFqdnFieldName, kstrVarcharFieldType, null, true, kszStrSFBPersistentChatServerFieldNameAndTypes);
        #endregion

        #region SFB Chat Category table
        private const string kstrTableNameSFBChatCategory = "sfbchatcategorytable";
        private static readonly string[] kszStrSFBChatCategoryFieldNameAndTypes = new string[]
        {
            SFBChatCategoryInfo.kstrIdentityFieldName, kstrVarcharFieldType,
            SFBChatCategoryInfo.kstrNameFieldName, kstrVarcharFieldType,
            SFBChatCategoryInfo.kstrDescriptionFieldName, kstrVarcharFieldType,
            SFBChatCategoryInfo.kstrNumberOfChatRoomsFieldName, kstrVarcharFieldType,
            SFBChatCategoryInfo.kstrEnableInvitationsFieldName, kstrVarcharFieldType,
            SFBChatCategoryInfo.kstrEnableFileUploadFieldName, kstrVarcharFieldType,
            SFBChatCategoryInfo.kstrEnableChatHistoryFieldName, kstrVarcharFieldType,
            SFBChatCategoryInfo.kstrAllowedMembersFieldName, kstrLongTxtFieldType,
            SFBChatCategoryInfo.kstrDeniedMembersFieldName, kstrLongTxtFieldType,
            SFBChatCategoryInfo.kstrCreatorsFieldName, kstrLongTxtFieldType,
            SFBChatCategoryInfo.kstrPersistentChatPoolFieldName, kstrVarcharFieldType
        };
        private static readonly StuTableInfo kstuSFBChatCategoryTable = new StuTableInfo(kstrTableNameSFBChatCategory, SFBChatCategoryInfo.kstrUriFieldName, kstrVarcharFieldType, null, true, kszStrSFBChatCategoryFieldNameAndTypes);
        #endregion
        
        #region SFB Chat Room table
        private const string kstrTableNameSFBChatRoom = "sfbchatroomtable";
        private static readonly string[] kszStrSFBChatRoomFieldNameAndTypes = new string[]
        {
            SFBChatRoomInfo.kstrNameFieldName, kstrVarcharFieldType,
            SFBChatRoomInfo.kstrDescriptionFieldName, kstrVarcharFieldType,
            SFBChatRoomInfo.kstrPrivacyFieldName, kstrVarcharFieldType,
            SFBChatRoomInfo.kstrAddInFieldName, kstrVarcharFieldType,
            SFBChatRoomInfo.kstrCategoryUriFieldName, kstrVarcharFieldType,
            SFBChatRoomInfo.kstrMembersFieldName, kstrLongTxtFieldType,
            SFBChatRoomInfo.kstrManagersFieldName, kstrLongTxtFieldType,
            SFBChatRoomInfo.kstrEnableInvitationsFieldName, kstrVarcharFieldType,
            SFBChatRoomInfo.kstrCreateTimeFieldName, kstrVarcharFieldType,
            SFBChatRoomInfo.kstrPresentersFieldName, kstrLongTxtFieldType
        };
        private static readonly StuTableInfo kstuSFBChatRoomTable = new StuTableInfo(kstrTableNameSFBChatRoom, SFBChatRoomInfo.kstrUriFieldName, kstrVarcharFieldType, null, true, kszStrSFBChatRoomFieldNameAndTypes);

        private const string kstrTableNameSFBChatRoomVariable = "sfbchatroomvariabletable";
        private static readonly string[] kszStrSFBChatRoomVariableFieldNameAndTypes = new string[]
        {
            SFBChatRoomVariableInfo.kstrParticipatesFieldName, kstrLongTxtFieldType,
            SFBChatRoomVariableInfo.kstrClassifyTagsFieldName, kstrLongTxtFieldType,
            SFBChatRoomVariableInfo.kstrDoneManulClassifyFieldName, kstrVarcharFieldType,
            SFBChatRoomVariableInfo.kstrClassifyManagersFieldName, kstrLongTxtFieldType
        };
        private static readonly StuTableInfo kstuSFBChatRoomVariableTable = new StuTableInfo(kstrTableNameSFBChatRoomVariable, SFBChatRoomVariableInfo.kstrUriFieldName, kstrVarcharFieldType, null, true, kszStrSFBChatRoomVariableFieldNameAndTypes);
        #endregion
        
        #region SFB Meeting table
        private const string kstrTableNameSFBMeeting = "sfbmeetingtable";
        private static readonly string[] kszStrSFBMeetingFieldNameAndTypes = new string[]
        {
            SFBMeetingInfo.kstrCreatorFieldName, kstrVarcharFieldType,
            SFBMeetingInfo.kstrCreateTimeFieldName, kstrVarcharFieldType,
            SFBMeetingInfo.kstrEntryInformationFieldName, kstrVarcharFieldType,
            SFBMeetingInfo.kstrMeetingTypeFieldName, kstrVarcharFieldType,
            SFBMeetingInfo.kstrExpiryTimeFieldName, kstrVarcharFieldType
        };
        private static readonly StuTableInfo kstuSFBMeetingTable = new StuTableInfo(kstrTableNameSFBMeeting, SFBMeetingInfo.kstrUriFieldName, kstrVarcharFieldType, null, true, kszStrSFBMeetingFieldNameAndTypes);

        private const string kstrTableNameSFBMeetingVariable = "sfbmeetingvariabletable";
        private static readonly string[] kszStrSFBMeetingVariableFieldNameAndTypes = new string[]
        {
            SFBMeetingVariableInfo.kstrParticipatesFieldName, kstrLongTxtFieldType,
            SFBMeetingVariableInfo.kstrClassifyTagsFieldName, kstrLongTxtFieldType,
            SFBMeetingVariableInfo.kstrDoneManulClassifyFieldName, kstrVarcharFieldType,
            SFBMeetingVariableInfo.kstrClassifyManagersFieldName, kstrLongTxtFieldType
        };
        private static readonly StuTableInfo kstuSFBMeetingVariableTable = new StuTableInfo(kstrTableNameSFBMeetingVariable, SFBMeetingVariableInfo.kstrUriFieldName, kstrVarcharFieldType, null, true, kszStrSFBMeetingVariableFieldNameAndTypes);
        #endregion
        
        #region SFB Meeting Share table
        private const string kstrTableNameSFBMeetingShare = "sfbmeetingsharetable";
        private static readonly string[] kszStrSFBMeetingShareFieldNameAndTypes = new string[]
        {
            SFBMeetingShareInfo.kstrSharerFieldName, kstrVarcharFieldType
        };
        private static readonly StuTableInfo kstuSFBMeetingShareTable = new StuTableInfo(kstrTableNameSFBMeetingShare, SFBMeetingShareInfo.kstrShareUriFieldName, kstrVarcharFieldType, null, true, kszStrSFBMeetingShareFieldNameAndTypes);
        #endregion
        
        #region NL Chat Category table
        private const string kstrTableNameNLChatCategory = "nlchatcategorytable";
        private static readonly string[] kszStrNLChatCategoryFieldNameAndTypes = new string[]
        {
            NLChatCategoryInfo.kstrEnforcerFieldName, kstrVarcharFieldType,
            NLChatCategoryInfo.kstrForceEnforcerFieldName, kstrVarcharFieldType,
            NLChatCategoryInfo.kstrClassifyTagsFieldName, kstrVarcharFieldType,
            NLChatCategoryInfo.kstrClassificationFieldName, kstrLongTxtFieldType,
            NLChatCategoryInfo.kstrClassificationSchemaNameFieldName, kstrVarcharFieldType
        };
        private static readonly StuTableInfo kstuNLChatCategoryTable = new StuTableInfo(kstrTableNameNLChatCategory, NLChatCategoryInfo.kstrUriFieldName, kstrVarcharFieldType, null, true, kszStrNLChatCategoryFieldNameAndTypes);
        #endregion
        
        #region NL Chat Room table
        private const string kstrTableNameNLChatRoom = "nlchatroomtable";
        private static readonly string[] kszStrNLChatRoomFieldNameAndTypes = new string[]
        {
            NLChatRoomInfo.kstrEnforcerFieldName, kstrVarcharFieldType,
        };
        private static readonly StuTableInfo kstuNLChatRoomTable = new StuTableInfo(kstrTableNameNLChatRoom, NLChatRoomInfo.kstrUriFieldName, kstrVarcharFieldType, null, true, kszStrNLChatRoomFieldNameAndTypes);
        #endregion
        
        #region NL Meeting table
        private const string kstrTableNameNLMeeting = "nlmeetingtable";
        private static readonly string[] kszStrNLMeetingFieldNameAndTypes = new string[]
        {
            NLMeetingInfo.kstrEnforcerFieldName, kstrVarcharFieldType,
            NLMeetingInfo.kstrManulClassifyObsFieldName, kstrLongTxtFieldType,
            NLMeetingInfo.kstrForceManulClassifyFieldName, kstrVarcharFieldType
        };
        private static readonly StuTableInfo kstuNLMeetingTable = new StuTableInfo(kstrTableNameNLMeeting, NLMeetingInfo.kstrUriFieldName, kstrVarcharFieldType, null, true, kszStrNLMeetingFieldNameAndTypes);
        #endregion

        #region SFB User Info
        private const string kstrTableNameSFBUser = "sfbusertable";
        private static readonly string[] kszStrSFBUserFieldNameAndTypes = new string[]
        {
            SFBUserInfo.kstrFullName, kstrVarcharFieldType,
            SFBUserInfo.kstrDisplayName, kstrVarcharFieldType,
            SFBUserInfo.kstrRegistrarPool, kstrVarcharFieldType,
            SFBUserInfo.kstrSamAccountName, kstrVarcharFieldType,
            SFBUserInfo.kstrUserIdentity, kstrVarcharFieldType
        };
        private static readonly List<string> klsStrSFBUserIndexFieldNames = new List<string>()
        {
            SFBUserInfo.kstrDisplayName
        };
        private static readonly StuTableInfo kstuSFBUserTable = new StuTableInfo(kstrTableNameSFBUser, SFBUserInfo.kstrSipAddress, kstrVarcharFieldType, klsStrSFBUserIndexFieldNames, true, kszStrSFBUserFieldNameAndTypes);
        #endregion

        #region NL Assistant Service Info
        private const string kstrTableNameNLAssistantService = "NLAssistantServiceTable";
        private static readonly string[] kszStrNLAssistantServiceFieldNameAndTypes = new string[]
        {
            NLAssistantServiceInfo.kstrAssistantTokenFieldName, kstrLongTxtFieldType
        };
        private static readonly StuTableInfo kstuNLAssistantServiceTable = new StuTableInfo(kstrTableNameNLAssistantService, NLAssistantServiceInfo.kstrUriFieldName, kstrVarcharFieldType, null, true, kszStrNLAssistantServiceFieldNameAndTypes);
        #endregion

        #region NL Assistant Query Policy Info
        private const string kstrTableNameNLAssistantQueryPolicyIdentify = "NLAssistantQueryPolicyIdentifyTable";
        private static readonly string[] kszStrNLAssistantQueryPolicyIdentifyFieldNameAndTypes = new string[]
        {
            NLAssistantQueryPolicyInfo.kstrResponseStatusFieldName, kstrVarcharFieldType,
            NLAssistantQueryPolicyInfo.kstrResponseInfoFieldName, kstrLongTxtFieldType,
        };
        private static readonly StuTableInfo kstuNLAssistantQueryPolicyIdentifyTable = new StuTableInfo(kstrTableNameNLAssistantQueryPolicyIdentify, NLAssistantQueryPolicyInfo.kstrRequestIdentifyFieldName, kstrVarcharFieldType, null, true, kszStrNLAssistantQueryPolicyIdentifyFieldNameAndTypes);
        #endregion
      
        #region Chat Room Attachment Info
        private const string kstrTableNameSFBChatRoomAttachmentEntryVariable = "SFBChatRoomAttachmentEntryVariabeTable";
        private static readonly string[] kszStrSFBChatRoomAttachmentEntryVariableFieldNameAndTypes = new string[]
        {
            SFBChatRoomAttachmentEntryVariableInfo.kstrChatRoomUriFieldName, kstrVarcharFieldType,
            SFBChatRoomAttachmentEntryVariableInfo.kstrUserFieldName, kstrVarcharFieldType,
            SFBChatRoomAttachmentEntryVariableInfo.kstrActionFieldName, kstrVarcharFieldType,
            SFBChatRoomAttachmentEntryVariableInfo.kstrFileOrgNameFieldName, kstrVarcharFieldType
        };
        private static readonly StuTableInfo kstuSFBChatRoomAttachmentEntryVariabeTable = new StuTableInfo(kstrTableNameSFBChatRoomAttachmentEntryVariable,
            SFBChatRoomAttachmentEntryVariableInfo.kstrTokenIDFieldName, kstrVarcharFieldType, null, true, kszStrSFBChatRoomAttachmentEntryVariableFieldNameAndTypes);

        private const string kstrTableNameNLChatRoomAttachment = "NLChatRoomAttachmentTable";
        private static readonly string[] kszStrNLChatRoomAttachmentFieldNameAndTypes = new string[]
        {
            NLChatRoomAttachmentInfo.kstrFilePathFieldName, kstrLongTxtFieldType,
            NLChatRoomAttachmentInfo.kstrFileTagsFieldName, kstrLongTxtFieldType,
            NLChatRoomAttachmentInfo.kstrFileOwnerFieldName, kstrVarcharFieldType,
            NLChatRoomAttachmentInfo.kstrChatRoomUriFieldName, kstrVarcharFieldType,
            NLChatRoomAttachmentInfo.kstrLastModifyTimeFieldName, kstrVarcharFieldType,
            NLChatRoomAttachmentInfo.kstrFileOrgNameFieldName, kstrVarcharFieldType
        };
        private static readonly StuTableInfo kstuNLChatRoomAttachmentTable = new StuTableInfo(kstrTableNameNLChatRoomAttachment,
            NLChatRoomAttachmentInfo.kstrAttachmentUniqueFlagFieldName, kstrVarcharFieldType, null, true, kszStrNLChatRoomAttachmentFieldNameAndTypes);
        #endregion

        #region NL Tag Label
        private const string kstrTableNameNLTagLabel = "NLTagLabelTable";
        private static readonly string[] kszStrNLTagLabelFieldNameAndTypes = new string[]
        {
            NLTagLabelInfo.kstrTagValuesFieldName, kstrLongTxtFieldType,
            NLTagLabelInfo.kstrDefaultValueFieldName, kstrVarcharFieldType,
            NLTagLabelInfo.kstrEditableFieldName, kstrVarcharFieldType,
            NLTagLabelInfo.kstrMultiSelectFieldName, kstrVarcharFieldType,
            NLTagLabelInfo.kstrMandatoryFieldName, kstrVarcharFieldType,
            NLTagLabelInfo.kstrDeprecatedFieldName, kstrVarcharFieldType
        };
        private static readonly StuTableInfo kstuNLTagLabelTable = new StuTableInfo(kstrTableNameNLTagLabel,
            NLTagLabelInfo.kstrTagNameFieldName, kstrVarcharFieldType, null, true, kszStrNLTagLabelFieldNameAndTypes);
        #endregion

        #region NL Classification Schema
        private const string kstrTableNameNLClassificationSchema = "NLClassificationSchemaTable";
        private static readonly string[] kszStrNLClassificationSchemaFieldNameAndTypes = new string[]
        {
            NLClassificationSchemaInfo.kstrDataFieldName, kstrLongTxtFieldType,
            NLClassificationSchemaInfo.kstrDescriptionFieldName, kstrLongTxtFieldType,
            NLClassificationSchemaInfo.kstrDeprecatedFieldName, kstrVarcharFieldType
        };
        private static readonly StuTableInfo kstuNLClassificationSchemaTable = new StuTableInfo(kstrTableNameNLClassificationSchema,
            NLClassificationSchemaInfo.kstrSchemaNameFieldName, kstrVarcharFieldType, null, true, kszStrNLClassificationSchemaFieldNameAndTypes);
        #endregion

        public static readonly Dictionary<EMSFB_INFOTYPE, StuTableInfo> kdirTableNames = new Dictionary<EMSFB_INFOTYPE,StuTableInfo>()
        {
            { EMSFB_INFOTYPE.emInfoCommonConfigure,         kstuCommonConfigureTable},
            { EMSFB_INFOTYPE.emInfoSFBChatCategory,         kstuSFBChatCategoryTable},
            { EMSFB_INFOTYPE.emInfoSFBFileStoreService,     kstuSFBFileStoreServiceTable},
            { EMSFB_INFOTYPE.emInfoSFBPersistentChatServer, kstuSFBPersistentChatServerTable},
            { EMSFB_INFOTYPE.emInfoSFBChatRoom,             kstuSFBChatRoomTable},
            { EMSFB_INFOTYPE.emInfoSFBChatRoomVariable,     kstuSFBChatRoomVariableTable},
            { EMSFB_INFOTYPE.emInfoSFBMeeting,              kstuSFBMeetingTable},
            { EMSFB_INFOTYPE.emInfoSFBMeetingVariable,      kstuSFBMeetingVariableTable},
            { EMSFB_INFOTYPE.emInfoSFBMeetingShare,         kstuSFBMeetingShareTable},
            { EMSFB_INFOTYPE.emInfoNLChatCategory,          kstuNLChatCategoryTable},
            { EMSFB_INFOTYPE.emInfoNLChatRoom,              kstuNLChatRoomTable},
            { EMSFB_INFOTYPE.emInfoNLMeeting,               kstuNLMeetingTable},
            { EMSFB_INFOTYPE.emInfoSFBUserInfo,             kstuSFBUserTable},
            { EMSFB_INFOTYPE.emInfoNLAssistantServiceInfo,  kstuNLAssistantServiceTable },
            { EMSFB_INFOTYPE.emInfoNLAssistantQueryPolicyInfo,  kstuNLAssistantQueryPolicyIdentifyTable },
            { EMSFB_INFOTYPE.emInfoSFBChatRoomAttachmentEntryVariable,  kstuSFBChatRoomAttachmentEntryVariabeTable },
            { EMSFB_INFOTYPE.emInfoNLChatRoomAttachment,  kstuNLChatRoomAttachmentTable },
            { EMSFB_INFOTYPE.emInfoNLTagLabel,  kstuNLTagLabelTable },
            { EMSFB_INFOTYPE.emInfoNLClassificationSchema,  kstuNLClassificationSchemaTable }
        };
        #endregion

        #region Init SFB Database flag
        static private bool m_bInitSFBDatabase = false;
        static private bool GetInitSFBDatabaseFlag() { return m_bInitSFBDatabase; }
        static private void SetInitSFBDatabaseFlag(bool bInitSFBDatabase) { m_bInitSFBDatabase = bInitSFBDatabase; }
        #endregion

        #region Members
        private EMSFB_DBTYPE m_emDBType = EMSFB_DBTYPE.emDBTypeUnknown;
        private AbstractDBOpHelper m_DBHelper = null;
        private bool m_bEstablishSFBMgrSuccess = false;
        #endregion

        #region Constructors
        public SFBDBMgr(string strDBServerAddr, uint unPortNumber, string strCatalogName, string strUserName, string strPassword, EMSFB_DBTYPE emDBType = EMSFB_DBTYPE.emDBTypeUnknown)
        {
            SetEstablishSFBMgrFlag(false);
            m_emDBType = emDBType;
            switch (emDBType)
            {
            case EMSFB_DBTYPE.emDBTypeMSSQL:
            {
                m_DBHelper = new DBOpMSSQLHelper(strDBServerAddr, unPortNumber, strCatalogName, strUserName, strPassword);
                break;
            }
            case EMSFB_DBTYPE.emDBTypeMYSQL:
            {
                m_DBHelper = new DBOpMYSQLHelper(strDBServerAddr, unPortNumber, strCatalogName, strUserName, strPassword);
                break;
            }
            default:
            {
                m_emDBType = EMSFB_DBTYPE.emDBTypeMYSQL;
                m_DBHelper = new DBOpMYSQLHelper(strDBServerAddr, unPortNumber, strCatalogName, strUserName, strPassword);
                break;
            }
            }
            if (m_DBHelper.GetEstablishConnectionFlag())
            {
                InitSFBDatabase();  // this function only need invoke once, if the database address is incorrect this will be take a lot of time
                SetEstablishSFBMgrFlag(true);
            }
            else
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
            }
        }
        #endregion

        #region Interface: IDisposable
        public void Dispose()
        {
            m_DBHelper.Dispose();
        }
        #endregion

        #region Interface: IPersistantStorage
        public Dictionary<string, string>[] GetAllObjInfo(EMSFB_INFOTYPE emInfoType)
        {
            Dictionary<string, string>[] szDirAllObjInfo = null;
            if (GetEstablishSFBMgrFlag())
            {
                StuTableInfo obStuTableInfo = kdirTableNames[emInfoType];

                DataTable obDataTable = m_DBHelper.SelectItem<string>(obStuTableInfo.strTableName, false, false);
                if (null != obDataTable)
                {
                    szDirAllObjInfo = ConvertDataTableToDictionary(obDataTable);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "user try to get obj info for database but failed:[{0}], it maybe no value or read failed", LastErrorRecorder.GetLastError());
                }
            }
            else
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error: The SFBMgr establish flag is false, please check you  database!");
            }
            return szDirAllObjInfo;
        }
        public Dictionary<string, string>[] GetObjInfoEx(EMSFB_INFOTYPE emInfoType, string strFieldName, params string[] szStrValues)
        {
            Dictionary<string, string>[] szDirAllObjInfo = null;
            if (GetEstablishSFBMgrFlag())
            {
                StuTableInfo obStuTableInfo = kdirTableNames[emInfoType];
                if ((!string.IsNullOrEmpty(strFieldName)) && (null != szStrValues))
                {
                    int nValueLength = szStrValues.Length;
                    string[] szStrFieldKeyAndValues = new string[nValueLength * 2];
                    for (int i = 0; i < nValueLength; ++i)
                    {
                        szStrFieldKeyAndValues[(2 * i)] = strFieldName;
                        szStrFieldKeyAndValues[(2 * i) + 1] = szStrValues[i];
                    }
                    DataTable obDataTable = m_DBHelper.SelectItem(obStuTableInfo.strTableName, false, false, szStrFieldKeyAndValues);
                    if (null != obDataTable)
                    {
                        szDirAllObjInfo = ConvertDataTableToDictionary(obDataTable);
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "user try to get obj info for database but failed:[{0}]. it maybe a wrong read operation or read out wrong data\n", LastErrorRecorder.GetLastError());
                    }
                }
                else
                {
                    LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_KEY_INCORRECT);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error in GetObjInfo, user try to get obj info but pass in a incorrect field name\n");
                }
            }
            else
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error: The SFBMgr establish flag is false, please check you  database!");
            }
            return szDirAllObjInfo;
        }
        public Dictionary<string, string> GetObjInfo(EMSFB_INFOTYPE emInfoType, string strKey, string strValue) // Return null => select failed, return new Dictionary<string,string>() => no value
        {
            Dictionary<string, string> obDirInfo = null;
            if (GetEstablishSFBMgrFlag())
            {
                StuTableInfo obStuTableInfo = kdirTableNames[emInfoType];
                if (obStuTableInfo.strKeyFieldName.Equals(strKey, StringComparison.OrdinalIgnoreCase))
                {
                    DataTable obDataTable = m_DBHelper.SelectItem(obStuTableInfo.strTableName, false, false, strKey, strValue);
                    if ((null != obDataTable))
                    {
                        obDirInfo = new Dictionary<string,string>();
                        if (1 == obDataTable.Rows.Count)
                        {
                            Dictionary<string, string>[] szDirAllObjInfo = ConvertDataTableToDictionary(obDataTable);
                            if ((null != szDirAllObjInfo) && (1 == szDirAllObjInfo.Length))
                            {
                                obDirInfo = szDirAllObjInfo[0];
                            }
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "user try to get obj info for database but failed:[{0}]. it maybe a wrong read operation\n", LastErrorRecorder.GetLastError());
                    }
                }
                else
                {
                    LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_KEY_INCORRECT);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error in GetObjInfo, user try to get obj info but pass in a incorrect key\n");
                }
            }
            else
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error: The SFBMgr establish flag is false, please check you  database!");
            }
            return obDirInfo;
        }
        public Dictionary<string, string>[] GetObjInfoByLikeValue(EMSFB_INFOTYPE emInfoType, string strKeyFieldName, string strLikeValue)
        {
            Dictionary<string, string>[] szDirAllObjInfo = null;
            if (GetEstablishSFBMgrFlag())
            {
                StuTableInfo obStuTableInfo = kdirTableNames[emInfoType];
                if ((!string.IsNullOrEmpty(strKeyFieldName)) && obStuTableInfo.strKeyFieldName.Equals(strKeyFieldName, StringComparison.OrdinalIgnoreCase) && (!string.IsNullOrEmpty(strLikeValue)))
                {
                    DataTable obDataTable = m_DBHelper.SelectItem(obStuTableInfo.strTableName, false, true, strKeyFieldName, strLikeValue);
                    if (null != obDataTable)
                    {
                        szDirAllObjInfo = ConvertDataTableToDictionary(obDataTable);
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "user try to GetObjInfoByLikeValue from database failed:[{0}]. it maybe a wrong read operation or read out wrong data\n", LastErrorRecorder.GetLastError());
                    }
                }
                else
                {
                    LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_KEY_INCORRECT);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error in GetObjInfo, user try to get obj info but pass in a incorrect field name\n");
                }
            }
            else
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error: The SFBMgr establish flag is false, please check you  database!");
            }
            return szDirAllObjInfo;
        }
        public bool SaveObjInfo(EMSFB_INFOTYPE emInfoType, Dictionary<string, string> dirParamInfos)
        {
            bool bRet = false;
            if (GetEstablishSFBMgrFlag())
            {
                StuTableInfo obStuTableInfo = kdirTableNames[emInfoType];
                if (dirParamInfos.Keys.Contains(obStuTableInfo.strKeyFieldName))
                {
                    Dictionary<string, string> dirInfos = new Dictionary<string,string>(dirParamInfos); // Clone
                 
                    string strKeyFieldValue = dirInfos[obStuTableInfo.strKeyFieldName];
                    dirInfos.Remove(obStuTableInfo.strKeyFieldName);
                    KeyValuePair<string, string>[] szFieldKeyAndValues = dirInfos.ToArray();
                    bRet = m_DBHelper.AddItem(obStuTableInfo.strTableName, obStuTableInfo.strKeyFieldName, strKeyFieldValue, szFieldKeyAndValues);
                    if (!bRet)
                    {
                        LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DATA_WRITE_FAILED);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error in SaveObjInfo, user try to save obj info but failed\n");
                    }
                }
                else
                {
                    LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_KEY_NOT_EXIST);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Error in SaveObjInfo, user try to save obj info but do not contains the key value\n");
                }
            }
            else
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error: The SFBMgr establish flag is false, please check you  database!");
            }
            return bRet;
        }
        public bool DeleteObjInfo(EMSFB_INFOTYPE emInfoType, string strKeyValue)
        {
            bool bRet = false;
            if (GetEstablishSFBMgrFlag())
            {
                StuTableInfo obStuTableInfo = kdirTableNames[emInfoType];
                if (String.IsNullOrEmpty(strKeyValue))
                {
                    LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DATA_WRITE_FAILED);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error in DeleteObjInfo, user try to delete obj info but the specify key value is empty\n");
                }
                else
                {
                    bRet = m_DBHelper.DeleteItem(obStuTableInfo.strTableName, obStuTableInfo.strKeyFieldName, strKeyValue);
                    if (!bRet)
                    {
                        LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DATA_WRITE_FAILED);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error in DeleteObjInfo, user try to delete obj info but failed\n");
                    }
                }
            }
            else
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error: The SFBMgr establish flag is false, please check you  database!");
            }
            return bRet;
        }
        // lsSpecifyOutFields:
        //      1. can be null or empty means return all
        //      2. do not suggest to using null or empty
        //      3. if some filed is same in difference tables only return the last one
        // lsSearchScopes cannot be null or empty
        // lsSearchConditions cannot be null or empty
        public Dictionary<EMSFB_INFOTYPE, Dictionary<string, string>>[] GetObjInfoWithFullSearchConditions(List<STUSFB_INFOFIELD> lsSpecifyOutFields, List<EMSFB_INFOTYPE> lsSearchScopes, List<KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>> lsSearchConditions)
        {
            try
            {
                if (GetEstablishSFBMgrFlag())
                {
                    // Check parameters
                    if (((null != lsSearchScopes) && (0 < lsSearchScopes.Count)) && ((null != lsSearchConditions) && (0 < lsSearchConditions.Count)))
                    {
                        // Establish output filed string
                        DataTable obDataTable = m_DBHelper.SelectItemEx(lsSpecifyOutFields, lsSearchScopes, lsSearchConditions);
                        if (null != obDataTable)
                        {
                            // If lsSpecifyOutFields is null or empty and obDataTable contains the same clomun value, only return the last one
                            return ConvertDataTableToDictionaryArray(obDataTable, lsSpecifyOutFields);
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "user try to GetObjInfoByLikeValue from database failed:[{0}]. it maybe a wrong read operation or read out wrong data\n", LastErrorRecorder.GetLastError());
                        }
                    }
                    else
                    {
                        LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_KEY_INCORRECT);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error in GetObjInfoWithFullSearchConditions, user try to get obj info but pass in a incorrect field name\n");
                    }
                }
                else
                {
                    LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error in GetObjInfoWithFullSearchConditions: The SFBMgr establish flag is false, please check you  database!");
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!!Exception in GetObjInfoWithFullSearchConditions, [{0}]\n", ex.Message);
            }
            return null;
        }
        #endregion

        #region SFB Database
        private void InitSFBDatabase()
        {
            if (!GetInitSFBDatabaseFlag())
            {
                // Create table
                foreach (var obItem in kdirTableNames)
                {
                    StuTableInfo obStuTableInfo = obItem.Value;
                    m_DBHelper.CreateTable(true, obStuTableInfo.bKeyNotNull, obStuTableInfo.strTableName, obStuTableInfo.strKeyFieldName, obStuTableInfo.strKeyFieldType, obStuTableInfo.szStrFieldNameAndTypes);

                    // Check columns
                    for (int i=1; i<obStuTableInfo.szStrFieldNameAndTypes.Length; i += 2)
                    {
                        bool bAddRet = m_DBHelper.AddColumn(obStuTableInfo.strTableName, obStuTableInfo.szStrFieldNameAndTypes[i-1], obStuTableInfo.szStrFieldNameAndTypes[i]);
                        if (!bAddRet)
                        {
                            // Add failed, maybe the column is already exist, here to make sure the value type is correct.
                            m_DBHelper.ModifyColumnType(obStuTableInfo.strTableName, obStuTableInfo.szStrFieldNameAndTypes[i - 1], obStuTableInfo.szStrFieldNameAndTypes[i]);
                        }
                    }

                    // Add Index
                    if (null != obStuTableInfo.lsIndexFieldNames)
                    {
                        foreach (string strFieldName in obStuTableInfo.lsIndexFieldNames)
                        {
                            if ((!string.IsNullOrEmpty(strFieldName)) && (!strFieldName.Equals(obStuTableInfo.strKeyFieldName, StringComparison.OrdinalIgnoreCase)))
                            {
                                m_DBHelper.AddIndex(obStuTableInfo.strTableName, strFieldName);
                            }
                        }
                    }
                }

                // Init common configure table
                foreach (KeyValuePair<string, string> pairItem in CommonConfigureInfo.szPairInitValues)
                {
                    DataTable obDataTable = m_DBHelper.SelectItem(kstrTableNameCommonConfigure, false, false, CommonConfigureInfo.kstrNameFieldName, pairItem.Key);
                    if (null != obDataTable)
                    {
                        if (0 == obDataTable.Rows.Count) // obDataTable means query database failed
                        {
                            m_DBHelper.AddItem(kstrTableNameCommonConfigure, CommonConfigureInfo.kstrNameFieldName, pairItem.Key, CommonConfigureInfo.kstrValueFieldName, pairItem.Value);
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "The  table:{0} already initialized", kstrTableNameCommonConfigure);
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!!After init database but try to query data in {0} table failed\n", kstrTableNameCommonConfigure);
                    }
                }
                SetInitSFBDatabaseFlag(true);
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "The SFB database already initialized\n");
            }
        }
        private void UnInitSFBDatabase()
        {
#if false   // Back up, no need delete table
            // Delete table
            foreach (var obItem in kdirTableNames)
            {
                StuTableInfo obStuTableInfo = obItem.Value;
                m_DBHelper.DeleteTable(m_emDBType, true, obStuTableInfo.strTableName);
            }
#endif
            SetInitSFBDatabaseFlag(false);
        }
        private void SetEstablishSFBMgrFlag(bool bEstablishSFBMgrSuccess) { m_bEstablishSFBMgrSuccess = bEstablishSFBMgrSuccess; }
        public bool GetEstablishSFBMgrFlag() { return m_bEstablishSFBMgrSuccess; }
        #endregion

        #region Private tools
        private Dictionary<string, string>[] ConvertDataTableToDictionary(DataTable obDataTable)
        {
            Dictionary<string, string>[] szDicAllObjInfo = null;
            if (null != obDataTable)
            {
                int nRowsCount = obDataTable.Rows.Count;
                szDicAllObjInfo = new Dictionary<string, string>[nRowsCount];
                DataColumnCollection obDataColumns = obDataTable.Columns;
                for (int nRow = 0; nRow < nRowsCount; ++nRow)
                {
                    DataRow obDataRow = obDataTable.Rows[nRow];
                    szDicAllObjInfo[nRow] = new Dictionary<string, string>();
                    for (int nColumn = 0; nColumn < obDataColumns.Count; ++nColumn)
                    {
                        string strCurColumnValue = obDataRow.ItemArray[nColumn] as string;
                        if (null == strCurColumnValue)
                        {
                            strCurColumnValue = obDataRow.ItemArray[nColumn].ToString();
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!! Current column:{0}:{1} is not string type or is empty", obDataColumns[nColumn].ColumnName, strCurColumnValue);
                        }
                        szDicAllObjInfo[nRow].Add(obDataColumns[nColumn].ColumnName, strCurColumnValue);
                    }
                }
            }
            return szDicAllObjInfo;
        }
        private Dictionary<EMSFB_INFOTYPE, Dictionary<string, string>>[] ConvertDataTableToDictionaryArray(DataTable obDataTable, List<STUSFB_INFOFIELD> lsSpecifyOutFields)
        {
            Dictionary<EMSFB_INFOTYPE, Dictionary<string, string>>[] szDicAllObjInfo = null;
            if (null != obDataTable)
            {
                int nRowsCount = obDataTable.Rows.Count;
                DataColumnCollection obDataColumns = obDataTable.Columns;
 
                    szDicAllObjInfo = new Dictionary<EMSFB_INFOTYPE,Dictionary<string,string>>[nRowsCount];
                    for (int nRow = 0; nRow < nRowsCount; ++nRow)
                    {
                        szDicAllObjInfo[nRow] = new Dictionary<EMSFB_INFOTYPE,Dictionary<string,string>>();
                        DataRow obDataRow = obDataTable.Rows[nRow];
                        for (int nColumn = 0; nColumn < obDataColumns.Count; ++nColumn)
                        {
                            STUSFB_INFOFIELD stuCurOutField = GetInfoFiedFormList(lsSpecifyOutFields, nColumn);
                            string strCurOutFieldName = stuCurOutField.strField;
                            string strCurColumnName = obDataColumns[nColumn].ColumnName;
                            if ((!string.IsNullOrEmpty(strCurOutFieldName)) && (!strCurOutFieldName.Equals(strCurColumnName, StringComparison.OrdinalIgnoreCase)))  // Out field name is null or empty means get all columns
                            {
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!! Attention:Current out field name:[{0}] is not the same as the column name:[{1}], please check, maybe something error.", strCurOutFieldName, strCurColumnName);
                            }

                            EMSFB_INFOTYPE emCurInfoType;//it'd be better to retrive table name from obDataTable.TableName property
                            bool isSuccess = Enum.TryParse<EMSFB_INFOTYPE>(obDataTable.TableName, out emCurInfoType);
                            if (!isSuccess)
                            {
                                emCurInfoType = EMSFB_INFOTYPE.emInfoUnknown;
                            }
                            string strCurColumnValue = obDataRow.ItemArray[nColumn] as string;
                            if (null == strCurColumnValue)
                            {
                                strCurColumnValue = obDataRow.ItemArray[nColumn].ToString();
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Attention: current column:{0}:{1} is not string type or is empty", strCurColumnName, strCurColumnValue);
                            }
                            AddDateTableItemInfo(szDicAllObjInfo[nRow], emCurInfoType, strCurColumnName, strCurColumnValue);
                        }
                    }
            }
            return szDicAllObjInfo;
        }
        private void AddDateTableItemInfo(Dictionary<EMSFB_INFOTYPE, Dictionary<string, string>> dicAllObjInfo, EMSFB_INFOTYPE emTableInfoType, string strColumnName, string strColumnValue)
        {
            Dictionary<string,string> dicCurInfo = CommonHelper.GetValueByKeyFromDir(dicAllObjInfo, emTableInfoType, new Dictionary<string,string>());
            if (null != dicCurInfo)
            {
                CommonHelper.AddKeyValuesToDir(dicCurInfo, strColumnName, strColumnValue);
                CommonHelper.AddKeyValuesToDir(dicAllObjInfo, emTableInfoType, dicCurInfo);
            }
        }
        private STUSFB_INFOFIELD GetInfoFiedFormList(List<STUSFB_INFOFIELD> lsSpecifyOutFields, int nIndex)
        {
            if (null != lsSpecifyOutFields)
            {
                if ((0 <= nIndex) && (nIndex < lsSpecifyOutFields.Count))
                {
                    return lsSpecifyOutFields[nIndex];
                }
            }

            //if lsSpecifyOutFields is null, all fields will be selected in function AbstractDBOpHelpe.GetSearchOutFiledsPartSQLCommand().
            //But the STUSFB_INFOFIELD object here returned with EMSFB_INFOTYPE.emInfoUnknown & empty field, resulting an inconsistent behavior with GetSearchOutFiledsPartSQLCommand function
            return new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoUnknown, "");
        }
        #endregion
    }
}
