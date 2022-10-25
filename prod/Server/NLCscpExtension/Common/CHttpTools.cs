using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Diagnostics;
using System.Xml;
using SFBCommon.NLLog;

namespace NLCscpExtension.Common
{
    class CHttpTools
    {
        #region logger
        private static CLog theLog = CLog.GetLogger(typeof(CHttpTools));
        #endregion

        #region Const/read only values
        public const string kstrDefaultEnforceForChatRoom = "false"; //default didn't enforce
        public const string kstrNLNeedEnforceNodeName = "NLNeedEnforce";
        public const string SIP_URI_PREFIX = "sip:";
        #endregion

        static public string GetRequestBody(HttpRequest request)
        {
            string strContent = "";
            if (request.HttpMethod.Equals("post", StringComparison.OrdinalIgnoreCase))
            {
                if (request.InputStream != null)
                {
                    long lOldPosition = request.InputStream.Position; //we must save the old position and reset it to this old value after we finished our process
                    try
                    {
                        StreamReader reader = new StreamReader(request.InputStream);
                        strContent = reader.ReadToEnd();
                    }
                    finally
                    {
                        request.InputStream.Position = lOldPosition;  //set old position
                    }
                }
            }
            return strContent;
        }
        static public string GetXmlAttributeValueByName(XmlAttributeCollection attributes, string strAttName)
        {
            foreach(XmlAttribute att in attributes)
            {
                if(att.Name.Equals(strAttName, StringComparison.OrdinalIgnoreCase))
                {
                    return att.Value;
                }
            }
            return "";
        }
        static public void OutputHttpHeaderInfo(HttpRequest obHttpRequest)
        {
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Debug info start: method:[{0}], file path:[{1}]\n", obHttpRequest.HttpMethod, obHttpRequest.FilePath);
                foreach (string strHeaderKey in obHttpRequest.Headers.AllKeys)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Header [{0}={1}]\n", strHeaderKey, obHttpRequest.Headers[strHeaderKey]);
                }
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Debug info end: content lenght:[{0}], content:[\n{1}\n]\n", obHttpRequest.ContentLength, CHttpTools.GetRequestBody(obHttpRequest));
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "End\n");
            }
        }
    }
}
