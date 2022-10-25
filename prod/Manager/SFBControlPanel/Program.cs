using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SFBControlPanel
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SFBBaseCommon.AssemblyLoadHelper.InitAssemblyLoaderHelper();

            MainProxy();
        }

        static void MainProxy()
        {
            // Init
            SFBCommon.Startup.InitSFBCommon(SFBCommon.Common.EMSFB_MODULE.emSFBModule_SFBControlPanel);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SFBControlPanel());

            // Uninit
            SFBCommon.Startup.UninitSFBCommon();
        }
    }
}
