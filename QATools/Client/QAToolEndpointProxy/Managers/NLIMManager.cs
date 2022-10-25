// Basic
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// UCMA
using Microsoft.Rtc.Collaboration;  // Basic namespace
using Microsoft.Rtc.Signaling;      // SipTransportType

// Other project
using QAToolNLUCMA;
using QAToolSFBCommon.NLLog;
using QAToolSFBCommon.Common;

// Current project
using NLLyncEndpointProxy.Common;
using NLLyncEndpointProxy.NLCoversation;
using NLLyncEndpointProxy.NLMessagingCall;
using QAToolNLChatRobot;

namespace NLLyncEndpointProxy.Managers
{
    class NLIMManager : NLManager
    {
        #region Const/Readonly values
        public const string kstrIMCallToastMessage = "";
        #endregion

        #region Constructors
        public NLIMManager()
        {

        }
        #endregion

        #region public functions
        public void BeginListenIncommingCall(UserEndpoint obUserEndpoint)
        {
            obUserEndpoint.RegisterForIncomingCall<InstantMessagingCall>(EventIncomingCallReceiver);
        }
        public void EndListenIncommingCall(UserEndpoint obUserEndpoint)
        {
            obUserEndpoint.UnregisterForIncomingCall<InstantMessagingCall>(EventIncomingCallReceiver);
        }
        public IChatSpeaker GetChatSpeaker(UserEndpoint obUserEndpoint, string strDesSipUri)
        {
            strDesSipUri = CommonHelper.GetStandardSipUri(strDesSipUri); // Make a standard SipUri
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In IMManager, Send out message, strDesSipUri:[{0}]\n", strDesSipUri);
            NLLyncEndpointProxy.NLMessagingCall.NLIMCall obNLMessageCall = GetMessageCall(obUserEndpoint, strDesSipUri);
            if (null != obNLMessageCall)
            {
                return obNLMessageCall.GetNLIMFlow();
            }
            return null;
        }
        public NLIMCall GetIMCall(UserEndpoint obUserEndpoint, string strDesSipUri)
        {
            strDesSipUri = CommonHelper.GetStandardSipUri(strDesSipUri); // Make a standard SipUri
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In IMManager, Send out message, strDesSipUri:[{0}]\n", strDesSipUri);
            return GetMessageCall(obUserEndpoint, strDesSipUri);
        }
        #endregion

        #region Private tools
        private NLLyncEndpointProxy.NLMessagingCall.NLIMCall GetMessageCall(UserEndpoint obUserEndpoint, string strDesSipUri)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In IMManager, begin get IM call, strDesSipUri:[{0}]\n", strDesSipUri);

            strDesSipUri = CommonHelper.GetStandardSipUri(strDesSipUri); // Make a standard SipUri
            NLConversation obNLConversation = GetNLConversation(EMSFB_NLCOVERSATIONTYPE.emNLConversation_Notify, obUserEndpoint, strDesSipUri, NLLyncEndpointProxyConfigInfo.s_endpointProxyConfigInfo.ConversationUserAgent);
            if (null != obNLConversation)
            {
                obNLConversation.EstablishIMCall(kstrIMCallToastMessage);
                return obNLConversation.GetIMCall();
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Get notify conversation failed when we try to get IM call with {0}\n", strDesSipUri);
            }
            return null;
        }
        #endregion

        #region Events
        private void EventIncomingCallReceiver(object sender, CallReceivedEventArgs<InstantMessagingCall> e)
        {
            try
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Received incoming call event");
                if ((null != e) && (null != e.Call))
                {
                    string strRemoteUri = EndpointProxyCommonTools.GetCallRemoteUserSipUri(e.Call);
                    NLIMConversation obIncomingConversation = new NLIMConversation(e.Call.Conversation, e.Call, strRemoteUri);
                    SaveNLConversation(obIncomingConversation);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Received an incoming call but the parameters call object is null");
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception in EventIncomingCallReceiver: [{0}:{1}]", ex.HResult, ex.Message);
            }
        }
        #endregion
    }
}
