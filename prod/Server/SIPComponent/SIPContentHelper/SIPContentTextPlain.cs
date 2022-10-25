using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using SFBCommon.NLLog;
using SFBCommon.Common;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.SIPContentHelper
{
    class SIPContentTextPlain : SIPContent
    {
        #region Members
        private XCCosContent m_obXCCosContent = null;
        #endregion

        #region Construnctors
        public SIPContentTextPlain(string strContentValue, string strContentInfo) : base(strContentValue, strContentInfo)
        {
            AnalysisContent();
        }
        public SIPContentTextPlain(EMSIP_CONTENT_TYPE emContentType, string strContentInfo) : base(emContentType, strContentInfo)
        {
            AnalysisContent();
        }
        #endregion

        #region Public functions
        public XCCosContent GetXCCosContent()
        {
            return m_obXCCosContent;
        }
        #endregion

        #region Private analysis functions
        private bool AnalysisContent()
        {
            try
            {
                m_obXCCosContent = null;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(m_strContentInfo);
                string strInfoType = xmlDoc.DocumentElement.Name;
                if (strInfoType.Equals(XCCosContent.kstrXMLXCCosFlag, StringComparison.OrdinalIgnoreCase))
                {
                    XmlNode firstChildNode = xmlDoc.DocumentElement.FirstChild;
                    if (null != firstChildNode)
                    {
                        if (firstChildNode.Name.Equals(XCCosContent.kstrXMLCmdFlag, StringComparison.OrdinalIgnoreCase))
                        {
                            string cmdType = firstChildNode.Attributes[XCCosContent.kstrXMLIDAttr].Value;
                            if (cmdType.Equals(XCCosContent.kstrXMLIDCmdJoinAttrValue, StringComparison.OrdinalIgnoreCase))
                            {
                                m_obXCCosContent = new XCCosContentCmdJoin(xmlDoc);
                            }
                            else if (cmdType.Equals(XCCosContent.kstrXMLIDCmdPartAttrValue, StringComparison.OrdinalIgnoreCase))
                            {
                                m_obXCCosContent = new XCCosContentCmdPart(xmlDoc);
                            }
                            else if (cmdType.Equals(XCCosContent.kstrXMLIDCmdGetFileUploadTokenAttrValue, StringComparison.OrdinalIgnoreCase))
                            {
                                m_obXCCosContent = new XCCosContentCmdGetFileUploadToken(xmlDoc);
                            }
                            else if (cmdType.Equals(XCCosContent.kstrXMLIDCmdGetFileDownloadTokenAttrValue, StringComparison.OrdinalIgnoreCase))
                            {
                                m_obXCCosContent = new XCCosContentCmdGetFileDownloadToken(xmlDoc);
                            }
                        }
                        else if (firstChildNode.Name.Equals(XCCosContent.kstrXMLReplyFlag, StringComparison.OrdinalIgnoreCase))
                        {
                            string rplType = firstChildNode.Attributes[XCCosContent.kstrXMLIDAttr].Value;
                            if (rplType.Equals(XCCosContent.kstrXMLIDReplyBatchJoinAttrValue, StringComparison.OrdinalIgnoreCase))
                            {
                                m_obXCCosContent = new XCCosContentReplyBatchJoin(xmlDoc);
                            }
                            else if (rplType.Equals(XCCosContent.kstrXMLIDReplyJoinAttrValue, StringComparison.OrdinalIgnoreCase))
                            {
                                m_obXCCosContent = new XCCosContentReplyJoin(xmlDoc);
                            }
                            else if (rplType.Equals(XCCosContent.kstrXMLIDReplyGetFileTokenAttrValue, StringComparison.OrdinalIgnoreCase))
                            {
                                m_obXCCosContent = new XCCosContentReplyGetFileToken(xmlDoc);
                            }
                        }
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current content is type is TextPlain but content is not XCCos, [{0}]\n", m_strContentInfo);
                }
            }
            catch (Exception ex)
            {
                m_obXCCosContent = null;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in content analysis:[{0}]\n", ex.Message);
            }
            return (null == m_obXCCosContent) ? false : true;
        }
        #endregion
    }


}
