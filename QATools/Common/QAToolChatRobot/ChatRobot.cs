using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Other project
using QAToolSFBCommon.NLLog;

namespace QAToolNLChatRobot
{
    public class ChatMsg
    {
        public string strMessageInfo = "";
        public string strTime = "";
        public string strSender = "";

        public ChatMsg(string strParamMessageInfo, string strParamTime, string strParamSender)
        {
            strMessageInfo = strParamMessageInfo;
            strTime = strParamTime;
            strSender = strParamSender;
        }
    }

    public abstract class ChatRobot
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(ChatRobot));
        #endregion

        #region Fields
        protected IChatSpeaker ChatSpeaker { get { return m_pChatSpeaker; } }
        #endregion

        #region Members
        private IChatSpeaker m_pChatSpeaker = null;

        private ErrorInfo m_obLastErrorInfo = new ErrorInfo(0, "");
        private List<ChatMsg> m_lsChatMessage = new List<ChatMsg>();
        #endregion

        #region Constructor
        public ChatRobot(IChatSpeaker pIChatSpeaker)
        {
            m_pChatSpeaker = pIChatSpeaker;
        }
        #endregion

        #region Abstract functions
        virtual public bool SayHi(string strHiMessage)
        {
            return ChatSpeaker.SendMessage(strHiMessage, false);
        }
        abstract public bool AutoReply(string strUserReplay);
        virtual public bool SayEnd(string strEndMessage)
        {
            return ChatSpeaker.SendMessage(strEndMessage, false);
        }
        abstract public void Start();
        abstract public void Exit();
        #endregion

        #region Public tools
        public ErrorInfo GetErrorInfo()
        {
            return m_obLastErrorInfo;
        }
        #endregion

        #region Inner tools
        protected void SetErrorInfo(ErrorInfo obErrorInfo)
        {
            m_obLastErrorInfo = obErrorInfo;
        }
        protected void RecordUserReplayMessage(string strParamMessageInfo, string strParamTime, string strParamSender)
        {
            m_lsChatMessage.Add(new ChatMsg(strParamMessageInfo, strParamTime, strParamSender));
        }
        #endregion
    }
}
