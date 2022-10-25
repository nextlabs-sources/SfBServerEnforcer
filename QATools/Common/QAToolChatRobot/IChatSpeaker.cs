using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QAToolNLChatRobot
{
    public interface IChatSpeaker
    {
        bool SendMessage(string strMessage, bool bAsynchronous);

        void SetReceivedMessage(string strMessage);

        string GetReveivedMessage();

        string GetRemoteChatSpeakerUri();
    }
}
