using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Other project
using QAToolSFBCommon.NLLog;
using QAToolSFBCommon.Common;
using QAToolNLChatRobot.Managers;

// Current project
using NLLyncEndpointProxy.Listener;

namespace NLLyncEndpointProxy
{
    class NLLyncEndpointProxyObj
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(NLLyncEndpointProxyObj));
        #endregion

        #region Fields
        public NLLyncEndpoint CurLyncEndpoint { get { return m_obNLLyncEndpoint; } }
        public EMSFB_ENDPOINTTTYPE CurEndpointType { get { return m_emEndpointType; } }
        public IMChatRobotManager IMRobotMgr { get { return m_obIMChatRobotMgr; } }
        #endregion

        #region Member
        private NLLyncEndpoint m_obNLLyncEndpoint = null;
        private CommandListener m_obCommandListener = null; // A listener using to receive the commands from other modules
        private EMSFB_ENDPOINTTTYPE m_emEndpointType = EMSFB_ENDPOINTTTYPE.emTypeUnknown;
        private IMChatRobotManager m_obIMChatRobotMgr = new IMChatRobotManager();
        private bool m_bSucceed = false;
        #endregion

        #region Constructor
        public NLLyncEndpointProxyObj(EMSFB_ENDPOINTTTYPE emEndpointType)
        {
            SetSucceedFlag(false);
            if (EMSFB_ENDPOINTTTYPE.emTypeUnknown != emEndpointType)
            {
                STUSFB_ENDPOINTPROXYACCOUNT stuEndpointProxyAccount = NLLyncEndpointProxyConfigInfo.s_endpointProxyConfigInfo.EndpointProxyAccount;
                STUSFB_ENDPOINTPROXYTCPINFO stuEndpointProxyTcpIp = NLLyncEndpointProxyConfigInfo.s_endpointProxyConfigInfo.EndpointProxyTcpIp;
                if ((null != stuEndpointProxyAccount) && (null != stuEndpointProxyTcpIp))
                {
                    m_emEndpointType = emEndpointType;
                 //   m_obNLLyncEndpoint = new NLLyncEndpoint(stuEndpointProxyAccount, true);
                    m_obCommandListener = new CommandListener(stuEndpointProxyTcpIp);
                    SetSucceedFlag((null != m_obNLLyncEndpoint) && (null != m_obCommandListener));
                }
            }
        }
        #endregion

        #region Public tools
        public void Exit()
        {
            if (null != m_obCommandListener)
            {
                m_obCommandListener.StopListen();
            }
            if (null != m_obNLLyncEndpoint)
            {
                m_obNLLyncEndpoint.CloseNLUserEndpoint();
            }
            // If support chat robot: m_obIMChatRobotMgr, here need delete all chat robots
        }
        public bool IsSucceed() { return m_bSucceed; }
        #endregion

        #region Private tools
        private void SetSucceedFlag(bool bSucceed)
        {
            m_bSucceed = bSucceed;
        }
        #endregion
    }
}
