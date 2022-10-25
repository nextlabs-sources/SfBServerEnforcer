using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// UCMA
using Microsoft.Rtc.Collaboration;  // Basic namespace
using Microsoft.Rtc.Signaling;      // SipTransportType

// Other project
using QAToolNLChatRobot;
using QAToolNLUCMA;
using QAToolSFBCommon.NLLog;

// Current project
using NLLyncEndpointProxy.Common;
using NLLyncEndpointProxy.NLMessagingFlow;
using System.Threading;

namespace NLLyncEndpointProxy.NLMessagingCall
{
    abstract class NLIMCall : IDisposable
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(NLIMCall));
        #endregion

        #region UCMA
        private InstantMessagingCall m_obIMCall = null;
        private ReaderWriterLockSlim m_obReaderWriterLockSlimForNLIMFlow = new ReaderWriterLockSlim();
        private NLIMFlow m_obNLIMFlow = null;

        #endregion

        #region Constructors
        public NLIMCall(InstantMessagingCall obIMCall)
        {
            m_obIMCall = obIMCall;
        }
        ~NLIMCall()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLMessagingCall destructor");
            Dispose();
        }
        #endregion

        #region Implement interface: IDisposable
        virtual public void Dispose()
        {
            try
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLlIMCall Dispose\n");
                DisposeNLIMFlow();
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in NLIMCall dispose: [{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Public functions
        public void AcceptedCall(bool bAsynchronous)
        {
            if (bAsynchronous)
            {
                m_obIMCall.BeginAccept(CBIMCallAccepted, m_obIMCall);
            }
            else
            {
                IAsyncResult obAysncResult = m_obIMCall.BeginAccept(null, m_obIMCall);
                m_obIMCall.EndAccept(obAysncResult);
            }
        }
        public NLIMFlow GetNLIMFlow()
        {
            m_obReaderWriterLockSlimForNLIMFlow.EnterReadLock();    // Note: lock(object), if the object is null it will throw an exception
            try
            {
                return m_obNLIMFlow;
            }
            finally
            {
                m_obReaderWriterLockSlimForNLIMFlow.ExitReadLock();
            }
        }
        #endregion

        #region Private/Protected functions
        protected bool SetCallEvents()
        {
            bool bRet = (null != m_obIMCall);
            if (bRet)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Begin config call events, SetCallEvents");

                // Raised when InstantMessagingFlow is created. Applications can use this event handler to register for flow related event handlers and optionally set flow configuration.
                m_obIMCall.InstantMessagingFlowConfigurationRequested += EventIMFlowConfigurationRequested;
                // Message call state changed, Hooked up for logging, to show call state transitions.
                m_obIMCall.StateChanged += EventIMCallStateChanged;
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Set call events failed, because the message call is empty");
            }
            return bRet;
        }
        protected void SetNLIMFlow(InstantMessagingFlow obIMFlow)
        {
            m_obReaderWriterLockSlimForNLIMFlow.EnterWriteLock();    // Note: lock(object), if the object is null it will throw an exception
            try
            {
                if (null == m_obNLIMFlow)
                {
                    m_obNLIMFlow = EstablishNLIMFlow(obIMFlow);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current IM Flow already set or the input parameter is null\n");
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in SetNLIMFlow, [{0}]\n", ex.Message);
            }
            finally
            {
                m_obReaderWriterLockSlimForNLIMFlow.ExitWriteLock();
            }
        }
        private void DisposeNLIMFlow()
        {
            m_obReaderWriterLockSlimForNLIMFlow.EnterWriteLock();    // Note: lock(object), if the object is null it will throw an exception
            try
            {
                if (null != m_obNLIMFlow)
                {
                    m_obNLIMFlow.Dispose();
                    m_obNLIMFlow = null;
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in DisposeNLIMFlow, [{0}]\n", ex.Message);
            }
            finally
            {
                m_obReaderWriterLockSlimForNLIMFlow.ExitWriteLock();
            }
        }
        #endregion

        #region
        abstract protected NLIMFlow EstablishNLIMFlow(InstantMessagingFlow obIMFlow);
        #endregion

        #region CallBack functions
        virtual protected void CBIMCallAccepted(IAsyncResult obAysncResult)
        {
            try
            {
                InstantMessagingCall obCall = obAysncResult.AsyncState as InstantMessagingCall;
                if (null != obCall)
                {
                    obCall.EndAccept(obAysncResult);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception： [{0}]", ex.Message);
            }
        }
        virtual protected void CBIMCallEstablished(IAsyncResult obAysncResult)
        {
            try
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Call established :[{0}] \n", obAysncResult.AsyncState);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception： [{0}]", ex.Message);
            }
        }
        #endregion

        #region Call events
        virtual protected void EventIMFlowConfigurationRequested(object sender, InstantMessagingFlowConfigurationRequestedEventArgs e)
        {
            // Set message flow events
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "ReceivedEvent: IMFlow configuration request. we can set flow events here");
        }
        virtual protected void EventIMCallStateChanged(object sender, CallStateChangedEventArgs e)
        {
            // Runtime message call status
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "ReceivedEvent: IMCall state changed. we can check the runtime statues and recover resource");
        }
        #endregion end Call events
    }
}
