using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;

namespace SFBCommon.SFBObjectInfo
{
    public class NLAssistantQueryPolicyInfo : SFBObjectInfo
    {
        #region Static values: field names
        public static readonly string kstrStatusProcessing = "processing";
        public static readonly string kstrStatusComplete = "complete";

        public static readonly string kstrRequestIdentifyFieldName = "requestidentify";
        public static readonly string kstrResponseInfoFieldName = "responseinfo";
        public static readonly string kstrResponseStatusFieldName = "status";
        #endregion

        #region Constructors
        public NLAssistantQueryPolicyInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public NLAssistantQueryPolicyInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public NLAssistantQueryPolicyInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoNLAssistantQueryPolicyInfo;
        }
        #endregion
    }
}
