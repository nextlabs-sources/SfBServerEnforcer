using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

using SFBCommon.Common;

namespace NLAssistantWebService
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            SFBCommon.Startup.InitSFBCommon(EMSFB_MODULE.emSFBModule_NLAssistantWebService);
            bool bLoadSuccess = NLConfigurationHelper.s_obConfigInfo.LoadConfigInfo(EMSFB_MODULE.emSFBModule_NLAssistantWebService, EMSFB_CFGINFOTYPE.emCfgInfoNLAssistantWebService);
            if (!bLoadSuccess)
            {
                throw new Exception(NLConfigurationHelper.s_obConfigInfo.GetLoadStatusInfo());
            }           
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            SFBCommon.Startup.UninitSFBCommon();
        }
    }
}