using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using SFBCommon.NLLog;
using SFBCommon.Common;

using NLCscpExtension.Parser;
using NLCscpExtension.Common;

namespace NLCscpExtension
{
    /// <summary>
    /// This is a HTTP module and used in SFB CSCP service for SFBControlPanel.
    /// This HTTP module will change the response to add a link for Nextlabs SFBControlPanel.
    /// User can click this link to visit Nextlabs SFBControlPanel.
    /// </summary>
    public class NLCscpExtensionMain : IHttpModule
    {
        #region logger
        private static CLog theLog = CLog.GetLogger(typeof(NLCscpExtensionMain));
        #endregion

        #region Static Constructor
        static NLCscpExtensionMain()
        {
            CommonCfgMgr.GetInstance(); // CommonCfgMgr is a singleton pattern but "GetInstance" function is not thread safety
            SFBCommon.Startup.InitSFBCommon(EMSFB_MODULE.emSFBModule_HTTPComponent);
        }
        #endregion

        #region Static functions
        #endregion

        #region Members
        private bool m_bInited = false;
        private CHttpParserBase m_httpParser = null;
        #endregion

        #region Constructor
        public NLCscpExtensionMain()
        {

        }
        #endregion

        #region Interface impletement: IHttpModule
        public void Dispose()
        {

        }
        public void Init(HttpApplication obHttpApplication)
        {
            if (!m_bInited)
            {
                obHttpApplication.PreRequestHandlerExecute += (new EventHandler(this.ApplicationPreRequestHandler));
                obHttpApplication.EndRequest += (new EventHandler(this.ApplicationEndRequestEvent));
                m_bInited = true;
            }
        }
        #endregion

        #region HTTP Module Events
        private void ApplicationPreRequestHandler(Object source, EventArgs e)
        {
            try
            {
                HttpApplication obHttpApplication = (HttpApplication)source;
                HttpContext obHttpContext = obHttpApplication.Context;

                EMSFB_CSCP_REQUEST_TYPE emCscpRequestType = GetHttpRequestType(obHttpContext.Request);
                m_httpParser = CreateHttpParserByRequestType(emCscpRequestType, obHttpContext.Request);
                if (null == m_httpParser)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "PreRequestHandler: http parser is null with type:[{0}]\n", emCscpRequestType);
                }
                else
                {
                    m_httpParser.PreRequestHandler(source, e);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "exception on PreRequestHandler:{0}", ex.ToString());
            }
        }
        private void ApplicationEndRequestEvent(Object source, EventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Enter ApplicationEndRequest");
            try
            {
                HttpApplication obHttpApplication = (HttpApplication)source;
                HttpContext obHttpContext = obHttpApplication.Context;

                if (null != m_httpParser)
                {
                    m_httpParser.EndRequest(source, e);
                    m_httpParser = null;
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "the HTTP parser is null in end request event\n");
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "exception on ApplicationEndRequest:{0}", ex.ToString());
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exit ApplicationEndRequest");
        }
        #endregion

        #region Inner Tools
        private EMSFB_CSCP_REQUEST_TYPE GetHttpRequestType(HttpRequest obHttpRequest)
        {
            EMSFB_CSCP_REQUEST_TYPE emCscpRequestType = EMSFB_CSCP_REQUEST_TYPE.emRequestUnknown;

            string strCurFilePath = obHttpRequest.FilePath;
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Request file path:[{0}]\n", strCurFilePath);

            if (strCurFilePath.EndsWith("/cscp/default.ashx", StringComparison.OrdinalIgnoreCase))
            {
                emCscpRequestType = EMSFB_CSCP_REQUEST_TYPE.emRequestCscpStartUp;
            }
            return emCscpRequestType;
        }
        private CHttpParserBase CreateHttpParserByRequestType(EMSFB_CSCP_REQUEST_TYPE emCscpRequestType, HttpRequest obHttpRequest)
        {
            CHttpParserBase obHttpParser = null;
            switch (emCscpRequestType)
            {
            case EMSFB_CSCP_REQUEST_TYPE.emRequestCscpStartUp:
            {
                obHttpParser = new CHttpParserCscpStartUp(obHttpRequest);
                break;
            }
            default:
            {
                break;
            }
            }
            return obHttpParser;
        }
        #endregion
    }
}
