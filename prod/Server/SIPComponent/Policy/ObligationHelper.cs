using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nextlabs.SFBServerEnforcer.PolicyHelper;
using SFBCommon.SFBObjectInfo;
using SFBCommon.CommandHelper;
using SFBCommon.NLLog;
using SFBCommon.Common;

using Nextlabs.SFBServerEnforcer.SIPComponent.Common;
using Nextlabs.SFBServerEnforcer.SIPComponent.DataManagement;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.Policy
{
    static class ObligationHelper
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(ObligationHelper));
        #endregion

        #region Const/Readonly values
        private delegate void DelegateProcessMeetingObligation(List<PolicyObligation> lsObs, string strInviter, string strCurUserUri, ref CConference conf, ref SFBMeetingVariableInfo meetingVar, ref bool bMeetingVarChanged);
        private static Dictionary<string, DelegateProcessMeetingObligation> kdicMeetingObligationNameAndProcessFunctionMapping = new Dictionary<string, DelegateProcessMeetingObligation>()
        {
            {PolicyMain.kStrObNameNotification, ProcessNotifyObligation},
            {PolicyMain.KStrObNameAutoClassify, ProcessAutoClassifyObliation},
            {PolicyMain.KStrObNameManualClassify, ProcessManualClassifyObliation},
            {PolicyMain.KStrObNameEnableClassifyManager, ProcessEnableClassifyManagerObliation}
        };
        #endregion

        #region Public functions
        static public bool ProcessMeetingCommonObligations(EMSFB_ACTION emAction, PolicyResult policyResult, string strInviter, string strCurUserUri, ref CConference conf, ref SFBMeetingVariableInfo meetingVar)
        {
            // Check parameters
            if ((EMSFB_ACTION.emUnknwon == emAction) || (null == policyResult) || (string.IsNullOrWhiteSpace(strCurUserUri)) || (null == conf) || (null == meetingVar))
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Parameters error in ProcessMeetingCommonObligations, action:[{0}], policyResult:[{1}], currentUser:[{2}], CConference:[{3}], SFBMeetingVariableInfo:[{4}]\n", emAction, policyResult, strCurUserUri, conf, meetingVar);
                return false;
            }

            bool bMeetingVarChanged = false;
            List<string> lsAllSuppotObligationNames = ActionAndObligationMapper.GetAllSupportObligationNames(emAction, !policyResult.IsDeny());
            if (null != lsAllSuppotObligationNames)
            {
                foreach (string strCurObligationName in lsAllSuppotObligationNames)
                {
                    List<PolicyObligation> lsPolicyObligations = policyResult.GetAllObligationByName(strCurObligationName);
                    DelegateProcessMeetingObligation pDelegateProcessMeetingObligation = CommonHelper.GetValueByKeyFromDir(kdicMeetingObligationNameAndProcessFunctionMapping, strCurObligationName, null);
                    if (null == pDelegateProcessMeetingObligation)
                    {
                        // Current obligation is supported but it is not common obligation. It is logic workaround obligations: SFB_AutoSet_Enforcement, SFB_User_Condition
                    }
                    else
                    {
                        pDelegateProcessMeetingObligation(lsPolicyObligations, strInviter, strCurUserUri, ref conf, ref meetingVar, ref bMeetingVarChanged);
                    }
                }
                if (bMeetingVarChanged)
                {
                    meetingVar.PersistantSave();
                }
            }
            return true;
        }
        #endregion

        #region Innder do an obligation
        static private void ProcessNotifyObligation(List<PolicyObligation> lsObNotification, string strInviter, string strCurUserUri, ref CConference conf, ref SFBMeetingVariableInfo meetingVar, ref bool bMeetingVarChanged/*No used*/)
        {
            if ((null != lsObNotification) && (0 < lsObNotification.Count))
            {
                PolicyObligation notifyObligation = lsObNotification[0];
                if (notifyObligation != null)
                {
                    string strNotifyMsg = notifyObligation.GetAttribute(PolicyMain.kStrObAttributeNotifyMessage);
                    EMSFB_NOTIFYINFOTYPE notifyInfoType = NotifyCommandHelper.GetNotifyInfoType(strNotifyMsg);
                    if (notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNormal)
                    {
                        ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(CSIPTools.SIP_URI_PREFIX + strCurUserUri, ""/*"Deny Enter Meeting"*/, strNotifyMsg);    // No need header info for bug38174
                    }
                    else if ((notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNatural) || (notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeXML))
                    {
                        ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(strNotifyMsg, conf, meetingVar, strInviter, strCurUserUri, notifyObligation.PolicyName);
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Unknown type of notify message:{0}", strNotifyMsg);
                    }
                }
            }
        }
        static private void ProcessAutoClassifyObliation(List<PolicyObligation> lsObAutoTag, string strInviter, string strCurUserUri/**No used*/, ref CConference conf/*No used*/, ref SFBMeetingVariableInfo meetingVar, ref bool bMeetingVarChanged)
        {
            if ((null != lsObAutoTag) && (0 < lsObAutoTag.Count))
            {
                bMeetingVarChanged |= CEntityClassifyManager.ProcessAutoClassifyObligations(lsObAutoTag, meetingVar);
            }
        }
        static private void ProcessManualClassifyObliation(List<PolicyObligation> lsObManualTag, string strInviter, string strCurUserUri/*No used*/, ref CConference conf, ref SFBMeetingVariableInfo meetingVar/*No used*/, ref bool bMeetingVarChanged)
        {
            if ((null != lsObManualTag) && (0 < lsObManualTag.Count))
            {
                string strForceManualClassify = PolicyMain.KStrObAttributeForceClassifyYes;
                string strManualTagXmlAll = CEntityClassifyManager.ProcessManualClassifyObligations(lsObManualTag, ref strForceManualClassify);
                if (!string.IsNullOrWhiteSpace(strManualTagXmlAll))
                {
                    conf.ManualTagXml = strManualTagXmlAll;
                    conf.ForceManualTag = strForceManualClassify;

                    bMeetingVarChanged = true;
                }
            }
        }
        static private void ProcessEnableClassifyManagerObliation(List<PolicyObligation> lsObEnableClassifyManager, string strInviter, string strCurUserUri, ref CConference conf, ref SFBMeetingVariableInfo meetingVar, ref bool bMeetingVarChanged)
        {
            if ((null != lsObEnableClassifyManager) && (0 < lsObEnableClassifyManager.Count))
            {
                PolicyObligation obligationClassifyMgr = lsObEnableClassifyManager[0];
                if (null != obligationClassifyMgr)
                {
                    meetingVar.AddedClassifyManager(strCurUserUri);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Added classify manager:{0}", strCurUserUri);
                }
                else
                {
                    meetingVar.RemoveClassifyManager(strCurUserUri);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Remove classify manager:{0}", strCurUserUri);
                }
                bMeetingVarChanged = true;
            }
        }
        #endregion
    }

    static class ActionAndObligationMapper
    {
        #region Private readonly values: action and obligation mapping
        static private readonly Dictionary<EMSFB_ACTION, List<string>> s_dicAllowActionAndSupportObligations = new Dictionary<EMSFB_ACTION, List<string>>()
        {
            {EMSFB_ACTION.emMeetingCreate, new List<string>(){PolicyMain.kStrObNameAutoEnforcement, PolicyMain.KStrObNameAutoClassify, PolicyMain.KStrObNameManualClassify, PolicyMain.kStrObNameNotification}},
            {EMSFB_ACTION.emMeetingInvite, new List<string>(){PolicyMain.kStrObNameNotification}},
            {EMSFB_ACTION.emMeetingJoin, new List<string>(){PolicyMain.KStrObNameUserConditoin, PolicyMain.KStrObNameAutoClassify, PolicyMain.KStrObNameEnableClassifyManager, PolicyMain.kStrObNameNotification}},

            {EMSFB_ACTION.emMeetingShare, new List<string>(){PolicyMain.kStrObNameNotification}},
            {EMSFB_ACTION.emMeetingShareJoin, new List<string>(){PolicyMain.kStrObNameNotification}},

            {EMSFB_ACTION.emChatRoomCreate, new List<string>(){PolicyMain.KStrObNameAutoClassify}},
            {EMSFB_ACTION.emChatRoomManagerInvite, new List<string>()},
            {EMSFB_ACTION.emChatRoomInvite, new List<string>()},
            {EMSFB_ACTION.emChatRoomJoin, new List<string>(){PolicyMain.KStrObNameUserConditoin, PolicyMain.KStrObNameAutoClassify, PolicyMain.KStrObNameEnableClassifyManager, PolicyMain.kStrObNameNotification}},

            {EMSFB_ACTION.emChatRoomUpload, new List<string>(){}},
            {EMSFB_ACTION.emChatRoomDownload, new List<string>(){}},

            {EMSFB_ACTION.emNotifyUsers, new List<string>()},
            {EMSFB_ACTION.emUserConditionQuery, new List<string>()}
        };

        static private readonly Dictionary<EMSFB_ACTION, List<string>> s_dicDenyActionAndSupportObligations = new Dictionary<EMSFB_ACTION, List<string>>()
        {
            {EMSFB_ACTION.emMeetingCreate, new List<string>()},
            {EMSFB_ACTION.emMeetingInvite, new List<string>(){PolicyMain.kStrObNameNotification}},
            {EMSFB_ACTION.emMeetingJoin, new List<string>(){PolicyMain.KStrObNameUserConditoin, PolicyMain.kStrObNameNotification}},

            {EMSFB_ACTION.emMeetingShare, new List<string>(){PolicyMain.kStrObNameNotification}},
            {EMSFB_ACTION.emMeetingShareJoin, new List<string>(){PolicyMain.kStrObNameNotification}},

            {EMSFB_ACTION.emChatRoomCreate, new List<string>()},
            {EMSFB_ACTION.emChatRoomInvite, new List<string>()},
            {EMSFB_ACTION.emChatRoomManagerInvite, new List<string>()},
            {EMSFB_ACTION.emChatRoomJoin, new List<string>(){PolicyMain.KStrObNameUserConditoin, PolicyMain.kStrObNameNotification}},

            {EMSFB_ACTION.emChatRoomUpload, new List<string>(){PolicyMain.kStrObNameNotification}},
            {EMSFB_ACTION.emChatRoomDownload, new List<string>(){PolicyMain.kStrObNameNotification}},

            {EMSFB_ACTION.emNotifyUsers, new List<string>()},
            {EMSFB_ACTION.emUserConditionQuery, new List<string>(){}}
        };
        #endregion

        #region Public tools
        static public bool IsSupportCurrentObligation(EMSFB_ACTION emAction, string strInObligationName, bool bAllowPolicy)
        {
            List<string> lsAllSupportObligationNames = GetAllSupportObligationNames(emAction, bAllowPolicy);
            if (null != lsAllSupportObligationNames)
            {
                foreach (string strCurObliationName in lsAllSupportObligationNames)
                {
                    if (strInObligationName.Equals(strCurObliationName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        static public List<string> GetAllSupportObligationNames(EMSFB_ACTION emAction, bool bAllowPolicy)
        {
            if (bAllowPolicy)
            {
                return CommonHelper.GetValueByKeyFromDir(s_dicAllowActionAndSupportObligations, emAction, null);
            }
            else
            {
                return CommonHelper.GetValueByKeyFromDir(s_dicDenyActionAndSupportObligations, emAction, null);
            }
        }
        #endregion
    }
}
