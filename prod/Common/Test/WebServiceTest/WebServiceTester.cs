using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using SFBCommon.NLLog;
using SFBCommon.Common;
using SFBCommon.WebServiceHelper;


namespace TestProject.WebServiceTest
{
    class WebServiceTester
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(WebServiceTester));
        #endregion

        #region Const/Read only values
        private const string kstrWebServiceUrl = "http://localhost/NLAssistant/Services/ClassifyAssitant.asmx";
        private const string kstrMethod = "GetUserClassifyMeetingUrl";
        #endregion

        static public void Test()
        {
            Dictionary<string, string> dicParams = new Dictionary<string, string>(); // using to set the parameters which the web service need.
            dicParams.Add(ClassifyMeetingWebServiceHelper.kstrMethodParam_StrUserSipUri, "UserSipUri");
            dicParams.Add(ClassifyMeetingWebServiceHelper.kstrMethodParam_StrMeetingUri, "MeetingUri");
            {
                XmlDocument obResponse = WebServiceHelper.QuerySoapWebService(kstrWebServiceUrl, kstrMethod, dicParams, ClassifyMeetingWebServiceHelper.kstrUserAgent_Assistant);
                if (null != obResponse)
                {
                    CommonTools.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Response Soap:[{0}]\n", obResponse.OuterXml);
                }
            }
            {
                XmlDocument obResponse = WebServiceHelper.QueryGetWebService(kstrWebServiceUrl, kstrMethod, dicParams, ClassifyMeetingWebServiceHelper.kstrUserAgent_Assistant);
                if (null != obResponse)
                {
                    CommonTools.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Response Get:[{0}]\n", obResponse.OuterXml);
                }
            }
            {
                XmlDocument obResponse = WebServiceHelper.QueryPostWebService(kstrWebServiceUrl, kstrMethod, dicParams, ClassifyMeetingWebServiceHelper.kstrUserAgent_Assistant);
                if (null != obResponse)
                {
                    CommonTools.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Response Post:[{0}]\n", obResponse.OuterXml);
                }
            }
        }

        static public void TestEx()
        {
            {
                ClassifyMeetingServiceResponseInfo obClassifyMeetingWebServiceResponseInfo = ClassifyMeetingWebServiceHelper.GetClassifyMeetingUrlFromAssistantWebService(kstrWebServiceUrl, "UserSipUri", "", ClassifyMeetingWebServiceHelper.kstrUserAgent_Assistant);
                if (null != obClassifyMeetingWebServiceResponseInfo)
                {
                    CommonTools.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Uri:[{0}], Status:[{1}]\n", obClassifyMeetingWebServiceResponseInfo.strClassifyMeetingUrl, obClassifyMeetingWebServiceResponseInfo.strStatus);
                }
            }
            {
                string strResponse = ClassifyMeetingWebServiceHelper.EstablishClassifyMeetingResponseInfo(new ClassifyMeetingServiceResponseInfo("Test", ""));
                ClassifyMeetingServiceResponseInfo obClassifyMeetingWebServiceResponseInfo = NLJsonSerializerHelper.LoadFromJson<ClassifyMeetingServiceResponseInfo>(strResponse);
                if (null != obClassifyMeetingWebServiceResponseInfo)
                {
                    CommonTools.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Uri:[{0}], Status:[{1}]\n", obClassifyMeetingWebServiceResponseInfo.strClassifyMeetingUrl, obClassifyMeetingWebServiceResponseInfo.strStatus);
                }
            }
        }
    }
}
