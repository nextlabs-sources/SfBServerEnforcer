using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QAToolSFBCommon.Common;
using QAToolSFBCommon.NLLog;

namespace QAToolSFBCommon.Performance
{
    public class ClientMeetingPerformanceLogInfo
    {
        public string m_strTimeBeginJoin = null;   // // 2016-47-27 09:47:18:099
        public string m_strTimeEndJoin = null;
        private int m_nTimeDiff = 0;    // ms

        public ClientMeetingPerformanceLogInfo(string strTimeBeginJoin, string strTimeEndJoin)
        {
            m_strTimeBeginJoin = strTimeBeginJoin;
            m_strTimeEndJoin = strTimeEndJoin;
        }
    }

    public class ClientMeetingPerformanceLogAnalyzer : Logger
    {
        #region Const value

        #endregion

        #region Members
        private Dictionary<string, ClientMeetingPerformanceLogInfo> m_dicMeetingPerformanceLog = new Dictionary<string, ClientMeetingPerformanceLogInfo>();
        private string m_strLogFolder = null;
        private string m_strResultFolder = null;
        #endregion

        #region Constructors
        public ClientMeetingPerformanceLogAnalyzer(string strLogFolder, string strResultFolder)
        {
            m_strLogFolder = strLogFolder;
            m_strResultFolder = strResultFolder;
            if (!Directory.Exists(m_strLogFolder))
            {
                Directory.CreateDirectory(m_strLogFolder);
            }
            if (!Directory.Exists(m_strResultFolder))
            {
                Directory.CreateDirectory(m_strResultFolder);
            }
        }
        #endregion

        #region Public Functions
        public void Analysis(int nTotalJoinCount = 0, int nTotalJoinFailedCount = 0, int nTotalIMSendCount = 0, int nTotalIMReceivedCount = 0)
        {
            if ((string.IsNullOrEmpty(m_strLogFolder)) || (!Directory.Exists(m_strLogFolder)))
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "The log folder:[{0}] is not exists\n", m_strLogFolder);
            }
            else
            {
                DirectoryInfo dirLogFolder = new DirectoryInfo(m_strLogFolder);
                FileInfo[] szFileInfo = dirLogFolder.GetFiles();
                foreach (FileInfo obFileInfo in szFileInfo)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Performance log file name:[{0}]\n", obFileInfo.FullName);
                    string strFileContent = FileOpHelper.ReadAllFileContent(obFileInfo.FullName, FileMode.Open);

                    string[] szLogInfoLines = strFileContent.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 1; i < szLogInfoLines.Length; ++i)    // The first line is header
                    {
                        string[] szLogInfoItems = szLogInfoLines[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (3 == szLogInfoItems.Length)
                        {
                            string strConferenceUri = szLogInfoItems[0];
                            string strTimeBeginJoin = szLogInfoItems[1]; // 2016-47-27 09:47:18:099
                            string strTimeEndJoin = szLogInfoItems[2];

                            ClientMeetingPerformanceLogInfo obClientMeetingPerformanceLogInfo = CommonHelper.GetValueByKeyFromDir(m_dicMeetingPerformanceLog, strConferenceUri, null);
                            if (null == obClientMeetingPerformanceLogInfo)
                            {
                                CommonHelper.AddKeyValuesToDir(m_dicMeetingPerformanceLog, strConferenceUri, new ClientMeetingPerformanceLogInfo(strTimeBeginJoin, strTimeEndJoin));
                            }
                            else
                            {
                                string strNewTimeBeginJoin = obClientMeetingPerformanceLogInfo.m_strTimeBeginJoin;
                                if (0 > CompareStringTime(strTimeBeginJoin, strNewTimeBeginJoin))
                                {
                                    strNewTimeBeginJoin = strTimeBeginJoin;
                                }

                                string strNewTimeEndJoin = obClientMeetingPerformanceLogInfo.m_strTimeEndJoin;
                                if (0 < CompareStringTime(strTimeEndJoin, strNewTimeEndJoin))
                                {
                                    strNewTimeEndJoin = strTimeEndJoin;
                                }
                                CommonHelper.AddKeyValuesToDir(m_dicMeetingPerformanceLog, strConferenceUri, new ClientMeetingPerformanceLogInfo(strNewTimeBeginJoin, strNewTimeEndJoin));
                            }
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current item:[{0}] in file:[{1}] is not correct\n", i, obFileInfo.FullName);
                        }
                    }
                }
                SaveAnalysisResult(nTotalJoinCount, nTotalJoinFailedCount, nTotalIMSendCount, nTotalIMReceivedCount);
            }
        }
        private void SaveAnalysisResult(int nTotalJoinCount, int nTotalJoinFailedCount, int nTotalIMSendCount, int nTotalIMReceivedCount)
        {
            PerformanceMoniter obPerformanceMoniter = new PerformanceMoniter(EMSFB_MONITERTYPE.emClientMeetingPerformanceMoniter, m_strResultFolder);
            foreach (KeyValuePair<string, ClientMeetingPerformanceLogInfo> pairMeetingPerformanceLogInfo in m_dicMeetingPerformanceLog)
            {
                DateTime dtBeginJoinTime = ConverStringToDateTime(pairMeetingPerformanceLogInfo.Value.m_strTimeBeginJoin);
                DateTime dtEndJoinTime = ConverStringToDateTime(pairMeetingPerformanceLogInfo.Value.m_strTimeEndJoin);
                double dbTimeDiff = TimeDiff(dtEndJoinTime, dtBeginJoinTime);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "ConferenceUri:[{0}], BeginJoin:[{1}], EndJoin:[{2}], TotalJoinTime:[{3}]\n", pairMeetingPerformanceLogInfo.Key, pairMeetingPerformanceLogInfo.Value.m_strTimeBeginJoin, pairMeetingPerformanceLogInfo.Value.m_strTimeEndJoin, dbTimeDiff);

                obPerformanceMoniter.AddPerformanceInfo(pairMeetingPerformanceLogInfo.Key, pairMeetingPerformanceLogInfo.Value.m_strTimeBeginJoin, pairMeetingPerformanceLogInfo.Value.m_strTimeEndJoin, dbTimeDiff.ToString(), nTotalJoinCount.ToString(), nTotalJoinFailedCount.ToString(), nTotalIMSendCount.ToString(), nTotalIMReceivedCount.ToString(), "\n");
            }
        }
        #endregion

        #region Tools
        static private DateTime ConverStringToDateTime(string strTime) // 2016-47-27 09:47:18:099
        {
            try
            {
                string[] szStrTime = strTime.Split(new char[] { '-', ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Input strTime:[{0}], ArrayTime length:[{1}]\n", strTime, szStrTime.Length);
                    for (int nIndex = 0; nIndex < szStrTime.Length; ++nIndex)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Index:[{0}], time:[{1}]\n", nIndex, szStrTime[nIndex]);
                    }
                }

                if (7 <= szStrTime.Length)
                {
                    return new DateTime(Int32.Parse(szStrTime[0]), Int32.Parse(szStrTime[1]), Int32.Parse(szStrTime[2]), Int32.Parse(szStrTime[3]), Int32.Parse(szStrTime[4]), Int32.Parse(szStrTime[5]), Int32.Parse(szStrTime[6]));
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Convert string time:[{0}] to DateTime failed, {1}\n", strTime, ex.Message);
            }
            return new DateTime(0);
        }
        static private int CompareStringTime(string strTime1, string strTime2)
        {
            return string.Compare(strTime1, strTime2);
        }
        static private double TimeDiff(DateTime dataTimeFirst, DateTime dataTimeSecond)  // ms
        {
            return (dataTimeFirst - dataTimeSecond).TotalMilliseconds;
        }
        #endregion
    }
}
