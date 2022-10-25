using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Power shell, reference Ststem.Management.Automation.dll
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;

// Other project
using SFBCommon.Common;
using SFBCommon.SFBObjectInfo;


namespace TestProject.PowerShellTest
{
    class PowerShellTester
    {
        static public List<SFBPersistentChatServerInfo> TestSFBPersistentChatServerInfo()
        {
            // Get all current chat category info
            List<SFBPersistentChatServerInfo> lsSFBPersistentChatServerInfo = SFBPersistentChatServerInfo.GetAllCurPersistentChatServerInfo();
            if (null != lsSFBPersistentChatServerInfo)
            {
                // Show message
                foreach (SFBPersistentChatServerInfo obSFBPersistentChatServerInfo in lsSFBPersistentChatServerInfo)
                {
                    obSFBPersistentChatServerInfo.OutputObjInfo();
                }
            }
            else
            {
                Console.Write("Get file store info failed\n");
            }
            return lsSFBPersistentChatServerInfo;
        }

#if false
        static public List<SFBFileStoreServiceInfo> TestFileStoreService()
        {
            // Get all current chat category info
            List<SFBFileStoreServiceInfo> lsFileStoreServiceInfo = SFBFileStoreServiceInfo.GetAllCurFileStoreServiceInfo();
            if (null != lsFileStoreServiceInfo)
            {
                // Show message
                foreach (SFBFileStoreServiceInfo obChatCategoryInfo in lsFileStoreServiceInfo)
                {
                    obChatCategoryInfo.OutputObjInfo();
                }
            }
            else
            {
                Console.Write("Get file store info failed\n");
            }
            return lsFileStoreServiceInfo;
        }
#endif

        static public List<SFBChatCategoryInfo> TestChatCategory()
        {
            // Get all current chat category info
            List<SFBChatCategoryInfo> lsChatCategoryInfo = SFBChatCategoryInfo.GetAllCurChatCategoryInfo();
            if (null != lsChatCategoryInfo)
            {
                // Show message
                foreach (SFBObjectInfo obChatCategoryInfo in lsChatCategoryInfo)
                {
                    obChatCategoryInfo.OutputObjInfo();
                }
            }
            else
            {
                Console.Write("Get chat category info failed\n");
            }
            return lsChatCategoryInfo;
        }

        static public List<SFBChatRoomInfo> TestChatRoom()
        {
            // Get all current chat category info
            List<SFBChatRoomInfo> lsChatRoomInfo =  SFBChatRoomInfo.GetAllCurChatRoomInfo();
            if (null != lsChatRoomInfo)
            {
                // Show message
                foreach (SFBChatRoomInfo obChatCategoryInfo in lsChatRoomInfo)
                {
                    obChatCategoryInfo.OutputObjInfo();
                }
            }
            else
            {
                Console.Write("Get chat category info failed\n");
            }
            return lsChatRoomInfo;
        }

        static public List<SFBUserInfo> TestSFBUserInfo()
        {
            List<SFBUserInfo> lsSFBUserInfo =  SFBUserInfo.GetAllCurSFBUserInfo(null);
            if (null != lsSFBUserInfo)
            {
                // Show message
                foreach (SFBUserInfo obUserInfo in lsSFBUserInfo)
                {
                    obUserInfo.OutputObjInfo();
                    obUserInfo.PersistantSave();
                }
            }
            else
            {
                Console.Write("Get SFB user info failed\n");
            }
            return lsSFBUserInfo;
        }
    }
}
