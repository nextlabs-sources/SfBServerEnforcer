using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;
using SFBCommon.NLLog;
using SFBCommon.SFBObjectInfo;
using SFBCommon.ClassifyHelper;

namespace SFBControlPanel
{
    class ChatCategorySetting : Logger
    {
        #region Constr/Read only info
        static private readonly bool kbDefaultEnforcer = false;
        static private readonly bool kbDefaultForceInheritEnforcer = true;

        private const string kstrWildcardStartFlag = "\\";
        private const string kstrWildcardEndFlag = ";";
        private const string kstrWildcardOrgClassificationSchema = "ORGCLASSIFICATIONSCHEMANAME";
        #endregion

        #region Static constructor
        static ChatCategorySetting()
        {
            CommonCfgMgr commonCfgMgr = CommonCfgMgr.GetInstance();
            kbDefaultEnforcer = commonCfgMgr.DefaultChatRoomCategoryEnforcer;
            kbDefaultForceInheritEnforcer = commonCfgMgr.DefaultForceInheritChatCategoryEnforcer;       
            
        }
        #endregion

        #region Members
        private EMSFB_RUNTIMESTATUS m_emRuntimeStatus = EMSFB_RUNTIMESTATUS.emUnknownError;

        // Runtime setting
        private string m_strUri = "";
        private bool m_bApplied =false;

        // NLSetting
        private bool m_bEnforcer = kbDefaultEnforcer;
        private bool m_bForceEnforcer = kbDefaultForceInheritEnforcer;
        private string m_strClassficationDisplayName = "";
        private string m_strClassificationOrgName = "";
        private string m_strClassificationOrgInfo = "";

        private string m_strClassificationWarningInfo = "";
        private Dictionary<string, string> m_dicWildcardClassificationWarning = new Dictionary<string,string>()
        {
            {kstrWildcardOrgClassificationSchema, ""}
        };
        #endregion

        #region Constructors
        public ChatCategorySetting(string strParamUri, bool bParamApplied)
        {
            try
            {
                InitRuntimeSetting(strParamUri, bParamApplied);
                m_emRuntimeStatus = InitNLSettingInfoByUri(strParamUri);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! Init ChatCategorySetting failed, please check.{0}\n", ex.Message);
            }
        }
        #endregion

        #region Public functions
        public EMSFB_RUNTIMESTATUS GetRuntimeStatus() { return m_emRuntimeStatus; }

        public bool GetAppliedFlag() { return m_bApplied; }
        public void SetAppliedFlag(bool bApplied) { m_bApplied = bApplied; }

        public string GetUri() { return m_strUri; }

        public bool IsNeedEnforcer() { return m_bEnforcer; }
        public bool IsNeedForceEnforcer() { return m_bForceEnforcer; }
        public string GetClassficationDisplayName() { return m_strClassficationDisplayName; }
        public string GetClassificationWarningInfo() { return m_strClassificationWarningInfo; }
        public string GetClassficationOrgName() { return m_strClassificationOrgName; }
        public string GetClassificationInfo()
        {
            return ManulClassifyObligationHelper.GetClassificationInfoByName(m_strClassficationDisplayName, true);
        }

        public void UpdateSettingInfo(bool bParamEnforcer, bool bParamForceEnforcer, string strParamClassficationDisplayName)
        {
            m_bEnforcer = bParamEnforcer;
            m_bForceEnforcer = bParamForceEnforcer;
            m_strClassficationDisplayName = strParamClassficationDisplayName;

        }
        public void UpdateSettingInfo(string strParamEnforcer, string strParamForceEnforcer,string strParamClassficationDisplayName)
        {
            try
            {
                m_bEnforcer = CommonHelper.ConverStringToBoolFlag(strParamEnforcer, kbDefaultEnforcer);
                m_bForceEnforcer = CommonHelper.ConverStringToBoolFlag(strParamForceEnforcer, kbDefaultForceInheritEnforcer);
                m_strClassficationDisplayName = strParamClassficationDisplayName;
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! Init ChatCategorySetting failed, please check\n", ex.Message);
            }
        }
        #endregion

        #region Inner tools
        private EMSFB_RUNTIMESTATUS InitNLSettingInfoByUri(string strUri)
        {
                            EMSFB_RUNTIMESTATUS emRuntimeStatus = EMSFB_RUNTIMESTATUS.emUnknownError;
            try
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "UriFieldName:[{0}], Uri:[{1}]\n", NLChatCategoryInfo.kstrUriFieldName, strUri);

                NLChatCategoryInfo obNLChatCategoryInfo = new NLChatCategoryInfo();
                bool bEstablished = obNLChatCategoryInfo.EstablishObjFormPersistentInfo(NLChatCategoryInfo.kstrUriFieldName, strUri);
                if (bEstablished)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "1 Establish NLChatCategoryInfo object with uri:[{0}] success\n", strUri);

                    string strEnforcer = obNLChatCategoryInfo.GetItemValue(NLChatCategoryInfo.kstrEnforcerFieldName);
                    string strForceEnforcer = obNLChatCategoryInfo.GetItemValue(NLChatCategoryInfo.kstrForceEnforcerFieldName);

                    m_strClassificationOrgName = obNLChatCategoryInfo.GetItemValue(NLChatCategoryInfo.kstrClassificationSchemaNameFieldName);
                    m_strClassificationOrgInfo = obNLChatCategoryInfo.GetItemValue(NLChatCategoryInfo.kstrClassificationFieldName);

                    string strCurClassificationSchemaInfo = ManulClassifyObligationHelper.GetClassificationInfoByName(m_strClassificationOrgName, true);
                    if ((string.IsNullOrWhiteSpace(m_strClassificationOrgName)) ||
                        (string.IsNullOrWhiteSpace(m_strClassificationOrgInfo)) ||
                        (string.IsNullOrWhiteSpace(strCurClassificationSchemaInfo)) ||
                        (!string.Equals(strCurClassificationSchemaInfo, m_strClassificationOrgInfo, StringComparison.OrdinalIgnoreCase))
                       )
                    {
                        UpdateSettingInfo(strEnforcer, strForceEnforcer, "");
                    }
                    else
                    {
                        UpdateSettingInfo(strEnforcer, strForceEnforcer, m_strClassificationOrgName);
                    }
                    InitClassificationWarningInfo();
                    
                    emRuntimeStatus = EMSFB_RUNTIMESTATUS.emSuccess;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "222 Establish NLChatCategoryInfo object with uri:[{0}], status:[{1}] success\n", strUri, emRuntimeStatus);
                }
                else
                {
                    int nLastError = LastErrorRecorder.GetLastError();
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Establish NLChatCategoryInfo object with uri:[{0}] do not success, the last error is:[{1}]\n", strUri, nLastError);
                    if (LastErrorRecorder.ERROR_SUCCESS != nLastError)  // if nLastError is ERROR_SUCCESS, it means select item in database success but the item do not exist.
                    {
                        if (LastErrorRecorder.ERROR_DBCONNECTION_FAILED == nLastError)
                        {
                            emRuntimeStatus = EMSFB_RUNTIMESTATUS.emDBConnectionError;
                        }
                        else if (LastErrorRecorder.ERROR_DATA_READ_FAILED == nLastError)
                        {
                            emRuntimeStatus = EMSFB_RUNTIMESTATUS.emReadDefaultValueError;
                        }
                        else
                        {
                            emRuntimeStatus = EMSFB_RUNTIMESTATUS.emUnknownError;
                        }
                    }
                    else
                    {
                        emRuntimeStatus = EMSFB_RUNTIMESTATUS.emSuccess;    // Current chat category do not initialize and cannot find in database, using default values
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!!Exception in init chat category setting, message:[{0}]\n", ex.Message);
            }

            return emRuntimeStatus;
        }
        private void InitClassificationWarningInfo()
        {
            CommonHelper.AddKeyValuesToDir(m_dicWildcardClassificationWarning, kstrWildcardOrgClassificationSchema, m_strClassificationOrgName);
            m_strClassificationWarningInfo = "";
            if ((string.IsNullOrWhiteSpace(m_strClassficationDisplayName)) && (!string.IsNullOrWhiteSpace(m_strClassificationOrgName)))
            {
                m_strClassificationWarningInfo = CommonHelper.GetValueByKeyFromDir(SFBControlPanel.s_dirDisplayMsgInfo, (long)EMSFB_RUNTIMESTATUS.emClassificationSchemaWarning, "");
                if (!string.IsNullOrWhiteSpace(m_strClassificationWarningInfo))
                {
                    m_strClassificationWarningInfo = CommonHelper.ReplaceWildcards(m_strClassificationWarningInfo, m_dicWildcardClassificationWarning, kstrWildcardStartFlag, kstrWildcardEndFlag, true);
                }
            }
        }
        private void InitRuntimeSetting(string strParamUri, bool bParamApplied)
        {
            m_strUri = strParamUri;
            m_bApplied = bParamApplied;
        }
        #endregion
    }
}
