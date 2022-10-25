using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Current project
using NLChatRobot;

// Other project
using SFBCommon.NLLog;

namespace NLChatRobot.NotifyRobot
{
    public class NotifyChatRobot : ChatRobot
    {
        #region Members
        private readonly string m_strAutoReply = "This is nextlabs agent. If you want to tag meeting please ask nextlabs assitant.";
        #endregion

        #region Constructor
        public NotifyChatRobot(IChatSpeaker pIChatRobotOutput, string strAutoReply) : base(pIChatRobotOutput)
        {
            m_strAutoReply = strAutoReply;
        }
        #endregion

        #region Overwrite function
        override public bool AutoReply(string strUserReplay)
        {
            return ChatSpeaker.SendMessage(m_strAutoReply, false);
        }
        override public void Start()
        {
            // Notify talker no need support bAsynchronous, just sendout user message
            ChatSpeaker.SendMessage(m_strAutoReply, false);
        }
        override public void Exit()
        {

        }
        #endregion
    }
}
