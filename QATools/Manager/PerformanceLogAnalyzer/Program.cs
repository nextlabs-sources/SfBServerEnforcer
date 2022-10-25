using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QAToolSFBCommon.NLLog;
using QAToolSFBCommon.Performance;

namespace PerformanceLogAnalyzer
{
    class Program
    {
        #region Command key and value
        public const string kstrCommandKeyLogType = "-LogType";
        public const string kstrCommandValueLogType = "MeetingClientPerformanceLog";
        
        public const string kstrCommandKeylogFolder = "-LogFolder";
        public const string kstrCommandKeyResultFolder = "-ResultFolder";
        #endregion

        static void Main(string[] szArgs)
        {
            // Analysis start up parameters
            EMSFB_MONITERTYPE emMoniterType = EMSFB_MONITERTYPE.emUnknownMoniter;
            string strLogFolder = null;
            string strResultFolder = null;
            if ((null != szArgs) && (0 < szArgs.Length) && (0 == (szArgs.Length % 2)))
            {
                for (int i = 1; i < szArgs.Length; i += 2)
                {
                    if (szArgs[i - 1].Equals(kstrCommandKeyLogType, StringComparison.OrdinalIgnoreCase))
                    {
                        if (szArgs[i].Equals(kstrCommandValueLogType, StringComparison.OrdinalIgnoreCase))
                        {
                            emMoniterType = EMSFB_MONITERTYPE.emClientMeetingPerformanceMoniter;
                        }
                    }
                    else if (szArgs[i - 1].Equals(kstrCommandKeylogFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        strLogFolder = szArgs[i];
                    }
                    else if (szArgs[i - 1].Equals(kstrCommandKeyResultFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        strResultFolder = szArgs[i];
                    }
                }
                StartAnalysisPerformanceLogs(emMoniterType, strLogFolder, strResultFolder);
            }
            else
            {
                Console.WriteLine("Command parameters error \n");
                OutputCommandHelperInfo();
            }

            Console.Write("End test\nYou can input q/Q to exit\n");
            ConsoleKeyInfo obKey = new ConsoleKeyInfo();
            while (true)
            {
                obKey = Console.ReadKey();
                if ((obKey.Key == ConsoleKey.Q))
                {
                    break;
                }
            }
        }

        #region Tools
        static private void OutputCommandHelperInfo()
        {
            Console.WriteLine("[{0}:{1}]\n[{2}:log folder]\n[{3}:result folder]\n", kstrCommandKeyLogType, kstrCommandValueLogType, kstrCommandKeylogFolder, kstrCommandKeyResultFolder);
        }
        static private void StartAnalysisPerformanceLogs(EMSFB_MONITERTYPE emMoniterType, string strLogFolder, string strResultFolder)
        {
            switch(emMoniterType)
            {
            case EMSFB_MONITERTYPE.emClientMeetingPerformanceMoniter:
            {
                ClientMeetingPerformanceLogAnalyzer obClientMeetingPerformanceLogAnalyzer = new ClientMeetingPerformanceLogAnalyzer(strLogFolder, strResultFolder);
                obClientMeetingPerformanceLogAnalyzer.Analysis();
                break;
            }
            case EMSFB_MONITERTYPE.emServerMeetingPerformanceMoniter:
            {
                break;
            }
            default:
            {
                break;
            }
            }
        }
        #endregion
    }
}
