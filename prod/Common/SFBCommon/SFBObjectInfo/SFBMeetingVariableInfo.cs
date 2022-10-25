using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;
using SFBCommon.ClassifyHelper;

namespace SFBCommon.SFBObjectInfo
{
    public class SFBMeetingVariableInfo : SFBObjectClassifyInfo
    {
        #region Static values
        public static readonly string kstrDoneManulClassifyYes = "Yes";
        public static readonly string kstrDoneManulClassifyNo = "No";
        #endregion

        #region Constructors
        public SFBMeetingVariableInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public SFBMeetingVariableInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public SFBMeetingVariableInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoSFBMeetingVariable;
        }
        #endregion
    }
}
