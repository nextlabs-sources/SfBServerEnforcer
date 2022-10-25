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
using QAToolSFBCommon.NLLog;
using QAToolNLUCMA;

// Current project
using NLLyncEndpointProxy.Common;
using NLLyncEndpointProxy.NLCoversation;
using NLLyncEndpointProxy.NLMessagingCall;

namespace NLLyncEndpointProxy.Managers
{
    class NLConferenceManager : NLManager
    {
        #region Const/Readonly values
        public const string kstrConferenceCallToastMessage = "";
        #endregion

        #region Constructors
        public NLConferenceManager()
        {
            
        }
        #endregion

        #region public functions
        public void BeginListeConferenceInvitation(UserEndpoint obUserEndpoint)
        {
            obUserEndpoint.ConferenceInvitationReceived += EventConferenceInvitationReceived;
        }
        public void EndListeConferenceInvitation(UserEndpoint obUserEndpoint)
        {
            obUserEndpoint.ConferenceInvitationReceived -= EventConferenceInvitationReceived;
        }

        #endregion

        #region Events
        private void EventConferenceInvitationReceived(object sender, ConferenceInvitationReceivedEventArgs e)
        {
            try
            {
                // Accept to join meeting
                ConferenceInvitation obConferenceInvitation = e.Invitation;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Conference Uri:[{0}]", obConferenceInvitation.ConferenceUri);
                NLConferenceConversation obNLConferenceConversation = new NLConferenceConversation(obConferenceInvitation);
                SaveNLConversation(obNLConferenceConversation);
                obNLConferenceConversation.EstablishIMCall(kstrConferenceCallToastMessage);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception, EventConferenceInvitationReceived: [{0}:{1}]", ex.HResult, ex.Message);
            }
        }
        #endregion
    }
}
