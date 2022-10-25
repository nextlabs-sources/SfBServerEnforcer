using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.Common;
using SFBCommon.NLLog;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.Common
{
    static class SIPComponentConfig
    {
        #region Const value
        private static readonly string kstrDefaultMeetingInviteMsgDenyBeforeManualClassifyDone = "Please classify the meeting before you can invite others to join.";
        private static readonly string kstrDefaultMeetingJoinMsgDenyBeforeManualClassifyDone = "You are not authorized to join the meeting. Please contact meeting owner for help.";
        private static readonly string kstrDefaultMeetingShareCreateMsgDenyBeforeManualClassifyDone = "Please classify the meeting before you sharing.";
        private static readonly string kstrDefaultMeetingShareJoinMsgDenyBeforeManualClassifyDone = "You are not authorized to view the meeting sharing information. Please contact meeting owner for help.";
        #endregion

        #region config value
        public static readonly bool kbNeedRecordPerformanceLog = false;
        public static readonly int knMinThreadPoolWorkerThreads = 0;
        public static readonly string kstrMeetingInviteMsgDenyBeforeManualClassifyDone = kstrDefaultMeetingInviteMsgDenyBeforeManualClassifyDone;
        public static readonly string kstrMeetingJoinMsgDenyBeforeManualClassifyDone = kstrDefaultMeetingJoinMsgDenyBeforeManualClassifyDone;
        public static readonly string kstrMeetingShareCreateMsgDenyBeforeManualClassifyDone = kstrDefaultMeetingShareCreateMsgDenyBeforeManualClassifyDone;
        public static readonly string kstrMeetingShareJoinMsgDenyBeforeManualClassifyDone = kstrDefaultMeetingShareJoinMsgDenyBeforeManualClassifyDone;
        #endregion

        #region
        private static ConfigureFileManager s_obCfgFileMgr = null;
        private static readonly Dictionary<EMSFB_ENDPOINTTTYPE,STUSFB_ENDPOINTPROXYTCPINFO> s_dicEndpointTcpInfo = null;
        #endregion

        static SIPComponentConfig()
        {
            try
            {
                s_obCfgFileMgr = new ConfigureFileManager(EMSFB_MODULE.emSFBModule_SIPComponent);

                //get endpointproxy config
                s_dicEndpointTcpInfo = s_obCfgFileMgr.GetEndpointProxyTcpInfo();

                //get prompt message
                STUSFB_PROMPTMSG prompMsg = s_obCfgFileMgr.GetPromptMsg(EMSFB_CFGINFOTYPE.emCfgInfoSip);
                string strRecordPerformance = CommonHelper.GetValueByKeyFromDir(prompMsg.m_dirRuntimeInfo, ConfigureFileManager.kstrXMLRecordPerformanceLogFlag, false.ToString());
                kbNeedRecordPerformanceLog = Convert.ToBoolean(strRecordPerformance);

                string strThreadPoolMinThread = CommonHelper.GetValueByKeyFromDir(prompMsg.m_dirRuntimeInfo, ConfigureFileManager.kstrXMLThreadPoolMinThreadCountFlag, "0");
                knMinThreadPoolWorkerThreads = int.Parse(strThreadPoolMinThread);

                //Error message
                kstrMeetingInviteMsgDenyBeforeManualClassifyDone = CommonHelper.GetValueByKeyFromDir(prompMsg.m_dirErrorMsg, ConfigureFileManager.kstrXMLMeetingInviteMsgDenyBeforeManualClassifyDone, new STUSFB_ERRORMSG(kstrDefaultMeetingInviteMsgDenyBeforeManualClassifyDone, 0)).m_strErrorMsg;
                kstrMeetingJoinMsgDenyBeforeManualClassifyDone = CommonHelper.GetValueByKeyFromDir(prompMsg.m_dirErrorMsg, ConfigureFileManager.kstrXMLMeetingJoinMsgDenyBeforeManualClassifyDone, new STUSFB_ERRORMSG(kstrDefaultMeetingJoinMsgDenyBeforeManualClassifyDone,0)).m_strErrorMsg;
                kstrMeetingShareCreateMsgDenyBeforeManualClassifyDone = CommonHelper.GetValueByKeyFromDir(prompMsg.m_dirErrorMsg, ConfigureFileManager.kstrXMLMeetingShareCreateMsgDenyBeforeManualClassifyDone, new STUSFB_ERRORMSG(kstrDefaultMeetingShareCreateMsgDenyBeforeManualClassifyDone, 0)).m_strErrorMsg;
                kstrMeetingShareJoinMsgDenyBeforeManualClassifyDone = CommonHelper.GetValueByKeyFromDir(prompMsg.m_dirErrorMsg, ConfigureFileManager.kstrXMLMeetingShareJoinMsgDenyBeforeManualClassifyDone, new STUSFB_ERRORMSG(kstrDefaultMeetingShareJoinMsgDenyBeforeManualClassifyDone, 0)).m_strErrorMsg;
            }
            catch(Exception ex)
            {
                CLog.OutputTraceLog("Exceptioin on LoadConfig:{0}", ex.ToString());
                throw ex;
            }
        }

        public static STUSFB_ENDPOINTPROXYTCPINFO GetEndpointProxyTcpInfo(EMSFB_ENDPOINTTTYPE emEndpointType)
        {
            return CommonHelper.GetValueByKeyFromDir(s_dicEndpointTcpInfo, emEndpointType, null);
        }
    }
}
