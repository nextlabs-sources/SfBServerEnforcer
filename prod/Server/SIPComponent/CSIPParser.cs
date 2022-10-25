using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Rtc.Sip;
using System.Xml;
using System.Diagnostics;
using System.Configuration;
using Nextlabs.SFBServerEnforcer.PolicyHelper;
using SFBCommon.SFBObjectInfo;
using SFBCommon.NLLog;
using SFBCommon.CommandHelper;
using SFBCommon.Common;

using Nextlabs.SFBServerEnforcer.SIPComponent.DataManagement;
using Nextlabs.SFBServerEnforcer.SIPComponent.Common;
using Nextlabs.SFBServerEnforcer.SIPComponent.NLAnalysis;
using Nextlabs.SFBServerEnforcer.SIPComponent.Policy;
using Nextlabs.SFBServerEnforcer.SIPComponent.SIPContentHelper;

namespace Nextlabs.SFBServerEnforcer.SIPComponent
{
    class CSIPParser
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(CSIPParser));
        #endregion

        #region Const/Read only values
        private const string kstrXmlReponseNodeCodeFlag = "code";
        private const string kstrXmlReponseNodeEnvidFlag = "envid";
        private const string kstrXmlReponseNodeTokenFlag = "token";
        private const string kstrXmlReponseNodeUriFlag = "uri";
        private const string kstrChatRoomChannelUriFlag = "channelUri";
        private const string kstrChatRoomFileUriFlag = "fileUrl";
        #endregion

        #region Static constructor
        static CSIPParser()
        {
            m_sipParser = new CSIPParser();
        }
        #endregion

        #region Singleton
        static private CSIPParser m_sipParser;
        static public CSIPParser GetParser()
        {
            return m_sipParser;
        }
        #endregion

        #region Constructors
        // Normal constructor
        private CSIPParser() { }
        // Copy constructor
        private CSIPParser(CSIPParser parser){}
        #endregion

        #region Pulbic functions, parse request, response interface
        public void ParseSIPRequest(Request request, ref Response ourResponse)
        {
            if (request.StandardMethod == Request.StandardMethodType.Invite)
            {
                SIP_INVITE_TYPE emInviteType = CSIPTools.GetInviteRequestType(request);
                if (emInviteType == SIP_INVITE_TYPE.INVITE_IM_INVITE)
                {
                    ParseIMCallInviteRequest(request);
                }
                else if (emInviteType == SIP_INVITE_TYPE.INVITE_CONF_INVITE)
                {
                    ParseConferenceInviteRequest(request,ref ourResponse);
                }
                else if (emInviteType == SIP_INVITE_TYPE.INVITE_CONF_JOIN)
                {
                    ParseConferenceJoinRequest(request, ref ourResponse);
                }
                else if(emInviteType == SIP_INVITE_TYPE.INVITE_PERSISTENT_CHAT_ENDPOINT)
                {
                    //do nothing for Get session with persistent chat endpoint"
                }
                else if (emInviteType == SIP_INVITE_TYPE.INVITE_CONF_SHARE_CREATE)
                {
                    NLConferenceShareAnalyser.ParseConferenceShareCreateRequest(request, ref ourResponse);
                }
                else if (emInviteType == SIP_INVITE_TYPE.INVITE_CONF_SHARE_JOIN)
                {
                    NLConferenceShareAnalyser.ParseConferenceShareJoinRequest(request, ref ourResponse);
                }
            }
            else if (request.StandardMethod == Request.StandardMethodType.Service)
            {
                SIP_SERVICE_TYPE emServiceType = CSIPTools.GetServiceRequestType(request);
                if (emServiceType == SIP_SERVICE_TYPE.SERVICE_CONFERENCE_CREATE)
                {
                   ParseConferenceCreateRequest(request);
                }
                else if(emServiceType == SIP_SERVICE_TYPE.SERVICE_PUBLISH_CATEGORY)
                {
                    ParseCategoryPublishRequest(request, ref ourResponse);
                }
            }
           else if(request.StandardMethod == Request.StandardMethodType.Info)
           {
              ParseInfoSipRequest(request, ref ourResponse);
           }
           else if(request.StandardMethod == Request.StandardMethodType.Bye)
           {
               ParseByeRequest(request);
           }
//            else if(request.StandardMethod == Request.StandardMethodType.Message)
//            {
//                string strMsSender = CSIPTools.GetUserAtHost(request.AllHeaders.FindFirst("Ms-Sender").Value);
//                bool bIsEndProxy = ContactToEndpointProxy.GetProxyContact().IsEndpointProxy(strMsSender);
// 
// 
//            }
        }
        public void ParseSIPResponse(Response sipResponse)
        {   
            try
            {
                Header ContentTypeHdr = sipResponse.AllHeaders.FindFirst(CSIPTools.SIP_HDR_CONTENTTYPE);
                if(ContentTypeHdr!=null)
                {
                    string strContentType = ContentTypeHdr.Value;
                    if (strContentType.Equals("application/cccp+xml", StringComparison.OrdinalIgnoreCase))
                    {
                        ParseCCCPResponse(sipResponse);
                    }
                }
       
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, ex.ToString());
            }



        }
        #endregion

        #region Private functions
        private void ParseCCCPResponse(Response sipResponse)
        {
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(sipResponse.Content);
                XmlNode rootNode = xmldoc.DocumentElement;
                if (rootNode == null)
                    return;

                XmlNode firstChild = rootNode.FirstChild;
                if (firstChild != null)
                {
                    if (firstChild.Name.Equals("addConference", StringComparison.OrdinalIgnoreCase))
                    {
                        ParseConferenceCreateResponse(sipResponse, xmldoc);
                    }
                    else if (firstChild.Name.Equals("getConferences", StringComparison.OrdinalIgnoreCase))
                    {
                        ParseConferenceGetResponse(sipResponse, xmldoc);
                    }
                }
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "ParseCCCPResponse exception:[{0}]\n", ex.ToString());
            }

        }

        private void ParseConferenceGetResponse(Response sipResponse, XmlDocument xmlResponse)
        {
            //get info from xml
            try
            {
                XmlNode confInfoNode = xmlResponse.DocumentElement.FirstChild.FirstChild.FirstChild;
                if (confInfoNode != null)
                {
                    string strFocusUri = confInfoNode.Attributes["entity"].Value;
                    string strFromUser = CSIPTools.GetUserAtHost(sipResponse.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value);

                    CConference conf = CConferenceManager.GetConferenceManager().GetConferenceByFocusUri(strFocusUri);
                    if (conf == null)
                    {
                        conf = CConferenceManager.GetConferenceManager().LoadConferenceFromDB(strFocusUri);
                    }

                    if(conf==null)
                    {
                        string strConfID = CSIPTools.GetConfIDFromConfEntry(strFocusUri);
                        conf = new CConference(strFromUser, strConfID);
                        conf.FocusUri = strFocusUri;

                        CConferenceManager.GetConferenceManager().AddConference(conf);
                    }

                     conf.SFBMeetingType = EMSFB_MeetingType.ScheduleStatic;            

                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Get static conference, Creator={0},confEntry={1}", conf.Creator, conf.FocusUri);

                    //query policy to see if this meeting need enforcer.
                    PolicyResult policyResult = new PolicyResult();
                    CPolicy.Instance().QueryPolicyForMeetingCreate(conf, policyResult);

                    SFBMeetingVariableInfo meetingVar = new SFBMeetingVariableInfo(SFBMeetingVariableInfo.kstrUriFieldName, conf.FocusUri,
                        SFBMeetingVariableInfo.kstrDoneManulClassifyFieldName, SFBMeetingVariableInfo.kstrDoneManulClassifyNo);

                    ParsePolicyResultForMeetingCreate(policyResult, conf, meetingVar);
                    

                    //write conference info to database
                    conf.SaveConferenceInfo();
                    meetingVar.PersistantSave();

                    //notify manual classify, this must be doing after conf and meetingVar is saved.
                    if (conf.NeedManualClassify())
                    {
                        ContactToEndpointProxy.GetAssistantProxyContact().NotifyUserToClassifyMeeting(conf.Creator, conf);
                    }

                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "ParseConferenceGetResponse exception:[{0}]\n", ex.ToString());
            } 
        }

        private void ParsePolicyResultForMeetingCreate(PolicyResult policyResult, CConference conf, SFBMeetingVariableInfo meetingVar)
        {
            //if need enforce
            PolicyObligation obAutoEnforce = policyResult.GetFirstObligationByName(PolicyMain.kStrObNameAutoEnforcement);
            if (obAutoEnforce != null)
            {
                conf.Enforcer = NLMeetingInfo.kstrEnforceAutoYes;
            }
            else
            {
                conf.Enforcer = NLMeetingInfo.kstrEnforceAutoNo;
            }

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "QueryPolicyForMeetingCreate result, conf.Enforce={0}, needEnforce={1}", conf.Enforcer, conf.NeedEnforce());

            //the creator is the classify manager
            meetingVar.AddedClassifyManager(conf.Creator);
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Added creator as classify manager:{0}", conf.Creator);

            //process auto/manual tags obligations 
            if (conf.NeedEnforce())
            {
                //auto tag
                List<PolicyObligation> lstObAutoTag = policyResult.GetAllObligationByName(PolicyMain.KStrObNameAutoClassify);
                if ((lstObAutoTag != null) && (lstObAutoTag.Count > 0))
                {
                     CEntityClassifyManager.ProcessAutoClassifyObligations(lstObAutoTag, meetingVar);
                }
                else
                {
                    // For auto tags, keep the old tags no need remove.
                }

                // manual tag
                // For manual tags, as default there is no manual classification define.
                conf.ManualTagXml = "";
                conf.ForceManualTag = "";
                List<PolicyObligation> lstManualTag = policyResult.GetAllObligationByName(PolicyMain.KStrObNameManualClassify);
                if ((lstManualTag != null) && (lstManualTag.Count > 0))
                {
                    string strForceManualClassify = PolicyMain.KStrObAttributeForceClassifyYes;
                    string strManualTagXmlAll = CEntityClassifyManager.ProcessManualClassifyObligations(lstManualTag, ref strForceManualClassify);
                    if(!string.IsNullOrWhiteSpace(strManualTagXmlAll))
                    {
                        conf.ManualTagXml = strManualTagXmlAll;
                        conf.ForceManualTag = strForceManualClassify;
                    }
                }
            }

            //send notify message(e.g. when user schedule a meeting in outlook, we notify user to use dynamic meeting address.
            PolicyObligation notifyObligation = policyResult.GetFirstObligationByName(PolicyMain.kStrObNameNotification);
            if (notifyObligation != null)
            {
                string strNotifyMsg = notifyObligation.GetAttribute(PolicyMain.kStrObAttributeNotifyMessage);
                EMSFB_NOTIFYINFOTYPE notifyInfoType = NotifyCommandHelper.GetNotifyInfoType(strNotifyMsg);
                if(notifyInfoType==EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNormal)
                {
                    ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(conf.Creator, ""/*"Deny Enter Meeting"*/, strNotifyMsg);    // No need header info for bug38174
                }     
                else if((notifyInfoType==EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNatural) || (notifyInfoType==EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeXML))
                {
                    ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(strNotifyMsg, conf, meetingVar, "", conf.Creator, notifyObligation.PolicyName);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Unknown type of notify message:{0}", strNotifyMsg);
                }
            }

        }

        private void ParseConferenceCreateResponse(Response sipResponse, XmlDocument xmlResponse)
        {
            //get info from xml
            try
            {
                XmlNode confInfoNode = xmlResponse.DocumentElement.FirstChild.FirstChild;
                if (confInfoNode != null)
                {
                    string strFocusUri = confInfoNode.Attributes["entity"].Value;
                    string strFromUser = CSIPTools.GetUserAtHost(sipResponse.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value);

                    string strConferenceID = CSIPTools.GetConferenceIDFromUri(strFocusUri);
                    CConference conf = CConferenceManager.GetConferenceManager().PopupPendingCreateConference(strConferenceID);
                    if(conf==null)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Can't find conference in ParseConferenceCreateResponse, id={0}", strConferenceID);
                        return;
                    }

                    conf.FocusUri = strFocusUri;
                    CConferenceManager.GetConferenceManager().AddConference(conf);

                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Begin query policy, Conference Create response, focusUri={0}", strFocusUri);

                    //query policy to see if this meeting need enforcer.
                    PolicyResult policyResult = new PolicyResult();
                    CPolicy.Instance().QueryPolicyForMeetingCreate(conf, policyResult);

                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Eegin query policy, Conference Create response, focusUri={0}", strFocusUri);

                    SFBMeetingVariableInfo meetingVar = new SFBMeetingVariableInfo(SFBMeetingVariableInfo.kstrUriFieldName, conf.FocusUri,
                        SFBMeetingVariableInfo.kstrDoneManulClassifyFieldName, SFBMeetingVariableInfo.kstrDoneManulClassifyNo);

                    ParsePolicyResultForMeetingCreate(policyResult, conf, meetingVar);

                    //write conference info to database
                   conf.SaveConferenceInfo();
                   meetingVar.PersistantSave();

                   //notify user to classify the meeting scheduled, this must called after meetingVar saved
                   if ((conf.SFBMeetingType == EMSFB_MeetingType.ScheduleDynamic) && conf.NeedManualClassify())
                   {
                       ContactToEndpointProxy.GetAssistantProxyContact().NotifyUserToClassifyMeeting(conf.Creator, conf);
                   }
                }
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, ex.ToString());
            }
        }

        private void ParseIMCallInviteRequest(Request sipRequest)
        {
            try
            {
                string strCallID = sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_CALLID).Value;
                string strFromUser = CSIPTools.GetUserAtHost(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value);
                string strTo = CSIPTools.GetUserAtHost(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_TO).Value);
                string strConverID = sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_CONVERSTATIONID).Value;

                CIMCall imCall = new CIMCall(strFromUser, strTo, strCallID, strConverID);
                CIMCallManager.GetIMCallManager().AddIMCall(imCall);
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, ex.ToString());
            }

        }

        private void ParseConferenceCreateRequest(Request sipRequest)
        { 
            CConference conf = GetConferenceFromRequest(sipRequest);
            if(conf != null)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "ParseConferenceCreateRequest, creator={0}, id={1}, createtime={2}, expire-tim={3}, meetingType={4}", conf.Creator, conf.MeetingID, conf.CreateTime, conf.ExpireTime, conf.SFBMeetingType.ToString());
                CConferenceManager.GetConferenceManager().AddPendingCreateConference(conf);
            }
        }

        private void ParseCategoryPublishRequest(Request sipRequest, ref Response ourResponse)
        {
            //get info from xml
            XmlDocument xmldoc = new XmlDocument();
            try
            {
                xmldoc.LoadXml(sipRequest.Content);
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Except on ParseCategoryPublishRequest loadxml:{0}", ex.ToString());
                return;
            }
         
            const string strDefaultNameSpace = "defaultNS";
            XmlNamespaceManager xmlnsm = new XmlNamespaceManager(xmldoc.NameTable);
            xmlnsm.AddNamespace(strDefaultNameSpace, "http://schemas.microsoft.com/2006/09/sip/rich-presence");

            XmlNode publishNode = xmldoc.SelectSingleNode("//" + strDefaultNameSpace + ":publication", xmlnsm);
            if(null != publishNode)
            {
                XmlAttribute xmlAttr = publishNode.Attributes["categoryName"];
                if((null!=xmlAttr) && !string.IsNullOrWhiteSpace(xmlAttr.Value))
                {
                    if(xmlAttr.Value.Equals("roomSetting"))
                    {
                        try
                        {
                            ParseCategoryPublishRoomSettingRequest(sipRequest, xmldoc, xmlnsm, strDefaultNameSpace, ref ourResponse);
                        }
                        catch(Exception ex)
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Exception on ParseCategoryPublishRoomSettingRequest:{0}" + ex.ToString());
                        }

                    }
                }
            }
        }

        private void ParseCategoryPublishRoomSettingRequest(Request sipRequest, XmlDocument xmlDoc, XmlNamespaceManager xmlnsm, string strDefaultNs, ref Response ourResponse)
        {
            string strFromUser = CSIPTools.GetUserAtHost(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value);

            xmlnsm.AddNamespace("defaultNs2", "http://schemas.microsoft.com/2010/12/sip/propertyList");
            XmlNodeList lstNodeProperty = xmlDoc.SelectNodes("//" + "defaultNs2" + ":property", xmlnsm);
            if ((null != lstNodeProperty) && (lstNodeProperty.Count>0))
            {
                string strRoomID = "";
                string strRoomName = "";
                foreach (XmlNode nodeProp in lstNodeProperty)
                {
                    if (nodeProp.Attributes["name"].Value.Equals("url"))
                    {
                        string strRoomUri = nodeProp.FirstChild.InnerText;
                        strRoomID = CSIPTools.GetChatRoomIDFromUri(strRoomUri);
                    }
                    else if (nodeProp.Attributes["name"].Value.Equals("name"))
                    {
                        strRoomName = nodeProp.FirstChild.InnerText;
                    }
                }

                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Follow chat room, user={0}, roomName={1}, roomID={2}", strFromUser, strRoomName, strRoomID);
                CChatRoomManager chatRoomManage = CChatRoomManager.GetInstance();
                CChatRoom chatRoom = chatRoomManage.GetChatRoomByID(strRoomID);
                if (chatRoom == null)
                {
                    chatRoom = chatRoomManage.GetChatRoomFromDB(strRoomID);
                }

                if ((chatRoom != null) && chatRoom.NeedEnforce())
                {
                    SFBChatRoomVariableInfo roomVar = CEntityVariableInfoManager.GetChatroomVariableInfoFromDB(chatRoom.ID);
                    DoEnforceForUserJoinChatRoom(strFromUser, chatRoom, roomVar, sipRequest, ref ourResponse);
                }
            }
        }

        private bool  DoEnforceForUserJoinChatRoom(string strFromUser, CChatRoom chatRoom, SFBChatRoomVariableInfo roomVar, Request sipRequest, ref Response ourResponse)
        {
            //get participant
            List<string> lstDistinctParticipants = SFBCommon.SFBObjectInfo.SFBParticipantManager.GetDistinctParticipantsAsList(roomVar);
            SFBCommon.Common.CommonHelper.ListStringRemove(lstDistinctParticipants, strFromUser);

            //query policy
            PolicyResult policyResult = new PolicyResult();
            CPolicy.Instance().QueryPolicyForChatRoomJoin(strFromUser, chatRoom, roomVar, lstDistinctParticipants, policyResult);

            //do evaluation on condition within obligation
            CondtionObligationEvaluation.DoEvalutionForConditionWithinObligation(policyResult, SFBParticipantManager.GetDistinctParticipantsAsList(roomVar) );

            bool bAllow = !policyResult.IsDeny();
            if (!bAllow)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Deny {0} join chat room:{1}", strFromUser, chatRoom.Name);
                ourResponse = sipRequest.CreateResponse(440);

                //send notify message on deny
                PolicyObligation notifyObligation = policyResult.GetFirstObligationByName(PolicyMain.kStrObNameNotification);
                if (notifyObligation != null)
                {
                    string strNotifyMsg = notifyObligation.GetAttribute(PolicyMain.kStrObAttributeNotifyMessage);
                    EMSFB_NOTIFYINFOTYPE notifyInfoType = NotifyCommandHelper.GetNotifyInfoType(strNotifyMsg);
                    if (notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNormal)
                    {
                        ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(strFromUser, ""/*"Deny join chat room"*/, strNotifyMsg); // No need header info for bug38174    
                    }
                    else if ((notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNatural) || (notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeXML))
                    {
                        ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(strNotifyMsg, chatRoom, roomVar, "", strFromUser, notifyObligation.PolicyName);
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Unknown type of notify message:{0}", strNotifyMsg);
                    }
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Allow {0} join chat room:{1}, we will process other obligation on Join response, because it maybe denied by Lync itself.", strFromUser, chatRoom.Name);
            }

            return bAllow;
        }

        private void DoEnforceForUserJoinChatRoomSuccess(string strFromUser, CChatRoom chatRoom, SFBChatRoomVariableInfo roomVar, Request sipRequest)
        {
            //get participant
            List<string> lstDistinctParticipants = SFBCommon.SFBObjectInfo.SFBParticipantManager.GetDistinctParticipantsAsList(roomVar);
            SFBCommon.Common.CommonHelper.ListStringRemove(lstDistinctParticipants, strFromUser);

            //query policy
            PolicyResult policyResult = new PolicyResult();
            CPolicy.Instance().QueryPolicyForChatRoomJoin(strFromUser, chatRoom, roomVar, lstDistinctParticipants, policyResult);

            //do evaluation on condition within obligation
            CondtionObligationEvaluation.DoEvalutionForConditionWithinObligation(policyResult, SFBParticipantManager.GetDistinctParticipantsAsList(roomVar));

            if (policyResult.IsDeny())
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "An error happened, we need to deny user join chat room on join request not join response");
            }
            else
            {
                //auto classification
                List<PolicyObligation> lstObAutoTag = policyResult.GetAllObligationByName(PolicyMain.KStrObNameAutoClassify);
                if ((lstObAutoTag != null) && (lstObAutoTag.Count > 0))
                {
                    CEntityClassifyManager.ProcessAutoClassifyObligations(lstObAutoTag, roomVar);
                }

                //send notify message
                PolicyObligation notifyObligation = policyResult.GetFirstObligationByName(PolicyMain.kStrObNameNotification);
                if (notifyObligation != null)
                {
                    string strNotifyMsg = notifyObligation.GetAttribute(PolicyMain.kStrObAttributeNotifyMessage);
                    EMSFB_NOTIFYINFOTYPE notifyInfoType = NotifyCommandHelper.GetNotifyInfoType(strNotifyMsg);
                    if (notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNormal)
                    {
                        ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(strFromUser, ""/*"Deny join chat room"*/, strNotifyMsg); // No need header info for bug38174
                    }
                    else if ((notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNatural) || (notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeXML))
                    {
                        ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(strNotifyMsg, chatRoom, roomVar, "", strFromUser, notifyObligation.PolicyName);
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Unknown type of notify message:{0}", strNotifyMsg);
                    }
                }
            }
        }

        private CConference GetConferenceFromRequest(Request sipRequest)
        {
            string strFromUser = CSIPTools.GetUserAtHost(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value);
            string strConfID="";
            //get info from xml
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(sipRequest.Content);

            XmlNamespaceManager xmlnsm = new XmlNamespaceManager(xmldoc.NameTable);
            xmlnsm.AddNamespace("myaddconference", "urn:ietf:params:xml:ns:cccp");
            xmlnsm.AddNamespace("mscp", "http://schemas.microsoft.com/rtc/2005/08/cccpextensions");
            xmlnsm.AddNamespace("ci", "urn:ietf:params:xml:ns:conference-info");
            xmlnsm.AddNamespace("msci", "http://schemas.microsoft.com/rtc/2005/08/confinfoextensions");

            //get conference id
            XmlNode confIDNode = xmldoc.DocumentElement.SelectSingleNode("//msci:conference-id", xmlnsm);
            if(confIDNode!=null)
            {
                strConfID = confIDNode.InnerText;
            }

            //is static?
            //note: static meeting is also created by client when he first use his static meeting
            bool bStatic = false;
            XmlNode confInfoNode = xmldoc.DocumentElement.SelectSingleNode("//ci:conference-info", xmlnsm);
            if (confInfoNode != null)
            {
                XmlAttribute staticAttr = confInfoNode.Attributes["static"];
                if (staticAttr != null && staticAttr.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    bStatic = true;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Static meeting created: confID={0}", strConfID);
                }
            }


            //get expire time
            string strExpireTime = "";
            XmlNode expireNode = xmldoc.DocumentElement.SelectSingleNode("//msci:expiry-time", xmlnsm);
            if(expireNode!=null)
            {
                strExpireTime = expireNode.InnerText;
            }

            //get current time as the create time of the meeting
            string strCreateTime = DateTime.UtcNow.ToString(SFBObjectInfo.kstrTimeFormatUTCSecond);

            //
            if(strConfID.Length>0)
            {
                CConference conf = new CConference(strFromUser, strConfID)
                {
                    CreateTime = strCreateTime,
                    ExpireTime = strExpireTime,
                    SFBMeetingType = bStatic ? EMSFB_MeetingType.ScheduleStatic : ( (expireNode == null) ? EMSFB_MeetingType.ScheduleDynamic : EMSFB_MeetingType.ClientMeeting )
                };

                string strCallID = sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_CALLID).Value;
                conf.CreateCallID = strCallID;
                return conf;
            }

            return null;
        }

        private void ParseConferenceInviteRequest(Request sipRequest, ref Response ourResponse)
        {
            //get conference information from request
            string strConfUri="";
            string strConverSationID="";
            GetConfInviteRequestInfo(sipRequest, ref strConfUri, ref strConverSationID);

            //
            string strToUser = CSIPTools.GetUserAtHost(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_TO).Value);
            string strFromUser = CSIPTools.GetUserAtHost(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value);
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo,"Invite meeting request, from={0}, to={1}, meeting={2}", strFromUser, strToUser, strConfUri);

            //get conference
            CConference conf = CConferenceManager.GetConferenceManager().GetConferenceByFocusUri(strConfUri);
            if(conf==null)
            {
                conf = CConferenceManager.GetConferenceManager().LoadConferenceFromDB(strConfUri);
                if (conf == null)
                {//may be this conference is created before Enforcer installed, we create a conference object for it
                    conf = PrepareExistConference(strConfUri);
                }
            }

            if(conf != null)
            {
                //if this Invited request based on the previous IM conversation
                if(conf.PreIMCall==null)
                {
                    CIMCall imCall = CIMCallManager.GetIMCallManager().GetIMCallByConversationID(strConverSationID);
                    if (imCall != null)
                    {
                        conf.PreIMCall = imCall;
                    }
                }

           
                if (!conf.NeedEnforce())
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo,"Auto allowed, reason: the meeting is not under enforcement.");
                }
                else if (ContactToEndpointProxy.GetAgentProxyContact().IsEndpointProxy(strToUser))
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "auto allowed. reason: it is EndpointProxy");
                }
                else if(conf.IsTheOldSpeaker(strToUser))
                {//Invite the user that in the previous conversation
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "auto allowed, reason: it is a User in previous IM call.");

                }
                else 
                {
                    SFBMeetingVariableInfo meetingVar = CEntityVariableInfoManager.GetMeetingVariableInfoFromDB(strConfUri);
                    if (conf.NeedManualClassify() && (!meetingVar.IsManualClassifyDone()) && conf.IsForceManualClassify())
                    {//if the conference need manual classify but it doesn't done, we notify the inviter
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Config Deny. meeting needs manual classify, before it has done, reject to invite anyone.");
                        ourResponse = sipRequest.CreateResponse(606);
                        ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(CSIPTools.SIP_URI_PREFIX + strFromUser, "", SIPComponentConfig.kstrMeetingInviteMsgDenyBeforeManualClassifyDone); 
                    }
                    else
                    {
                        //get participant
                        List<string> lstDistinctParticipants = SFBCommon.SFBObjectInfo.SFBParticipantManager.GetDistinctParticipantsAsList(meetingVar);
                        SFBCommon.Common.CommonHelper.ListStringRemove(lstDistinctParticipants, strFromUser);

                        PolicyResult policyResult = new PolicyResult();
                        CPolicy.Instance().QueryPolicyForMeetingInvite(strFromUser, strToUser, conf, meetingVar, lstDistinctParticipants, policyResult);
                        if (policyResult.IsDeny())
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Policy Deny");

                            ourResponse = sipRequest.CreateResponse(606);

                            //send notify message
                            PolicyObligation notifyObligation = policyResult.GetFirstObligationByName(PolicyMain.kStrObNameNotification);
                            if (notifyObligation != null)
                            {
                                string strNotifyMsg = notifyObligation.GetAttribute(PolicyMain.kStrObAttributeNotifyMessage);
                                EMSFB_NOTIFYINFOTYPE notifyInfoType = NotifyCommandHelper.GetNotifyInfoType(strNotifyMsg);
                                if (notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNormal)
                                {
                                    ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(CSIPTools.SIP_URI_PREFIX + strFromUser, ""/*"Deny Invite"*/, strNotifyMsg);   // No need header info for bug38174
                                }
                                else if ((notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNatural) || (notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeXML))
                                {
                                    ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(strNotifyMsg, conf, meetingVar, strFromUser, strToUser, notifyObligation.PolicyName);
                                }
                                else
                                {
                                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Unknown type of notify message:{0}", strNotifyMsg);
                                }
                            }
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Policy Allow");
                        }
                    }

                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "can't find the meeting:{0}", strConfUri);
            }


        }

        private void GetConfInviteRequestInfo(Request sipRequest, ref string strConfUri, ref string strConverationID)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(sipRequest.Content);

            XmlNode confUriNode = xmldoc.DocumentElement.SelectSingleNode("//focus-uri");
            if(confUriNode != null)
            {
                strConfUri = confUriNode.InnerText;
            }

            XmlNode confConverationID = xmldoc.DocumentElement.SelectSingleNode("//conversation-id");
            if(confConverationID != null)
            {
                strConverationID = confConverationID.InnerText;
            }
        }

        private void ParseConferenceJoinRequest(Request sipRequest, ref Response ourResponse)
        {
            System.DateTime timeBeginJoinConference = System.DateTime.Now;
           
            string strConfEntry = CSIPTools.GetUriFromSipAddrHdr(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_TO).Value);
            string strFromHdr = sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value;
            string strFromUser = CSIPTools.GetUserAtHost(strFromHdr);
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Join meeting request, from={0}, meeting={1}", strFromUser, strConfEntry);

            CConference conf = CConferenceManager.GetConferenceManager().GetConferenceByFocusUri(strConfEntry);
            if (conf == null)
            {
                conf = CConferenceManager.GetConferenceManager().LoadConferenceFromDB(strConfEntry);
                if(conf==null)
                {//may be this conference is created before Enforcer installed, we create a conference object for it
                    conf = PrepareExistConference(strConfEntry);
                }
            }

            bool bDenyJoin = false;
            SFBMeetingVariableInfo meetingVar = CEntityVariableInfoManager.GetMeetingVariableInfoFromDB(strConfEntry);
            if(conf != null)
            {     
                if(!conf.NeedEnforce())
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Auto allowed, reason: the meeting is not under enforcement.");
                }
                else if (ContactToEndpointProxy.GetAgentProxyContact().IsEndpointProxy(strFromUser))
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Auto allowed, reason: it is EndpointProxy");
                }
                else if(conf.IsCreator(strFromUser))
                {//the creator enter the conference
                     theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Auto allowed, reason: it is the creator");
                     DoEnforcementForConferenceJoinRequest(conf, strFromUser, meetingVar, true, false);//may be have obligations, so must query policy
                     
                    if(conf.NeedManualClassify() && !meetingVar.IsManualClassifyDone() )
                    {
                        ContactToEndpointProxy.GetAssistantProxyContact().NotifyUserToClassifyMeeting(strFromUser, conf);
                    }

                }
                 else if(conf.IsTheOldSpeaker(strFromUser))
                {//old speaker enter the conference
                     theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Auto allowed, reason: the user is in previours IM call.");
                     DoEnforcementForConferenceJoinRequest(conf, strFromUser, meetingVar, false, true);
                }
                else
                {// user enter the conference
                    if (conf.NeedManualClassify() && (!meetingVar.IsManualClassifyDone()) && conf.IsForceManualClassify() )
                    {//if the conference need manual classify but it doesn't done, we notify the invitee
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Config Deny. meeting force manual classify, before it has done, reject anyone to join.");
                        bDenyJoin = true;
                        ourResponse = sipRequest.CreateResponse(600);
                        ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(CSIPTools.SIP_URI_PREFIX + strFromUser, "", SIPComponentConfig.kstrMeetingJoinMsgDenyBeforeManualClassifyDone);
                    }
                    else
                    {
                        bDenyJoin = DoEnforcementForConferenceJoinRequest(conf, strFromUser, meetingVar, false, false);
                        if(bDenyJoin)
                        {
                            ourResponse = sipRequest.CreateResponse(600);
                        }
                    }
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Can't find the meeting:{0}", strConfEntry);
            }

            //Record the join action
            if (!bDenyJoin)
            {
                string strEpid = CSIPTools.GetUserParameterFromSIPAddress(strFromHdr, SIP_ADDRESS_PARAMETER.EPID);
                string strTag = CSIPTools.GetUserParameterFromSIPAddress(strFromHdr, SIP_ADDRESS_PARAMETER.TAG);
                SFBParticipantManager.AddedParticipantWithParameter(meetingVar, strFromUser, strEpid, strTag);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "meeting add participate: user={0}, {1}, {2}, meeting={3}", strFromUser, strEpid, strTag, strConfEntry);

                meetingVar.PersistantSave();
            }

            if(SIPComponentConfig.kbNeedRecordPerformanceLog)
            {
                System.DateTime timeEndJoinConference = System.DateTime.Now;
                PerformanceMoniter obPerformanceMoniter = new PerformanceMoniter(EMSFB_MONITERTYPE.emServerMeetingPerformanceMoniter, SIPComponentMain.m_strPerformaceLogFolder);
                obPerformanceMoniter.AddPerformanceInfo(strConfEntry, timeBeginJoinConference.ToString(PerformanceMoniter.kstrTimeFormat), timeEndJoinConference.ToString(PerformanceMoniter.kstrTimeFormat), "\n");
            }
          
        }

        private bool DoEnforcementForConferenceJoinRequest(CConference conf, string strFromUser, SFBMeetingVariableInfo meetingVar, bool IsCreatorJoin, bool IsOldSpeakerJoin)
        {
            bool bDenyJoin = false;
           
            //get participant
            List<string> lstDistinctParticipants = SFBCommon.SFBObjectInfo.SFBParticipantManager.GetDistinctParticipantsAsList(meetingVar);
            SFBCommon.Common.CommonHelper.ListStringRemove(lstDistinctParticipants, strFromUser);

            //check policy
            PolicyResult policyResult = new PolicyResult();
            CPolicy.Instance().QueryPolicyForMeetingJoin(strFromUser, conf, meetingVar, lstDistinctParticipants, policyResult);

            //do evaluation on condition within obligation
            CondtionObligationEvaluation.DoEvalutionForConditionWithinObligation(policyResult, SFBParticipantManager.GetDistinctParticipantsAsList(meetingVar));

            //do enforcement
            bool bAutoAllow = IsCreatorJoin || IsOldSpeakerJoin;
            if(policyResult.IsDeny())
            {
                if (!bAutoAllow)
                {
                    bDenyJoin = true;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Deny Join meeting, reason: policy deny.");
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Policy deny Creator/OldSpeaker join meeting, ignore this policy.");
                    return false;
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Allow Join meeting, reason:{0}", IsCreatorJoin ? "Creator join" : (IsOldSpeakerJoin ? "Old Speaker Join" : "Policy Allow"));

                //auto classification
                List<PolicyObligation> lstObAutoTag = policyResult.GetAllObligationByName(PolicyMain.KStrObNameAutoClassify);
                if ((lstObAutoTag != null) && (lstObAutoTag.Count > 0))
                {
                    CEntityClassifyManager.ProcessAutoClassifyObligations(lstObAutoTag, meetingVar);
                }

                //check if the join-er can classify the conference
                if(!IsCreatorJoin)
                {
                    PolicyObligation obligationClassifyMgr = policyResult.GetFirstObligationByName(PolicyMain.KStrObNameEnableClassifyManager);
                    if (null != obligationClassifyMgr)
                    {
                        meetingVar.AddedClassifyManager(strFromUser);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Added classify manager:{0}", strFromUser);
                    }
                    else
                    {//remove classify mananger
                        meetingVar.RemoveClassifyManager(strFromUser);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Remove classify manager:{0}", strFromUser);
                    }
                }
          
            }

            //send notify message
            PolicyObligation notifyObligation = policyResult.GetFirstObligationByName(PolicyMain.kStrObNameNotification);
            if (notifyObligation != null)
            {
                string strNotifyMsg = notifyObligation.GetAttribute(PolicyMain.kStrObAttributeNotifyMessage);
                EMSFB_NOTIFYINFOTYPE notifyInfoType = NotifyCommandHelper.GetNotifyInfoType(strNotifyMsg);
                if (notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNormal)
                {
                    ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(CSIPTools.SIP_URI_PREFIX + strFromUser, ""/*"Deny Enter Meeting"*/, strNotifyMsg);    // No need header info for bug38174
                }
                else if ((notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNatural) || (notifyInfoType == EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeXML))
                {
                    ContactToEndpointProxy.GetAgentProxyContact().SendMessageToUser(strNotifyMsg, conf, meetingVar, "", strFromUser, notifyObligation.PolicyName);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Unknown type of notify message:{0}", strNotifyMsg);
                }
            }

            return bDenyJoin;
        }

        private void ParseInfoSipRequest(Request sipRequest, ref Response ourResponse)
        {
            try
            {
                string strContentLength = sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_CONTENTLENGTH).Value;
                int nContentLen = int.Parse(strContentLength);
                string strContentType = sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_CONTENTTYPE).Value;
                if (10 < nContentLen)
                {
                    SIPContent obSIPContent = SIPContent.CreateSIPContentObjByFlag(strContentType, sipRequest.Content);
                    EMSIP_CONTENT_TYPE emContentType =  obSIPContent.GetContentType();
                    if (EMSIP_CONTENT_TYPE.emContent_Text_Plain == emContentType)
                    {
                        SIPContentTextPlain obSIPContentTextPlain = obSIPContent as SIPContentTextPlain;
                        if (null != obSIPContentTextPlain)
                        {
                            XCCosContent obXCCosContent = obSIPContentTextPlain.GetXCCosContent();
                            if (null != obXCCosContent)
                            {
                                ParsePersistentChatRoomInfoRequest(sipRequest, ref ourResponse, obXCCosContent);
                            }
                            else
                            {
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Content type TextPlain but content info is not XCCos.\n");
                            }
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Logic error, content type is TextPlain but object cannot convert to TextPlain.\n");
                        }
                    }
                    else if (EMSIP_CONTENT_TYPE.emContent_Application_Ccp_Xml == emContentType)
                    {
                        ParseConferenceInfoRequest(sipRequest, ref ourResponse);
                    }
                }
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, ex.ToString());
            }
        }

        private void ParsePersistentChatRoomLeaveCommand(XmlNode xmlCmdNode, Request sipRequest)
        {
            string strRoomUri = "";
            XmlNode nodeChanid = xmlCmdNode.FirstChild.FirstChild;
            if (nodeChanid != null)
            {
                strRoomUri = nodeChanid.Attributes["uri"].Value;
            }

            //find room
            CChatRoom chatRoom = CChatRoomManager.GetInstance().GetChatRoomByUri(strRoomUri);
            if(chatRoom != null)
            {
                string strFromHdr = sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value;
                string strFromUser = CSIPTools.GetUserAtHost(strFromHdr);
                string strEpid = CSIPTools.GetUserParameterFromSIPAddress(strFromHdr, SIP_ADDRESS_PARAMETER.EPID);
                string strTag = CSIPTools.GetUserParameterFromSIPAddress(strFromHdr, SIP_ADDRESS_PARAMETER.TAG);

                SFBChatRoomVariableInfo chatroomVar = CEntityVariableInfoManager.GetChatroomVariableInfoFromDB(chatRoom.ID);
                SFBCommon.SFBObjectInfo.SFBParticipantManager.RemoveParticipantWithParameter(chatroomVar, strFromUser, strEpid, strTag);
                chatroomVar.PersistantSave();
 
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "{0} leave chat room:{1}", strFromUser, chatRoom.Name);
            }
        }

        private XmlNode CreateFailedNodeForBatchJoinResult(XmlDocument xmlDoc, XmlNamespaceManager xmlnsMgr, string strDefaultNs, string strRoomID, string strRoomDomain)
        {
            try
            {
                XmlNode nodeFailed = xmlDoc.CreateNode(XmlNodeType.Element, "fib", xmlnsMgr.LookupNamespace(strDefaultNs));

                XmlAttribute attrKey = xmlDoc.CreateAttribute("key");
                attrKey.Value = "OPERATION_FAILED";
                nodeFailed.Attributes.Append(attrKey);

                XmlAttribute attrValue = xmlDoc.CreateAttribute("value");
                attrValue.Value = strRoomID;
                nodeFailed.Attributes.Append(attrValue);

                XmlAttribute attrDomain = xmlDoc.CreateAttribute("domain");
                attrDomain.Value = strRoomDomain;
                nodeFailed.Attributes.Append(attrDomain);

                return nodeFailed;
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "CreateFailedNodeForBatchJoinResult failed, roomID={0}, ex={1}", strRoomID, ex.ToString());
                return null;
            }
        }

        private void ParsePeristentChatRoomBatchJoinResult(XmlDocument xmlDoc, XmlNamespaceManager xmlnsMgr, string strDefaultNs, Request sipRequest)
        {
           XmlNodeList lstNodeChanib =  xmlDoc.DocumentElement.SelectNodes("//" + strDefaultNs + ":chanib", xmlnsMgr);
            if((null!=lstNodeChanib) && lstNodeChanib.Count>0)
            {
                string strToHdr = sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_TO).Value;
                string strUser = CSIPTools.GetUserAtHost(strToHdr);

                bool bModifyContent = false;
                foreach(XmlNode nodeChanib in lstNodeChanib)
                {
                    string strRoomUri = nodeChanib.Attributes["uri"].Value;
                    string strRoomID = CSIPTools.GetChatRoomIDFromUri(strRoomUri);
                    CChatRoomManager chatRoomManager = CChatRoomManager.GetInstance();
                    CChatRoom chatRoom = chatRoomManager.GetChatRoomByID(strRoomID);
                    if(null==chatRoom)
                    {
                        chatRoom = chatRoomManager.GetChatRoomFromDB(strRoomID);
                    }

                    if((null!=chatRoom) && chatRoom.NeedEnforce())
                    {
                        bool bAllow = true;
                        SFBChatRoomVariableInfo roomVar = CEntityVariableInfoManager.GetChatroomVariableInfoFromDB(chatRoom.ID);
                        //get participant
                        List<string> lstDistinctParticipants = SFBCommon.SFBObjectInfo.SFBParticipantManager.GetDistinctParticipantsAsList(roomVar);
                        SFBCommon.Common.CommonHelper.ListStringRemove(lstDistinctParticipants, strUser);

                        PolicyResult policyResult = new PolicyResult();
                        CPolicy.Instance().QueryPolicyForChatRoomJoin(strUser, chatRoom, roomVar, lstDistinctParticipants,  policyResult);
                        if (policyResult.IsDeny())
                        {
                            bAllow = false;
                            bModifyContent = true;

                            //added node represent failed
                            XmlNode nodeFailed = CreateFailedNodeForBatchJoinResult(xmlDoc, xmlnsMgr, strDefaultNs, strRoomID, CSIPTools.GetChatRoomDomainFromUri(strRoomUri));
                            if (null != nodeFailed)
                            {
                                nodeChanib.ParentNode.AppendChild(nodeFailed);
                                nodeChanib.ParentNode.RemoveChild(nodeChanib);
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Remove batch join chat room:{0}", chatRoom.Name);
                            }

                        }

                        //added participate
                        if(bAllow)
                        {
                            string strEpid = CSIPTools.GetUserParameterFromSIPAddress(strToHdr, SIP_ADDRESS_PARAMETER.EPID);
                            string strTag = CSIPTools.GetUserParameterFromSIPAddress(strToHdr, SIP_ADDRESS_PARAMETER.TAG);

                            SFBParticipantManager.AddedParticipantWithParameter(roomVar, strUser, strEpid, strTag);
                            roomVar.PersistantSave();   
                        }
                    }
                }

                //modify content in Reqesut.
                if(bModifyContent)
                {
                    string strNewContent = SFBCommon.Common.CommonHelper.XmlDocmentToString(xmlDoc);
                    sipRequest.Content = strNewContent;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "New Content:{0}", strNewContent);
                }

            }

        }

        private void ParsePersistentChatRoomJoinCommand(XmlNode xmlCmdNode, Request sipRequest, ref Response ourResponse)
        {
            string strRoomID = "";
            XmlNode nodeChanid = xmlCmdNode.FirstChild.FirstChild;
            if (nodeChanid != null)
            {
                strRoomID = nodeChanid.Attributes["value"].Value;//room id with format: eb1f978a-f9f1-4a8b-b56a-bb62f91ae423
            }

            CChatRoomManager chatRoomManager = CChatRoomManager.GetInstance();
            CChatRoom chatRoom = chatRoomManager.GetChatRoomByID(strRoomID);
            if(chatRoom==null)
            {
                chatRoom = chatRoomManager.GetChatRoomFromDB(strRoomID);
            }

            if ((chatRoom != null) && chatRoom.NeedEnforce())
            {
                string strFromUser = CSIPTools.GetUserAtHost(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value);
                bool bAllow = true;
                SFBChatRoomVariableInfo roomVar = CEntityVariableInfoManager.GetChatroomVariableInfoFromDB(chatRoom.ID);
           
                //for chat room join, the behavior for creator is the same with normal member.
                bAllow = DoEnforceForUserJoinChatRoom(strFromUser, chatRoom, roomVar, sipRequest, ref ourResponse);        

                //update meeting variable
                if (bAllow)
                {
                   //   CParticipateManager.AddedParticipate(roomVar, strFromUser); we will added the user to participate on Join-Response.
                   // roomVar.PersistantSave();
                }

            }
            else if(chatRoom==null)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Can't find ChatRoom information");
            }

        }

        //process join result.
        //only process allow result
        private void ParsePersistentChatRoomJoinResult(XmlNode xmlRplNode, XmlNamespaceManager xmlnsMgr, string strDefaultNs, Request sipRequest)
        {
            //get result
            bool bJoinSuccess = false;

            string strRespCode = XMLTools.GetXMLSingleNodeAttributeValue(xmlRplNode,"//" + strDefaultNs + ":resp", xmlnsMgr, kstrXmlReponseNodeCodeFlag);
            bJoinSuccess = strRespCode.Equals("200");

            if (bJoinSuccess)
            {
                //get user who join the chat room
                string strToHdr = sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_TO).Value;
                string strToUser = CSIPTools.GetUserAtHost(strToHdr);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "{0} join chat room result:{1}, we will query policy to do obligation if join result is success.", strToUser, bJoinSuccess.ToString());

                //query policy when successed join a chat room
                if (bJoinSuccess)
                {
                    //get room uri
                    string strRoomUri = "";
                    try
                    {
                        strRoomUri = XMLTools.GetXMLSingleNodeAttributeValue(xmlRplNode,"//" + strDefaultNs + ":chanib", xmlnsMgr, kstrXmlReponseNodeUriFlag);
                    }
                    catch (Exception ex)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "ParsePersistentChatRoomJoinResult, Get chatroom Uri exception:{0}", ex.ToString());
                    }

                    if (!string.IsNullOrWhiteSpace(strRoomUri))
                    {
                        string strRoomID = CSIPTools.GetChatRoomIDFromUri(strRoomUri);
                        CChatRoomManager chatRoomManager = CChatRoomManager.GetInstance();
                        CChatRoom chatRoom = chatRoomManager.GetChatRoomByID(strRoomID);
                        if (null == chatRoom)
                        {
                            chatRoom = chatRoomManager.GetChatRoomFromDB(strRoomID);
                        }

                        if ((null != chatRoom) && chatRoom.NeedEnforce())
                        {
                            SFBChatRoomVariableInfo roomVar = CEntityVariableInfoManager.GetChatroomVariableInfoFromDB(chatRoom.ID);

                            DoEnforceForUserJoinChatRoomSuccess(strToUser, chatRoom, roomVar, sipRequest);

                            //update meeting variable
                            string strEpid = CSIPTools.GetUserParameterFromSIPAddress(strToHdr, SIP_ADDRESS_PARAMETER.EPID);
                            string strTag = CSIPTools.GetUserParameterFromSIPAddress(strToHdr, SIP_ADDRESS_PARAMETER.TAG);
                            SFBParticipantManager.AddedParticipantWithParameter(roomVar, strToUser, strEpid, strTag);
                            roomVar.PersistantSave();
                        }
                    }
                }
            }
        }

        private void ParsePersistentChatRoomInfoRequest(Request sipRequest, ref Response ourResponse, XCCosContent obXCCosContent)
        {
            try
            {
                XmlDocument xmlDoc = obXCCosContent.GetXCCosXmlDocument();
                if ((null == xmlDoc) || (null == xmlDoc.DocumentElement) || (null == xmlDoc.DocumentElement.FirstChild))
                {
                    return;
                }

                XmlNamespaceManager xmlnsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
                xmlnsMgr.AddNamespace(XCCosContent.kstrDefaultNsPrefix, XCCosContent.kstrXCCosXMLNs);
                xmlnsMgr.AddNamespace(XCCosContent.kstrNsXsdFlag, XCCosContent.kstrXCCosNsXsdValue);
                xmlnsMgr.AddNamespace(XCCosContent.kstrNsXsiFlag, XCCosContent.kstrXCCosNsXsiValue);

                XmlNode firstChildNode = xmlDoc.DocumentElement.FirstChild;
                EMSIP_CONTENT_XCCOS_TYPE emContentXCCosType = obXCCosContent.GetContentXCCosType();
                switch (emContentXCCosType)
                {
                case EMSIP_CONTENT_XCCOS_TYPE.emXCCosCmdJoin:
                {
                    //join chat room
                    ParsePersistentChatRoomJoinCommand(firstChildNode, sipRequest, ref ourResponse);
                    break;
                }
                case EMSIP_CONTENT_XCCOS_TYPE.emXCCosCmdPart:
                {
                    //leave chat room
                    ParsePersistentChatRoomLeaveCommand(firstChildNode, sipRequest);
                    break;
                }
                case EMSIP_CONTENT_XCCOS_TYPE.emXCCosCmdGetFileUploadToken:
                {
                    //upload file to chat room
                    NLChatRoomAttachmentAnalyser.GetInstance().ParseChatRoomGetFileUploadTokenRequest(sipRequest, obXCCosContent as XCCosContentCmdGetFileUploadToken);
                    break;
                }
                case EMSIP_CONTENT_XCCOS_TYPE.emXCCosCmdGetFileDownloadToken:
                {
                    //download file to chat room
                    NLChatRoomAttachmentAnalyser.GetInstance().ParseChatRoomGetFileDownloadTokenRequest(sipRequest, obXCCosContent as XCCosContentCmdGetFileDownloadToken);
                    break;
                }
                case EMSIP_CONTENT_XCCOS_TYPE.emXCCosReplyBatchJoin:
                {
                    // login join all followed chat room
                    ParsePeristentChatRoomBatchJoinResult(xmlDoc, xmlnsMgr, XCCosContent.kstrDefaultNsPrefix, sipRequest);
                    break;
                }
                case EMSIP_CONTENT_XCCOS_TYPE.emXCCosReplyJoin:
                {
                    // join chat room
                    ParsePersistentChatRoomJoinResult(firstChildNode, xmlnsMgr, XCCosContent.kstrDefaultNsPrefix, sipRequest);
                    break;
                }
                case EMSIP_CONTENT_XCCOS_TYPE.emXCCosReplyGetFileToken:
                {
                    // chat room upload/download file get token
                    NLChatRoomAttachmentAnalyser.GetInstance().ParseChatRoomReplyGetFileTokenRequest(sipRequest, obXCCosContent as XCCosContentReplyGetFileToken);
                    break;
                }
                default:
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Unknown content xccos type:[{0}], break and nothing to do.\n", emContentXCCosType);
                    break;
                }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, ex.ToString());
            }
        }

        private void ParseByeRequest(Request request)
        {
            string strTo = CSIPTools.GetUriFromSipAddrHdr(request.AllHeaders.FindFirst(CSIPTools.SIP_HDR_TO).Value);

            string strFromHdr = request.AllHeaders.FindFirst(CSIPTools.SIP_HDR_FROM).Value;
            string strFromUser = CSIPTools.GetUserAtHost(strFromHdr);
            string strEpid = CSIPTools.GetUserParameterFromSIPAddress(strFromHdr, SIP_ADDRESS_PARAMETER.EPID);
            string strTag = CSIPTools.GetUserParameterFromSIPAddress(strFromHdr, SIP_ADDRESS_PARAMETER.TAG);

            if (CSIPTools.IsConferenceEndPoint(strTo))//leave a meeting or close a channel of the meeting
            {
                if(CSIPTools.IsConferenceFocusEndPoint(strTo)) //leave a meeting
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Leave meeting: user={0}, {1}, {2}, meeting={1}", strFromUser, strEpid, strTag, strTo);
                    RemoveUserFromMeeting(strTo, strFromUser, strEpid, strTag);
                }
            }

        }

        private CConference PrepareExistConference(string strConfUri)
        {
            //Create conference object from conference Uri
            CConference conf = null;
            string strCreator = CSIPTools.GetUserAtHost(strConfUri);
            string strConfID = CSIPTools.GetConferenceIDFromUri(strConfUri);

            if (!string.IsNullOrWhiteSpace(strCreator) && !string.IsNullOrWhiteSpace(strConfID) )
            {
                conf = new CConference(strCreator, strConfID)
                {
                    FocusUri = strConfUri,
                    SFBMeetingType = EMSFB_MeetingType.Unknown
                };
                CConferenceManager.GetConferenceManager().AddConference(conf);
            }

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "PrepareExistConference, focusUri={0}, creator={1}", strConfUri, strCreator);

            //query policy to check if this conference need enforce
            if(conf!=null)
            {
                PolicyResult policyResult = new PolicyResult();
                CPolicy.Instance().QueryPolicyForMeetingCreate(conf, policyResult);

                SFBMeetingVariableInfo meetingVar = new SFBMeetingVariableInfo(SFBMeetingVariableInfo.kstrUriFieldName, conf.FocusUri,
                                        SFBMeetingVariableInfo.kstrDoneManulClassifyFieldName, SFBMeetingVariableInfo.kstrDoneManulClassifyNo);

                ParsePolicyResultForMeetingCreate(policyResult, conf, meetingVar);

                //write conference info to database
                conf.SaveConferenceInfo();
                meetingVar.PersistantSave();
            }

            return conf;
        }

        private void ParseConferenceInfoRequest(Request sipRequest, ref Response ourResponse)
        {
            XmlDocument xmlDoc = new XmlDocument();
           
            //load xml
            try
            {
                xmlDoc.LoadXml(sipRequest.Content);
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Exception on ParseConferenceInfoRequest,LoadXml.xml={0}, exception={1}", sipRequest.Content, ex.ToString());
                return;
            }

            //Parse request
            string strInfoType = xmlDoc.DocumentElement.Name;
            if(strInfoType.Equals("request", StringComparison.OrdinalIgnoreCase))
            {
                XmlNode firstChildNode = xmlDoc.DocumentElement.FirstChild;
                if((null!=firstChildNode) && (firstChildNode.Name.Equals("deleteUser", StringComparison.OrdinalIgnoreCase)))
                {
                    ParseCocferenceDeleteUserRequest(firstChildNode);
                }
            }
        }

        private void ParseCocferenceDeleteUserRequest(XmlNode deleteUserNode)
        {
            try
            {
                XmlNode delInfoNode = deleteUserNode.FirstChild;
                if ((delInfoNode != null) && delInfoNode.Name.Equals("userKeys", StringComparison.OrdinalIgnoreCase))
                {
                    string strConfFocusUri = delInfoNode.Attributes["confEntity"].Value;
                    string strUserSipAddr = delInfoNode.Attributes["userEntity"].Value;
                    string strUserName = CSIPTools.GetUserAtHost(strUserSipAddr);

                    string strEpid = ""; //when remove user, this parameter is "",  CSIPTools.GetUserParameterFromSIPAddress(strUserSipAddr, SIP_ADDRESS_PARAMETER.EPID);
                    string strTag = ""; //when remove user, this parameter is "",   CSIPTools.GetUserParameterFromSIPAddress(strUserSipAddr, SIP_ADDRESS_PARAMETER.TAG);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Delete user from meeting: user={0}, {1}, {2},meeting={3}", strUserName, strEpid, strTag, strConfFocusUri);
                    RemoveUserFromMeeting(strConfFocusUri, strUserName, strEpid, strTag);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Exception on ParseCocferenceDeleteUserRequest. {0}", ex.ToString());
            }
        }

        private void RemoveUserFromMeeting(string strMeetingFocusUri, string strUser, string strEpid, string strTag)
        {
            SFBMeetingVariableInfo meetingVar = CEntityVariableInfoManager.GetMeetingVariableInfoFromDB(strMeetingFocusUri);
            if (null != meetingVar)
            {
                if(string.IsNullOrEmpty(strEpid) || string.IsNullOrEmpty(strTag))
                {
                    SFBCommon.SFBObjectInfo.SFBParticipantManager.RemoveParticipant(meetingVar, strUser);
                }
                else
                {
                    SFBCommon.SFBObjectInfo.SFBParticipantManager.RemoveParticipantWithParameter(meetingVar, strUser, strEpid, strTag);
                }
               
                meetingVar.PersistantSave();
            }
        }
        #endregion
    }
}
