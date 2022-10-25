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
using NLLyncEndpointProxy.NLMessagingFlow;
using NLLyncEndpointProxy.Common;

namespace NLLyncEndpointProxy.NLMessagingCall
{
    class NLConferenceIMCall : NLIMCall
    {
        #region Const values
        private const string kstrSalutationOfStartTagging = "According policy you need to set tags for your meeting and we we will ask you to tag it.";
        private const string kstrSalutationOfEndTagging = "End tagging and thanks for you";
        #endregion

        #region Constructors
        public NLConferenceIMCall(InstantMessagingCall obIMCall) : base(obIMCall)
        {
            SetCallEvents();
            SetNLIMFlow(obIMCall.Flow);
        }
        public NLConferenceIMCall(InstantMessagingCall obIMCall, string strToastMessage, string strDesSipUri) : base(obIMCall)
        {
            SetCallEvents();
            UCMAHelper.EstablishMessageCall(obIMCall, strToastMessage, strDesSipUri, null);
            SetNLIMFlow(obIMCall.Flow);
        }
        #endregion

        #region Override Abstract/virtual NLIMCall functions
        override protected NLIMFlow EstablishNLIMFlow(InstantMessagingFlow obIMFlow)
        {
            return new NLConferenceIMFlow(obIMFlow);
        }
        #endregion

        #region Call events
        override protected void EventIMFlowConfigurationRequested(object sender, InstantMessagingFlowConfigurationRequestedEventArgs e)
        {
            // Set message flow events
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLConferenceCall, ReceivedEvent: IMFlow configuration request. we can set flow events here");
            try
            {
                if ((null != e) && (null != e.Flow))
                {
                    SetNLIMFlow(e.Flow);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Try to configure IMFlow events but parameters or flow object is null");
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception in EventIMFlowConfigurationRequested in IM conference call: [{0}:{1}]", ex.HResult, ex.Message);
            }
        }
        override protected void EventIMCallStateChanged(object sender, CallStateChangedEventArgs e)
        {
            // Runtime message call status
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLConferenceCall, ReceivedEvent: IMCall state changed. we can check the runtime statues and recover resource");
            try
            {
                if (null != e)
                {
                    InstantMessagingCall obIMCall = sender as InstantMessagingCall;
                    CallStateTransitionReason emTransitionReason = e.TransitionReason;
                    CallState emPreviousState = e.PreviousState;
                    CallState emState = e.State;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "TransitionReason:[{0}] PreviousState:[{1}], State:[{2}]\n", emTransitionReason.ToString(), emPreviousState.ToString(), emState.ToString());

                    switch (emState)
                    {
                    case CallState.Established:
                    {
                        break;
                    }
                    default:
                    {
                        break;
                    }
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception, NLConferenceIMCall IMCallStateChanged exception: " + ex.Message);
            }
        }
        #endregion end Call events
    }
}
