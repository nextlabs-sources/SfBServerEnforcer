using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;

namespace SFBCommon.SFBObjectInfo
{
    public class NLChatRoomAttachmentInfo : SFBObjectClassifyTags
    {
        #region Static values: field names
        public static readonly string kstrAttachmentUniqueFlagFieldName = "attachmentuniqueflag";
        public static readonly string kstrFilePathFieldName = "filepath";
        public static readonly string kstrFileTagsFieldName = "filetags";
        public static readonly string kstrFileOwnerFieldName = "fileowner";
        public static readonly string kstrChatRoomUriFieldName = "chatroomuri";
        public static readonly string kstrLastModifyTimeFieldName = "lastmodifytime";
        public static readonly string kstrFileOrgNameFieldName = "fileorgname";
        #endregion

        #region Constructors
        public NLChatRoomAttachmentInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
            SetClassifyTagsFieldName(kstrFileTagsFieldName);
        }
        public NLChatRoomAttachmentInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
            SetClassifyTagsFieldName(kstrFileTagsFieldName);
        }
        public NLChatRoomAttachmentInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
            SetClassifyTagsFieldName(kstrFileTagsFieldName);
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoNLChatRoomAttachment;
        }
        #endregion
    }
}
