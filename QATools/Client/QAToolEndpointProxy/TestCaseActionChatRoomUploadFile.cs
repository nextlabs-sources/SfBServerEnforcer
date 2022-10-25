using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;

// UCMA
using Microsoft.Rtc.Collaboration;  // Basic namespace
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Collaboration.PersistentChat;
using Microsoft.Rtc.Signaling;      // SipTransportType

namespace NLLyncEndpointProxy
{
    class TestCaseActionChatRoomUploadFile : TestCaseActionBase
    {
        public string RoomName;
        public string FileFolder;
        public int FileCount;
        public string LogFile;


        public TestCaseActionChatRoomUploadFile(string strRoomName, string strFileFolder, int nFileCount, string strLogFile)
            : base(TEST_CASE_TYPE.ChatRoomUploadFile)
        {
            RoomName = strRoomName;
            FileFolder = strFileFolder;
            FileCount = nFileCount;
            LogFile = strLogFile;
        }

        public override void TestCaseWorkThread()
        {
            Trace.WriteLine(string.Format("test case:{0} running", TEST_CASE_TYPE.ChatRoomUploadFile.ToString()));


            //collect files
            List<string> lstAllFiles = new List<string>();
            try
            {
                DirectoryInfo d = new DirectoryInfo(FileFolder);
                foreach (var file in d.GetFiles())
                {
                    lstAllFiles.Add(file.FullName);
                    Trace.WriteLine(string.Format("Find files:{0}", file.FullName));

                    if(lstAllFiles.Count>=FileCount)
                    {
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine(string.Format("Exception on list files in {0}, ex={1}", FileFolder, ex.ToString()));
                goto _Exit;
            }
          

            //login chat room
            ChatRoomSnapshot room = RoomManager.GetChatRoomByName(RoomName);
            if(room==null)
            {
                Trace.WriteLine(string.Format("Room:{0} not exist.", RoomName));
                goto _Exit;
            }
            ChatRoomSession roomSession = RoomManager.GetChatRoomSession(room.ChatRoomUri.ToString());
            if (null == roomSession)
            {
                Trace.WriteLine(string.Format("Room:{0} didn't join.", RoomName));
                goto _Exit;
            }

//             if(!roomSession.IsFilePostAllowed)
//             {
//                 Trace.WriteLine(string.Format("Room:{0} don't support file upload", RoomName));
//                 return;
//             }

            //upload files
            List<long> lstAllTimes = new List<long>();
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(LogFile))
                {
                    file.WriteLine(string.Format("Upload file to chatroom:{0}", RoomName));

                    foreach (string strFile in lstAllFiles)
                    {
                        Trace.WriteLine(string.Format("begin Upload file, room:{0}, file:{1}", roomSession.Name, strFile));

                        Stopwatch stopwatch = Stopwatch.StartNew();
                        bool bRes = RoomManager.UploadFileToChatRoom(roomSession, strFile);
                        stopwatch.Stop();

                        lstAllTimes.Add(stopwatch.ElapsedMilliseconds);

                        file.WriteLine(string.Format("{0}ms {1} {2}", stopwatch.ElapsedMilliseconds, bRes ? "Success" : "Failed", strFile));

                        Trace.WriteLine(string.Format("End Upload file, room:{0}, file:{1}", roomSession.Name, strFile));
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
            catch(Exception ex)
            {
                Trace.WriteLine(string.Format("Exception on upload. {0}", ex.ToString()));
            }


            ////didn't leave chat room. because maybe other case still need stand in this chat room
             // roomManager.LeaveChatRoom(roomSession);

            _Exit:
            Trace.WriteLine(string.Format("test case:{0} finished", TEST_CASE_TYPE.ChatRoomUploadFile.ToString()));
            Status = TEST_CASE_STATUS.finished;
            FinishTestCaseCallback(1);
        }
    }
}
