using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QAToolSFBCommon.NLLog;

namespace QAToolControlCenter.Common
{
    class BasicConfigInfo
    {
        #region Members
        public string m_strPerformanceOrgFolder = "";
        public string m_strPerformanceResultLogFolder = "";
        public int m_nPort = 0;
        #endregion

        #region Constructor
        public BasicConfigInfo(string strPerformanceOrgFolder, string strPerformanceResultLogFolder, int nPort)
        {
            m_strPerformanceOrgFolder = strPerformanceOrgFolder;
            m_strPerformanceResultLogFolder = strPerformanceResultLogFolder;
            m_nPort = nPort;
        }
        #endregion
    }

    class BasicUserInfo
    {
        #region Members
        public string m_strServerFQDN = "";
        public string m_strUserDomain = "";
        public string m_strUserBaseFirstName = "";
        public string m_strUserLastName = "";
        public string m_strPassword = "";
        public int m_nUserMinIndex = 1;
        public int m_nUserMaxIndex = 100;
        #endregion

        #region Constructor
        public BasicUserInfo(string strServerFQDN, string strUserDomain, string strUserBasicFirstName, string strUserLastName, string strPassword, int nUserMinIndex, int nUserMaxIndex)
        {
            m_strServerFQDN = strServerFQDN;
            m_strUserDomain = strUserDomain;
            m_strUserBaseFirstName = strUserBasicFirstName;
            m_strUserLastName = strUserLastName;
            m_strPassword = strPassword;
            m_nUserMinIndex = nUserMinIndex;
            m_nUserMaxIndex = nUserMaxIndex;
        }
        #endregion
    }

    class ConferenceInfo
    {
        #region Members
        public string m_strConferenceUri = "";
        public int m_nConferenceLength = 30;
        public int m_nJoinTimes = 10;
        public int m_nJoinInterval = 3;
        public int m_nIMInterval = 5;
        public int m_nJoinedUserMinIndex = 1;
        public int m_nJoinedUserMaxIndex = 1;
        #endregion

        #region Constructor
        public ConferenceInfo(string strConferenceUri, int nConferenceLength=30, int nJoinTimes=10, int nJoinInterval=3, int nIMInterval=5, int nJoinedUserMinIndex=1, int nJoinedUserMaxIndex=1)
        {
            m_strConferenceUri = strConferenceUri;
            m_nConferenceLength = nConferenceLength;
            m_nJoinTimes = nJoinTimes;
            m_nJoinInterval = nJoinInterval;
            m_nIMInterval = nIMInterval;
            m_nJoinedUserMinIndex = nJoinedUserMinIndex;
            m_nJoinedUserMaxIndex = nJoinedUserMaxIndex;
        }
        #endregion
    }

    class ControlCenterConfigInfo
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(ControlCenterConfigInfo));
        #endregion

        #region Config XML define
        private const string kstrXMLPerformanceOrgFolderFlag = "PerformanceOrgFolder";
        private const string kstrXMLPerformanceResultLogFolderFlag = "PerformanceResultLog";
        #endregion

        #region Members





        #endregion

        #region Constructor
        public ControlCenterConfigInfo()
        {

        }
        #endregion

        #region Public functions
        #endregion

        #region Private tools
        
        #endregion
    }
}
