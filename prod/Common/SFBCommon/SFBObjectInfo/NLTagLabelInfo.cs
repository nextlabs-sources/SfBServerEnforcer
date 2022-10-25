using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;

namespace SFBCommon.SFBObjectInfo
{
    public class NLTagLabelInfo : SFBObjectInfo
    {
        #region Static values: field names
        public static readonly string kstrTagNameFieldName = "tagname";
        public static readonly string kstrTagValuesFieldName = "tagvalues";
        public static readonly string kstrDefaultValueFieldName = "defaultvalue";
        public static readonly string kstrEditableFieldName = "editable";
        public static readonly string kstrMultiSelectFieldName = "multiselect";
        public static readonly string kstrMandatoryFieldName = "mandatory";
        public static readonly string kstrDeprecatedFieldName = "deprecated";
        public static readonly string kstrTrueFieldValue = "true";
        public static readonly string kstrFalseFieldValue = "false";
        #endregion

        #region Constructors
        public NLTagLabelInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public NLTagLabelInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public NLTagLabelInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoNLTagLabel;
        }
        #endregion
    }
}
