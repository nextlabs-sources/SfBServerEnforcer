using System;
using System.Web;

using SFBCommon.NLLog;
using SFBCommon.Common;

using Nextlabs.SFBServerEnforcer.HTTPComponent.Parser;
using Nextlabs.SFBServerEnforcer.HTTPComponent.Common;
using Nextlabs.SFBServerEnforcer.HTTPComponent.Data;

namespace Nextlabs.SFBServerEnforcer.HTTPComponent
{
    public enum REQUEST_TYPE
    {
        REQUEST_NORMAL = 0,
        REQUEST_CHATROOM_MAIN,
        REQUEST_ROOMFORM_JS,
        REQUEST_MANAGER_HANDLE,
        REQUEST_MANAGER_CHATROOM_ATTACHMENT,
    }

    public class HTTPModuleMain : IHttpModule
    {
        #region Const/read only values
        private const string kstrCharRoomServicePath = "/PersistentChat/MGCWebService.asmx";
        #endregion

        #region Static values
        private static bool s_bCommonInit = false;
        private static CLog theLog = null;
        #endregion

        #region Static functions
        private static bool InitCommonInfoEx()
        {
            if (!s_bCommonInit)
            {
                // if (SFBBaseCommon.AssemblyLoadHelper.InitAssemblyLoaderHelper()) // No used for HTTPModule in w3wp process
                {
                    InitCommonInfo();

                    CommonCfgMgr.GetInstance();

                    s_bCommonInit = true;
                }
            }
            return s_bCommonInit;
        }
        private static void InitCommonInfo()
        {
            SFBCommon.Startup.InitSFBCommon(EMSFB_MODULE.emSFBModule_HTTPComponent);
            theLog = CLog.GetLogger(typeof(HTTPModuleMain));
        }
        #endregion

        #region Members
        protected bool m_bInited = false;
        private CHttpParserBase m_httpParser = null;
        #endregion

        #region Constructors
        public HTTPModuleMain()
        {
            m_bInited = false;
            m_httpParser = null;
        }
        #endregion

        #region Implement interface: IHttpModule
        // In the Init function, register for HttpApplication events by adding your handlers.
        public void Init(HttpApplication application)
        {
            bool bInitCommon = InitCommonInfoEx();
            if (bInitCommon)
            {
                if (!m_bInited)
                {
                    application.PreRequestHandlerExecute += (new EventHandler(this.ApplicationPreRequestHandler));
                    application.EndRequest += (new EventHandler(this.ApplicationEndRequest));
                    m_bInited = true;
                }
            }
        }
        public void Dispose()
        {
        }
        #endregion

        #region HTTP module events
        private void ApplicationPreRequestHandler(Object source, EventArgs e)
        {
            try
            {
                HttpApplication application = (HttpApplication)source;
                HttpContext context = application.Context;

#if false
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Debug info in ApplicationPreRequestHandler, method:[{0}], file path:[{1}]\n", context.Request.HttpMethod, context.Request.FilePath);
                    foreach (string strHeaderKey in context.Request.Headers.AllKeys)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Header [{0}={1}]\n", strHeaderKey, context.Request.Headers[strHeaderKey]);
                    }
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Request content lenght:[{0}], content:[\n{1}\n]\n", context.Request.ContentLength, CHttpTools.GetRequestBody(context.Request));
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "End\n");
                }
#endif

                REQUEST_TYPE requestType = GetRequestType(context);
                CHttpParserBase httpParser = CreateHttpParserByRequestType(requestType, context.Request);
                if (httpParser != null)
                {
                    httpParser.PreRequestHandler(source, e);
                }

            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "exception on PreRequestHandler:{0}", ex.ToString());
            }
        }
        private void ApplicationEndRequest(Object source, EventArgs e)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Enter ApplicationEndRequest");
            try
            {
                HttpApplication application = (HttpApplication)source;
                HttpContext context = application.Context;

                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Debug info in ApplicationEndRequest:\n");
                    foreach (string strHeaderKey in context.Response.Headers.AllKeys)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Header [{0}={1}]\n", strHeaderKey, context.Response.Headers[strHeaderKey]);
                    }
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "End\n");
                }

                //clear state, one SfbHttpModule object may process more than one HTTP request, so when end process to the request, we reset the state of this object
                if (m_httpParser != null)
                {
                    m_httpParser.EndRequest(source, e);
                    m_httpParser = null;
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "exception on ApplicationEndRequest:{0}", ex.ToString());
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exit ApplicationEndRequest");
        }
        #endregion

        #region Inner tools
        private CHttpParserBase CreateHttpParserByRequestType(REQUEST_TYPE emRequestType, HttpRequest request)
        {
            if (m_httpParser == null)
            {
                if (emRequestType == REQUEST_TYPE.REQUEST_CHATROOM_MAIN)
                {
                    m_httpParser = new CHttpParserPersistentChat(request);
                }
                else if (emRequestType == REQUEST_TYPE.REQUEST_MANAGER_HANDLE)
                {
                    m_httpParser = new CHttpParserManageHandler(request);
                }
                else if (emRequestType == REQUEST_TYPE.REQUEST_ROOMFORM_JS)
                {
                    m_httpParser = new CHttpParserRoomFormJS(request);
                }
                else if (emRequestType == REQUEST_TYPE.REQUEST_MANAGER_CHATROOM_ATTACHMENT)
                {
                    m_httpParser = new CHttpParserManagerChatRoomAttachment(request);
                }
                else
                {
                    m_httpParser = null;
                }
            }
            return m_httpParser;
        }
        private REQUEST_TYPE GetRequestType(HttpContext httpContext)
        {
            string strFilePath = httpContext.Request.FilePath;
            if (strFilePath.Equals("/PersistentChat/RM/default.aspx", StringComparison.OrdinalIgnoreCase))
            {
                return REQUEST_TYPE.REQUEST_CHATROOM_MAIN;
            }
            else if (strFilePath.Equals("/PersistentChat/RM/JScripts/RoomForm.js", StringComparison.OrdinalIgnoreCase))
            {
                return REQUEST_TYPE.REQUEST_ROOMFORM_JS;
            }
            else if (strFilePath.Equals("/PersistentChat/RM/Handler/ManagementHandler.ashx", StringComparison.OrdinalIgnoreCase))
            {
                return REQUEST_TYPE.REQUEST_MANAGER_HANDLE;
            }
            else if (strFilePath.Equals("/PersistentChat/MGCWebService.asmx", StringComparison.OrdinalIgnoreCase))
            {
                return REQUEST_TYPE.REQUEST_MANAGER_CHATROOM_ATTACHMENT;
            }
            else
            {
                return REQUEST_TYPE.REQUEST_NORMAL;
            }
        }
        #endregion
    }
}
