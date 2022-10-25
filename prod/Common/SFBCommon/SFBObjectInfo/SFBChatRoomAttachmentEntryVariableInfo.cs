using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;

namespace SFBCommon.SFBObjectInfo
{
    public class SFBChatRoomAttachmentEntryVariableInfo : SFBObjectInfo
    {
        #region Static values: field names
        public static readonly string kstrTokenIDFieldName = "tokenid";
        public static readonly string kstrChatRoomUriFieldName = "chatroomuri";
        public static readonly string kstrUserFieldName = "currentuser";
        public static readonly string kstrActionFieldName = "action";
        public static readonly string kstrFileOrgNameFieldName = "fileorgname";
        #endregion

        #region Constructors
        public SFBChatRoomAttachmentEntryVariableInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public SFBChatRoomAttachmentEntryVariableInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public SFBChatRoomAttachmentEntryVariableInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoSFBChatRoomAttachmentEntryVariable;
        }
        #endregion
    }
}
