using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

// UCMA
using Microsoft.Rtc.Collaboration;  // Basic namespace
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Collaboration.PersistentChat;
using Microsoft.Rtc.Signaling;      // SipTransportType

namespace NLLyncEndpointProxy
{
    class TestCaseActionChatRoomDownloadFile : TestCaseActionBase
    {
        public string RoomName;
        public string FileFolder;
        public int FileCount;
        public string LogFile;


        public TestCaseActionChatRoomDownloadFile(string strRoomName, string strFileFolder, int nFileCount, string strLogFile):
            base(TEST_CASE_TYPE.ChatRoomDownloadFile)
        {
            RoomName = strRoomName;
            FileFolder = strFileFolder;
            FileCount = nFileCount;
            LogFile = strLogFile;
        }

        public override void TestCaseWorkThread()
        {
            Trace.WriteLine(string.Format("test case:{0} running", TEST_CASE_TYPE.ChatRoomDownloadFile.ToString()));

            //login chat room
            ChatRoomSnapshot room = RoomManager.GetChatRoomByName(RoomName);
            if (room == null)
            {
                Trace.WriteLine(string.Format("Room:{0} not exist.", RoomName));
                goto _Exit;
            }

            ChatRoomSession roomSession = RoomManager.GetChatRoomSession(room.ChatRoomUri.ToString());
            if(null==roomSession)
            {
                Trace.WriteLine(string.Format("Room:{0} didn't join.", RoomName));
                goto _Exit;
            }

            //get download link
            List<MessagePartFileDownloadLink> lstDownloadLink = new System.Collections.Generic.List<MessagePartFileDownloadLink>();

            try
            {
                ChatHistoryQueryOptions query = new ChatHistoryQueryOptions(null);
                var chatHistory = roomSession.EndQueryChatHistory(roomSession.BeginQueryChatHistory(query, null, null));
                foreach (ChatMessage message in chatHistory.Messages.Reverse() )
                {
                    foreach (MessagePart part in message.FormattedMessageParts.Reverse() )
                    {
                        if (part is MessagePartFileDownloadLink)
                        {
                            lstDownloadLink.Add(part as MessagePartFileDownloadLink);
                            if (lstDownloadLink.Count >= FileCount)
                            {
                                break;
                            }
                        }
                    }

                    if (lstDownloadLink.Count >= FileCount)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception on get file download link:{0}", ex.ToString());
                return;
            }

            //download  files and record to log file
            List<long> lstAllTimes = new List<long>();
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(LogFile))
                {
                    file.WriteLine(string.Format("Download from chatroom:{0}", RoomName));

                    foreach (MessagePartFileDownloadLink downloadLink in lstDownloadLink)
                    {
                        string strDestFileName = FileFolder + "\\" + Guid.NewGuid().ToString() + "--" + downloadLink.FileName;

                        Trace.WriteLine(string.Format("Begin download file: {0}", strDestFileName));

                        Stopwatch stopwatch = Stopwatch.StartNew();
                        bool bRes = RoomManager.DownloadFile(roomSession, downloadLink, strDestFileName);
                        stopwatch.Stop();

                        lstAllTimes.Add(stopwatch.ElapsedMilliseconds);

                        file.WriteLine(string.Format("{0}ms {1} {2}", stopwatch.ElapsedMilliseconds, bRes ? "Success" : "Failed", strDestFileName));

                        Trace.WriteLine(string.Format("End download file: {0}, time:{1}", strDestFileName, stopwatch.ElapsedMilliseconds));
                    }

                    //write Statistics info
                    List<long> lstAllTimes2 = new List<long>(lstAllTimes);
                    lstAllTimes2.Remove(lstAllTimes2.Max());
                    lstAllTimes2.Remove(lstAllTimes2.Min());

                    file.WriteLine(string.Format("Totalcount:{0}, MaxTime:{1}, MinTime:{2}, AvTime:{3}, AvTime2:{4}", lstAllTimes.Count,
                        lstAllTimes.Max(), lstAllTimes.Min(), lstAllTimes.Average(), lstAllTimes2.Average()));

                    file.Flush();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception on download. {0}", ex.ToString() ));
            }
          

           
          
            //leave chat room
            //didn't leave chat room. because maybe other case still need stand in this chat room
           // roomManager.LeaveChatRoom(roomSession);

            _Exit:
            Trace.WriteLine(string.Format("test case:{0} finished", TEST_CASE_TYPE.ChatRoomDownloadFile.ToString()));

            Status = TEST_CASE_STATUS.finished;
            FinishTestCaseCallback(1);
        }
    }
}
