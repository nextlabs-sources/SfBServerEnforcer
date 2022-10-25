using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// UCMA
using Microsoft.Rtc.Signaling;      // SipTransportType
using Microsoft.Rtc.Collaboration;  // Basic namespace

// Other project
using QAToolSFBCommon.NLLog;
using QAToolSFBCommon.Common;
using QAToolNLUCMA;

// Current project
using NLLyncEndpointProxy.NLCoversation;

namespace NLLyncEndpointProxy.Managers
{
    class NLManager
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(NLManager));
        #endregion

        #region Members
        private Dictionary<string, NLConversation> m_dicNLConversation = new Dictionary<string,NLConversation>();
        #endregion

        #region Constructor
        public NLManager() { }
        #endregion

        #region Tools
        protected NLConversation GetNLConversation(EMSFB_NLCOVERSATIONTYPE emNLConversationType, UserEndpoint obUserEndpoint, string strRemoteUri, string strConversationSubject)
        {
            NLConversation obNLConversation = null;
            lock (m_dicNLConversation)
            {
                if (null == m_dicNLConversation)
                {
                    m_dicNLConversation = new Dictionary<string, NLConversation>();
                }
                obNLConversation = InnerGetNLConversation(strRemoteUri);
                bool bNeedCreateNewConversation = (null == obNLConversation);
                if (!bNeedCreateNewConversation)
                {
                    ConversationState emConversationState = obNLConversation.GetConversationState();
                    bNeedCreateNewConversation = ((ConversationState.Terminating == emConversationState) || (ConversationState.Terminated == emConversationState));
                }
                if (bNeedCreateNewConversation)
                {
                    Conversation obConversation = UCMAHelper.CreateConversation(obUserEndpoint, strConversationSubject);
                    obNLConversation = NLConversation.CreateConversationByType(emNLConversationType, obConversation, strRemoteUri);
                    SaveNLConversation(obNLConversation);
                }
            }
            return obNLConversation;
        }
        protected void SaveNLConversation(NLConversation obNLConversation)
        {
            lock (m_dicNLConversation)
            {
                if (null != obNLConversation)
                {
                    string strRemoteUri = obNLConversation.GetRemoteUri();
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Save conversation:[{0}], remote uri:[{1}]\n", obNLConversation.GetHashCode(), strRemoteUri);

                    InnerSaveNLConversation(strRemoteUri, obNLConversation);
                    obNLConversation.EventConversationChanged += EventConversationStateChanged;
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Save conversation, but the object is null\n");
                }
            }
        }
        protected void DeleteNLConversationAccordingState(string strRemoteUri)
        {
            lock (m_dicNLConversation)
            {
                if (!string.IsNullOrEmpty(strRemoteUri))
                {
                    NLConversation obNLConversation = InnerGetNLConversation(strRemoteUri);
                    if (null != obNLConversation)
                    {
                        // Note: a special case is this function is invoke in terminated event, but anther thread checked and create a new conversation for this item and here the conversation state is not terminated.
                        if (ConversationState.Terminated == obNLConversation.GetConversationState())
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Delete conversation:[{0}], remote uri:[{1}]\n", obNLConversation.GetHashCode(), strRemoteUri);
                            InnerRemoveNLConversation(strRemoteUri);
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Get an null conversation object in DeleteNLConversationAccordingState, maybe the conversation already delete, remote URI:{0}\n", strRemoteUri);
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Delete conversation, but the key remote URI is null\n");
                }
            }
        }

        #region Inner conversation op tools
        private void InnerSaveNLConversation(string strRemoteUri, NLConversation obNLConversation)
        {
            if (!string.IsNullOrEmpty(strRemoteUri))
            {
                CommonHelper.AddKeyValuesToDir(m_dicNLConversation, strRemoteUri.ToLower(), obNLConversation);
            }
        }
        private NLConversation InnerGetNLConversation(string strRemoteUri)
        {
            if (!string.IsNullOrEmpty(strRemoteUri))
            {
                return CommonHelper.GetValueByKeyFromDir(m_dicNLConversation, strRemoteUri.ToLower(), null);
            }
            return null;
        }
        private void InnerRemoveNLConversation(string strRemoteUri)
        {
            if (!string.IsNullOrEmpty(strRemoteUri))
            {
                NLConversation obNLConversation = CommonHelper.GetValueByKeyFromDir(m_dicNLConversation, strRemoteUri.ToLower(), null);
                if (null != obNLConversation)
                {
                    obNLConversation.Dispose();
                    CommonHelper.RemoveKeyValuesFromDir(m_dicNLConversation, strRemoteUri.ToLower());
                }
            }
        }
        #endregion
        #endregion

        #region Events
        virtual protected void EventConversationStateChanged(object sender, StateChangedEventArgs<ConversationState> e)
        {
            try
            {
                if ((null != sender) && (null != e))
                {
                    NLConversation obNLConversation = sender as NLConversation;
                    if (null != obNLConversation)
                    {
                        ConversationState emConversationState = e.State;
                        if (ConversationState.Terminated == emConversationState)
                        {
                            string strRemoteUri = obNLConversation.GetRemoteUri();
                            DeleteNLConversationAccordingState(strRemoteUri);
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! NLManager::EventConversationStateChanged, sender is not NLConversation object. sender type:[{0}]", sender.GetType());
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! NLManager::EventConversationStateChanged, sender or argument is null");
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! Exception in NLManager::EventConversationStateChanged： [{0}]", ex.Message);
            }
        }
        #endregion
    }
}
