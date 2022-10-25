using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SFBCommon.NLLog;
using SFBCommon.Common;
using SFBCommon.SFBObjectInfo;

namespace NLAssistantWebService.Tokens
{
    public class QueryRequestIdentifyManager
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(QueryRequestIdentifyManager));
        #endregion

        #region Members
        // Cannot use simple memory to cache the token, bug 40106
        // If do not consider load balance which install the service in different machine, we can consider to use global shared memory to cache the identify info.
        // private Dictionary<QueryRequestIdentify, string> m_dicQueryRequestIdentify = new Dictionary<string, ClassifyToken>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Singleton
        static private object s_obLockForInstance = new object();
        static private QueryRequestIdentifyManager s_obQueryRequestIdentifyManagerIns = null;
        static public QueryRequestIdentifyManager GetInstance()
        {
            if (null == s_obQueryRequestIdentifyManagerIns)
            {
                lock (s_obLockForInstance)
                    if (null == s_obQueryRequestIdentifyManagerIns)
                    {
                        s_obQueryRequestIdentifyManagerIns = new QueryRequestIdentifyManager();
                    }
            }
            return s_obQueryRequestIdentifyManagerIns;
        }
        private QueryRequestIdentifyManager()
        {

        }
        #endregion

        #region Public functions
        public string GetNewQueryRequestIdentify()
        {
            return Guid.NewGuid().ToString();
        }
        public string GetQueryResponseInfoByRequestIdentify(string strQueryRequestIdentify, bool bAutoDeleteIfResponseComplete)
        {
            string strResponseInfo = "";
            NLAssistantQueryPolicyInfo obNLAssistantQueryPolicyInfo = GetNLAssistantQueryPolicyInfoByRequestIdentify(strQueryRequestIdentify);
            if (null == obNLAssistantQueryPolicyInfo)
            {
                // Empty, error
            }
            else
            {
                strResponseInfo = obNLAssistantQueryPolicyInfo.GetItemValue(NLAssistantQueryPolicyInfo.kstrResponseInfoFieldName);

                if (bAutoDeleteIfResponseComplete)
                {
                    string strResponseStatus = obNLAssistantQueryPolicyInfo.GetItemValue(NLAssistantQueryPolicyInfo.kstrResponseStatusFieldName);
                    if (String.Equals(NLAssistantQueryPolicyInfo.kstrStatusComplete == strResponseStatus, StringComparison.OrdinalIgnoreCase))
                    {
                        DeleteQueryRequestIdentify(strQueryRequestIdentify);
                    }
                }                                   
            }
            return strResponseInfo;
        }
        public bool SaveQueryResponseInfoByRequestIdentify(string strQueryRequestIdentify, bool bIsComplete, string strResponseInfo)
        {
            bool bRet = false;
            if (!String.IsNullOrEmpty(strQueryRequestIdentify))
            {
                NLAssistantQueryPolicyInfo obNLAssistantQueryPolicyInfo = new NLAssistantQueryPolicyInfo(
                    NLAssistantQueryPolicyInfo.kstrRequestIdentifyFieldName, strQueryRequestIdentify,
                    NLAssistantQueryPolicyInfo.kstrResponseStatusFieldName, (bIsComplete ? NLAssistantQueryPolicyInfo.kstrStatusComplete : NLAssistantQueryPolicyInfo.kstrStatusProcessing),
                    NLAssistantQueryPolicyInfo.kstrResponseInfoFieldName, CommonHelper.GetSolidString(strResponseInfo));
                bRet = obNLAssistantQueryPolicyInfo.PersistantSave();
            }
            return bRet;
        }
        public void DeleteQueryRequestIdentify(string strQueryRequestIdentify)
        {
#if false
            if (!String.IsNullOrEmpty(strQueryRequestIdentify))
            {
                NLAssistantQueryPolicyInfo obNLAssistantQueryPolicyInfo = new NLAssistantQueryPolicyInfo();
                obNLAssistantQueryPolicyInfo.DeleteObject(strQueryRequestIdentify);
            }
#else
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Delete request info with identify:[{0}]\n", strQueryRequestIdentify);
#endif
        }
        #endregion    
   
        #region Inner tools
        private NLAssistantQueryPolicyInfo GetNLAssistantQueryPolicyInfoByRequestIdentify(string strQueryRequestIdentify)
        {
            NLAssistantQueryPolicyInfo obNLAssistantQueryPolicyInfo = null;
            if (!String.IsNullOrEmpty(strQueryRequestIdentify))
            {
                obNLAssistantQueryPolicyInfo = new NLAssistantQueryPolicyInfo();
                bool bSuccess = obNLAssistantQueryPolicyInfo.EstablishObjFormPersistentInfo(NLAssistantQueryPolicyInfo.kstrRequestIdentifyFieldName, strQueryRequestIdentify);
                if (!bSuccess)
                {
                    obNLAssistantQueryPolicyInfo = null;
                }
            }
            return obNLAssistantQueryPolicyInfo;
        }
        #endregion

    }
}