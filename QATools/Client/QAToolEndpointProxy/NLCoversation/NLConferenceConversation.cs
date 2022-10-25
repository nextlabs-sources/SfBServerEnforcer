using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// UCMA
using Microsoft.Rtc.Collaboration;  // Basic namespace
using Microsoft.Rtc.Signaling;      // SipTransportType

// Other project
using QAToolSFBCommon.NLLog;

// Current project
using NLLyncEndpointProxy.NLMessagingCall;
using NLLyncEndpointProxy.Common;

namespace NLLyncEndpointProxy.NLCoversation
{
    class NLConferenceConversation : NLConversation
    {
        #region Constructors
        public NLConferenceConversation(Conversation obConversation, string strDesConferenceSipUri) : base(obConversation, strDesConferenceSipUri)
        {
            SetConversationEvents();

            ConferenceJoinOptions obConferenceJoinOptions = new ConferenceJoinOptions();    // Using default conference join options
            IAsyncResult obAysncResult = m_obConversation.ConferenceSession.BeginJoin(strDesConferenceSipUri, obConferenceJoinOptions, null, obConversation);
            m_obConversation.ConferenceSession.EndJoin(obAysncResult);
        }
        public NLConferenceConversation(ConferenceInvitation obInvitation) : base(obInvitation.Conversation, obInvitation.ConferenceUri)
        {
            SetConversationEvents();

            IAsyncResult obAysncResult = obInvitation.BeginAccept(null, obInvitation);
            obInvitation.EndAccept(obAysncResult);

            ConferenceJoinOptions obConferenceJoinOptions = new ConferenceJoinOptions();    // Using default conference join options
            obAysncResult = m_obConversation.ConferenceSession.BeginJoin(obConferenceJoinOptions, null, m_obConversation);
            m_obConversation.ConferenceSession.EndJoin(obAysncResult);
        }
        #endregion

        #region Public functions

        #endregion

        #region Override functions
        override public void EstablishIMCall(string strToastMessage)
        {
            NLLyncEndpointProxy.NLMessagingCall.NLIMCall obMessageCall = GetIMCall();
            if (null == obMessageCall)
            {
                SetIMCall(new NLConferenceIMCall(new InstantMessagingCall(m_obConversation), strToastMessage, ""));
            }
        }
        override public EMSFB_NLCOVERSATIONTYPE GetNLConversationType()
        {
            return EMSFB_NLCOVERSATIONTYPE.emNLConversation_Conference;
        }
        #endregion

        #region Callback functions
        private void CBConferenceInvitationAccepted(IAsyncResult obAysncResult)
        {
            try
            {
                ConferenceInvitation obInvite = obAysncResult.AsyncState as ConferenceInvitation;
                obInvite.EndAccept(obAysncResult);

                // Save conversation and set conversation events
                Conversation obConversation = obInvite.Conversation;
                // m_conversation = invite.Conversation;
                // RegisterConversationHandlers();
                ConferenceJoinOptions obConferenceJoinOptions = new ConferenceJoinOptions();    // Using default conference join options
                obConversation.ConferenceSession.BeginJoin(obConferenceJoinOptions, CBConferenceSessionJoinCompleted, obConversation);
            }
            catch (RealTimeException ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "invite.EndAccept failed. Exception: {0}", ex.ToString());
            }
            catch (InvalidOperationException ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "m_conversation.ConferenceSession.BeginJoin failed. Exception: {0}", ex.ToString());
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception, ConferenceInvitationReceived exception: " + ex.Message);
            }
        }
        private void CBConferenceSessionJoinCompleted(IAsyncResult obAysncResult)
        {
            try
            {
                Conversation obConversation = obAysncResult.AsyncState as Conversation;
                obConversation.ConferenceSession.EndJoin(obAysncResult);

                Collection<string> activeMediaTypes = obConversation.GetActiveMediaTypes();
                if (activeMediaTypes.Contains(MediaType.Message))
                {
                    SetIMCall(new NLConferenceIMCall(new InstantMessagingCall(m_obConversation), NLLyncEndpointProxy.Managers.NLConferenceManager.kstrConferenceCallToastMessage, ""));
                }
            }
            catch (RealTimeException ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception, CBConferenceSessionJoinCompleted exception: {0}", ex.ToString());
            }
        }
        #endregion
    }
}
