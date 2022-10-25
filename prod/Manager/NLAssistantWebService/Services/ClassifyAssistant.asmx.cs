using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Collections.Specialized;
using System.Security.Policy;

using SFBCommon.NLLog;
using SFBCommon.Common;
using SFBCommon.WebServiceHelper;
using SFBCommon.CommandHelper;

using NLAssistantWebService.Tokens;
using NLAssistantWebService.Common;

namespace NLAssistantWebService.Services
{
    /// <summary>
    /// Summary description for ClassifyAssitant
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class ClassifyAssitant : System.Web.Services.WebService
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(ClassifyAssitant));
        #endregion

        #region Const/Read only values
        #region HTTP Header flag
        public const string kstrHTTPHeaderUserAgentFlag = "User-Agent";
        #endregion

        public const string kstrRelativeClassifyAssistantAsmx = "/Services/ClassifyAssistant.asmx";
        #endregion

        #region Members
        private TokenManager m_obTokenManager = new TokenManager();
        #endregion

        #region Web Methods

        [WebMethod]
        public string GetUserClassifyMeetingUrl(string strUserSipUri, string strMeetingUri)
        {
            try
            {
                HttpRequest obCurHttpRequest = HttpContext.Current.Request;
                {
                    OutputRequestInfo(obCurHttpRequest);
                }
                ClassifyMeetingServiceResponseInfo obClassifyMeetingServiceResponseInfo = new ClassifyMeetingServiceResponseInfo("", ClassifyMeetingServiceResponseInfo.kstrStatusUnknownError);
                if (!string.IsNullOrEmpty(strUserSipUri))
                {
                    bool bIsValidRequest = IsValidRequest(obCurHttpRequest);
                    if (bIsValidRequest)
                    {
                        if (!string.IsNullOrEmpty(strMeetingUri))
                        {
                            bIsValidRequest = ClassifyCommandHelper.IsClassifyManager(new STUSFB_CLASSIFYCMDINFO(EMSFB_COMMAND.emCommandClassifyMeeting, strUserSipUri, strMeetingUri));
                        }
                        if (bIsValidRequest)
                        {
                            obClassifyMeetingServiceResponseInfo.strStatus = ClassifyMeetingServiceResponseInfo.kstrStatusSuccess;
                            obClassifyMeetingServiceResponseInfo.strClassifyMeetingUrl = CreateNewUserClassifyUrl(obCurHttpRequest.Url, strUserSipUri, strMeetingUri);
                        }
                        else
                        {
                            obClassifyMeetingServiceResponseInfo.strStatus = ClassifyMeetingServiceResponseInfo.kstrStatusNoClassifyPermission;
                        }
                    }
                    else
                    {
                        obClassifyMeetingServiceResponseInfo.strStatus = ClassifyMeetingServiceResponseInfo.kstrStatusInvalidRequest;
                    }
                }
                return ClassifyMeetingWebServiceHelper.EstablishClassifyMeetingResponseInfo(obClassifyMeetingServiceResponseInfo);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!!Exception in GetUserClassifyMeetingUrl, [{0}]\n", ex.Message);
            }
            return "";
        }
        #endregion

        #region Inner tools
        private void OutputRequestInfo(HttpRequest obCurHttpRequest)
        {
            // Debug
            NameValueCollection httpCurRquestHeaders = obCurHttpRequest.Headers;
            for (int i = 0; i < httpCurRquestHeaders.Count; ++i)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "HeaderName:[{0}],HeaderValue:[{1}]\n", httpCurRquestHeaders.GetKey(i), httpCurRquestHeaders.GetValues(i));
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Url:[{0}], UserAgent:[{1}], WebPageUrl:[{2}]\n", obCurHttpRequest.Url, obCurHttpRequest.UserAgent, GetClassifyMeetingPageUrl(obCurHttpRequest.Url));
        }
        private bool IsValidRequest(HttpRequest obCurHttpRequest)
        {
            // Only support POST method with user agent "Assistant"
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "User Agent:[{0}], Method:[{1}]\n", obCurHttpRequest.UserAgent, obCurHttpRequest.HttpMethod);
            return (WebServiceHelper.kstrHTTPMethodPost.Equals(obCurHttpRequest.HttpMethod, StringComparison.OrdinalIgnoreCase) && (ClassifyMeetingWebServiceHelper.kstrUserAgent_Assistant.Equals(obCurHttpRequest.UserAgent, StringComparison.OrdinalIgnoreCase)));
        }
        private string GetClassifyMeetingPageUrl(Uri obCurRquestUri)
        {
            if (null != obCurRquestUri)
            {
                const string kstrProtocolHeader = "http://";
                int nIndex = obCurRquestUri.AbsoluteUri.IndexOf(kstrRelativeClassifyAssistantAsmx, StringComparison.OrdinalIgnoreCase);
                if (kstrProtocolHeader.Length < nIndex)
                {
                    return obCurRquestUri.AbsoluteUri.Substring(0, nIndex) + CommonValues.kstrRelativeClassifyMeetingUrl;
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error to analysis page url, using the fix default one\n");
                }
            }
            return "";
        }
        private string CreateNewUserClassifyUrl(Uri obCurRquestUri, string strUserSipUri, string strMeetingSipUri)
        {
            if ((null != obCurRquestUri) && (!string.IsNullOrEmpty(strUserSipUri)))
            {
                string strNewClassifyUri = GetClassifyMeetingPageUrl(obCurRquestUri);
                if (!string.IsNullOrEmpty(strNewClassifyUri))
                {
                    ClassifyToken obClassifyToken = m_obTokenManager.GetNewUserToken(strUserSipUri, EMSFB_TOKENTYPE.enTokenType_ClassifyToken);
                    if ((null != obClassifyToken) && (obClassifyToken.IsValidToken()))
                    {
                        Dictionary<string, string> obParameters = new Dictionary<string,string>()
                        {
                            {CommonValues.kstrURLParamUserFlag, strUserSipUri},
                            {CommonValues.kstrURLParamTokenIDFlag, obClassifyToken.TokenID},
                            {CommonValues.kstrURLParamMeetingUriFlag, CommonHelper.GetSolidString(strMeetingSipUri)}
                        };
                        strNewClassifyUri += "?" + CommonHelper.ConnectionDicKeyAndValues(obParameters, true, false, CommonValues.kstrSepURLParamKeys, CommonValues.kstrSepURLParamKeyValue);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "New classify url:[{0}]\n", strNewClassifyUri);
                        return strNewClassifyUri;
                    }
                }
            }
            return "";
        }
        #endregion

    }
}
