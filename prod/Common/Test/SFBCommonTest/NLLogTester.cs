using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Other projects
using SFBCommon.Common;
using SFBCommon.NLLog;

namespace TestProject.SFBCommonTest
{
    class LogTester : Logger
    {
        static public void Test()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Log test begin");
            // Using NLLog
            EMSFB_LOGLEVEL[] szEmLogLevels = {EMSFB_LOGLEVEL.emLogLevelDebug, EMSFB_LOGLEVEL.emLogLevelError, EMSFB_LOGLEVEL.emLogLevelInfo, EMSFB_LOGLEVEL.emLogLevelWarn, EMSFB_LOGLEVEL.emLogLevelFatal};
            foreach (EMSFB_LOGLEVEL emLogLevel in szEmLogLevels)
            {
                theLog.OutputLog(emLogLevel, "The current log level is:[{0}]\n", emLogLevel);
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Log test end");
        }

        static public void TestLastErrorRecord()
        {
            LastErrorRecorder.SetLastError(123);
           
            int nLastError = LastErrorRecorder.GetLastError();
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Last error:[{0}]\n", nLastError);
        }
    }
}
