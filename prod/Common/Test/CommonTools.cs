using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.NLLog;

namespace TestProject
{
    class CommonTools : Logger
    {
        static public void OutputLog(EMSFB_LOGLEVEL emLogLevel, string strFormat, params object[] szArgs)
        {
            theLog.OutputLog(emLogLevel, strFormat, szArgs);
            Console.Write(strFormat, szArgs);
        }
    }
}
