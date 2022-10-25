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

using Nextlabs.SFBServerEnforcer.HTTPComponent.Data;
using Nextlabs.SFBServerEnforcer.HTTPComponent.Common;

namespace Nextlabs.SFBServerEnforcer.HTTPComponent.Policy
{
    static class ObligationHelper
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(ObligationHelper));
        #endregion

        #region Const/Readonly values
        private delegate void DelegateProcessChatRoomObligation(List<PolicyObligation> lsObs, string strInviter, string strCurUserUri, HttpChatRoomInfo obHttpChatRoomInfo, NLChatRoomAttachmentInfo obChatRoomAttachmentInfo, ref bool bChatRoomVarChanged);
        private static Dictionary<string, DelegateProcessChatRoomObligation> kdicMeetingObligationNameAndProcessFunctionMapping = new Dictionary<string, DelegateProcessChatRoomObligation>()
        {
            {PolicyMain.kStrObNameNotification, ProcessNotifyObligation},
            {PolicyMain.KStrObNameAutoClassify, ProcessAutoClassifyObliation}
        };
        #endregion

        #region Public functions
        static public bool ProcessChatRoomAttachmentCommonObligations(EMSFB_ACTION emAction, PolicyResult policyResult, string strInviter, string strCurUserUri, HttpChatRoomInfo obHttpChatRoomInfo, NLChatRoomAttachmentInfo obChatRoomAttachmentInfo)
        {
            // Check parameters
            if ((EMSFB_ACTION.emUnknwon == emAction) || (null == policyResult) || (string.IsNullOrWhiteSpace(strCurUserUri)) || (null == obHttpChatRoomInfo))
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Parameters error in ProcessChatRoomAttachmentCommonObligations, action:[{0}], policyResult:[{1}], currentUser:[{2}], chatRoomInfo:[{3}]\n", emAction, policyResult, strCurUserUri, obHttpChatRoomInfo);
                return false;
            }

            bool bChatRoomVarChanged = false;
            List<string> lsAllSuppotObligationNames = ActionAndObligationMapper.GetAllSupportObligationNames(emAction, !policyResult.IsDeny());
            if (null != lsAllSuppotObligationNames)
            {
                foreach (string strCurObligationName in lsAllSuppotObligationNames)
                {
                    List<PolicyObligation> lsPolicyObligations = policyResult.GetAllObligationByName(strCurObligationName);
                    DelegateProcessChatRoomObligation pDelegateProcessChatRoomObligation = CommonHelper.GetValueByKeyFromDir(kdicMeetingObligationNameAndProcessFunctionMapping, strCurObligationName, null);
                    if (null == pDelegateProcessChatRoomObligation)
                    {
                        // Current obligation is supported but it is not common obligation. It is logic workaround obligations: SFB_AutoSet_Enforcement, SFB_User_Condition
                    }
                    else
                    {
                        pDelegateProcessChatRoomObligation(lsPolicyObligations, strInviter, strCurUserUri, obHttpChatRoomInfo, obChatRoomAttachmentInfo, ref bChatRoomVarChanged);
                    }
                }
                if (bChatRoomVarChanged)
                {
                    obHttpChatRoomInfo.sfbRoomVar.PersistantSave();
                }
            }
            return true;
        }
        #endregion

        #region Innder do an obligation
        static private void ProcessNotifyObligation(List<PolicyObligation> lsObNotification, string strInviter, string strCurUserUri, HttpChatRoomInfo obHttpChatRoomInfo, NLChatRoomAttachmentInfo obChatRoomAttachmentInfo, ref bool bChatRoomVarChanged)
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
                        ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(CHttpTools.SIP_URI_PREFIX + strCurUserUri, ""/*"Deny Enter Meeting"*/, strNotifyMsg);    // No need header info for bug38174
                    }
                    else if ((notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNatural) || (notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeXML))
                    {
                        ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(strNotifyMsg, obHttpChatRoomInfo, strInviter, strCurUserUri, notifyObligation.PolicyName, obChatRoomAttachmentInfo);
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Unknown type of notify message:{0}", strNotifyMsg);
                    }
                }
            }
        }
        static private void ProcessAutoClassifyObliation(List<PolicyObligation> lstAutoTagObligations, string strInviter, string strCurUserUri, HttpChatRoomInfo obHttpChatRoomInfo, NLChatRoomAttachmentInfo obChatRoomAttachmentInfo, ref bool bChatRoomVarChanged)
        {
            //get tags from obligations
            Dictionary<string, string> dicNewTags = PolicyObligation.GetDictionaryTagsFromAutoClassifyObligation(lstAutoTagObligations);

            //added to exist tag
            bool bHaveNewTags = dicNewTags.Count > 0;
            if (bHaveNewTags)
            {
                obHttpChatRoomInfo.sfbRoomVar.AddedNewTags(dicNewTags);
                bChatRoomVarChanged = true;
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
