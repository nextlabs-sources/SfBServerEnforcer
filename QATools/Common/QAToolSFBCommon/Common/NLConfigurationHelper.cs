using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QAToolSFBCommon.Common;

namespace QAToolSFBCommon.Common
{
    public class NLConfigurationHelper
    {
        #region Static sington
        static public NLConfigurationHelper s_obConfigInfo = new NLConfigurationHelper();
        #endregion

        #region Config default info

        #endregion

        #region Members
        private ConfigureFileManager m_obCfgFileMgr = null; 
        private STUSFB_PROMPTMSG m_stuPromptMsg = null;
        private bool m_bLoadSuccess = false;
        #endregion

        #region Constructor
        protected NLConfigurationHelper()
        {
            SetLoadFlag(false);
        }
        #endregion

        #region Public functions
        public bool LoadConfigInfo(EMSFB_MODULE emModuleType)
        {
            if (!GetLoadFlag())
            {
                m_obCfgFileMgr = new ConfigureFileManager(emModuleType);
                if (m_obCfgFileMgr.IsLoadSuccess())
                {
                    m_stuPromptMsg = m_obCfgFileMgr.GetPromptMsg();
                    if (null != m_stuPromptMsg)
                    {
                        SetLoadFlag(true);
                    }
                }
            }
            return GetLoadFlag();
        }
        public string GetLoadStatusInfo()
        {
            if (GetLoadFlag())
            {
                if (null != m_obCfgFileMgr)
                {
                    return m_obCfgFileMgr.GetLoadStatusInfo();
                }
            }
            return "";
        }
        public string GetRuntimeConfigInfoByKeyFlag(string strKeyFlag, string strDefaultValue)
        {
            if (GetLoadFlag())
            {
                if (null != m_stuPromptMsg)
                {
                    return CommonHelper.GetValueByKeyFromDir(m_stuPromptMsg.m_dirRuntimeInfo, strKeyFlag, strDefaultValue);
                }
            }
            return strDefaultValue;
        }
        public STUSFB_ERRORMSG GetErrorMsgConfigInfoByKeyFlag(string strKeyFlag, STUSFB_ERRORMSG stuDefaultValue)
        {
            if (GetLoadFlag())
            {
                if (null != m_stuPromptMsg)
                {
                    return CommonHelper.GetValueByKeyFromDir(m_stuPromptMsg.m_dirErrorMsg, strKeyFlag, stuDefaultValue);
                }
            }
            return stuDefaultValue;
        }
        public bool GetLoadFlag() { return m_bLoadSuccess; }
        #endregion

        #region Inner tools
        private void SetLoadFlag(bool bLoadSuccess)
        {
            m_bLoadSuccess = bLoadSuccess;
        }
        #endregion
    }
}
