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
using QAToolSFBCommon.CommandHelper;
using QAToolNLChatRobot;

// Current project
using NLLyncEndpointProxy.Common;


namespace NLLyncEndpointProxy.NLMessagingFlow
{
    class NLGeneralIMFlow : NLIMFlow
    {
        #region Contructors
        public NLGeneralIMFlow(InstantMessagingFlow obIMFlow) : base(obIMFlow)
        {
            SetFlowEvents();
        }
        ~NLGeneralIMFlow()
        {
            Dispose();
        }
        #endregion

        #region Events
        override protected void EventIMFlowMessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In Notify flow, EventIMFlowMessageReceived");
            try
            {
                // Auto replay, chat robot
                InstantMessagingFlow obIMFlow = sender as InstantMessagingFlow; // Sender is InstantMessagingFlow
                if (null != obIMFlow)
                {
                    string strReveivedMessageBody = e.TextBody;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Received message: [{0}]\n", strReveivedMessageBody);

                    if (!string.IsNullOrEmpty(strReveivedMessageBody))
                    {
                        DisplayMessage(strReveivedMessageBody);
                        AutoReplyInEndpoint(obIMFlow, strReveivedMessageBody);
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception, IMFlowMessageReceived exception: " + ex.Message);
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Out Notify flow, IMFlowMessageReceived");
        }
        override protected void EventIMFlowRemoteComposingStateChanged(object sender, ComposingStateChangedEventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLNotifyFlow, state changed: [{0}], URI:[{1}]\n", e.ComposingState, e.Participant.Uri);
        }
        override protected void EventIMFlowDeliverNotificationReceived(object sender, DeliveryNotificationReceivedEventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In Notify flow, Received flow deliver");
        }
        #endregion

        #region Private tools
        private void AutoReplyInEndpoint(InstantMessagingFlow obIMFlow, string strReveivedMessageBody)
        {
            /* If the remote user is agent or assistant no need reply
             *      1. Agent send message to agent or assistant
             *      2. Assistant send message to agent or assistant
             * If there is a lot of message received in a shot time, terminate the call
            */
            if ((null != obIMFlow) && (!string.IsNullOrEmpty(strReveivedMessageBody)))
            {
                if (IsFrequentMessage())
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "The received message is frequent message, ignore\n");
                    UCMAHelper.TerminateCall(obIMFlow.Call, null);
                }
                else
                {
                    SetReceivedMessage(strReveivedMessageBody);  // If using robot only received the message here, no need send out message
                }
            }
        }
        #endregion
    }
}
