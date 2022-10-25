using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Diagnostics;
using System.Xml;

namespace Nextlabs.SFBServerEnforcer.HTTPComponent.Common
{
    class CHttpTools
    {
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
    }
}
