using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;

namespace SFBCommon.SFBObjectInfo
{
    public class NLClassificationSchemaInfo : SFBObjectInfo
    {
        #region Static values: field names
        public static readonly string kstrSchemaNameFieldName = "schemaname";
        public static readonly string kstrDataFieldName = "data";
        public static readonly string kstrDescriptionFieldName = "description";
        public static readonly string kstrDeprecatedFieldName = "deprecated";
        public static readonly string kstrTrueFieldValue = "true";
        public static readonly string kstrFalseFieldValue = "false";
        #endregion

        #region Constructors
        public NLClassificationSchemaInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public NLClassificationSchemaInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public NLClassificationSchemaInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoNLClassificationSchema;
        }
        #endregion
    }
}
