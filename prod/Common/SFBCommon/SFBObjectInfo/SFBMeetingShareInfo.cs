using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;

namespace SFBCommon.SFBObjectInfo
{
    public class SFBMeetingShareInfo : SFBObjectInfo
    {
        #region Static values: field names
        public static readonly string kstrShareUriFieldName = "shareuri";
        public static readonly string kstrSharerFieldName = "sharer";
        #endregion

        #region Constructors
        public SFBMeetingShareInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public SFBMeetingShareInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public SFBMeetingShareInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoSFBMeetingShare;
        }
        #endregion
    }
}
