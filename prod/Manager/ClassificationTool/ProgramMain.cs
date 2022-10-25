using SFBCommon.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassificationTool
{
    static class ProgramMain
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            LoadSFBCommon();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ClassificationForm());
        }

        private static void LoadSFBCommon()
        {
            SFBCommon.Startup.InitSFBCommon(EMSFB_MODULE.emSFBModule_ClassificationTool);
            bool isSuccess = NLConfigurationHelper.s_obConfigInfo.LoadConfigInfo(EMSFB_MODULE.emSFBModule_ClassificationTool, EMSFB_CFGINFOTYPE.emCfgInfoClassificationTool);
            if(!isSuccess)
            {
                throw new Exception(string.Format("Load SFBCommon Failed"));
            }
        }
    }
}
