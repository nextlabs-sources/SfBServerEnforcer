using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Management.Automation;         // Power shell

// Current project
using SFBCommon.Common;
using SFBCommon.NLLog;

namespace SFBCommon.SFBObjectInfo
{
    public class SFBChatCategoryInfo : SFBObjectInfo
    {
        #region Static values: field names
        public static readonly string kstrUriFieldName = "uri";                                 // CategoryUri
        public static readonly string kstrIdentityFieldName = "chatcategoryidentity";
        public static readonly string kstrNameFieldName = "name";
        public static readonly string kstrDescriptionFieldName = "description";
        public static readonly string kstrNumberOfChatRoomsFieldName = "numberofchatrooms";
        public static readonly string kstrEnableInvitationsFieldName = "enableinvitations";
        public static readonly string kstrEnableFileUploadFieldName = "enablefileupload";
        public static readonly string kstrEnableChatHistoryFieldName = "enablechathistory";
        public static readonly string kstrAllowedMembersFieldName = "allowedmembers";
        public static readonly string kstrDeniedMembersFieldName = "deniedmembers";
        public static readonly string kstrCreatorsFieldName = "creators";
        public static readonly string kstrPersistentChatPoolFieldName = "persistentchatpool";
        #endregion

        #region Static values: power shell command
        static private readonly string kstrPSCmdGetChatCategory = "Get-CsPersistentChatCategory";
        #endregion

        #region Constructors
        public SFBChatCategoryInfo(Microsoft.Rtc.Management.Chat.Cmdlets.Category obChatCmdletsCategory)
        {
            InitChatCategoryInfoByPSCategory(obChatCmdletsCategory);
        }
        public SFBChatCategoryInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public SFBChatCategoryInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public SFBChatCategoryInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoSFBChatCategory;
        }
        #endregion

        #region Tools
        private void InitChatCategoryInfoByPSCategory(Microsoft.Rtc.Management.Chat.Cmdlets.Category obChatCmdletsCategory)
        {
            InnerSetItem(kstrUriFieldName, CommonHelper.GetObjectStringValue(obChatCmdletsCategory.CategoryUri));

            string strIdentity = CommonHelper.GetSolidString(obChatCmdletsCategory.Identity);
            InnerSetItem(kstrIdentityFieldName, strIdentity);
            string strChatPoolFQDN = GetChatPoolFQDNFromCategoryIdentity(strIdentity);
            InnerSetItem(kstrPersistentChatPoolFieldName, CommonHelper.GetSolidString(strChatPoolFQDN));

            InnerSetItem(kstrNameFieldName, CommonHelper.GetSolidString(obChatCmdletsCategory.Name));
            InnerSetItem(kstrDescriptionFieldName, CommonHelper.GetSolidString(obChatCmdletsCategory.Description));
            InnerSetItem(kstrNumberOfChatRoomsFieldName, CommonHelper.GetObjectStringValue(obChatCmdletsCategory.NumberOfChatRooms));
            InnerSetItem(kstrEnableInvitationsFieldName, CommonHelper.GetObjectStringValue(obChatCmdletsCategory.Invites));
            InnerSetItem(kstrEnableFileUploadFieldName, CommonHelper.GetObjectStringValue(obChatCmdletsCategory.FileUpload));
            InnerSetItem(kstrEnableChatHistoryFieldName, CommonHelper.GetObjectStringValue(obChatCmdletsCategory.ChatHistory));
            InnerSetItem(kstrAllowedMembersFieldName, CommonHelper.ConvertListToString(obChatCmdletsCategory.AllowedMembers, SFBObjectInfo.kstrMemberSeparator, false));
            InnerSetItem(kstrDeniedMembersFieldName, CommonHelper.ConvertListToString(obChatCmdletsCategory.DeniedMembers, SFBObjectInfo.kstrMemberSeparator, false));
            InnerSetItem(kstrCreatorsFieldName, CommonHelper.ConvertListToString(obChatCmdletsCategory.Creators, SFBObjectInfo.kstrMemberSeparator, false));
        }
        private string GetChatPoolFQDNFromCategoryIdentity(string strCategoryIdentity)
        {
            // PoolFqdn\CategoryName
            int nIndex = strCategoryIdentity.IndexOf('\\');
            if ((0 < nIndex) && (strCategoryIdentity.Length > nIndex))
            {
                return strCategoryIdentity.Substring(0, nIndex);
            }
            return "";
        }
        #endregion

        #region Powershell functions
        static public List<SFBChatCategoryInfo> GetAllCurChatCategoryInfo()
        {
            List<SFBChatCategoryInfo> lsChatCategoryInfo = null;
            Collection<PSObject> collectionPSChatCategoryInfos = PowerShellHelper.RunPowershell(kstrLyncPSModuleName, kstrPSCmdGetChatCategory);
            if (null != collectionPSChatCategoryInfos)
            {
                lsChatCategoryInfo = new List<SFBChatCategoryInfo>();
                foreach (PSObject obPSObject in collectionPSChatCategoryInfos)
                {
                    Microsoft.Rtc.Management.Chat.Cmdlets.Category obChatCmdletsCategory = obPSObject.BaseObject as Microsoft.Rtc.Management.Chat.Cmdlets.Category;
                    if (null != obChatCmdletsCategory)
                    {
                        lsChatCategoryInfo.Add(new SFBChatCategoryInfo(obChatCmdletsCategory));
                    }
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "failed to invoke:[{0}]\n", kstrPSCmdGetChatCategory);
            }
            return lsChatCategoryInfo;
        }
        #endregion
    }
}
