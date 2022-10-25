using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.NLLog;

namespace Nextlabs.SFBServerEnforcer.SIPComponent
{
    #region Command info objects
    class UserStartupCmdInfo
    {
        #region Const values
        public const string kstrCmdHeaderFlag = "-";
        public const string kstrCmdNLDebugFlag = "-NLDebug";
        #endregion

        #region Static functions
        static public bool IsStartWithDebugMode(string[] szArgs)
        {
            return szArgs.Contains(UserStartupCmdInfo.kstrCmdNLDebugFlag);
        }
        #endregion
    }
    #endregion

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] szArgs)
        {
            if (UserStartupCmdInfo.IsStartWithDebugMode(szArgs))
            {
                // Debug
                WaitForStart();
                SIPComponentMain.MainX(szArgs);
                WaitForStop();

                SIPComponentMain.Exit();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                {
                    new SIPComponentService() 
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        #region Debug
        static private bool WaitForStart()  // Return true, continue
        {
            Console.WriteLine("Debug running, you can enter q/Q to exit, others continue.\n");
            ConsoleKeyInfo obKeyInfo = Console.ReadKey();
            bool bRet = (ConsoleKey.Q != obKeyInfo.Key);
            if (bRet)
            {
                Console.WriteLine("\nRunning\n");
            }
            return bRet;
        }
        static private void WaitForStop()
        {
            Console.WriteLine("If you want to exit, you can entry q/Q to exit.\n");
            while (true)
            {
                ConsoleKeyInfo obKeyInfo = Console.ReadKey();
                if (ConsoleKey.Q == obKeyInfo.Key)
                {
                    break;
                }
            }
            Console.WriteLine("\nStart exit\n");
        }
        #endregion
    }
}
