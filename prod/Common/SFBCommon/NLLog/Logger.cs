using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFBCommon.NLLog
{
    public abstract class Logger
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(Logger));
        #endregion
    }
}
