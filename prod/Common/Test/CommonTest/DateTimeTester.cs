using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.NLLog;

namespace TestProject.CommonTest
{
    class DateTimeTester
    {
        #region Logger
        protected static CLog theLog = CLog.GetLogger("HTTPComponent:CHttpParserBase");
        #endregion

        #region Const/Read only values
        public const string kstrTimeFormat = "yyyy-MM-dd HH:mm:ss:fff";
        #endregion

        static public void Test()
        {
            const string kstrTestFilePath = "C:\\Kim_Work\\Temp\\tempData.txt";
            DateTime timeFileLastModifyTimeUtc = File.GetLastWriteTimeUtc(kstrTestFilePath);
            string strFileLastModifyTime = timeFileLastModifyTimeUtc.ToString(kstrTimeFormat);
            DateTime dtNew = GetDateTimeFromString(strFileLastModifyTime, kstrTimeFormat);
            bool bIsSameTime = IsSameTime(timeFileLastModifyTimeUtc, dtNew, new TimeSpan(0, 0, 0, 0, 10));

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "OrgTime:[{0}], NewTime:[{1}], TheSame:[{2}]\n", timeFileLastModifyTimeUtc, dtNew, bIsSameTime);
        }

        static public bool IsSameTime(DateTime dtFirst, DateTime dtSecond, TimeSpan timSpanAccuracy)
        {
            DateTime dtBigger = dtFirst + timSpanAccuracy;
            DateTime dtSmaller = dtFirst - timSpanAccuracy;
            return ((dtBigger >= dtSecond) && (dtSmaller <= dtSecond));
        }
        static public DateTime GetDateTimeFromString(string strDateTime, string strDatetTimePattern)
        {
            DateTime timeRet = new DateTime(0);
            try
            {
                timeRet = DateTime.ParseExact(strDateTime, strDatetTimePattern, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in GetDateTimeFromString, string time:[{0}], pattern:[{1}], Message:[{2}]\n", strDateTime, strDatetTimePattern, ex.Message);
            }
            return timeRet;
        }
    }
}
