using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// UCMA
using Microsoft.Rtc.Collaboration;  // Basic namespace
using Microsoft.Rtc.Signaling;      // SipTransportType

// Other project
using NLUCMA;
using SFBCommon.NLLog;
using SFBCommon.Common;
using SFBCommon.CommandHelper;
using NLChatRobot;
using NLChatRobot.ClassifyRobot;

// Current project
using NLLyncEndpointProxy.NLMessagingFlow;
using NLLyncEndpointProxy.Common;
using SFBCommon.ClassifyHelper;

namespace NLLyncEndpointProxy.NLMessagingCall
{
    class NLGeneralIMCall : NLIMCall
    {
        #region Members
        private ClassifyChatRobot m_obChatRobot = null;
        #endregion

        #region Constructors
        public NLGeneralIMCall(InstantMessagingCall obIMCall) : base(obIMCall)  // Incoming
        {
            SetCallEvents();
            SetNLIMFlowWithEstablishChatRobot(obIMCall.Flow);
        }
        public NLGeneralIMCall(InstantMessagingCall obIMCall, string strToastMessage, string strDesSipUri) : base(obIMCall)
        {
            SetCallEvents();
            UCMAHelper.EstablishMessageCall(obIMCall, strToastMessage, strDesSipUri, null);
            SetNLIMFlowWithEstablishChatRobot(obIMCall.Flow);
        }
        ~NLGeneralIMCall()
        {
            try
            {
                Dispose();
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in NLIMCall desctrcutor: [{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Implement interface: IDisposable
        override public void Dispose()
        {
            try
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLGeneralIMFlow Dispose\n");
                ExitChatRobot();
                base.Dispose();
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in NLIMCall dispose: [{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Override Abstract/virtual NLIMCall functions
        override protected NLIMFlow EstablishNLIMFlow(InstantMessagingFlow obIMFlow)
        {
            return new NLGeneralIMFlow(obIMFlow);
        }
        #endregion

        #region Call events
        override protected void EventIMFlowConfigurationRequested(object sender, InstantMessagingFlowConfigurationRequestedEventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLCall, IMFlow configuration event: we can configuration the IM flow here");
            try
            {
                if ((null != e) && (null != e.Flow))
                {
                    SetNLIMFlowWithEstablishChatRobot(e.Flow);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Try to configure IMFlow events but parameters or flow object is null");
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception in EventIMFlowConfigurationRequested in genera IM call: [{0}:{1}]", ex.HResult, ex.Message);
            }
        }
        override protected void EventIMCallStateChanged(object sender, CallStateChangedEventArgs e)
        {
            // Runtime message call status
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "IncomingCall, ReceivedEvent: IMCall state changed. we can check the runtime statues and recover resource");
            try
            {
                if (null != e)
                {
                    CallStateTransitionReason emTransitionReason = e.TransitionReason;
                    CallState emPreviousState = e.PreviousState;
                    CallState emState = e.State;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "TransitionReason:[{0}] PreviousState:[{1}], State:[{2}]\n", emTransitionReason.ToString(), emPreviousState.ToString(), emState.ToString());

                    if (CallState.Terminated == emState)
                    {
                        Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception, NLNotifyCall, IMCallStateChanged exception: " + ex.Message);
            }
        }
        #endregion end Call events

        #region Public robot tools
        public void SetManulClassifyInfo(STUSFB_CLASSIFYCMDINFO stuClassifyCmdInfo, ManulClassifyObligationHelper obManulClassifyObligationHelper, ClassifyTagsHelper obClassifyTagsInfo)
        {
            if (null != m_obChatRobot)
            {
                m_obChatRobot.SetClassifyInfo(stuClassifyCmdInfo, obManulClassifyObligationHelper, obClassifyTagsInfo);
            }
        }
        #endregion

        #region Inner robot tools
        private void ExitChatRobot()
        {
            if (null != m_obChatRobot)
            {
                m_obChatRobot.Exit();
                m_obChatRobot = null;
            }
        }
        private void SetNLIMFlowWithEstablishChatRobot(InstantMessagingFlow obIMFlow)
        {
            if (null != obIMFlow)
            {
                SetNLIMFlow(obIMFlow);
                // Currently no need using a chat robot
                // EstablishChatRobot();
            }
        }
        private bool EstablishChatRobot()
        {
            bool bRet = false;
            try
            {
                if (null == m_obChatRobot)
                {
                    // Check endpoint type
                    EMSFB_ENDPOINTTTYPE emEndpointType = NLLyncEndpointProxyMain.s_obNLLyncEndpointProxyObj.CurEndpointType;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "EstablishChatRobot, endpoint type:[{0}]]\n", emEndpointType);

                    if (EMSFB_ENDPOINTTTYPE.emTypeAgent == emEndpointType)
                    {
                        // Current notify auto reply is simple, no need new the notify robot
                        // Using function AutoReplyInAgentEndpoint to do auto rely for agent endpoint
                        bRet = true;
                    }
                    else if (EMSFB_ENDPOINTTTYPE.emTypeAssistant == emEndpointType)
                    {
                        NLIMFlow obNLIMFlow = GetNLIMFlow();
                        if (null != obNLIMFlow)
                        {
                            m_obChatRobot = new ClassifyChatRobot(obNLIMFlow);
                            m_obChatRobot.Start();
                            bRet = true;
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLIMFlow is null");
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Received an incoming message but the endpoint type:[{0}] is wrong\n", emEndpointType);
                    }
                }
                else
                {
                    m_obChatRobot.Start();  // make sure the chat robot in start status, if the robot already start, this function nothing to do.
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in EstablishChatRobot, [{0}]\n", ex.Message);
            }
            return bRet;
        }
        #endregion
    }
}
