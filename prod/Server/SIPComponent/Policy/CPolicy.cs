using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Nextlabs.SFBServerEnforcer.PolicyHelper;
using SFBCommon.SFBObjectInfo;
using SFBCommon.NLLog;
using SFBCommon.Common;
using SFBCommon.CommandHelper;

using Nextlabs.SFBServerEnforcer.SIPComponent.DataManagement;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.Policy
{
    class CPolicy
    {
        #region Logger
        private static CLog theLog = CLog.GetLogger(typeof(CPolicy));
        #endregion

        #region Const/Read only value
        static public readonly bool kbDefaultPolicyResult = CommonCfgMgr.GetInstance().DefaultPolicyResult;
        #endregion

        #region Sington
        private static CPolicy s_thePolicy;
        public static CPolicy Instance()
        {
            if (null == s_thePolicy)
            {
                s_thePolicy = new CPolicy();
            }
            return s_thePolicy;
        }
        #endregion

        #region Members
        private PolicyMain m_policyCenter;
        #endregion

        #region Constructors
        private CPolicy()
        {
            m_policyCenter = new PolicyMain();
        }
        #endregion

        #region Query policy for meeting actions
        public int QueryPolicyForMeetingCreate(CConference conference, PolicyResult policyResult)
        {
            SFBObjectInfo[] objs = new SFBObjectInfo[1];
            objs[0] = conference.sfbMeetingInfo;
            int nRes = m_policyCenter.QueryPolicy(EMSFB_ACTION.emMeetingCreate, conference.Creator, conference.Creator, objs, null, policyResult);

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "QueryPolicyForMeetingCreate, meeting={0}", conference.FocusUri);

            return nRes;
        }
        public int QueryPolicyForMeetingInvite(string strFrom, string strTo, CConference conference, SFBMeetingVariableInfo meetingVar, List<string> lstParticipants, PolicyResult policyResult)
        {
            SFBObjectInfo[] objs= new SFBObjectInfo[2];
            objs[0] = conference.sfbMeetingInfo;
            objs[1] = meetingVar;

            int nRes = m_policyCenter.QueryPolicy(EMSFB_ACTION.emMeetingInvite, strFrom, strTo, objs, lstParticipants, policyResult);

            if (nRes == 0)
            {
                policyResult.Enforcement = CommonCfgMgr.GetInstance().DefaultPolicyResult ? EnforceResult.Enforce_Allow : EnforceResult.Enforce_Deny;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "QueryPolicyForMeetingInvite failed, use default Enforce:{0}", policyResult.Enforcement);
            }

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo,"QueryPolicyForMeetingInvite, from={0}, to={1}, Meeting={2}, Enforcer={3}", strFrom, strTo, conference.FocusUri, policyResult.Enforcement);

            return nRes;
        }
        public int QueryPolicyForMeetingJoin(string strUser, CConference conference, SFBMeetingVariableInfo meetingVar, List<string> lstParticipants, PolicyResult policyResult)
        {
            SFBObjectInfo[] objs = new SFBObjectInfo[2];
            objs[0] = conference.sfbMeetingInfo;
            objs[1] = meetingVar;

            int nRes = m_policyCenter.QueryPolicy(EMSFB_ACTION.emMeetingJoin, strUser, conference.Creator, objs, lstParticipants, policyResult);

            if (nRes == 0)
            {
                policyResult.Enforcement = CommonCfgMgr.GetInstance().DefaultPolicyResult ? EnforceResult.Enforce_Allow : EnforceResult.Enforce_Deny;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "QueryPolicyForMeetingJoin failed, use default Enforce:{0}", policyResult.Enforcement);
            }

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "QueryPolicyForMeetingJoin, user={0},meeting={1}, Enforcer={2}", strUser, conference.FocusUri, policyResult.Enforcement);

            return nRes;
        }
        public int QueryPolicyForMeeting(EMSFB_ACTION emAction, string strFrom, string strTo, CConference conference, SFBMeetingVariableInfo meetingVar, List<string> lstParticipants, PolicyResult policyResult)
        {
            SFBObjectInfo[] objs = new SFBObjectInfo[2];
            objs[0] = conference.sfbMeetingInfo;
            objs[1] = meetingVar;

            int nRes = m_policyCenter.QueryPolicy(emAction, strFrom, strTo, objs, lstParticipants, policyResult);
            if (nRes == 0)
            {
                policyResult.Enforcement = kbDefaultPolicyResult ? EnforceResult.Enforce_Allow : EnforceResult.Enforce_Deny;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "Query policy for meeting with action:[{0}] failed, use default enforcement:[{1}].\n", emAction, policyResult.Enforcement);
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Query policy for meeting with action:[{0}], from={1}, to={2}, Meeting={3}, Enforcer={4}", emAction, strFrom, strTo, conference.FocusUri, policyResult.Enforcement);
            return nRes;
        }
        #endregion

        #region Query policy for chat room actions
        public int QueryPolicyForChatRoomJoin(string strSIPUser, CChatRoom chatRoom, SFBChatRoomVariableInfo roomVar, List<string> lstParticipants, PolicyResult policyResult)
        {
            SFBObjectInfo[] objs = new SFBObjectInfo[2];
            objs[0] = chatRoom.SFBChatRoom;
            objs[1] = roomVar;

            int nRes = m_policyCenter.QueryPolicy(EMSFB_ACTION.emChatRoomJoin, strSIPUser, chatRoom.CreatorUri, objs, lstParticipants, policyResult);

            if (nRes == 0)
            {
                policyResult.Enforcement = CommonCfgMgr.GetInstance().DefaultPolicyResult ? EnforceResult.Enforce_Allow : EnforceResult.Enforce_Deny;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "QueryPolicyForChatRoomJoin failed, use default Enforce:{0}", policyResult.Enforcement);
            }

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "QueryPolicyForChatRoomJoin, user={0}, chatRoom={1}, Enforcer={2}", strSIPUser, chatRoom.Name, policyResult.Enforcement);

            return nRes;
        }
        #endregion

        #region Query policy for special case: AnchorWildcard, user condition obligation
        public int QueryPolicyForWildcardAnchorMulti(KeyValuePair<string,string>[] arrayWildcardInfo, out PolicyResult[] arrayPolicyResult)
        {
            //construct request
            PolicyRequestInfo[] arrayRequestInfo = new PolicyRequestInfo[arrayWildcardInfo.Length];
            for (int nIndexWildcard = 0; nIndexWildcard < arrayWildcardInfo.Length; nIndexWildcard++)
            {
                SFBObjectInfo[] objs = new SFBObjectInfo[1];
                objs[0] =  new SFBMeetingInfo(); //just a fake object, can be anything object 

                string strFrom = arrayWildcardInfo[nIndexWildcard].Value;
                string strTo = strFrom;
                List<KeyValuePair<string, string>> lstUserAttribute = new List<KeyValuePair<string, string>>();
                lstUserAttribute.Add(new KeyValuePair<string, string>(CommandHelper.kstrWildcardAnchorKey, arrayWildcardInfo[nIndexWildcard].Key));

                arrayRequestInfo[nIndexWildcard] = new PolicyRequestInfo(EMSFB_ACTION.emNotifyUsers, strFrom, strTo, objs, lstUserAttribute);

                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "QueryPolicyForWildcardAnchorMulti, from={0}, to={1}, wildcard={2}", strFrom, strTo, arrayWildcardInfo[nIndexWildcard].Key);
            }

            //query policy
            int nRes = m_policyCenter.QueryPolicyMulti(arrayRequestInfo, out arrayPolicyResult);
            if (nRes == 0)
            {
                SetDefaultEnforcementResult(arrayWildcardInfo.Length, CommonCfgMgr.GetInstance().DefaultPolicyResult, out arrayPolicyResult);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "QueryPolicyForWildcardAnchorMulti failed, user default Enforce:{0}", CommonCfgMgr.GetInstance().DefaultPolicyResult);
            }

            return nRes;

        }
        public int QueryPolicyForUserConditionObligation(List<string> lstParticipate, List<KeyValuePair<string,string>> lstUserConditions, out PolicyResult[] arrayPolicyResult)
        {
            //construct request
            PolicyRequestInfo[] arrayRequestInfo = new PolicyRequestInfo[lstParticipate.Count];
            for (int nIndexParticipate = 0; nIndexParticipate < lstParticipate.Count; nIndexParticipate++)
            {             
                string strFrom = lstParticipate[nIndexParticipate];
                arrayRequestInfo[nIndexParticipate] = new PolicyRequestInfo(EMSFB_ACTION.emUserConditionQuery, strFrom, strFrom, null, lstUserConditions);

                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "QueryPolicyForUserConditionObligation, from={0}, condition={1}", strFrom, lstUserConditions.ToString() );
            }

            //query policy
            int nRes = m_policyCenter.QueryPolicyMulti(arrayRequestInfo, out arrayPolicyResult);
            if (nRes == 0)
            {
                SetDefaultEnforcementResult(lstParticipate.Count, false, out arrayPolicyResult);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "QueryPolicyForUserConditionObligation failed, user default Enforce:{0}", false);
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
        #endregion
    }
}
