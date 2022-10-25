using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;
using SFBCommon.NLLog;
using SFBCommon.ClassifyHelper;

namespace SFBCommon.SFBObjectInfo
{
    public class NLMeetingInfo : SFBObjectInfo
    {
        #region Static values: field names
        public static readonly string kstrUriFieldName = "uri";
        public static readonly string kstrEnforcerFieldName = "enforcer";
        public static readonly string kstrManulClassifyObsFieldName = "manulclassifyobs";
        public static readonly string kstrForceManulClassifyFieldName = "forcemanulclassify";
        #endregion

        #region Static values: enfoce status
        public static readonly string kstrEnforceNA = "NA";
        public static readonly string kstrEnforceAutoYes = "Auto_YES";
        public static readonly string kstrEnforceAutoNo = "Auto_NO";
        public static readonly string kstrEnforceManualNotSet = "Manaul_NotSet";
        public static readonly string kstrEnforceManualYes = "Manual_YES";
        public static readonly string kstrEnforceManualNo = "Manual_NO";
        #endregion 

        #region Constructors
        public NLMeetingInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public NLMeetingInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public NLMeetingInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        public NLMeetingInfo()
        {
            SetItem(kstrEnforcerFieldName, kstrEnforceNA);
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoNLMeeting;
        }
        #endregion

        #region Public functions
        public ManulClassifyObligationHelper GetManulClassifyObligation()
        {
            string strManulClassifyObsInfo = GetItemValue(kstrManulClassifyObsFieldName);
            if (!string.IsNullOrEmpty(strManulClassifyObsInfo))
            {
                return new ManulClassifyObligationHelper(strManulClassifyObsInfo, false);
            }
            return null;
        }
        #endregion
    }
}
