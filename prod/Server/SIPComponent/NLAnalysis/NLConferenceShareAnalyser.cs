using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Rtc.Sip;

using SFBCommon.Common;
using SFBCommon.NLLog;
using SFBCommon.SFBObjectInfo;
using Nextlabs.SFBServerEnforcer.PolicyHelper;
using SFBCommon.CommandHelper;

using Nextlabs.SFBServerEnforcer.SIPComponent.Common;
using Nextlabs.SFBServerEnforcer.SIPComponent.DataManagement;
using Nextlabs.SFBServerEnforcer.SIPComponent.Policy;
using Nextlabs.SFBServerEnforcer.SIPComponent.SIPContentHelper;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.NLAnalysis
{
    static class NLConferenceShareAnalyser
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(NLConferenceShareAnalyser));
        #endregion

        #region Public functions, conference share request analysi tools
        static public SIP_INVITE_TYPE GetConferenceShareInviteRequestType(Request obConferenceShareRequest)
        {
            /* *
            * application sharing role is sharer and connection is new, it is share application create event
            * application sharing role is sharer and connection is existing, it is sharer join event and no need care
            * application sharing role is viewer and connection is new, it is viewer join event
            * application sharing role is viewer and connection is exiting, it is viewer get share content event and no need care
            */
            SIP_INVITE_TYPE emSipInviteType = SIP_INVITE_TYPE.INVITE_UNKNOWN;

            Header headerContentType = obConferenceShareRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_CONTENTTYPE);
            if ((null != headerContentType))
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Content type is:[{0}]={1}\n", headerContentType.StandardType, headerContentType.Value);
                        
                SIPContentApplicationSdp obSIPContentApplicationSdp = SIPContent.CreateSIPContentObjByFlag(headerContentType.Value, obConferenceShareRequest.Content) as SIPContentApplicationSdp;
                if (null != obSIPContentApplicationSdp)
                {
                    string strConnectionValue = obSIPContentApplicationSdp.GetFirstContentValueByKey(SIPContentApplicationSdp.kstrContentKeyConnection);
                    string strApplicationSharingRoleValue = obSIPContentApplicationSdp.GetFirstContentValueByKey(SIPContentApplicationSdp.kstrContentKeyApplicationSharingRole);
                    if ((!string.IsNullOrWhiteSpace(strConnectionValue)) && (!string.IsNullOrWhiteSpace(strApplicationSharingRoleValue)))
                    {
                        EMSIP_CONNECTION_TYPE emConnectionType = SIPContentApplicationSdp.GetConnectionType(strConnectionValue);
                        EMSIP_USER_ROLE emUserRole = SIPContentApplicationSdp.GetUserRoleType(strApplicationSharingRoleValue);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Role:[{0}={1}], Connection:[{2}={3}]\n", strApplicationSharingRoleValue, emUserRole, strConnectionValue, emConnectionType);
                        if (EMSIP_USER_ROLE.emRole_Sharer == emUserRole)
                        {
                            if (EMSIP_CONNECTION_TYPE.emConnection_New == emConnectionType)
                            {
                                emSipInviteType = SIP_INVITE_TYPE.INVITE_CONF_SHARE_CREATE;
                            }
                            else if (EMSIP_CONNECTION_TYPE.emConnection_Existing == emConnectionType)
                            {
                                // it is sharer join event and no need care
                            }
                        }
                        else if (EMSIP_USER_ROLE.emRole_Viewer == emUserRole)
                        {
                            if (EMSIP_CONNECTION_TYPE.emConnection_New == emConnectionType)
                            {
                                emSipInviteType = SIP_INVITE_TYPE.INVITE_CONF_SHARE_JOIN;
                            }
                            else
                            {
                                // it is viewer get share content event and no need care
                            }
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "One of baisc info is empty, ConnectionValue:[{0}], ApplicationSharingRole:[{1}]\n", strConnectionValue, strApplicationSharingRoleValue);
                    }
                }
            }
            return emSipInviteType;
        }
        #endregion

        #region Public functions, conference share action parser: share create, share join
        static public void ParseConferenceShareCreateRequest(Request sipRequest, ref Response outResponse)
        {
            ParseConferenceShareRequestCommonWorkflow(EMSFB_ACTION.emMeetingShare, sipRequest, ref outResponse);
        }
        static public void ParseConferenceShareJoinRequest(Request sipRequest, ref Response outResponse)
        {
            ParseConferenceShareRequestCommonWorkflow(EMSFB_ACTION.emMeetingShareJoin, sipRequest, ref outResponse);
        }
        #endregion

        #region Private tools, conference share request analyse
        static public void ParseConferenceShareRequestCommonWorkflow(EMSFB_ACTION emAction, Request sipRequest, ref Response outResponse)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In parse conference share {0} request, header:[{1}] content:[{2}]\n", emAction, sipRequest.AllHeaders, sipRequest.Content);

            // Get user URI, conference share URI, conference focus URI
            string strCurUserUri = "", strConferenceShareUri;
            GetUserAndConferenceShareUriForShareRequest(sipRequest, out strCurUserUri, out strConferenceShareUri);
            string strConferenceFocusUri = CSIPTools.GetConferenceFocusUriAccordingShareUri(strConferenceShareUri);
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "meeting share create request, CurrentUser:{0}, ShareUri:[{1}], FocusUri:[{2}]", strCurUserUri, strConferenceShareUri, strConferenceFocusUri);

            // Get all information according conference URI
            CConference conf = GetConferenceInfoByFocusUri(strConferenceFocusUri, false);

            // For meeting share case, no invite action, inviter alway empty
            bool bAllowConferenceShareOp = ProcessMeetingCommonActionWorkflow(emAction, strCurUserUri, strConferenceFocusUri, strConferenceShareUri, conf);
            if (!bAllowConferenceShareOp)
            {
                outResponse = CSIPTools.CreateDenySIPResponse(sipRequest);
            }

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Out parse conference share {0} request\n", emAction);
        }
        static private bool ProcessMeetingCommonActionWorkflow(EMSFB_ACTION emAction, string strCurUserUri, string strConferenceFocusUri, string strConferenceShareUri, CConference conf)
        {
            bool bAllowConferenceShareJoin = CPolicy.kbDefaultPolicyResult;
            if ((null != conf))
            {
                if (conf.NeedEnforce())
                {
                    ContactToEndpointProxy obContactToEndpointProxy = ContactToEndpointProxy.GetAgentProxyContact();
                    if (obContactToEndpointProxy.IsEndpointProxy(strCurUserUri))
                    {
                        bAllowConferenceShareJoin = true;
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Auto allowed, reason: current user:[{0}] is EndpointProxy\n", strCurUserUri);
                    }
                    else
                    {
                        SFBMeetingVariableInfo meetingVar = CEntityVariableInfoManager.GetMeetingVariableInfoFromDB(strConferenceFocusUri);
                        if (conf.NeedManualClassify() && (!meetingVar.IsManualClassifyDone()) && conf.IsForceManualClassify())
                        {
                            bAllowConferenceShareJoin = false;

                            // if the conference need force to do manual classify but it doesn't done, we notify current user
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Config Deny. meeting force manual classify, before it has done, reject anyone to join.");
                            obContactToEndpointProxy.SendMessageToUser(CSIPTools.SIP_URI_PREFIX + strCurUserUri, "", CSIPTools.GetMsgDenyBeforeManualClassifyDoneByAction(emAction));
                        }
                        else
                        {
                            string strInviter = "";
                            if (EMSFB_ACTION.emMeetingShare == emAction)
                            {
                                CConferenceManager.GetConferenceManager().SetConferenceShareInfo(strConferenceShareUri, strCurUserUri); // Cache the sharer info, this will used in share join action
                            }
                            else
                            {
                                strInviter = (EMSFB_ACTION.emMeetingShareJoin == emAction) ? CConferenceManager.GetConferenceManager().GetSharerInfoByConferenceShareUri(strConferenceShareUri) : "";
                            }

                            PolicyResult obPolicyResult = DoEnforcementForConferenceShare(emAction, conf, strCurUserUri, strInviter, meetingVar);
                            if (null != obPolicyResult)
                            {
                                bAllowConferenceShareJoin = !obPolicyResult.IsDeny(); // Allow, DontCare both need continue and Deny need block

                                // Process obligations
                                ObligationHelper.ProcessMeetingCommonObligations(emAction, obPolicyResult, strInviter, strCurUserUri, ref conf, ref meetingVar);
                            }
                        }
                    }
                }
                else
                {
                    bAllowConferenceShareJoin = true;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Auto allowed, reason: the meeting is not under enforcement.\n");
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Can't find the meeting info with uri:{0}\n", strConferenceFocusUri);
            }
            return bAllowConferenceShareJoin;
        }
        #endregion

        #region Private tool, independence: conference data, policy result
        static private void GetUserAndConferenceShareUriForShareRequest(Request obConferenceShareRequest, out string strCurUserUri, out string strConferenceShareUri)
        {
            // Get user URI and conference focus URI
            string strFromInfo = obConferenceShareRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value;     // <Sharer/Viewer:UserUri>
            string strToInfo = obConferenceShareRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_TO).Value;         // <Sharing:ConferenceUri>

            strCurUserUri = CSIPTools.GetUserAtHost(strFromInfo);
            strConferenceShareUri = CSIPTools.GetUriFromSipAddrHdr(strToInfo);
        }
        static private CConference GetConferenceInfoByFocusUri(string strConferenceFocusUri, bool bNeedProcessExistConference)
        {
            CConferenceManager obCConferenceManager = CConferenceManager.GetConferenceManager();
            CConference obCConference = obCConferenceManager.GetConferenceByFocusUri(strConferenceFocusUri);
            if (null == obCConference)
            {
                // Do not find in cache, need load from persistent info
                obCConference = CConferenceManager.GetConferenceManager().LoadConferenceFromDB(strConferenceFocusUri);
                if (null == obCConference)
                {
                    if (bNeedProcessExistConference)
                    {
                        // May be this conference is created before Enforcer installed
                        // For share operation, we no need create a conference object for it
                        // obCConference = PrepareExistConference(strConferenceFocusUri);   // Need update CSIPParser
                    }
                }
            }
            return obCConference;
        }
        static private PolicyResult DoEnforcementForConferenceShare(EMSFB_ACTION emAction, CConference conf, string strCurUserUri, string strRecipientUser, SFBMeetingVariableInfo meetingVar)
        {
            // get participant
            List<string> lstDistinctParticipants = SFBParticipantManager.GetDistinctParticipantsAsList(meetingVar);

            // check policy
            PolicyResult policyResult = new PolicyResult(CPolicy.kbDefaultPolicyResult ? EnforceResult.Enforce_Allow : EnforceResult.Enforce_Deny);
            CPolicy.Instance().QueryPolicyForMeeting(emAction, strCurUserUri, strRecipientUser, conf, meetingVar, lstDistinctParticipants, policyResult);  // return 0 means query policy failed, no need care

            // do evaluation on condition within obligation
            CondtionObligationEvaluation.DoEvalutionForConditionWithinObligation(policyResult, SFBParticipantManager.GetDistinctParticipantsAsList(meetingVar));
            return policyResult;
        }
        #endregion
    }
}
