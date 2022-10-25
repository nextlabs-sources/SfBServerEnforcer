using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.SFBObjectInfo;
using SFBCommon.Common;
using SFBCommon.NLLog;
using SFBCommon.ClassifyHelper;

namespace SFBCommon.Common
{
    public class CommonCfgMgr
    {
        #region Logger
        private static CLog theLog = CLog.GetLogger(typeof(CommonCfgMgr));
        #endregion

        #region Static values
        static private CommonCfgMgr m_CommonCfgInstance;
        
        #endregion

        #region Fields
        public bool DefaultPolicyResult { get { return m_bDefaultPolicyResult; } }
        public bool DefaultForceInheritChatCategoryEnforcer { get { return m_bDefaultForceInheritChatCategoryEnforcer; } }
        public bool DefaultChatRoomCategoryEnforcer { get { return m_bDefaultChatRoomCategoryEnforcer; } }
        public bool DefaultEnableChatRoomEnforcer { get { return m_bDefaultEnableChatRoomEnforcer; } }
        public bool DefaultEnableMeetingEnforcer { get { return m_bDefaultEnableMeetingEnforcer; } }
        #endregion

        #region Members
        private CommonConfigureInfo m_obComCfgInfo;
        private NLClassificationSchemaInfo m_obClassificationSchemaInfo;

        private bool m_bDefaultChatRoomCategoryEnforcer = false;
        private bool m_bDefaultEnableChatRoomEnforcer = false;
        private bool m_bDefaultEnableMeetingEnforcer = false;
        private bool m_bDefaultForceInheritChatCategoryEnforcer = true;
        private bool m_bDefaultPolicyResult = true;
        #endregion 

        public static CommonCfgMgr GetInstance()
        {
            if(m_CommonCfgInstance==null)
            {
                m_CommonCfgInstance = new CommonCfgMgr();
            }
            return m_CommonCfgInstance;
        }

        #region Constructor
        private CommonCfgMgr()
        {
            // Init common default config info
            m_obComCfgInfo = new CommonConfigureInfo();
            bool bEstablished = m_obComCfgInfo.EstablishObjFormPersistentInfo();
            if (bEstablished)
            {
                string strDefaultValue = m_obComCfgInfo.GetItemValue(CommonConfigureInfo.kstrKeyDefaultChatCategoryEnforcer);
                m_bDefaultChatRoomCategoryEnforcer = CommonHelper.ConverStringToBoolFlag(strDefaultValue, false);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "{0}={1}\n", CommonConfigureInfo.kstrKeyDefaultChatCategoryEnforcer, strDefaultValue);

                strDefaultValue = m_obComCfgInfo.GetItemValue(CommonConfigureInfo.kstrKeyDefaultForceInheritChatCategoryEnforcer);
                m_bDefaultForceInheritChatCategoryEnforcer = CommonHelper.ConverStringToBoolFlag(strDefaultValue, true);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "{0}={1}\n", CommonConfigureInfo.kstrKeyDefaultForceInheritChatCategoryEnforcer, strDefaultValue);

                strDefaultValue = m_obComCfgInfo.GetItemValue(CommonConfigureInfo.kstrKeyDefaultPolicyResult);
                m_bDefaultPolicyResult = strDefaultValue.Equals("allow", StringComparison.OrdinalIgnoreCase);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "{0}={1}\n", CommonConfigureInfo.kstrKeyDefaultPolicyResult, strDefaultValue);
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "CommonConfigureInfo.EstablishObjFormPersistentInfo failed.\n");
            }

            // Init classification schema info
            m_obClassificationSchemaInfo = new NLClassificationSchemaInfo();
        }
        #endregion
    }
}
