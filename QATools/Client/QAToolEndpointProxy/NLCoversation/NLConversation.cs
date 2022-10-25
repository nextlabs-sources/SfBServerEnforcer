using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// UCMA
using Microsoft.Rtc.Collaboration;  // Basic namespace
using Microsoft.Rtc.Signaling;      // SipTransportType

// Nextlabs
using NLLyncEndpointProxy.Common;
using NLLyncEndpointProxy.NLMessagingCall;

// Other project
using QAToolSFBCommon.NLLog;

namespace NLLyncEndpointProxy.NLCoversation
{
    enum EMSFB_NLCOVERSATIONTYPE
    {
        emNLConversation_Unknown = 0,

        emNLConversation_Notify,
        emNLConversation_Conference
    }

    abstract class NLConversation : IDisposable
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(NLConversation));
        #endregion

        #region Static functions
        static public NLConversation CreateConversationByType(EMSFB_NLCOVERSATIONTYPE emConversationType, Conversation obConversation, string strRemoteUri)
        {
            NLConversation obNLConversation = null;
            switch (emConversationType)
            {
            case EMSFB_NLCOVERSATIONTYPE.emNLConversation_Notify:
            {
                obNLConversation = new NLIMConversation(obConversation, strRemoteUri);
                break;
            }
            case EMSFB_NLCOVERSATIONTYPE.emNLConversation_Conference:
            {
                obNLConversation = new NLConferenceConversation(obConversation, strRemoteUri);
                break;
            }
            default:
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!!Unknown type in CreateConversationByType\n");
                break;
            }
            }
            if (null != obNLConversation)
            {
                if (obNLConversation.GetNLConversationType() != emConversationType)
                {
                    throw new Exception("!!!Inner error, the type cannot matched the object\n");
                }
            }
            return obNLConversation;
        }
        #endregion

        #region Events
        public delegate void EventHandlerConversationStatueChanged(object sender, StateChangedEventArgs<ConversationState> e);
        public event EventHandlerConversationStatueChanged EventConversationChanged = null;
        #endregion

        #region Members
        protected Conversation m_obConversation = null;
        private string m_strRemoteUri = null;         // For IM, it is remote user URI, for Conference it is the conference URI.

        private NLIMCall m_obNLIMCall = null;
        #endregion

        #region Constructors
        public NLConversation(Conversation obConversation, string strRemoteUri)
        {
            m_obConversation = obConversation;
            SetRemoteUri(strRemoteUri);
        }
        ~NLConversation()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLConversation destructor");
        }
        #endregion

        #region Implement interface: IDisposable
        virtual public void Dispose()
        {
            try
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLConversation dispose, conversation:[{0}], IMCall:[{1}]\n", m_obConversation, m_obNLIMCall);
                if (null != m_obNLIMCall)
                {
                    m_obNLIMCall.Dispose();
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in NLConversation dispose: [{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Public functions
        public ConversationState GetConversationState()
        {
            return m_obConversation.State;
        }
        public string GetRemoteUri()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Get RemoteUri:[{0}]\n", m_strRemoteUri);
            return m_strRemoteUri;
        }
        public void SetRemoteUri(string strRemoteUri)
        {
            m_strRemoteUri = strRemoteUri;
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Set RemoteUri:[{0}]\n", m_strRemoteUri);
        }
        public NLLyncEndpointProxy.NLMessagingCall.NLIMCall GetIMCall() { return m_obNLIMCall; }
        #endregion

        #region Common functions
        protected bool SetConversationEvents()
        {
            bool bRet = (null != m_obConversation);
            if (bRet)
            {
                m_obConversation.StateChanged += EventConversationStateChanged;
                m_obConversation.EscalateToConferenceRequested += EventEscalateToConferenceRequested;
                m_obConversation.LobbyParticipantAttendanceChanged += EventLobbyParticipantAttendanceChanged;
                m_obConversation.ParticipantPropertiesChanged += EventParticipantPropertiesChanged;
                m_obConversation.PropertiesChanged += EventPropertiesChanged;
                m_obConversation.RemoteParticipantAttendanceChanged += EventRemoteParticipantAttendanceChanged;
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Set call events failed, because the message call is empty");
            }
            return bRet;
        }
        protected void SetIMCall(NLLyncEndpointProxy.NLMessagingCall.NLIMCall obMessageCall)
        {
            if (obMessageCall != m_obNLIMCall)
            {
                if ((null != m_obNLIMCall))
                {
                    m_obNLIMCall.Dispose();
                }
                m_obNLIMCall = obMessageCall;
            }
        }
        #endregion

        #region Virtual/abstract functons
        abstract public EMSFB_NLCOVERSATIONTYPE GetNLConversationType();
        abstract public void EstablishIMCall(string strToastMessage);

        #region Events
        virtual protected void EventConversationStateChanged(object sender, StateChangedEventArgs<ConversationState> e)
        {
            try
            {
                if (null != e)
                {
                    ConversationState emConversationState = e.State;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Conversation state:[{0}]\n", emConversationState);
                    if (null != EventConversationChanged)
                    {
                        EventConversationChanged(this, e);
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception in NLConversation::EventConversationStateChanged： [{0}], [{1}]", ex.Message, ex.StackTrace);
            }
        }
        virtual protected void EventEscalateToConferenceRequested(object sender, EscalateToConferenceRequestedEventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Conversation event: EventEscalateToConferenceRequested\n");
        }
        virtual protected void EventLobbyParticipantAttendanceChanged(object sender, LobbyParticipantAttendanceChangedEventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Conversation event: EventLobbyParticipantAttendanceChanged\n");
        }
        virtual protected void EventParticipantPropertiesChanged(object sender, ParticipantPropertiesChangedEventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Conversation event: EventParticipantPropertiesChanged\n");
        }
        virtual protected void EventPropertiesChanged(object sender, PropertiesChangedEventArgs<ConversationProperties> e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Conversation event: EventPropertiesChanged\n");
        }
        virtual protected void EventRemoteParticipantAttendanceChanged(object sender, ParticipantAttendanceChangedEventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Conversation event: EventRemoteParticipantAttendanceChanged\n");
        }
        #endregion

        #endregion
    }
}
