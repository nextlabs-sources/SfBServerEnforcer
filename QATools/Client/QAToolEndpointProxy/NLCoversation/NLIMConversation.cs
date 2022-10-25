using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// UCMA
using Microsoft.Rtc.Collaboration;  // Basic namespace
using Microsoft.Rtc.Signaling;      // SipTransportType

// Nextlabs
using NLLyncEndpointProxy.NLMessagingCall;
using NLLyncEndpointProxy.Common;

// Other project
using QAToolSFBCommon.NLLog;

namespace NLLyncEndpointProxy.NLCoversation
{
    class NLIMConversation : NLConversation
    {
        #region Constructors
        public NLIMConversation(Conversation obConversation, string strRemoteUri) : base(obConversation, strRemoteUri)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLIMConversation constructor(1), Conversation:[{0}], strRemoteUri:[{1}]\n", obConversation, strRemoteUri);
            SetConversationEvents();
        }
        public NLIMConversation(Conversation obConversation, InstantMessagingCall obIMCall, string strRemoteUri) : base(obConversation, strRemoteUri)   // For incoming call
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLIMConversation constructor(2), Conversation:[{0}], obIMCall:[{1}], strRemoteUri:[{2}]\n", obConversation, obIMCall, strRemoteUri);

            NLGeneralIMCall obNLNotifyCall = null;
            try
            {
                SetConversationEvents();
                obNLNotifyCall = new NLGeneralIMCall(obIMCall);
                SetIMCall(obNLNotifyCall);
                obNLNotifyCall.AcceptedCall(true);
            }
            catch (Exception ex)
            {
                if (null != obNLNotifyCall)
                {
                    obNLNotifyCall.Dispose();
                    SetIMCall(null);
                }
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in NLIMConversation constructor(2), [{0}]\n", ex.Message);
            }
        }

        #endregion

        #region Override functions
        override public void EstablishIMCall(string strToastMessage)
        {
            if (null == GetIMCall())
            {
                NLGeneralIMCall obNLNotifyCall = GetMessageCall(GetRemoteUri(), strToastMessage, true);
                SetIMCall(obNLNotifyCall);
            }
        }
        override public EMSFB_NLCOVERSATIONTYPE GetNLConversationType()
        {
            return EMSFB_NLCOVERSATIONTYPE.emNLConversation_Notify;
        }
        #endregion

        #region Private functions
        private NLGeneralIMCall GetMessageCall(string strDesSipUri, string strToastMessage,  bool bCreateNew)
        {
            NLGeneralIMCall obNotifyCall = null;
            lock(this)
            {
                InstantMessagingCall obIMCall = null;
                foreach (Call obCall in m_obConversation.Calls)
                {
                    if (obCall is InstantMessagingCall)
                    {
                        if (strDesSipUri.Equals(obCall.RemoteEndpoint.Participant.Uri, StringComparison.OrdinalIgnoreCase))
                        {
                            obIMCall = obCall as InstantMessagingCall;
                            if (null != obIMCall)
                            {
                                break;
                            }
                        }
                    }
                }
                if (null == obIMCall)
                {
                    if (bCreateNew)
                    {
                        obNotifyCall = new NLGeneralIMCall(new InstantMessagingCall(m_obConversation), strToastMessage, strDesSipUri);
                    }
                }
                else
                {
                    obNotifyCall = new NLGeneralIMCall(obIMCall);
                }
            }
            return obNotifyCall;
        }
        #endregion
    }
}
