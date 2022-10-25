using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.NLLog;
using SFBCommon.Common;
using SFBCommon.SFBObjectInfo;

namespace CoreManager
{
    public class SFBUserManager
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(SFBUserManager));
        #endregion

        public delegate void DelegateOutputPromptInfo(string strFormat, params object[] szArgs);

        static public bool EstablishSFBUserInfoPersistentMapping(string strDisplayName, DelegateOutputPromptInfo pFuncOutputPromptInfo)
        {
            bool bRet = false;
            pFuncOutputPromptInfo("Begin to establish user info mapping\n");
            List<SFBUserInfo> lsSFBUserInfo = SFBUserInfo.GetAllCurSFBUserInfo(strDisplayName);
            if (null != lsSFBUserInfo)
            {
                pFuncOutputPromptInfo( "User total count:[{0}]\n", lsSFBUserInfo.Count);
                if (0 == lsSFBUserInfo.Count)
                {
                    bRet = true;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Do not find any users info wiht display name:[{0}]\n", strDisplayName);
                }
                else
                {
                    int i = 0;
                    foreach (SFBObjectInfo obUserInfo in lsSFBUserInfo)
                    {
                        LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_SUCCESS);
                        bRet = obUserInfo.PersistantSave();
                        if (!bRet)
                        {
                            pFuncOutputPromptInfo("Persistent save with last error:[{0}]\n", LastErrorRecorder.GetLastError());
                            break;
                        }
                        ++i;
                        if (0 == (i%100))
                        {
                            pFuncOutputPromptInfo("Remaining {0} users info\n", (lsSFBUserInfo.Count-i));
                        }
                    }
                }
            }
            else
            {
                pFuncOutputPromptInfo("Establish SFB User info persistent mapping failed, maybe you don't have \"RTC Server Universal\" Permission\n");
            }
            pFuncOutputPromptInfo("End establish user info mapping: [{0}]\n", bRet?"Success":"Failed");
            return bRet;
        }
    }
}
