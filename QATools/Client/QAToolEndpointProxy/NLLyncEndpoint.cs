// Basic
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using System.Diagnostics;

// UCMA
using Microsoft.Rtc.Collaboration;  // Basic namespace
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Collaboration.PersistentChat;
using Microsoft.Rtc.Signaling;      // SipTransportType

// Other project
using QAToolSFBCommon.Common;
using QAToolSFBCommon.NLLog;
using QAToolNLUCMA;
using QAToolSFBCommon.CommandHelper;
using QAToolNLChatRobot;
using QAToolSFBCommon.WebServiceHelper;


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
        public NLLyncEndpoint(STUSFB_ENDPOINTPROXYACCOUNT stuEndpointProxyAccount)
        {
            m_stuEndpointProxyAccount = stuEndpointProxyAccount;
        }

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
        public UserEndpoint GetUserEndPoint()
        {
            return m_obUserEndpoint;
        }

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
            ThreadHelper.AsynchronousInvokeHelper(bAsynchronous, TheadInvokeSendNotifyMessage, (new StuNotifyMessageInfo(strDesSipUri, strMessageInfo, strToastMessage)));
        }

        public void ConnectToServer()
        {
            EstablishNLUserEndpoint();
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
            try
            {
                if ((LocalEndpointState.Terminated == m_obUserEndpoint.State) || (LocalEndpointState.Terminating == m_obUserEndpoint.State))
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "The endpoint proxy is terminate, cannot send notify message now. Maybe the SFB Server is stopped or some error happened.");
                }
                else
                {
                    StuNotifyMessageInfo stuMessageInfo = obMessageInfo as StuNotifyMessageInfo;
                    NLInnerSendMessage(stuMessageInfo.strDesSipUri, stuMessageInfo.strMessageInfo);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Send message exception:{0}\n", ex.Message);
            }
        }
        #endregion

        #region Private functions
        private void SetEndpointEstablishFalg(bool bEstablishFlag) { m_bEndpointEstablished = bEstablishFlag; }
        private void EstablishNLUserEndpoint()
        {
            if (!GetEndpointEstablishFalg())
            {
                try
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Begin connect to server:{0}", m_stuEndpointProxyAccount.m_strUserName);
                    m_obUserEndpoint = UCMAHelper.CreateUserEndPoint(m_stuEndpointProxyAccount.m_strServerFQDN, m_stuEndpointProxyAccount.m_strUserSipURI, m_stuEndpointProxyAccount.m_strUserDomain, m_stuEndpointProxyAccount.m_strUserName, m_stuEndpointProxyAccount.m_strPassword, m_stuEndpointProxyAccount.m_strUserAgent);
                    UCMAHelper.StartupPlatform(m_obUserEndpoint, null);
                    UCMAHelper.EstablishEndpoint(m_obUserEndpoint, null);   // In win10 OS, cannot established success and block at here

                    m_obConferenceMgr = new NLConferenceManager();
                    m_obIMMgr = new NLIMManager();
                    AddEvents();
                    SetEndpointEstablishFalg(true);

                     theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Success establish NLUserEndpoint");
                }
                catch(Exception ex)
                {
                    Trace.WriteLine("Exception on establish NLUserEndpoint:{0}", ex.ToString());
                    return;
                }
     
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

        #region Endpoint events
        private void EventUserEndpointStateChanged(object sender, LocalEndpointStateChangedEventArgs e)
        {
            try
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Message Data:[{0}], Reason:[{1}], EndpointState:[{2}]\n", e.MessageData, e.Reason, m_obUserEndpoint.State);
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
                    CloseNLUserEndpoint();
                    EstablishNLUserEndpoint();
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception in EventUserEndpointStateChanged: [{0}:{1}]", ex.HResult, ex.Message);
            }
        }
        #endregion

        #region Inner tools

        #endregion

        #region Private backup functions

        #endregion
    }
}
