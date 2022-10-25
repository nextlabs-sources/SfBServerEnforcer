using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nextlabs.SFBServerEnforcer.PolicyHelper;
using SFBCommon.SFBObjectInfo;
using SFBCommon.NLLog;
using System.Collections;
using SFBCommon.CommandHelper;
using SFBCommon.Common;

namespace NLAssistantWebService.Policies
{
    public enum EMSFB_QueryResult
    {
        Failed = 0,
        Success = 1,
        Unknown = -1
    }

    public class MeetingPolicy
    {
        private PolicyMain m_MainPolicy = null;
        private static readonly MeetingPolicy m_MeetingPolicy = new MeetingPolicy();
        private static readonly CLog logger = CLog.GetLogger(typeof(MeetingPolicy));

        #region .Ctor
        private MeetingPolicy() 
        {
            m_MainPolicy = new PolicyMain();
        }
        #endregion

        public static MeetingPolicy GetInstance()
        {
            return m_MeetingPolicy;
        }

        public EMSFB_QueryResult QueryMeetingPolicy(EMSFB_ACTION emAction, string strSender, string strRecipient, SFBMeetingInfo meeting, SFBMeetingVariableInfo meetingVariable, NLMeetingInfo nlMeeting, PolicyResult policyResult)
        {
            int nResult = -1;

            SFBObjectInfo[] sfbObjects = new SFBObjectInfo[3];
            sfbObjects[0] = meeting;
            sfbObjects[1] = meetingVariable;
            sfbObjects[2] = nlMeeting;

            nResult = m_MainPolicy.QueryPolicy(emAction, strSender, strRecipient, sfbObjects, null, policyResult);

            logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "QueryMeetingJoinPolicy, Action: {0}, Joiner: {1}, Creator: {2}", emAction, strSender, strRecipient);

            return Enum.IsDefined(typeof(EMSFB_QueryResult), nResult) ? (EMSFB_QueryResult)nResult : EMSFB_QueryResult.Unknown;
        }

        public EMSFB_QueryResult QueryMeetingMultiForParticipantsRecheck(EMSFB_ACTION emAction, string[] szStrParticipants, string strRecipient, SFBMeetingInfo meeting, SFBMeetingVariableInfo meetingVariable, NLMeetingInfo nlMeeting, out PolicyResult[] policyResults)
        {
            int nResult = -1;
            policyResults = null;

            if ((szStrParticipants != null) && (0 < szStrParticipants.Length))
            {
                SFBObjectInfo[] sfbObjects = new SFBObjectInfo[3];
                sfbObjects[0] = meeting;
                sfbObjects[1] = meetingVariable;
                sfbObjects[2] = nlMeeting;   

                int participantCount = szStrParticipants.Length;
                PolicyRequestInfo[] requests = new PolicyRequestInfo[participantCount];

                for(int i = 0; i < participantCount; ++i)
                {
                    PolicyRequestInfo policyRequest = new PolicyRequestInfo(emAction, szStrParticipants[i], strRecipient, sfbObjects, null, null);
                    requests[i] = policyRequest;
                }

                try
                {
                    nResult = m_MainPolicy.QueryPolicyMulti(requests, out policyResults);
                }
                catch (Exception e)
                {
                    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "QueryMultiMeetingJoinPolicy() failed, QueryPolicyMulti throw an error, Error: {0}, \nStackTrace: {1}", e.Message, e.StackTrace);
                }
            }
            else
            {
                nResult = 1;//if no participants in the meeting, ignore and response success.
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "No participants, no need recheck participants\n");
            }

            return Enum.IsDefined(typeof(EMSFB_QueryResult), nResult) ? (EMSFB_QueryResult)nResult : EMSFB_QueryResult.Unknown;
        }

        public EMSFB_QueryResult QueryMeetingMultiPolices(EMSFB_ACTION emAction, bool bSingletonUserIsSender, string strSingletonUser, ICollection<string> obCollectionUsers,
            SFBMeetingInfo obSFBMeetingInfo, SFBMeetingVariableInfo obSFBMeetingVariableInfo, NLMeetingInfo obSFBNLMeetingInfo, out PolicyResult[] policyResults)
        {
            int nResult = -1;
            policyResults = null;

            // Split, if obCollectionUsers(recipient) to many, like 10000 > need split otherwise query will be failed
            // Condition policy complete

            PolicyRequestInfo[] szPolicyRequestInfoRet = EstablishQueryRequestInfoCollection(emAction, bSingletonUserIsSender, strSingletonUser, obCollectionUsers, obSFBMeetingInfo, obSFBMeetingVariableInfo, obSFBNLMeetingInfo, null, null);         
            try
            {
                if (null == szPolicyRequestInfoRet)
                {
                    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Get query request info collection failed, auto allow(this need controlled by config info is better)\n");
                }
                else
                {
                    nResult = m_MainPolicy.QueryPolicyMulti(szPolicyRequestInfoRet, out policyResults);
                }
            }
            catch (Exception e)
            {
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "QueryMultiMeetingJoinPolicy() failed, QueryPolicyMulti throw an error, Error: {0}, \nStackTrace: {1}", e.Message, e.StackTrace);
            }
            return Enum.IsDefined(typeof(EMSFB_QueryResult), nResult) ? (EMSFB_QueryResult)nResult : EMSFB_QueryResult.Unknown;
        }

        #region Query policy for special case: AnchorWildcard, user condition obligation
        public int QueryPolicyForWildcardAnchorMulti(KeyValuePair<string, string>[] arrayWildcardInfo, out PolicyResult[] arrayPolicyResult)
        {
            //construct request
            PolicyRequestInfo[] arrayRequestInfo = new PolicyRequestInfo[arrayWildcardInfo.Length];
            for (int nIndexWildcard = 0; nIndexWildcard < arrayWildcardInfo.Length; nIndexWildcard++)
            {
                SFBObjectInfo[] objs = new SFBObjectInfo[1];
                objs[0] = new SFBMeetingInfo(); //just a fake object, can be anything object 

                string strFrom = arrayWildcardInfo[nIndexWildcard].Value;
                string strTo = strFrom;
                List<KeyValuePair<string, string>> lstUserAttribute = new List<KeyValuePair<string, string>>();
                lstUserAttribute.Add(new KeyValuePair<string, string>(CommandHelper.kstrWildcardAnchorKey, arrayWildcardInfo[nIndexWildcard].Key));

                arrayRequestInfo[nIndexWildcard] = new PolicyRequestInfo(EMSFB_ACTION.emNotifyUsers, strFrom, strTo, objs, lstUserAttribute);

                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "QueryPolicyForWildcardAnchorMulti, from={0}, to={1}, wildcard={2}", strFrom, strTo, arrayWildcardInfo[nIndexWildcard].Key);
            }

            //query policy
            int nRes = m_MainPolicy.QueryPolicyMulti(arrayRequestInfo, out arrayPolicyResult);
            if (nRes == 0)
            {
                SetDefaultEnforcementResult(arrayWildcardInfo.Length, CommonCfgMgr.GetInstance().DefaultPolicyResult, out arrayPolicyResult);
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "QueryPolicyForWildcardAnchorMulti failed, user default Enforce:{0}", CommonCfgMgr.GetInstance().DefaultPolicyResult);
            }

            return nRes;
        }
        public int QueryPolicyForUserConditionObligation(List<string> lstParticipate, List<KeyValuePair<string, string>> lstUserConditions, out PolicyResult[] arrayPolicyResult)
        {
            //construct request
            PolicyRequestInfo[] arrayRequestInfo = new PolicyRequestInfo[lstParticipate.Count];
            for (int nIndexParticipate = 0; nIndexParticipate < lstParticipate.Count; nIndexParticipate++)
            {
                string strFrom = lstParticipate[nIndexParticipate];
                arrayRequestInfo[nIndexParticipate] = new PolicyRequestInfo(EMSFB_ACTION.emUserConditionQuery, strFrom, strFrom, null, lstUserConditions);

                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "QueryPolicyForUserConditionObligation, from={0}, condition={1}", strFrom, lstUserConditions.ToString());
            }

            //query policy
            int nRes = m_MainPolicy.QueryPolicyMulti(arrayRequestInfo, out arrayPolicyResult);
            if (nRes == 0)
            {
                SetDefaultEnforcementResult(lstParticipate.Count, false, out arrayPolicyResult);
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "QueryPolicyForUserConditionObligation failed, user default Enforce:{0}", false);
            }

            return nRes;
        }
        #endregion


        #region Inner tools
        private void SetDefaultEnforcementResult(int nResultCount, bool bDefaultEnforcement, out PolicyResult[] arrayPolicyResult)
        {
            arrayPolicyResult = new PolicyResult[nResultCount];
            for (int iResIndex = 0; iResIndex < nResultCount; iResIndex++)
            {
                arrayPolicyResult[iResIndex] = new PolicyResult();
                arrayPolicyResult[iResIndex].Enforcement = bDefaultEnforcement ? EnforceResult.Enforce_Allow : EnforceResult.Enforce_Deny;
            }
        }
        
        private PolicyRequestInfo[] EstablishQueryRequestInfoCollection(EMSFB_ACTION emAction, bool bSingletonUserIsSender, string strSingletonUser, ICollection<string> obCollectionUsers,
            SFBMeetingInfo obSFBMeetingInfo, SFBMeetingVariableInfo obSFBMeetingVariableInfo, NLMeetingInfo obSFBNLMeetingInfo,
            List<KeyValuePair<string, string>> lstUserAttribute, List<string> lstParticipants)
        {
            PolicyRequestInfo[] szPolicyRequestInfoRet = null;

            SFBObjectInfo[] szSFBMeetingObjects = new SFBObjectInfo[3] {
                obSFBMeetingInfo,
                obSFBMeetingVariableInfo,
                obSFBNLMeetingInfo
            };

            if ((null == obCollectionUsers) || (0 >= obCollectionUsers.Count))
            {
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "The users collections is empyt for action:[{0}]\n", emAction);
                if (bSingletonUserIsSender)
                {
                    szPolicyRequestInfoRet = new PolicyRequestInfo[1]
                    {
                        new PolicyRequestInfo(emAction, strSingletonUser, "", szSFBMeetingObjects, lstUserAttribute, lstParticipants)
                    };
                }
                else
                {
                    szPolicyRequestInfoRet = null;
                    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "The users collections is empyt for action:[{0}] but the singleton user is sender flag:[{0}] is false, no senders\n", emAction, bSingletonUserIsSender);
                }
            }
            else
            {
                int nCollectionUserCount = obCollectionUsers.Count;
                szPolicyRequestInfoRet = new PolicyRequestInfo[nCollectionUserCount];
                int i = 0;
                foreach (var obUserItem in obCollectionUsers)
                {
                    PolicyRequestInfo policyRequest = null;
                    if (bSingletonUserIsSender)
                    {
                        policyRequest = new PolicyRequestInfo(emAction, strSingletonUser, obUserItem, szSFBMeetingObjects, lstUserAttribute, lstParticipants);
                    }
                    else
                    {
                        policyRequest = new PolicyRequestInfo(emAction, obUserItem, strSingletonUser, szSFBMeetingObjects, lstUserAttribute, lstParticipants);
                    }
                    szPolicyRequestInfoRet[i] = policyRequest;
                    ++i;
                }
            }
            return szPolicyRequestInfoRet;
        }

        #endregion
    }
}