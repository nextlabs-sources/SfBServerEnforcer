using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QAToolSFBCommon.Common;

namespace QAToolSFBCommon.NLLog
{
    public enum EMSFB_MONITERTYPE
    {
        emUnknownMoniter,

        emClientMeetingPerformanceMoniter,
        emServerMeetingPerformanceMoniter
    }

    public class PerformanceMoniter : Logger
    {
        #region Const value
        public const string kstrTimeFormat = "yyyy-MM-dd HH:mm:ss:fff";
        #endregion

        #region Static values
        static private Dictionary<EMSFB_MONITERTYPE, string> s_dicMoniterFileNameHeader = new Dictionary<EMSFB_MONITERTYPE,string>()
        {
            {EMSFB_MONITERTYPE.emClientMeetingPerformanceMoniter, "ClientMeeting_"},
            {EMSFB_MONITERTYPE.emServerMeetingPerformanceMoniter, "ServerMeeting_"}
        };
        #endregion

        #region Members
        private EMSFB_MONITERTYPE m_emMoniterType = EMSFB_MONITERTYPE.emUnknownMoniter;
        private string m_strFolderName = "";
        private string m_strMoniterFileFullPath = "";
        #endregion

        #region Constructors
        public PerformanceMoniter(EMSFB_MONITERTYPE emMoniterType, string strFolderName)
        {
            m_emMoniterType = emMoniterType;
            m_strFolderName = CommonHelper.GetStandardFolderPath(strFolderName);
            InitPerformanceMoniter();
        }
        #endregion

        #region Public functions
        public void AddPerformanceInfo(params string[] szPerformanceInfo)   // Uri, BeginTime, EndTime, TotalJoinTime,TotalJoinCount,TotalJoinFailedCount,TotalIMSendCount,TotalIMReceivedCount, \n
        {
            string strPerformanceInfo = MakeStandardPerformanceItemInfo(szPerformanceInfo);
            FileOpHelper.SaveToFile(m_strMoniterFileFullPath, FileMode.Append, strPerformanceInfo);
        }
        public EMSFB_MONITERTYPE GetMoniterType() { return m_emMoniterType; }
        #endregion

        #region Tools
        private void InitPerformanceMoniter()
        {
            if (!Directory.Exists(m_strFolderName))
            {
                Directory.CreateDirectory(m_strFolderName);
            }

            int nThreadID = Thread.CurrentThread.ManagedThreadId;
            m_strMoniterFileFullPath = m_strFolderName + CommonHelper.GetValueByKeyFromDir(s_dicMoniterFileNameHeader, m_emMoniterType, "") + nThreadID.ToString() + ".csv";
            switch(m_emMoniterType)
            {
            case EMSFB_MONITERTYPE.emClientMeetingPerformanceMoniter:
            {
                if (!File.Exists(m_strMoniterFileFullPath))
                {
                    FileOpHelper.SaveToFile(m_strMoniterFileFullPath, System.IO.FileMode.CreateNew, "ConferenceUri,BeginJoinTime,EndJoinTime,TotalJoinTime,TotalJoinCount,TotalJoinFailedCount,TotalIMSendCount,TotalIMReceivedCount\n");
                }
                break;
            }
            case EMSFB_MONITERTYPE.emServerMeetingPerformanceMoniter:
            {
                if (!File.Exists(m_strMoniterFileFullPath))
                {
                    FileOpHelper.SaveToFile(m_strMoniterFileFullPath, System.IO.FileMode.CreateNew, "ConferenceUri,BeginJoinTime,EndJoinTime,TotalJoinTime\n");
                }
                break;
            }
            default:
            {
                break;
            }
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Moniter:[{0}], file path:[{0}]\n", m_emMoniterType, m_strMoniterFileFullPath);
        }
        private string MakeStandardPerformanceItemInfo(string[] szPerformanceInfo)
        {
            if (null != szPerformanceInfo)
            {
                return string.Join(",", szPerformanceInfo);
            }
            return null;
        }
        #endregion
    }
}
