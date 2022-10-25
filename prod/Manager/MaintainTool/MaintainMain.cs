using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CoreManager;
using SFBCommon.Common;
using SFBCommon.NLLog;

namespace MaintainTool
{
    class MaintainMain
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(MaintainMain));
        #endregion

        #region Const/ReadOnly common values
        private const EMSFB_MODULE kemCurModuleType = EMSFB_MODULE.emSFBModule_MaintainTool;
        #endregion

        #region Commands info
        private const string kstrCmdParamTypeFlag = "-type";

        static private readonly KeyValuePair<string, string>[] kszPairHelpInfos = new KeyValuePair<string,string>[]
        {
            new KeyValuePair<string,string>("\n"+CmdInfoWihtEstablishUserInfoMapping.kstrCmdParamHelpInfoPromptHeader, "\n"),
            new KeyValuePair<string,string>("\n"+CmdInfoWihtEstablishUserInfoMapping.kstrCmdParamHelpInfoFullCommandHeader, "\n"),
            new KeyValuePair<string,string>(CmdInfoWihtEstablishUserInfoMapping.kstrCmdParamHelpInfoTypeCommandHeader, CmdInfoWihtEstablishUserInfoMapping.kstrCmdParamHelpInfoTypeCommandBody),
            new KeyValuePair<string,string>(CmdInfoWihtEstablishUserInfoMapping.kstrCmdParamHelpInfoUserCommandHeader, CmdInfoWihtEstablishUserInfoMapping.kstrCmdParamHelpInfoUserCommandBody)
        };
        #endregion

        static void Main(string[] szCmdArgs)
        {
            SFBBaseCommon.AssemblyLoadHelper.InitAssemblyLoaderHelper();
            MainProxy(szCmdArgs);
        }

        static void MainProxy(string[] szCmdArgs)
        {
            SFBCommon.Startup.InitSFBCommon(kemCurModuleType);            // Init
            try
            {
                // Analysis command
                object obCommandInfo = null;
                EMSFB_MAINTAINTYPE emMaintainType = AnalysisUserCmdInfo(szCmdArgs, out obCommandInfo);
                switch (emMaintainType)
                {
                case EMSFB_MAINTAINTYPE.emMaintainType_establishUserInfoMapping:
                {
                    CmdInfoWihtEstablishUserInfoMapping obCmdInfoWihtEstablishUserInfoMapping = obCommandInfo as CmdInfoWihtEstablishUserInfoMapping;
                    if (null != obCmdInfoWihtEstablishUserInfoMapping)
                    {
                        if (!SFBUserManager.EstablishSFBUserInfoPersistentMapping(obCmdInfoWihtEstablishUserInfoMapping.DisplayName, OutputPromptInfo))
                        {
                            OutputPromptInfo("Failed to establish SFB user info mapping");
                        }
                    }
                    break;
                }
                case EMSFB_MAINTAINTYPE.emMaintainType_neatenMeetingInfo:
                {
                    break;
                }
                case EMSFB_MAINTAINTYPE.emMaintainType_neatenChatRoomInfo:
                {
                    break;
                }
                default:
                {
                    OutputHelpInfo();
                    break;
                }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in maintain tool:[{0}]\n", ex.Message);
                Console.Write("Some error happened in maintain tool\n");
            }
            SFBCommon.Startup.UninitSFBCommon();            // Uninit
            // WaitingForExit(); // As requirement, exit process directly
        }

        static void OutputPromptInfo(string strFormat, params object[] szArgs)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, strFormat, szArgs);
            Console.Write(strFormat, szArgs);
        }
        static void OutputHelpInfo()
        {
            const int knTabLength = 8;
            int knMaxTabNumInOneLine = (Console.WindowWidth / knTabLength) - 1;
            const int knStandardHeaderTabsNum = 4;
            const string strPreHeaderSpace = "  ";
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "ConsoleWidth:[{0}], maxTabsNum:[{1}]\n", Console.WindowWidth, knMaxTabNumInOneLine);
            foreach (KeyValuePair<string,string> pairHelpInfo in kszPairHelpInfos)
            {
                string strStandardHeader = GetStandardHeaderStr(strPreHeaderSpace+pairHelpInfo.Key, knStandardHeaderTabsNum, knTabLength);
                string strStandardBody = GetStandardBodyStr(pairHelpInfo.Value, knStandardHeaderTabsNum, knMaxTabNumInOneLine, knTabLength);
                Console.Write("{0}{1}\n", strStandardHeader, strStandardBody);
            }
        }
        static EMSFB_MAINTAINTYPE AnalysisUserCmdInfo(string[] szCmdArgs, out object obCommandInfo)
        {
            obCommandInfo = null;
            EMSFB_MAINTAINTYPE emMaintainType = EMSFB_MAINTAINTYPE.emMaintainType_unknown;
            if ((null != szCmdArgs) && (0 == (szCmdArgs.Length%2)))
            {
                for (int i=1; i<szCmdArgs.Length; i+=2)
                {
                    if (kstrCmdParamTypeFlag.Equals(szCmdArgs[i-1], StringComparison.OrdinalIgnoreCase))
                    {
                        if (CmdInfoWihtEstablishUserInfoMapping.kstrCmdParamTypeVlaue.Equals(szCmdArgs[i], StringComparison.OrdinalIgnoreCase))
                        {
                            emMaintainType = EMSFB_MAINTAINTYPE.emMaintainType_establishUserInfoMapping;
                            obCommandInfo = new CmdInfoWihtEstablishUserInfoMapping(szCmdArgs);
                        }
                    }
                }
            }
            return emMaintainType;
        }
        static void WaitingForExit()
        {
            Console.Write("\nYou can input q/Q to exit\n");
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

        #region Private tools
        static private string GetStandardHeaderStr(string strInHeader, int nStandardTabNumber, int nTabLength = 8)
        {
            if (!(string.IsNullOrEmpty(strInHeader)) && (0 < nStandardTabNumber) && (0 < nTabLength))
            {
                int nCurTabNumber = (strInHeader.Length / nTabLength);
                if (nCurTabNumber < nStandardTabNumber)
                {
                    strInHeader += CommonHelper.GetTabsStrByTabNum(nStandardTabNumber - nCurTabNumber);
                }
                else
                {
                    strInHeader += "\t";
                }
            }
            return strInHeader;
        }
        static private string GetStandardBodyStr(string strInBody, int nPreTabNumber, int nTotalTabLengthInOneLine = 9, int nTabLength = 8)
        {
            if (!(string.IsNullOrEmpty(strInBody)) && (0 <= nPreTabNumber) &&
                (0 < nTotalTabLengthInOneLine) &&
                (0 < nTabLength) && 
                (nPreTabNumber < nTotalTabLengthInOneLine))
            {
                int nLineMaxCharNum = (nTotalTabLengthInOneLine - nPreTabNumber) * nTabLength;
                string strPreTabs = CommonHelper.GetTabsStrByTabNum(nPreTabNumber);
                string[] szBody = CommonHelper.SplitStringByLength(strInBody, nLineMaxCharNum);
                return string.Join("\n" + strPreTabs, szBody);
            }
            else
            {
                return strInBody;
            }
        }
        #endregion
    }
}
