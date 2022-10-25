using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QAToolSFBCommon.Common
{
    public enum EMSFB_INFOTYPE
    {
        emInfoUnknown,
        
    }

    public enum EMSFB_INFOCOMPAREOP
    {
        emSearchOp_Equal,       // =
        emSearchOp_NotEqual,    // !=
        emSearchOp_AboveEqual,  // >=
        emSearchOp_LessEqual,   // <=
        emSearchOp_Above,       // >
        emSearchOp_Less,        // <
        emSearchOp_Like,        // LIKE
    }

    public enum EMSFB_INFOLOGICOP
    {
        emSearchLogicAnd,
        emSearchLogicOr,
    }

    public struct STUSFB_INFOFIELD
    {
        public EMSFB_INFOTYPE emTableInfoType;
        public string strField;                 // if this is field name or value

        public STUSFB_INFOFIELD(EMSFB_INFOTYPE emParamTableInfoType, string strParamField)
        {
            emTableInfoType = emParamTableInfoType;
            strField = strParamField;
        }
        public STUSFB_INFOFIELD(STUSFB_INFOFIELD stuInfoField)
        {
            emTableInfoType = stuInfoField.emTableInfoType;
            strField = stuInfoField.strField;
        }
    }

    public struct STUSFB_INFOITEM
    {
        public STUSFB_INFOFIELD stuFiledName;   // Table and FieldName
        public STUSFB_INFOFIELD stuFiledValue;  // Table and FieldValue, if table is unknown it is fixed value
        public EMSFB_INFOCOMPAREOP emInfoCompareOp;

        public STUSFB_INFOITEM(STUSFB_INFOFIELD stuParamFiledName, STUSFB_INFOFIELD stuParamFiledValue, EMSFB_INFOCOMPAREOP emParamInfoCompareOp)
        {
            stuFiledName = stuParamFiledName;
            stuFiledValue = stuParamFiledValue;
            emInfoCompareOp = emParamInfoCompareOp;
        }
        public STUSFB_INFOITEM(STUSFB_INFOITEM stuInfoItem)
        {
            stuFiledName = stuInfoItem.stuFiledName;
            stuFiledValue = stuInfoItem.stuFiledValue;
            emInfoCompareOp = stuInfoItem.emInfoCompareOp;
        }
    }

    public struct STUSFB_CONDITIONGROUP
    {
        public List<STUSFB_INFOITEM> lsComditonItems;
        public EMSFB_INFOLOGICOP emLogicOp;

        public STUSFB_CONDITIONGROUP(List<STUSFB_INFOITEM> lsParamComditonItems, EMSFB_INFOLOGICOP emParamLogicOp)
        {
            lsComditonItems = lsParamComditonItems;
            emLogicOp = emParamLogicOp;
        }
        public STUSFB_CONDITIONGROUP(STUSFB_CONDITIONGROUP stuConditionGroup)
        {
            lsComditonItems = stuConditionGroup.lsComditonItems;
            emLogicOp = stuConditionGroup.emLogicOp;
        }
    }

    // If failed, the functions will set last error code
    public interface IPersistentStorage : IDisposable
    {
        Dictionary<string, string>[] GetAllObjInfo(EMSFB_INFOTYPE emInfoType);
        Dictionary<string, string>[] GetObjInfoEx(EMSFB_INFOTYPE emInfoType, string strFieldName, params string[] szFieldValues);
        Dictionary<string, string> GetObjInfo(EMSFB_INFOTYPE emInfoType, string strKey, string strValue);
        Dictionary<string, string>[] GetObjInfoByLikeValue(EMSFB_INFOTYPE emInfoType, string strKeyFieldName, string strLikeValue);
        bool SaveObjInfo(EMSFB_INFOTYPE emInfoType, Dictionary<string, string> dirInfos);

        // lsSpecifyOutFields:
        //      1. can be null or empty means return all
        //      2. do not suggest to using null or empty
        //      3. if some filed is same in difference tables only return the last one
        // lsSearchScopes cannot be null or empty
        // lsSearchConditions cannot be null or empty
        Dictionary<EMSFB_INFOTYPE, Dictionary<string, string>>[] GetObjInfoWithFullSearchConditions(List<STUSFB_INFOFIELD> lsSpecifyOutFields, List<EMSFB_INFOTYPE> lsSearchScopes, List<KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>> lsSearchConditions);
    }
}
