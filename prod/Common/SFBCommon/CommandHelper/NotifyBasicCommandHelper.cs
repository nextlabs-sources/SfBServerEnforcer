using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using SFBCommon.Common;
using SFBCommon.NLLog;

namespace SFBCommon.CommandHelper
{
    class NotifyBasicCommandHelper : CommandHelper   // Now no used, code back
    {
        #region Notify info
        private string m_strXmlNotifyInfo = "";
        private string m_strDesSipUri = "";
        private string m_strToastMessage = "";
        private string m_strMessageHeader = "";
        private string m_strMessageBody = "";
        #endregion

        #region Constructors
        public NotifyBasicCommandHelper(string strXmlNotifyInfo)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strXmlNotifyInfo);

                // Select notify
                XmlNode obXMLMessageInfo = xmlDoc.SelectSingleNode(kstrXMLMessageInfoFlag);
                if (null != obXMLMessageInfo)
                {
                    // Get ID
                    string strCommandId = XMLTools.GetAttributeValue(obXMLMessageInfo.Attributes, kstrXMLIdAttr, 0);

                    // Select DesSipUri
                    m_strDesSipUri = XMLTools.GetXMLNodeText(obXMLMessageInfo.SelectSingleNode(kstrXMLDesSipUriFlag));

                    // Select ToastMessage, header, body
                    m_strToastMessage = XMLTools.GetXMLNodeText(obXMLMessageInfo.SelectSingleNode(kstrXMLToastMessageFlag));
                    m_strMessageHeader = XMLTools.GetXMLNodeText(obXMLMessageInfo.SelectSingleNode(kstrXMLHeaderFlag));
                    m_strMessageBody = XMLTools.GetXMLNodeText(obXMLMessageInfo.SelectSingleNode(kstrXMLBodyFlag));

                    // Save whole XML notify string
                    m_strXmlNotifyInfo = strXmlNotifyInfo;

                    SetCommandID(strCommandId);
                    SetAanlysisFlag(true);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in NotifyCommandInfoHelper constructor(1), [{0}]\n[{1}]\n", ex.Message, strXmlNotifyInfo);
            }
        }
        public NotifyBasicCommandHelper(string strDesSipUri, string strToastMessage, string strMessageHeader, string strMessageBody, string strID = null, EMSFB_STATUS emStatus = EMSFB_STATUS.emStatusOK)
        {
            try
            {
                m_strDesSipUri = strDesSipUri;
                m_strToastMessage = strToastMessage;
                m_strMessageHeader = strMessageHeader;
                m_strMessageBody = strMessageBody;

                if (string.IsNullOrEmpty(strID))
                {
                    strID = (Guid.NewGuid()).ToString();
                }
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement obMessageInfo = CreateCommandMessageInfoHeader(xmlDoc, kstrCommandNotify, strID, emStatus.ToString());
                XMLTools.CreateElement(xmlDoc, obMessageInfo, kstrXMLDesSipUriFlag, strDesSipUri);
                XMLTools.CreateElement(xmlDoc, obMessageInfo, kstrXMLToastMessageFlag, strToastMessage);
                XMLTools.CreateElement(xmlDoc, obMessageInfo, kstrXMLHeaderFlag, strMessageHeader);
                XMLTools.CreateElement(xmlDoc, obMessageInfo, kstrXMLBodyFlag, strMessageBody);

                m_strXmlNotifyInfo = xmlDoc.InnerXml;

                SetCommandID(strID);
                SetAanlysisFlag(true);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in NotifyCommandInfoHelper constructor(2), [{0}]\n[{1},{2},{3},{4}]\n", ex.Message, strDesSipUri, strToastMessage, strMessageHeader, strMessageBody);
            }
        }
        #endregion

        #region public tools
        public string GetDesSipUri() { return m_strDesSipUri; }
        public string GetToastMessage() { return m_strToastMessage; }
        public string GetMessageHeader() { return m_strMessageHeader; }
        public string GetMessageBody() { return m_strMessageBody; }
        public string GetXmlNotifyInfo() { return m_strXmlNotifyInfo; }
        #endregion

    }
}
