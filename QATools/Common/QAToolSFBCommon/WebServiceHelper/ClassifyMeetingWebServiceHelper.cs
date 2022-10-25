using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using QAToolSFBCommon.Common;
using QAToolSFBCommon.NLLog;

namespace QAToolSFBCommon.WebServiceHelper
{
    // Json: {strClassifyMeetingUrl:""; strErrorMessage:""}
    [DataContract(Namespace = "http://ClassifyMeetingWebServiceResponseInfo.cn.nextlabs.com")]
    public class ClassifyMeetingServiceResponseInfo
    {
        #region Const/Read only values: status define
        public const string kstrStatusSuccess = "Success";
        public const string kstrStatusUnknownError = "UnknownError";
        public const string kstrStatusNoClassifyPermission = "NoClassifyPermission";
        public const string kstrStatusInvalidRequest = "InvalidRequest";
        #endregion

        [DataMember(Order = 0)]
        public string strClassifyMeetingUrl = "";
        
        [DataMember(Order = 0)]
        public string strStatus = "";

        public ClassifyMeetingServiceResponseInfo(string strInClassifyMeetingUrl = "", string strInStatus = "")
        {
            strClassifyMeetingUrl = strInClassifyMeetingUrl;
            strStatus = strInStatus;
        }
    }

    public class ClassifyMeetingWebServiceHelper
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(ClassifyMeetingWebServiceHelper));
        #endregion

        #region Const/Read only values: classify service method and response define
        public const string kstrUserAgent_Assistant = "Assistant";
        public const string kstrMethodName_GetUserClassifyMeetingUrl = "GetUserClassifyMeetingUrl";
        public const string kstrMethodParam_StrUserSipUri = "strUserSipUri";
        public const string kstrMethodParam_StrMeetingUri = "strMeetingUri";
        #endregion

        #region Public functions
        static public ClassifyMeetingServiceResponseInfo GetClassifyMeetingUrlFromAssistantWebService(string strWebServiceUrl, string strUserSipUri, string strMeetingUri, string strUserAgent)
        {
            try
            {
                Dictionary<string, string> dicParams = new Dictionary<string, string>(); // using to set the parameters which the web service need.
                dicParams.Add(kstrMethodParam_StrUserSipUri, strUserSipUri);
                dicParams.Add(kstrMethodParam_StrMeetingUri, strMeetingUri);
                XmlDocument obXmlResponse = WebServiceHelper.QueryPostWebService(strWebServiceUrl, kstrMethodName_GetUserClassifyMeetingUrl, dicParams, strUserAgent);
                if (null != obXmlResponse)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Response Post:[{0}]\n", obXmlResponse.OuterXml);
                }
                return GetClassifyMeetingUrlFromResponse(obXmlResponse);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in GetClassifyMeetingUrlFromResponse, [{0}]\n", ex.Message);
            }
            return null;
        }
        static public ClassifyMeetingServiceResponseInfo GetClassifyMeetingUrlFromResponse(XmlDocument obXmlResponse)
        {
            try
            {
                if (null != obXmlResponse)
                {
                    //  obXmlResponse.InnerText: {strClassifyMeetingUrl:""; strErrorMessage:""}
                    return NLJsonSerializerHelper.LoadFromJson<ClassifyMeetingServiceResponseInfo>(obXmlResponse.InnerText);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in GetClassifyMeetingUrlFromResponse, [{0}]\n", ex.Message);
            }
            return null;
        }
        static public string EstablishClassifyMeetingResponseInfo(ClassifyMeetingServiceResponseInfo obClassifyMeetingServiceResponseInfo)
        {
            try
            {
                return NLJsonSerializerHelper.GetJsonStrngFromObj(obClassifyMeetingServiceResponseInfo);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in GetClassifyMeetingUrlFromResponse, [{0}]\n", ex.Message);
            }
            return "";
        }
        #endregion
    }
}
