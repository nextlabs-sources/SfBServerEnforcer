using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;

namespace SFBCommon.SFBObjectInfo
{
    public class NLChatCategoryInfo : SFBObjectInfo
    {
        #region Static values: field names
        public static readonly string kstrUriFieldName = "uri";
        public static readonly string kstrEnforcerFieldName = "enforcer";
        public static readonly string kstrForceEnforcerFieldName = "forceenforcer";
        public static readonly string kstrClassifyTagsFieldName = "classifytags";
        public static readonly string kstrClassificationFieldName = "classification";
        public static readonly string kstrClassificationSchemaNameFieldName = "classificationschemaname";
        #endregion

        #region Constructors
        public NLChatCategoryInfo(params string[] szStrKeyAndValus) : base (szStrKeyAndValus)
        {
        }
        public NLChatCategoryInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base (szPairKeyAndValus)
        {
        }
        public NLChatCategoryInfo(Dictionary<string, string> dirInfos) : base (dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoNLChatCategory;
        }
        #endregion
    }
}
