using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;

namespace SFBCommon.SFBObjectInfo
{
    public class NLChatRoomInfo : SFBObjectInfo
    {
        #region Static values: field names
        public static readonly string kstrUriFieldName = "uri";
        public static readonly string kstrEnforcerFieldName = "enforcer";
        #endregion

        #region Constructors
        public NLChatRoomInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public NLChatRoomInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public NLChatRoomInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoNLChatRoom;
        }
        #endregion
    }
}
