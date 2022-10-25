using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;         // Power shell

using SFBCommon.Common;
using SFBCommon.NLLog;

namespace SFBCommon.SFBObjectInfo
{
    public class SFBChatRoomInfo : SFBObjectInfo
    {
        #region Static values: field names
        public static readonly string kstrUriFieldName = "uri";
        public static readonly string kstrNameFieldName = "chatroom_name";
        public static readonly string kstrDescriptionFieldName = "description";
        public static readonly string kstrPrivacyFieldName = "privacy";
        public static readonly string kstrAddInFieldName = "addin";
        public static readonly string kstrCategoryUriFieldName = "categoryuri";
        public static readonly string kstrMembersFieldName = "members";
        public static readonly string kstrManagersFieldName = "managers";
        public static readonly string kstrEnableInvitationsFieldName = "enableinvitations";
        public static readonly string kstrCreateTimeFieldName = "createtime";
        public static readonly string kstrPresentersFieldName = "creator";
        #endregion

        #region Static values: power shell command
        static private readonly string kstrPSCmdGetChatRoom = "Get-CsPersistentChatRoom";
        #endregion

        #region Constructors
        public SFBChatRoomInfo(Microsoft.Rtc.Management.Chat.Cmdlets.ChatRoom obChatCmdletsChatRoom)
        {
            InitChatRoomInfoByPSChatRoom(obChatCmdletsChatRoom);
        }
        public SFBChatRoomInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public SFBChatRoomInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public SFBChatRoomInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoSFBChatRoom;
        }
        #endregion

        #region Tools
        private void InitChatRoomInfoByPSChatRoom(Microsoft.Rtc.Management.Chat.Cmdlets.ChatRoom obChatCmdletsChatRoom)
        {
            InnerSetItem(kstrUriFieldName, CommonHelper.GetObjectStringValue(obChatCmdletsChatRoom.ChatRoomUri));
            InnerSetItem(kstrNameFieldName, CommonHelper.GetObjectStringValue(obChatCmdletsChatRoom.Name));
            InnerSetItem(kstrDescriptionFieldName, CommonHelper.GetObjectStringValue(obChatCmdletsChatRoom.Description));
            InnerSetItem(kstrPrivacyFieldName, CommonHelper.GetObjectStringValue(obChatCmdletsChatRoom.Privacy));   // Open, Closed, Secret
            InnerSetItem(kstrAddInFieldName, CommonHelper.GetObjectStringValue(obChatCmdletsChatRoom.Addin));
            InnerSetItem(kstrCategoryUriFieldName, CommonHelper.GetObjectStringValue(obChatCmdletsChatRoom.CategoryUri));
            InnerSetItem(kstrMembersFieldName, CommonHelper.ConvertListToString(obChatCmdletsChatRoom.Members, SFBObjectInfo.kstrMemberSeparator, false));
            InnerSetItem(kstrManagersFieldName, CommonHelper.ConvertListToString(obChatCmdletsChatRoom.Managers, SFBObjectInfo.kstrMemberSeparator, false));
            InnerSetItem(kstrEnableInvitationsFieldName, CommonHelper.GetObjectStringValue(obChatCmdletsChatRoom.Invitations));     // Inherit, False
            InnerSetItem(kstrCreateTimeFieldName, "");   // this is runtime info
            InnerSetItem(kstrPresentersFieldName, CommonHelper.ConvertListToString(obChatCmdletsChatRoom.Presenters, SFBObjectInfo.kstrMemberSeparator, false));
        }
        #endregion

        #region Powershell test functions
        static public List<SFBChatRoomInfo> GetAllCurChatRoomInfo()
        {
            List<SFBChatRoomInfo> lsChatCategoryInfo = null;
            Collection<PSObject> collectionPSChatCategoryInfos = PowerShellHelper.RunPowershell(kstrLyncPSModuleName, kstrPSCmdGetChatRoom);
            if (null != collectionPSChatCategoryInfos)
            {
                lsChatCategoryInfo = new List<SFBChatRoomInfo>();
                foreach (PSObject obPSObject in collectionPSChatCategoryInfos)
                {
                     Microsoft.Rtc.Management.Chat.Cmdlets.ChatRoom obChatCmdletsCategory = obPSObject.BaseObject as Microsoft.Rtc.Management.Chat.Cmdlets.ChatRoom;
                     if (null != obChatCmdletsCategory)
                     {
                         lsChatCategoryInfo.Add(new SFBChatRoomInfo(obChatCmdletsCategory));
                     }
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "failed to invoke:[{0}]\n", kstrPSCmdGetChatRoom);
            }
            return lsChatCategoryInfo;
        }
        #endregion
    }
}