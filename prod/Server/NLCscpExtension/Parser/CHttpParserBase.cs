using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using System.IO;

using SFBCommon.NLLog;

using NLCscpExtension.ResponseFilters;
using NLCscpExtension.RequestFilters;

namespace NLCscpExtension.Parser
{
    public enum EMSFB_CSCP_REQUEST_TYPE
    {
        emRequestUnknown = 0,

        emRequestCscpStartUp
    }

    class CHttpParserBase
    {
        #region Logger
        protected static CLog theLog = CLog.GetLogger(typeof(CHttpParserBase));
        #endregion

        #region Members
        protected Stream m_oldResponseFilterStream = null;
        protected ResponseFilter m_newResponseFilter = null;

        protected Stream m_oldRequestFilterStream = null;
        protected RequestFilter m_newRequestFilter = null;

        protected HttpRequest m_httpRequest = null;
        #endregion

        #region Constructors
        public CHttpParserBase(HttpRequest obHttpRequest)
        {
            m_oldResponseFilterStream = null;
            m_newResponseFilter = null;
            m_oldRequestFilterStream = null;
            m_newRequestFilter = null;
            m_httpRequest = obHttpRequest;
        }
        #endregion

        #region virtual public functions
        public virtual void PreRequestHandler(Object source, EventArgs e)
        {
            try
            {
                HttpApplication application = (HttpApplication)source;
                HttpContext context = application.Context;

                //replace request filter, this must be placed at the entry, before any other code.
                ReplaceRequestFilter(context.Request);


                //replace response filter
                ReplaceResponseFilter(context.Response);
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, ex.ToString());
            }
        }
        public virtual void EndRequest(Object source, EventArgs e)
        {
            HttpApplication application = (HttpApplication)source;
            Reset(application);
        }
        #endregion

        #region Virtual protected functions
        protected virtual RequestFilter CreateRequestFilter(HttpRequest request)
        {
            if (null != request)
            {
                return new RequestFilter(request);
            }
            return null;
        }
        protected virtual ResponseFilter CreateResponseFilter(HttpResponse httpResponse)
        {
            if((null != httpResponse) && (null != httpResponse.Filter))
            {
                return new ResponseFilter(httpResponse);
            }
            return null;
        }
        protected virtual void Reset(HttpApplication application)
        {
            //reset response filter
            if ((m_oldResponseFilterStream != null) && (application.Context.Response.Filter != null))
            {
                application.Context.Response.Filter = m_oldResponseFilterStream;
                m_oldResponseFilterStream = null;
            }

            //reset response filter
            if ((m_oldRequestFilterStream != null) && (m_httpRequest.Filter != null))
            {
                m_httpRequest.Filter = m_oldRequestFilterStream;
                m_oldRequestFilterStream = null;
            }
        }
        #endregion

        #region Inner tools
        protected void ReplaceResponseFilter(HttpResponse httpResponse)
        {
            if(httpResponse.Filter!=null)
            {
                ResponseFilter newResponseFilter = CreateResponseFilter(httpResponse);
                if(newResponseFilter!=null)
                {
                    m_oldResponseFilterStream = httpResponse.Filter;
                    httpResponse.Filter = newResponseFilter;
                }
            }
        }
        protected void ReplaceRequestFilter(HttpRequest httpRequest)
        {
            if (httpRequest.Filter != null)
            {
                RequestFilter newRequestFilter = CreateRequestFilter(httpRequest);
                if (newRequestFilter != null)
                {
                    m_oldRequestFilterStream = httpRequest.Filter;
                    httpRequest.Filter = newRequestFilter;
                }
            }
        }
        #endregion
    }
}
