using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using QAToolSFBCommon.NLLog;
using QAToolSFBCommon.Common;

namespace QAToolNLChatRobot.AutoTestRobot
{
    public class AutoTestRobot : ChatRobot
    {
        #region Members
        private AutoResetEvent m_eventRobotTalker = new AutoResetEvent(false);
        private bool m_bStart = false;
        #endregion

        #region Constructor
        public AutoTestRobot(IChatSpeaker pIChatSpeaker) : base(pIChatSpeaker)
        {
            SetStartFlag(false);
        }
        #endregion

        #region Overwrite function
        override public bool SayHi(string strHiMessage)
        {
            return ChatSpeaker.SendMessage(strHiMessage, false);
        }
        override public bool AutoReply(string strReceivedMessage)
        {
            bool bRet = ChatSpeaker.SendMessage(strReceivedMessage, true);
            return bRet;
        }
        override public bool SayEnd(string strEndMessage)
        {
            return ChatSpeaker.SendMessage(strEndMessage, false);
        }
        override public void Start()
        {
            if (!GetStartFlag())
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Start classify chat robot for [{0}]\n", ChatSpeaker.GetRemoteChatSpeakerUri());
                ThreadHelper.AsynchronousTheadPoolInvokeHelper(true, StartWorkThread, null);
                SetStartFlag(true);
            }
        }
        override public void Exit()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exist classify chat robot for [{0}]\n", ChatSpeaker.GetRemoteChatSpeakerUri());
            m_eventRobotTalker.Set();
        }
        #endregion

        #region Thread functions
        private void StartWorkThread(object obStartInfo)
        {
            try
            {
                // Note:
                // 1. If invote ChatSpeaker.GetRemoteChatSpeakerUri() to early, it the function maybe return an empty string
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "An classify chat robot start. [{0}]\n", ChatSpeaker.GetRemoteChatSpeakerUri());
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in chat robot StartWorkThread\n {0}\n", ex.Message);
            }
        }
        #endregion

        #region Inner tools
        private bool GetStartFlag() { return m_bStart; }
        private void SetStartFlag(bool bStart) { m_bStart = bStart; }
        #endregion
    }
}
