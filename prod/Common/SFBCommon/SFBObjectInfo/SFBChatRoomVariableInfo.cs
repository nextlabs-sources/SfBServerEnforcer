using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;
using SFBCommon.ClassifyHelper;

namespace SFBCommon.SFBObjectInfo
{
    public class SFBChatRoomVariableInfo : SFBObjectClassifyInfo
    {
        #region Constructors
        public SFBChatRoomVariableInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public SFBChatRoomVariableInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public SFBChatRoomVariableInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoSFBChatRoomVariable;
        }
        #endregion
    }
}