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

// Current project
using NLLyncEndpointProxy.Common;

namespace NLLyncEndpointProxy.NLMessagingFlow
{
    class NLConferenceIMFlow : NLIMFlow
    {
        #region TagInfo
        private const string kstrSalutationOfStartTagging = "Start to set tags:";
        private const string kstrSalutationOfEndTagging = "Tag setting is finished and thanks for your setting";
        #endregion

        #region Constructors
        public NLConferenceIMFlow(InstantMessagingFlow obIMFlow) : base(obIMFlow)
        {
            SetFlowEvents();
        }
        #endregion

        #region functions

        #endregion

        #region Events
        override protected void EventIMFlowMessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In NLConference Flow, EventIMFlowMessageReceived");
            try
            {
                // Auto replay, chat robot
                InstantMessagingFlow obIMFlow = sender as InstantMessagingFlow; // Sender is InstantMessagingFlow
                if (null != obIMFlow)
                {
                    string strReveivedMessageBody = e.TextBody;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Received message: [{0}]\n", strReveivedMessageBody);
                    
                    SetReceivedMessage(strReveivedMessageBody);
                    DisplayMessage(strReveivedMessageBody);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception, IMFlowMessageReceived exception: " + ex.Message);
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Out NLConference Flow, IMFlowMessageReceived");
        }
        override protected void EventIMFlowRemoteComposingStateChanged(object sender, ComposingStateChangedEventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLConferenceFlow, state changed: [{0}], URI:[{1}]\n", e.ComposingState, e.Participant.Uri);
        }
        override protected void EventIMFlowDeliverNotificationReceived(object sender, DeliveryNotificationReceivedEventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In NLConferenceFlow, Received flow deliver");
        }
        #endregion
    }
}
