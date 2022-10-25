using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;

namespace SFBCommon.SFBObjectInfo
{
    public class NLAssistantServiceInfo : SFBObjectInfo
    {
        #region Static values: field names
        public static readonly string kstrUriFieldName = "uri";
        public static readonly string kstrAssistantTokenFieldName = "assistanttoken";
        #endregion

        #region Constructors
        public NLAssistantServiceInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public NLAssistantServiceInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public NLAssistantServiceInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoNLAssistantServiceInfo;
        }
        #endregion
    }
}
