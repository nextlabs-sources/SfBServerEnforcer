using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;
using SFBCommon.NLLog;

namespace SFBCommon.SFBObjectInfo
{
    public class CommonConfigureInfo : SFBObjectInfo
    {
        #region Const values: names
        public const string kstrKeyDefaultEnableMeetingEnforcer = "defaultenablemeetingenforcer";
        public const string kstrKeyDefaultEnableChatRoomEnforcer = "defaultenablechatroomenforcer";
        public const string kstrKeyDefaultPolicyResult = "defaultpolicyresult";
        public const string kstrKeyDefaultChatCategoryEnforcer = "defaultchatcategoryenforcer";
        public const string kstrKeyDefaultForceInheritChatCategoryEnforcer = "defaultforceinheritchatcategoryenforcer";

        public const string kstrValueDefaultEnableMeetingEnforcer = "false";
        public const string kstrValueDefaultEnableChatRoomEnforcer = "false";
        public const string kstrValueDefaultPolicyResult = "allow";
        public const string kstrValueDefaultChatCategoryEnforcer = "false";
        public const string kstrValueDefaultForceInheritChatCategoryEnforcer = "true";
        #endregion

        #region Static values: field names
        internal const string kstrNameFieldName = "name";
        internal const string kstrValueFieldName = "value";

        internal static readonly KeyValuePair<string, string>[] szPairInitValues = new KeyValuePair<string, string>[]
         {
             new KeyValuePair<string, string>(kstrKeyDefaultEnableMeetingEnforcer, kstrValueDefaultEnableMeetingEnforcer),
             new KeyValuePair<string, string>(kstrKeyDefaultEnableChatRoomEnforcer, kstrValueDefaultEnableChatRoomEnforcer),
             new KeyValuePair<string, string>(kstrKeyDefaultPolicyResult, kstrValueDefaultPolicyResult),
             new KeyValuePair<string, string>(kstrKeyDefaultChatCategoryEnforcer, kstrValueDefaultChatCategoryEnforcer),
             new KeyValuePair<string, string>(kstrKeyDefaultForceInheritChatCategoryEnforcer, kstrValueDefaultForceInheritChatCategoryEnforcer)
         };
        #endregion

        #region Constructors
        public CommonConfigureInfo(params string[] szStrKeyAndValus) : base (szStrKeyAndValus)  // ["DefaultPolicyResult", "allow"]
        {
        }
        public CommonConfigureInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base (szPairKeyAndValus)  // {"DefaultPolicyResult", "allow"}
        {
        }
        public CommonConfigureInfo(Dictionary<string, string> dirInfos) : base (dirInfos)   // {"DefaultPolicyResult", "allow"}
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoCommonConfigure;
        }
        #endregion

        #region Override Persistant read and write
        public override bool PersistantSave()
        {
            bool bRet = false;
            IPersistentStorage obIPersistentStorage = GetIPersistentStorage();
            if (null != obIPersistentStorage)
            {
                Dictionary<string, string> dicComCfgInfo = GetInfo();   //  {"DefaultPolicyResult", "allow"} ==> [{"name","DefaultPolicyResult"}, {"value","allow"}] ==> save to persistent storage
                if (null != dicComCfgInfo)
                {
                    bRet = true;
                    foreach (KeyValuePair<string, string> pairComCfgItem in dicComCfgInfo)
                    {
                        bRet &= obIPersistentStorage.SaveObjInfo(EMSFB_INFOTYPE.emInfoCommonConfigure, new Dictionary<string, string>() { { kstrNameFieldName, pairComCfgItem.Key }, { kstrValueFieldName, pairComCfgItem.Value } });
                    }
                }
            }
            return bRet;
        }
        public override bool EstablishObjFormPersistentInfo(string strKey = null, string strValue = null)   // for CommonConfigure, key is null, it will establish the class will all persistent info
        {
            bool bRet = false;
            IPersistentStorage obIPersistentStorage = GetIPersistentStorage();
            if (null != obIPersistentStorage)
            {
                Dictionary<string, string>[] szDirInfo = obIPersistentStorage.GetAllObjInfo(EMSFB_INFOTYPE.emInfoCommonConfigure);
                if (null != szDirInfo)
                {
                    bRet = true;
                    foreach (Dictionary<string, string> dirInfo in szDirInfo) // [{"name","DefaultPolicyResult"}, {"value","allow"}] ==> {"DefaultPolicyResult", "allow"} ==> save to inner dictinary
                    {
                        if (dirInfo.ContainsKey(kstrNameFieldName) && dirInfo.ContainsKey(kstrValueFieldName))
                        {
                            bRet &= SetItem(dirInfo[kstrNameFieldName], dirInfo[kstrValueFieldName]);
                        }
                        else
                        {
                            bRet = false;
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Inner error for common configure table in persistent storage.\n");
                            break;
                        }
                    }
                }
            }
            return bRet;
        }
        #endregion
    }
}
