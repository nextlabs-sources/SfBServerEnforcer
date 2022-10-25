using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nextlabs.SFBServerEnforcer.PolicyHelper;
using SFBCommon.SFBObjectInfo;
using SFBCommon.NLLog;
using SFBCommon.Common;
using SFBCommon.CommandHelper;

using Nextlabs.SFBServerEnforcer.HTTPComponent.Data;


namespace Nextlabs.SFBServerEnforcer.HTTPComponent.Policy
{
    class HttpPolicy
    {
        static HttpPolicy m_Instance = null;
        private PolicyMain m_policyCenter = null;
        private static CLog theLog = CLog.GetLogger("HTTPComonent.HttpPolicy");

        public static HttpPolicy GetInstance()
        {
            if(m_Instance==null)
            {
                m_Instance = new HttpPolicy();
            }
            return m_Instance;
        }

        private HttpPolicy() 
        {
            m_policyCenter = new PolicyMain();
        }
        
        public int QueryPolicyForChatRoomAttachmentAction(EMSFB_ACTION emAction, string strFromUser, string strToUser, HttpChatRoomInfo obHttpChatRoomInfo, NLChatRoomAttachmentInfo obChatRoomAttachmentInfo, out PolicyResult policyResult)
        {
            //get participant
            List<string> lstDistinctParticipants = obHttpChatRoomInfo.GetParticipants();

            SFBObjectInfo[] objs = new SFBObjectInfo[3];
            objs[0] = obHttpChatRoomInfo.sfbChatRoomInfo;
            objs[1] = obHttpChatRoomInfo.sfbRoomVar;
            objs[2] = obChatRoomAttachmentInfo;

            EnforceResult emDefaultEnforce = CommonCfgMgr.GetInstance().DefaultPolicyResult ? EnforceResult.Enforce_Allow : EnforceResult.Enforce_Deny;
            policyResult = new PolicyResult(emDefaultEnforce);
            int nRes = m_policyCenter.QueryPolicy(emAction, strFromUser, strToUser, objs, lstDistinctParticipants, policyResult);
            if (nRes == 0)
            {
                policyResult.Enforcement = emDefaultEnforce;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "QueryPolicyForChatRoomAttachmentAction failed, user default Enforce:{0}", policyResult.Enforcement);
            }
            
            // do evaluation on condition within obligation
            CondtionObligationEvaluation.DoEvalutionForConditionWithinObligation(policyResult, SFBParticipantManager.GetDistinctParticipantsAsList(obHttpChatRoomInfo.sfbRoomVar));

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "QueryPolicyForChatRoomAttachmentAction, action:[{0}], FromUser:[{1}], ToUser:[{2}], Enforcer:[{3}]\n", emAction, strFromUser, strToUser, policyResult.Enforcement);
            return nRes;
        }

        public int QueyPolicyForChatRoomCreate(string strCreator, HttpChatRoomInfo chatRoom, PolicyResult policyResult)
        {
            SFBObjectInfo[] objs = new SFBObjectInfo[2];
            objs[0] = chatRoom.sfbChatRoomInfo;
            objs[1] = chatRoom.sfbRoomVar;

            int nRes = m_policyCenter.QueryPolicy(EMSFB_ACTION.emChatRoomCreate, strCreator, strCreator, objs, null, policyResult);
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "QueyPolicyForChatRoomCreate, creator={0} chatroom={1}", strCreator, chatRoom.Name);
            return nRes;
        }

        public int QueyPolicyForChatRoomInvite(string strFrom, string strTo,  HttpChatRoomInfo chatRoom, PolicyResult policyResult)
        {
            SFBObjectInfo[] objs = new SFBObjectInfo[1];
            objs[0] = chatRoom.sfbChatRoomInfo;

            int nRes = m_policyCenter.QueryPolicy(EMSFB_ACTION.emChatRoomInvite, strFrom, strTo, objs, null, policyResult);
            if(nRes==0)
            {
                policyResult.Enforcement = CommonCfgMgr.GetInstance().DefaultPolicyResult ? EnforceResult.Enforce_Allow : EnforceResult.Enforce_Deny;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "QueyPolicyForChatRoomInvite failed, user default Enforce:{0}", policyResult.Enforcement);
            }

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "QueyPolicyForChatRoomInvite, from={0}, to={1}, chatroom={2}, Enforcer={3}", strFrom, strTo, chatRoom.Name, policyResult.Enforcement);

            return nRes;
        }


        public int QueyPolicyForChatRoomInviteMulti(string strFrom, List<KeyValuePair<string, string>> lstMembers, HttpChatRoomInfo chatRoom, out PolicyResult[] arrayPolicyResult)
        {
            //construct request
            PolicyRequestInfo[] arrayRequestInfo = new PolicyRequestInfo[lstMembers.Count];
            for(int nIndexMembers=0; nIndexMembers<lstMembers.Count; nIndexMembers++)
            {
                SFBObjectInfo[] objs = new SFBObjectInfo[2];
                objs[0] = chatRoom.sfbChatRoomInfo;
                objs[1] = chatRoom.sfbRoomVar;

                arrayRequestInfo[nIndexMembers] = new PolicyRequestInfo(EMSFB_ACTION.emChatRoomInvite, strFrom, lstMembers[nIndexMembers].Value, objs);

                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "QueyPolicyForChatRoomInviteMulti, from={0}, to={1}, chatroom={2}", strFrom, lstMembers[nIndexMembers].Value, chatRoom.Name);
            }
           
            //query policy
            int nRes = m_policyCenter.QueryPolicyMulti(arrayRequestInfo, out arrayPolicyResult);
            if (nRes == 0)
            {
              SetDefaultEnforcementResult(lstMembers.Count, CommonCfgMgr.GetInstance().DefaultPolicyResult, out arrayPolicyResult);
              theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "QueyPolicyForChatRoomInviteMulti failed, user default Enforce:{0}", CommonCfgMgr.GetInstance().DefaultPolicyResult);
            }

            return nRes;
        }

        public int QueyPolicyForChatRoomManagerInviteMulti(string strFrom, List<KeyValuePair<string, string>> lstManagers, HttpChatRoomInfo chatRoom, out PolicyResult[] arrayPolicyResult)
        {
            //construct request
            PolicyRequestInfo[] arrayRequestInfo = new PolicyRequestInfo[lstManagers.Count];
            for (int nIndexMembers = 0; nIndexMembers < lstManagers.Count; nIndexMembers++)
            {
                SFBObjectInfo[] objs = new SFBObjectInfo[2];
                objs[0] = chatRoom.sfbChatRoomInfo;
                objs[1] = chatRoom.sfbRoomVar;

                arrayRequestInfo[nIndexMembers] = new PolicyRequestInfo(EMSFB_ACTION.emChatRoomManagerInvite, strFrom, lstManagers[nIndexMembers].Value, objs);

                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "QueyPolicyForChatRoomManagerInviteMulti, from={0}, to={1}, chatroom={2}", strFrom, lstManagers[nIndexMembers].Value, chatRoom.Name);
            }

            //query policy
            int nRes = m_policyCenter.QueryPolicyMulti(arrayRequestInfo, out arrayPolicyResult);
            if (nRes == 0)
            {
                SetDefaultEnforcementResult(lstManagers.Count, CommonCfgMgr.GetInstance().DefaultPolicyResult, out arrayPolicyResult);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "QueyPolicyForChatRoomManagerInviteMulti failed, user default Enforce:{0}", CommonCfgMgr.GetInstance().DefaultPolicyResult);
            }
            

            return nRes;
        }

        private void SetDefaultEnforcementResult(int nResultCount, bool bDefaultEnforcement, out PolicyResult[] arrayPolicyResult)
        {
            arrayPolicyResult = new PolicyResult[nResultCount];
            for(int iResIndex=0; iResIndex<nResultCount; iResIndex++)
            {
                arrayPolicyResult[iResIndex] = new PolicyResult();
                arrayPolicyResult[iResIndex].Enforcement = bDefaultEnforcement ? EnforceResult.Enforce_Allow : EnforceResult.Enforce_Deny;
            }
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
        public int QueryPolicyForUserConditionObligation(List<string> lstParticipate, List<KeyValuePair<string, string>> lstUserConditions, out PolicyResult[] arrayPolicyResult)
        {
            //construct request
            PolicyRequestInfo[] arrayRequestInfo = new PolicyRequestInfo[lstParticipate.Count];
            for (int nIndexParticipate = 0; nIndexParticipate < lstParticipate.Count; nIndexParticipate++)
            {
                string strFrom = lstParticipate[nIndexParticipate];
                arrayRequestInfo[nIndexParticipate] = new PolicyRequestInfo(EMSFB_ACTION.emUserConditionQuery, strFrom, strFrom, null, lstUserConditions);

                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "QueryPolicyForUserConditionObligation, from={0}, condition={1}", strFrom, lstUserConditions.ToString());
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
    }
}
