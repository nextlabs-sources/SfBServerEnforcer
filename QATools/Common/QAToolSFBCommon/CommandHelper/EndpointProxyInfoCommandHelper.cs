using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

// Current project
// Xml
using QAToolSFBCommon.Common;

// Debug
using QAToolSFBCommon.NLLog;

namespace QAToolSFBCommon.CommandHelper
{
    public class EndpointProxyInfoCommandHelper : CommandHelper
    {
        #region members
        private STUSFB_ENDPOINTINFO m_stuEndpointProxyInfo = new STUSFB_ENDPOINTINFO("");
        private string m_strXmlEndpointProxyInfo = "";
        #endregion

        #region Constructors
        public EndpointProxyInfoCommandHelper() // This constructor used for command: EMNLPROXY_COMMAND.emCommandGetEndpointProxyInfo
        {
        }
        public EndpointProxyInfoCommandHelper(STUSFB_ENDPOINTINFO stuEndpointInfo, string strID = null, EMSFB_STATUS emStatus = EMSFB_STATUS.emStatusOK)  // This constructor used for command:EMNLPROXY_COMMAND.emCommandEndpointProxyInfo
        {
            try
            {
                m_stuEndpointProxyInfo = stuEndpointInfo;
                if (string.IsNullOrEmpty(strID))
                {
                    strID = (Guid.NewGuid()).ToString();
                }
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement obMessageInfo = CreateCommandMessageInfoHeader(xmlDoc, kstrCommandEndpointProxyInfo, strID, emStatus.ToString());
                XMLTools.CreateElement(xmlDoc, obMessageInfo, kstrXMLUserSipUriFlag, stuEndpointInfo.m_strSipUri);
                m_strXmlEndpointProxyInfo = xmlDoc.InnerXml;

                SetCommandID(strID);
                SetAanlysisFlag(true);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception, EndpointProxyInfoCommandHelper constructor(2)\n", ex);
            }
        }
        public EndpointProxyInfoCommandHelper(string strXmlEndpointInfo)    // This constructor used for command:EMNLPROXY_COMMAND.emCommandEndpointProxyInfo
        {
            try
            {
                m_strXmlEndpointProxyInfo = strXmlEndpointInfo;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strXmlEndpointInfo);

                // Select Message Info
                XmlNode obXMLMessageInfo = xmlDoc.SelectSingleNode(kstrXMLMessageInfoFlag);
                if (null != obXMLMessageInfo)
                {
                    // Select user sip URI
                    m_stuEndpointProxyInfo.m_strSipUri = XMLTools.GetXMLNodeText(obXMLMessageInfo.SelectSingleNode(kstrXMLUserSipUriFlag));
                    
                    // Get ID
                    string strCommandId = XMLTools.GetAttributeValue(obXMLMessageInfo.Attributes, kstrXMLIdAttr, 0);

                    SetCommandID(strCommandId);
                    SetAanlysisFlag(true);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception, EndpointProxyInfoCommandHelper constructor(3), [{0}], [{1}]\n", ex.Message, strXmlEndpointInfo);
            }
        }
        #endregion

        #region Public functions
        public STUSFB_ENDPOINTINFO GetEndpointProxyInfo()
        {
            return m_stuEndpointProxyInfo;
        }
        public string GetCommandXml(EMSFB_COMMAND emProxyCommand)
        {
            switch (emProxyCommand)
            {
            case EMSFB_COMMAND.emCommandEndpointProxyInfo:
            {
                return GetAanlysisFlag() ? m_strXmlEndpointProxyInfo : "";
            }
            case EMSFB_COMMAND.emCommandGetEndpointProxyInfo:
            {
                return EstablishGetEndpointProxyCommandXml();
            }
            default:
            {
                break;
            }
            }
            return "";
        }
        #endregion

        #region Private functions
        private string EstablishGetEndpointProxyCommandXml()
        {
            string strCommandId = Guid.NewGuid().ToString();
            XmlDocument xmlDoc = new XmlDocument();
            CreateCommandMessageInfoHeader(xmlDoc, kstrCommandGetEndpointProxyInfo, strCommandId, EMSFB_STATUS.emStatusOK.ToString());
            m_strXmlEndpointProxyInfo = xmlDoc.InnerXml;
            
            SetCommandID(strCommandId);
            SetAanlysisFlag(true);
            return xmlDoc.InnerXml;
        }
        #endregion
    }
}
