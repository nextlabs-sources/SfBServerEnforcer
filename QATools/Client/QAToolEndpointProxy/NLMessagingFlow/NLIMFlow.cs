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
using NLLyncEndpointProxy;
using NLLyncEndpointProxy.Common;
using System.Threading;

namespace NLLyncEndpointProxy.NLMessagingFlow
{
    abstract class NLIMFlow : IChatSpeaker, IDisposable
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(NLIMFlow));
        #endregion

        #region Static functions for display message
        static private IMessageDisplayer m_obIMessageDisplayer = null;
        static public void SetIMessageDisplayer(IMessageDisplayer obIMessageDisplayer) { m_obIMessageDisplayer = obIMessageDisplayer; }
        static private IMessageDisplayer GetIMessageDisplayer() { return m_obIMessageDisplayer; }
        #endregion

        #region Members
        private object m_obLockForReceivedMessage = new object();
        private string m_strReceivedMessage = "";   // only save the last one
        private DateTime m_dtReceivedMessageTime = new DateTime(0);
        protected InstantMessagingFlow m_obIMFlow = null;
        #endregion

        #region Constructors
        public NLIMFlow(InstantMessagingFlow obIMFlow)
        {
            m_obIMFlow = obIMFlow;
        }
        ~NLIMFlow()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLIMFlow destructor");
            Dispose();
        }
        #endregion

        #region Implement interface: IDisposable
        virtual public void Dispose()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLIMFlow dispose");
        }
        #endregion

        #region Implement Interface: IChatSpeaker
        public bool SendMessage(string strSendOutMessage, bool bAsynchronous)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NLIMFlow, send message, [{0}], Asynchronous:[{1}]\n", strSendOutMessage, bAsynchronous);
            if (bAsynchronous)
            {
                return UCMAHelper.SendMessage(m_obIMFlow, strSendOutMessage, CBSendOutMessageComplete);
            }
            else
            {
                return UCMAHelper.SendMessage(m_obIMFlow, strSendOutMessage, null);
            }
        }
        public void SetReceivedMessage(string strMessage)
        {
            lock(m_obLockForReceivedMessage)
            {
                m_strReceivedMessage = strMessage;
                m_dtReceivedMessageTime = DateTime.UtcNow;
            }
        }
        public string GetReveivedMessage()
        {
            lock (m_obLockForReceivedMessage)
            {
                string strReceivedMessage = m_strReceivedMessage;
                m_strReceivedMessage = "";
                return strReceivedMessage;
            }
        }
        public string GetRemoteChatSpeakerUri()
        {
            if (null != m_obIMFlow)
            {
                string strRemoteUri = EndpointProxyCommonTools.GetCallRemoteUserSipUri(m_obIMFlow.Call);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "The user remote uri is:[{0}]\n", strRemoteUri);
                return strRemoteUri;
            }
            return "";
        }
        #endregion

        #region Private/protected functions
        protected bool SetFlowEvents()
        {
            bool bRet = (null != m_obIMFlow);
            if (bRet)
            {
                // Register to receive messages.
                m_obIMFlow.MessageReceived += EventIMFlowMessageReceived;

                // Register to receive composing state changes.
                m_obIMFlow.RemoteComposingStateChanged += EventIMFlowRemoteComposingStateChanged;

                // Register to receive delivery notifications
                m_obIMFlow.DeliveryNotificationReceived += EventIMFlowDeliverNotificationReceived;
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Set flow events failed, because the message flow is empty");
            }
            return bRet;
        }
        protected void DisplayMessage(string strMessage)
        {
            IMessageDisplayer theISaveMessage = GetIMessageDisplayer();
            if (null != theISaveMessage)
            {
                theISaveMessage.SaveMessage(strMessage);
            }
        }
        protected bool IsFrequentMessage()
        {
            TimeSpan obMessageInterval = DateTime.UtcNow - GetReveivedLastMessageTime();
            return (obMessageInterval.TotalMilliseconds < NLLyncEndpointProxyConfigInfo.s_endpointProxyConfigInfo.MinMessageInterval);
        }
        
        private DateTime GetReveivedLastMessageTime()
        {
            lock (m_obLockForReceivedMessage)
            {
                return m_dtReceivedMessageTime;
            }
        }
        #endregion

        #region Events
        virtual protected void EventIMFlowMessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In MessageFlow, Received message, we can replay here as a chat robot");
        }
        virtual protected void EventIMFlowRemoteComposingStateChanged(object sender, ComposingStateChangedEventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In MessageFlow, Received flow state changed, we can get the flow runtime state and recover resource");
        }
        virtual protected void EventIMFlowDeliverNotificationReceived(object sender, DeliveryNotificationReceivedEventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In MessageFlow, Received flow deliver");
        }
        #endregion

        #region Call back functions
        virtual protected void CBSendOutMessageComplete(IAsyncResult obAysncResult)
        {
            InstantMessagingFlow obIMFlow = null;
            try
            {
                obIMFlow = obAysncResult.AsyncState as InstantMessagingFlow;
                if (null != obIMFlow)
                {
                    obIMFlow.EndSendInstantMessage(obAysncResult);
                }
            }
            catch (Exception ex)
            {
                if (null != obIMFlow)
                {
                    UCMAHelper.TerminateCall(obIMFlow.Call, null);
                }
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception, CallBackSendOutMessageComplete exception: " + ex.Message);
            }
        }
        #endregion
    }
}
