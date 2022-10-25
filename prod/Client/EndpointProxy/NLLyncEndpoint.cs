// Basic
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;

// UCMA
using Microsoft.Rtc.Collaboration;  // Basic namespace
using Microsoft.Rtc.Signaling;      // SipTransportType

// Other project
using SFBCommon.Common;
using SFBCommon.NLLog;
using NLUCMA;
using SFBCommon.ClassifyHelper;
using SFBCommon.CommandHelper;
using NLChatRobot;
using NLChatRobot.ClassifyRobot;
using SFBCommon.WebServiceHelper;


// Current project
using NLLyncEndpointProxy.Common;
using NLLyncEndpointProxy.Managers;
using NLLyncEndpointProxy.NLMessagingCall;


namespace NLLyncEndpointProxy
{
    public class NLLyncEndpoint
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(NLLyncEndpoint));
        #endregion

        #region Managers
        NLIMManager m_obIMMgr = null;
        NLConferenceManager m_obConferenceMgr = null;
        #endregion

        #region Endpoint info
        UserEndpoint m_obUserEndpoint = null;
        bool m_bEndpointEstablished = false;
        #endregion

        #region User info
        private STUSFB_ENDPOINTPROXYACCOUNT m_stuEndpointProxyAccount = null;
        #endregion

        #region Constructors
        public NLLyncEndpoint(STUSFB_ENDPOINTPROXYACCOUNT stuEndpointProxyAccount, bool bAsynchronous)
        {
            try
            {
                if (null != stuEndpointProxyAccount)
                {
                    m_stuEndpointProxyAccount = stuEndpointProxyAccount;
                    m_stuEndpointProxyAccount.OutputInfo();
                    ThreadHelper.AsynchronousInvokeHelper(bAsynchronous, EstablishNLUserEndpoint);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! Endpoint proxy account is null\n");
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception in NLLyncEndpoint: [{0}:{1}]", ex.HResult, ex.Message);
            }
        }
        #endregion

        #region Destructor and Disponse
        ~NLLyncEndpoint()
        {
            
        }
        #endregion

        #region Public functions
        public string GetUserSipURI()
        {
            if (null != m_stuEndpointProxyAccount)
            {
                return m_stuEndpointProxyAccount.m_strUserSipURI;
            }
            return "";
        }
        public bool GetEndpointEstablishFalg() { return m_bEndpointEstablished; }
        public void CloseNLUserEndpoint()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Begin close NLUserEndpoint\n");
            if (GetEndpointEstablishFalg())
            {
                RemoveEvents();
                Collection<Conversation> collectionConversations = m_obUserEndpoint.GetConversations();
                foreach (Conversation obConversation in collectionConversations)
                {
                    UCMAHelper.TerminateConversation(obConversation, null);
                }
                UCMAHelper.ShutdownPlatform(m_obUserEndpoint.Platform, null);
                SetEndpointEstablishFalg(false);
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Close NLUserEndpoint but the establish flag is false");
            }
        }
        public void SendNotifyMessage(bool bAsynchronous, string strDesSipUri, string strMessageInfo, string strToastMessage)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Send notify message, bAsynchronous:[{0}], User:[{1}], Message:[{2}]\n", bAsynchronous, strDesSipUri, strMessageInfo);
            ThreadHelper.AsynchronousTheadPoolInvokeHelper(bAsynchronous, TheadInvokeSendNotifyMessage, (new StuNotifyMessageInfo(strDesSipUri, strMessageInfo, strToastMessage)));
        }
        public void DoManulClassify(STUSFB_CLASSIFYCMDINFO stuClassifyCmdInfo)
        {
            try
            {
                if ((LocalEndpointState.Terminated == m_obUserEndpoint.State) || (LocalEndpointState.Terminating == m_obUserEndpoint.State))
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "The endpoint proxy is terminate, cannot do manul classify now. Maybe the SFB Server is stopped or some error happened.");
                }
                else
                {
                    // Check parameters
                    if (IsEffectiveClassifyCmd(stuClassifyCmdInfo))
                    {
                        string strAssistantAutoSendMessage = "";
                        bool bAllSuccess = false;
                        if (EMSFB_COMMAND.emCommandClassifyMeeting == stuClassifyCmdInfo.m_emCommandType)
                        {
                            strAssistantAutoSendMessage = EndpointProxyCommonTools.GetClassifyMeetingAutoReplyInfo(NLLyncEndpointProxyConfigInfo.s_endpointProxyConfigInfo.AssitantAutoSend, stuClassifyCmdInfo.m_strUserSipUri, stuClassifyCmdInfo.m_strSFBObjUri, out bAllSuccess);
                        }
                        else if (EMSFB_COMMAND.emCommandClassifyChatRoom == stuClassifyCmdInfo.m_emCommandType)
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current command type is:[{0}], we do not support to do manul classify now\n", stuClassifyCmdInfo.m_emCommandType);
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current command type is:[{0}], we do not support to do manul classify now\n", stuClassifyCmdInfo.m_emCommandType);
                        }
                        if ((!string.IsNullOrEmpty(strAssistantAutoSendMessage)) && bAllSuccess)
                        {
                            NLInnerSendMessage(stuClassifyCmdInfo.m_strUserSipUri, strAssistantAutoSendMessage);
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Something error happened when we try to get classify  meeting url in do manual classify, AssitantAutoSendMessage:[{0}]\n", strAssistantAutoSendMessage);
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Try to do manual classify but the parameters is null\n");
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Do manul classify exception:{0}\n", ex.Message);
            }
        }
        #endregion

        #region Asynchronous invoke send notify info
        private class StuNotifyMessageInfo
        {
            public string strDesSipUri = "";
            public string strMessageInfo = "";
            public string strToastMessage = "";

            public StuNotifyMessageInfo(string strParamDesSipUri, string strParamMessageInfo, string strParamToastMessage)
            {
                strDesSipUri = strParamDesSipUri;
                strMessageInfo = strParamMessageInfo;
                strToastMessage = strParamToastMessage;
            }
        }
        private void TheadInvokeSendNotifyMessage(object obMessageInfo)
        {
            int nSendMessageRetryTimes = NLLyncEndpointProxyConfigInfo.s_endpointProxyConfigInfo.SendMessageRetryTimes;
            int nSendMessageRetryInterval = NLLyncEndpointProxyConfigInfo.s_endpointProxyConfigInfo.SendMessageRetryInterval;
            bool bNeedRetry = false;
            for (int i=0; i<nSendMessageRetryTimes; ++i)
            {
                bNeedRetry = false;
                try
                {
                    if ((LocalEndpointState.Terminating == m_obUserEndpoint.State) || (LocalEndpointState.Terminated == m_obUserEndpoint.State))
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "The endpoint proxy is terminate, cannot send notify message now. Maybe the SFB Server is stopped or some error happened.");
                    }
                    else
                    {
                        StuNotifyMessageInfo stuMessageInfo = obMessageInfo as StuNotifyMessageInfo;
                        NLInnerSendMessage(stuMessageInfo.strDesSipUri, stuMessageInfo.strMessageInfo); // If send message failed, there will be throw out an exception
                    }
                }
                catch (Exception ex)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Send message exception:{0}\n", ex.Message);
                    const string kstrExceptionTemporarilyUnavailableError = "A 480 (Temporarily Unavailable) response";
                    if (ex.Message.StartsWith(kstrExceptionTemporarilyUnavailableError))
                    {
                        bNeedRetry = true;
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Send message failed with 480 error, this maybe a temporarily network error and need retry\n");
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Send message failed with an unknown exception, ignore\n");
                    }
                }
                if (bNeedRetry)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Retry to send out message after [{0}] seconds\n", nSendMessageRetryInterval);
                    Thread.Sleep(nSendMessageRetryInterval * 1000);
                }
                else
                {
                    break;
                }
            }
        }
        #endregion

        #region Private functions
        private void SetEndpointEstablishFalg(bool bEstablishFlag) { m_bEndpointEstablished = bEstablishFlag; }
        private void EstablishNLUserEndpoint()
        {
            if (!GetEndpointEstablishFalg())
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Begin to establish NLUserEndpoint");

                m_obUserEndpoint = UCMAHelper.CreateUserEndPoint(m_stuEndpointProxyAccount.m_strServerFQDN, m_stuEndpointProxyAccount.m_strUserSipURI, m_stuEndpointProxyAccount.m_strUserDomain, m_stuEndpointProxyAccount.m_strUserName, m_stuEndpointProxyAccount.m_strPassword, m_stuEndpointProxyAccount.m_strUserAgent);
                UCMAHelper.StartupPlatform(m_obUserEndpoint, null);
                UCMAHelper.EstablishEndpoint(m_obUserEndpoint, CBEndEstablishEndpoint);   // In win10 OS or inner terminate, cannot established success and block at here
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Logic error: try to establish NLUserEndpoint but the establish flag is true");
            }
        }
        private void AddEvents()
        {
            m_obUserEndpoint.StateChanged += EventUserEndpointStateChanged;
            m_obIMMgr.BeginListenIncommingCall(m_obUserEndpoint);
            m_obConferenceMgr.BeginListeConferenceInvitation(m_obUserEndpoint);
        }
        private void RemoveEvents()
        {
            m_obIMMgr.EndListenIncommingCall(m_obUserEndpoint);
            m_obConferenceMgr.EndListeConferenceInvitation(m_obUserEndpoint);
            m_obUserEndpoint.StateChanged -= EventUserEndpointStateChanged;
        }
        private void NLInnerSendMessage(string strUserSipUri, string strMessageInfo)
        {
            if ((!string.IsNullOrEmpty(strUserSipUri)) && (!string.IsNullOrEmpty(strMessageInfo)))
            {
                NLIMCall obNLMessageCall = m_obIMMgr.GetIMCall(m_obUserEndpoint, strUserSipUri);
                if (null != obNLMessageCall)
                {
                    NLGeneralIMCall obNLGeneralIMCall = obNLMessageCall as NLGeneralIMCall;
                    if (null != obNLGeneralIMCall)
                    {
                        IChatSpeaker pChatSpeaker = obNLMessageCall.GetNLIMFlow();
                        if (null != pChatSpeaker)
                        {
                            pChatSpeaker.SendMessage(strMessageInfo, true);
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Get chat speaker failed when we try to do manual classify, the speaker object is null\n");
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Get general IM call failed when we try to do manual classify, the general IM call  object is null\n");
                    }
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Inner send message params error, UserSipUri:[{0}], MessageInfo:[{1}]\n", strUserSipUri, strMessageInfo);
            }
        }
        #endregion

        #region Call back functions
        private void CBTerminateConversation(IAsyncResult ar)
        {
            if (null != ar)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "CallBack terminate conversation, [Type={0}], [IsCompleted={1}], [StateType={2}]\n", ar.GetType(), ar.IsCompleted, ar.AsyncState);
            }
        }
        private void CBShutdonwPlatform(IAsyncResult ar)
        {
            if (null != ar)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "CallBack shutdonw platform, [Type={0}], [IsCompleted={1}], [StateType={2}]\n", ar.GetType(), ar.IsCompleted, ar.AsyncState);
            }
        }
        private void CBTerminateUserEndpoint(IAsyncResult ar)
        {
            if (null != ar)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "CallBack terminate user endpoint, [Type={0}], [IsCompleted={1}], [StateType={2}]\n", ar.GetType(), ar.IsCompleted, ar.AsyncState);
            }
        }
        private void CBEndEstablishEndpoint(IAsyncResult ar)
        {
            if (null != ar)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "CallBack EstablishEndpoint, [Type={0}], [IsCompleted={1}], [StateType={2}]\n", ar.GetType(), ar.IsCompleted, ar.AsyncState);
                Microsoft.Rtc.Collaboration.UserEndpoint obUserEndpoint = ar.AsyncState as Microsoft.Rtc.Collaboration.UserEndpoint;
                if (null != obUserEndpoint)
                {
                    
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Call Back Establish Endpoint, [State={0}]\n", obUserEndpoint.State);
                    if (LocalEndpointState.Established == obUserEndpoint.State)
                    {
                        m_obConferenceMgr = new NLConferenceManager();
                        m_obIMMgr = new NLIMManager();
                        AddEvents();

                        SetEndpointEstablishFalg(true);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Success establish NLUserEndpoint");
                    }
                    else 
                    {
                        ReEstablishUserEndpoint();
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Convert AsyncState as UserEndpoint failed\n");
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "IAsyncResult parameter is null\n");
            }
        }
        #endregion

        #region Endpoint events
        private void EventUserEndpointStateChanged(object sender, LocalEndpointStateChangedEventArgs e)
        {
            try
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "EventUserEndpointStateChanged, Message Data:[{0}], Reason:[{1}], EndpointState:[{2}]\n", e.MessageData, e.Reason, m_obUserEndpoint.State);
                switch (e.Reason)
                {
                case LocalEndpointStateTransitionReason.TooManyActiveEndpoints:
                {
                    // Start multiple proxy endpoint or endpoint proxy account is stolen
                    break;
                }
                case LocalEndpointStateTransitionReason.OwnerDisabledOrRemoved:
                {
                    // Proxy login failed
                    break;
                }
                case LocalEndpointStateTransitionReason.RegistrationRefreshSucceeded:
                {
                    // refresh success. connect SFB Server success
                    break;
                }
                case LocalEndpointStateTransitionReason.RegistrationRefreshFailed:
                {
                    // refresh failed, maybe the SFB Server is stopped
                    break;
                }
                case LocalEndpointStateTransitionReason.ServerDiscoveryFailed:
                {
                    // Server address is error or network broken
                    break;
                }
                default:
                {
                    break;
                }
                }
                // Auto reconnection when the user endpoint is terminated
                // Note, this sentence can be invoke in terminating event, in this event, when the thread process this sentence the UserEndpoint state maybe become terminate event.
                if (LocalEndpointState.Terminated == m_obUserEndpoint.State)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "EventUserEndpointStateChanged: The EndpointProxy state is terminated, maybe the SFB Server is stopped. The endpoint proxy will auto reconnect to the SFB Server");
                    ReEstablishUserEndpoint();
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception in EventUserEndpointStateChanged: [{0}:{1}]", ex.HResult, ex.Message);
            }
        }
        #endregion

        #region Inner tools
        private void ReEstablishUserEndpoint()
        {
            CloseNLUserEndpoint();
            int nDelayEstablishTime = 60 * 1000;
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current endpoint established failed, delay [{0}] milliseconds to re-established\n", nDelayEstablishTime);
            Thread.Sleep(nDelayEstablishTime);
            EstablishNLUserEndpoint();
        }
        private bool IsEffectiveClassifyCmd(STUSFB_CLASSIFYCMDINFO stuClassifyCmdInfo)
        {
            if ((null != stuClassifyCmdInfo))
            {
                if ((EMSFB_COMMAND.emCommandClassifyMeeting == stuClassifyCmdInfo.m_emCommandType) || (EMSFB_COMMAND.emCommandClassifyChatRoom == stuClassifyCmdInfo.m_emCommandType))
                {
                    return (!string.IsNullOrEmpty(stuClassifyCmdInfo.m_strSFBObjUri)) && (!string.IsNullOrEmpty(stuClassifyCmdInfo.m_strUserSipUri));
                }
            }
            return false;
        }
        private bool IsEffectiveManulClassifyObligation(ManulClassifyObligationHelper obManulClassifyObligationHelper)
        {
            if (null != obManulClassifyObligationHelper)
            {
                return (EMSFB_CLASSIFYTYPE.emManulClassifyObligation == obManulClassifyObligationHelper.GetClassifyType());
            }
            return false;
        }
        #endregion

        #region Private backup functions
        private void BackUp_DoManulClassify(STUSFB_CLASSIFYCMDINFO stuClassifyCmdInfo, ManulClassifyObligationHelper obManulClassifyObligationHelper, ClassifyTagsHelper obClassifyTagsInfo)
        {
            try
            {
                if ((LocalEndpointState.Terminated == m_obUserEndpoint.State) || (LocalEndpointState.Terminating == m_obUserEndpoint.State))
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "The endpoint proxy is terminate, cannot do manul classify now. Maybe the SFB Server is stopped or some error happened.");
                }
                else
                {
                    // Check parameters
                    if (IsEffectiveClassifyCmd(stuClassifyCmdInfo) && IsEffectiveManulClassifyObligation(obManulClassifyObligationHelper))
                    {
                        NLIMCall obNLMessageCall = m_obIMMgr.GetIMCall(m_obUserEndpoint, stuClassifyCmdInfo.m_strUserSipUri);
                        if (null != obNLMessageCall)
                        {
                            NLGeneralIMCall obNLGeneralIMCall = obNLMessageCall as NLGeneralIMCall;
                            if (null != obNLGeneralIMCall)
                            {
                                obNLGeneralIMCall.SetManulClassifyInfo(stuClassifyCmdInfo, obManulClassifyObligationHelper, obClassifyTagsInfo);
                                IChatSpeaker pChatSpeaker = obNLMessageCall.GetNLIMFlow();
                                if (null != pChatSpeaker)
                                {
                                    // Set received message and the chat robot will talk with the remote user.
                                }
                                else
                                {
                                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Get chat speaker failed when we try to do manual classify, the speaker object is null\n");
                                }
                            }
                            else
                            {
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Get general IM call failed when we try to do manual classify, the general IM call  object is null\n");
                            }
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Try to do manual classify but the parameters is null\n");
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Do manul classify exception:{0}\n", ex.Message);
            }
        }
        #endregion
    }
}
