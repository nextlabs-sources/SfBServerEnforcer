using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// UCMA
using Microsoft.Rtc.Collaboration;  // Basic namespace
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Collaboration.PersistentChat;
using Microsoft.Rtc.Signaling;      // SipTransportType

using QAToolSFBCommon.NLLog;

namespace NLLyncEndpointProxy
{
    class NLChatRoomManager
    {

        private UserEndpoint m_userEndpoint = null;
        private PersistentChatEndpoint m_chatEndpoint = null;

        private ReadOnlyCollection<ChatRoomSnapshot> m_collectionRooms = null;
       
        //didn't protected, so the access must be serialize.
        private Dictionary<string, ChatRoomSession> m_dicRoomSessions = new System.Collections.Generic.Dictionary<string,ChatRoomSession>();

        private bool m_bConnectedToRoomServer = false;


        public NLChatRoomManager(UserEndpoint userEndPoint)
        {
            m_userEndpoint = userEndPoint;
        }

#region public function
        public bool IsConnectedToRoomServer()
        {
            return m_bConnectedToRoomServer;
        }

        public void DisconnectFromRoomServer()
        {
            try
            {
                m_chatEndpoint.EndTerminate(m_chatEndpoint.BeginTerminate(null, null));
                Trace.WriteLine("Disconnect from chat room server");
            }
            catch(Exception ex)
            {
                Trace.WriteLine(string.Format("Exception on disconnect from room server:{0}", ex.ToString()));
            }
            
        }
      

        public bool ConnectToRoomServer()
        {
            if(m_chatEndpoint!=null)
            {
                return true;
            }

            //connect to server
            try
            {
                ProvisioningData provisioningData = m_userEndpoint.EndGetProvisioningData(m_userEndpoint.BeginGetProvisioningData(null, null));
                PersistentChatConfiguration ChatConfiguration = provisioningData.PersistentChatConfiguration;
                Uri ChatServerUri = new Uri(ChatConfiguration.DefaultPersistentChatUri);

                Trace.WriteLine(string.Format("ChatRoom Uri:{0}", ChatConfiguration.DefaultPersistentChatUri));

                m_chatEndpoint = new PersistentChatEndpoint(ChatServerUri, m_userEndpoint);
                m_chatEndpoint.EndEstablish(m_chatEndpoint.BeginEstablish(null, null));

                Trace.WriteLine("Success connect to chat room server");
            }
            catch(Exception ex)
            {
                Trace.WriteLine(string.Format("Exception on connect to room server. {0}", ex.ToString()));
                return false;
            }
           

            //list chat rooms
            try
            {
                PersistentChatServices chatServices = m_chatEndpoint.PersistentChatServices;
                m_collectionRooms = chatServices.EndBrowseMyChatRooms(chatServices.BeginBrowseMyChatRooms(null, null, 1000));
            }
            catch(Exception ex)
            {
                Trace.WriteLine(string.Format("Exception on list rooms. {0}", ex.ToString()));
                return false;
            }
            m_bConnectedToRoomServer = true;

            //join all chat rooms
            foreach (ChatRoomSnapshot r in m_collectionRooms)
            {
                JoinChatRoom(r);
            }

            return true;
           
        }

        public ChatRoomSnapshot GetChatRoomByName(string strRoomName)
        {
            foreach(ChatRoomSnapshot r in m_collectionRooms)
            {
                if(r.Name.Equals(strRoomName, StringComparison.OrdinalIgnoreCase))
                {
                    return r;
                }
            }

            return null;
        }

       
        public ChatRoomSession GetChatRoomSession(string strRoomUri)
        {
            if(m_dicRoomSessions.ContainsKey(strRoomUri))
            {
                return m_dicRoomSessions[strRoomUri];
            }
            else
            {
                return null;
            }  
        }

        public void LeaveAllChatRoom()
        {
            foreach(var roomSessionPair in m_dicRoomSessions)
            {
                LeaveChatRoom(roomSessionPair.Value);
            }

            m_dicRoomSessions.Clear();
        }
       
        public void LeaveChatRoom(ChatRoomSession chatRoomSession)
        {
            try
            {
                if (chatRoomSession != null && chatRoomSession.State == ChatRoomSessionState.Established)
                {
                    chatRoomSession.EndLeave(chatRoomSession.BeginLeave(null, null));
                    Trace.WriteLine("Leave chat room success.");
                }   
            }
            catch(Exception ex)
            {
                Trace.WriteLine(string.Format("Leave chat room exception:{0}", ex.ToString()));
            }
        }

        public bool UploadFileToChatRoom(ChatRoomSession roomSession, string strFileName)
        {
//             if (!roomSession.IsFilePostAllowed)
//             {
//                 return false;
//             }

            //upload
            try
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(strFileName);
                ChatRoomFileUploadJob job = new ChatRoomFileUploadJob(fileInfo);
                // job.ProgressChanged += new EventHandler<ChatRoomFileTransferProgressEventArgs>(job_ProgressChanged);
                roomSession.EndUploadFile(roomSession.BeginUploadFile(job, null, null));
                return true;
            }
            catch(Exception ex)
            {
                Trace.WriteLine(string.Format("Exception on upload file to chat room. {0}", ex.ToString()));
            }

            return false;
        }


        public bool DownloadFile(ChatRoomSession roomSession, MessagePartFileDownloadLink downloadLink, string strDestFileName)
        {
            try
            {
                ChatRoomFileDownloadJob job = new ChatRoomFileDownloadJob(downloadLink, strDestFileName);
                //job.ProgressChanged += new EventHandler<ChatRoomFileTransferProgressEventArgs>(job_ProgressChanged);
                roomSession.EndDownloadFile(roomSession.BeginDownloadFile(job, null, null));

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("exception on download file:{0}", ex.ToString()));
            }
            return false;
        }

#endregion 

        #region private function

        private void AddedChatRoomSession(ChatRoomSnapshot room, ChatRoomSession roomSession)
        {
            m_dicRoomSessions.Add(room.ChatRoomUri.ToString(), roomSession);
        }

        private ChatRoomSession JoinChatRoom(ChatRoomSnapshot room)
        {
            try
            {
                ChatRoomSession crSession = new ChatRoomSession(m_chatEndpoint);

                // Register for events raised in this chat room session.
                // crSession.ChatMessageReceived += new EventHandler<ChatMessageReceivedEventArgs>(crSession_ChatMessageReceived);
                //crSession.ChatRoomSessionStateChanged += new EventHandler<ChatRoomSessionStateChangedEventArgs>(crSession_ChatRoomSessionStateChanged);

                crSession.EndJoin(crSession.BeginJoin(room.ChatRoomUri, null, null));

                Trace.WriteLine(string.Format("Join chat room success. {0}", room.Name));

                AddedChatRoomSession(room, crSession);

                return crSession;
            }
            catch(Exception ex)
            {
                Trace.WriteLine(string.Format("Exception on JoinChatRoom, {0}", ex.ToString()));
            }

            return null;
        }

        #endregion



        #region Event Call Back

        #endregion
    }
}
