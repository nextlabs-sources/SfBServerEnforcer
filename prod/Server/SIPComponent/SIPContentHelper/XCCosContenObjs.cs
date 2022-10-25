using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using SFBCommon.Common;
using SFBCommon.NLLog;

using Nextlabs.SFBServerEnforcer.SIPComponent.Common;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.SIPContentHelper
{
    public enum EMSIP_CONTENT_XCCOS_TYPE
    {
        emUnknown,

        emXCCosCmdJoin,
        emXCCosCmdPart,
        emXCCosCmdGetFileUploadToken,
        emXCCosCmdGetFileDownloadToken,

        emXCCosReplyBatchJoin,
        emXCCosReplyJoin,
        emXCCosReplyGetFileToken,
    }

    abstract class XCCosContent
    {
        #region Logger
        protected CLog theLog = CLog.GetLogger(typeof(XCCosContent));
        #endregion

        #region Const/Read only values: XCCos XML define
        public const string kstrDefaultNsPrefix = "defaultns";
        public const string kstrXCCosXMLNs = "urn:parlano:xml:ns:xccos";
        public const string kstrNsXsdFlag = "xsd";
        public const string kstrXCCosNsXsdValue = "http://www.w3.org/2001/XMLSchema";
        public const string kstrNsXsiFlag = "xsi";
        public const string kstrXCCosNsXsiValue = "http://www.w3.org/2001/XMLSchema-instance";
        
        public const string kstrXMLXCCosFlag = "xccos";
        public const string kstrXMLCmdFlag = "cmd";
        public const string kstrXMLReplyFlag = "rpl";
        public const string kstrXmlDataFlag = "data";
        public const string kstrXmlFtdbFlag = "ftdb";
        public const string kstrXmlCommandIDFlag = "commandid";
        public const string kstrXmlResponseFlag = "resp";
        public const string kstrXmlTokenFlag = "token";

        public const string kstrXMLXmlNsAttr = "xmlns";
        public const string kstrXMLXmlNsXsdAttr = "xmlns:xsd";
        public const string kstrXMLXmlNsXsiAttr = "xmlns:xsi";

        public const string kstrXMLIDAttr = "id";
        public const string kstrXMLSeqidAttr = "seqid";
        public const string kstrXmlEnvidAttr = "envid";
        public const string kstrXmlChannelUriAttr = "channelUri";
        public const string kstrXmlFileUrlAttr = "fileUrl";
        public const string kstrXmlCodeAttr = "code";
        public const string kstrXmlTokenAttr = "token";

        public const string kstrXMLIDCmdJoinAttrValue = "cmd:join";
        public const string kstrXMLIDCmdPartAttrValue = "cmd:part";
        public const string kstrXMLIDCmdGetFileUploadTokenAttrValue = "cmd:getfutok";
        public const string kstrXMLIDCmdGetFileDownloadTokenAttrValue = "cmd:getfdtok";

        public const string kstrXMLIDReplyBatchJoinAttrValue = "rpl:bjoin";
        public const string kstrXMLIDReplyJoinAttrValue = "rpl:join";
        public const string kstrXMLIDReplyGetFileTokenAttrValue = "rpl:getftok";
        #endregion

        #region Members
        protected XmlDocument m_xmlDocXCCos = null;
        protected XmlNamespaceManager m_xmlnsMgr = null;
        #endregion

        #region Constructors
        public XCCosContent(XmlDocument xmlDocXCCos)
        {
            if (null != xmlDocXCCos)
            {
                XmlNode obXmlRoot = xmlDocXCCos.DocumentElement;
                if (null != obXmlRoot)
                {
                    string strXMLNs = XMLTools.GetAttributeValue(obXmlRoot.Attributes, kstrXMLXmlNsAttr);
                    if (string.IsNullOrEmpty(strXMLNs))
                    {
                        m_xmlnsMgr = null;
                    }
                    else
                    {
                        m_xmlnsMgr = new XmlNamespaceManager(xmlDocXCCos.NameTable);
                        m_xmlnsMgr.AddNamespace(XCCosContent.kstrDefaultNsPrefix, strXMLNs);
                        string strXMLNsXsd = XMLTools.GetAttributeValue(obXmlRoot.Attributes, kstrXMLXmlNsXsdAttr);
                        if (!string.IsNullOrEmpty(strXMLNsXsd))
                        {
                            m_xmlnsMgr.AddNamespace(XCCosContent.kstrNsXsdFlag, strXMLNsXsd);
                        }
                        string strXMLNsXsi = XMLTools.GetAttributeValue(obXmlRoot.Attributes, kstrXMLXmlNsXsiAttr);
                        if (!string.IsNullOrEmpty(strXMLNsXsi))
                        {
                            m_xmlnsMgr.AddNamespace(XCCosContent.kstrNsXsiFlag, strXMLNsXsi);
                        }
                    }
                    m_xmlDocXCCos = xmlDocXCCos;
                }
            }
        }
        #endregion

        #region Public functions
        public XmlDocument GetXCCosXmlDocument()
        {
            return m_xmlDocXCCos;
        }
        #endregion

        #region Abstract functions, must be implement by child
        public abstract EMSIP_CONTENT_XCCOS_TYPE GetContentXCCosType();
        #endregion
    }
    class XCCosContentCmdJoin : XCCosContent
    {
        #region Constructors
        public XCCosContentCmdJoin(XmlDocument xmlDocXCCos) : base(xmlDocXCCos)
        {

        }
        #endregion

        #region Implement Abstract functions: XCCosContent
        public override EMSIP_CONTENT_XCCOS_TYPE GetContentXCCosType()
        {
            return EMSIP_CONTENT_XCCOS_TYPE.emXCCosCmdJoin;
        }
        #endregion
    }
    class XCCosContentCmdPart : XCCosContent
    {
        #region Constructors
        public XCCosContentCmdPart(XmlDocument xmlDocXCCos) : base(xmlDocXCCos)
        {

        }
        #endregion

        #region Implement Abstract functions: XCCosContent
        public override EMSIP_CONTENT_XCCOS_TYPE GetContentXCCosType()
        {
            return EMSIP_CONTENT_XCCOS_TYPE.emXCCosCmdPart;
        }
        #endregion
    }
    class XCCosContentCmdGetFileUploadToken : XCCosContent
    {
        #region Fields
        public string ChatRoomID { get { return m_strChatRoomID; } }
        public string SequenceID { get { return m_strSequenceID; } }
        public string EventID { get { return m_strEventID; } }
        public string FileName { get { return m_strFileName; } }
        #endregion

        #region Members
        private readonly string m_strChatRoomID = "";
        private readonly string m_strSequenceID = "";
        private readonly string m_strEventID = "";
        private readonly string m_strFileName = "";
        #endregion

        #region Constructors
        public XCCosContentCmdGetFileUploadToken(XmlDocument xmlDocXCCos) : base(xmlDocXCCos)
        {
            /*
                <xccos xmlns="urn:parlano:xml:ns:xccos" ver="1" envid="139132350">
                    <cmd id="cmd:getfutok" seqid="1">
                        <data>
                            <ftdb channelUri="ma-chan://lync.nextlabs.solutions/094f5a4a-c492-4f6b-91c2-385eba134fd3" fileUrl="TestInfoRecord.txt"/>
                        </data>
                    </cmd>
                </xccos>
            */
            try
            {
                if (null != xmlDocXCCos)
                {
                    XmlNode obXmlRoot = xmlDocXCCos.DocumentElement;
                    if ((null != obXmlRoot) && ((kstrXMLXCCosFlag.Equals(obXmlRoot.Name, StringComparison.OrdinalIgnoreCase))))
                    {
                        m_strEventID = XMLTools.GetAttributeValue(obXmlRoot.Attributes, kstrXmlEnvidAttr);
                        XmlNode obXmlCmd = XMLTools.NLSelectSingleNode(obXmlRoot, kstrXMLCmdFlag, m_xmlnsMgr, kstrDefaultNsPrefix);
                        if (null != obXmlCmd)
                        {
                            m_strSequenceID = XMLTools.GetAttributeValue(obXmlCmd.Attributes, kstrXMLSeqidAttr);
                            XmlNode obXmlData = XMLTools.NLSelectSingleNode(obXmlCmd, kstrXmlDataFlag, m_xmlnsMgr, kstrDefaultNsPrefix);
                            if (null != obXmlData)
                            {
                                XmlNode obXmlFtdb =  XMLTools.NLSelectSingleNode(obXmlData, kstrXmlFtdbFlag, m_xmlnsMgr, kstrDefaultNsPrefix);
                                if (null != obXmlFtdb)
                                {
                                    m_strChatRoomID = CSIPTools.GetChatRoomIDFromUri(XMLTools.GetAttributeValue(obXmlFtdb.Attributes, kstrXmlChannelUriAttr));
                                    m_strFileName = XMLTools.GetAttributeValue(obXmlFtdb.Attributes, kstrXmlFileUrlAttr);
                                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "EventID:[{0}], SequenceID:[{1}],ChatRoomUri:[{2}],FileName:[{3}]\n", m_strEventID, m_strSequenceID, m_strChatRoomID, m_strFileName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in XCCosContentCmdGetFileUploadToken, [{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Implement Abstract functions: XCCosContent
        public override EMSIP_CONTENT_XCCOS_TYPE GetContentXCCosType()
        {
            return EMSIP_CONTENT_XCCOS_TYPE.emXCCosCmdGetFileUploadToken;
        }
        #endregion
    }
    class XCCosContentCmdGetFileDownloadToken : XCCosContent
    {
        #region Fields
        public string ChatRoomID { get { return m_strChatRoomID; } }
        public string SequenceID { get { return m_strSequenceID; } }
        public string EventID { get { return m_strEventID; } }
        public string FileName { get { return m_strFileName; } }
        #endregion

        #region Members
        private readonly string m_strChatRoomID = "";
        private readonly string m_strSequenceID = "";
        private readonly string m_strEventID = "";
        private readonly string m_strFileName = ""; // For download event, this is the save as target file name
        #endregion

        #region Constructors
        public XCCosContentCmdGetFileDownloadToken(XmlDocument xmlDocXCCos) : base(xmlDocXCCos)
        {
            /*
                <xccos xmlns="urn:parlano:xml:ns:xccos" ver="1" envid="139132352">
                    <cmd id="cmd:getfdtok" seqid="1">
                        <data>
                            <ftdb channelUri="ma-chan://lync.nextlabs.solutions/094f5a4a-c492-4f6b-91c2-385eba134fd3" fileUrl="TestInfoRecord-Copy.txt"/>
                        </data>
                    </cmd>
                </xccos>
             */
            try
            {
                if (null != xmlDocXCCos)
                {
                    XmlNode obXmlRoot = xmlDocXCCos.DocumentElement;
                    if ((null != obXmlRoot) && ((kstrXMLXCCosFlag.Equals(obXmlRoot.Name, StringComparison.OrdinalIgnoreCase))))
                    {
                        m_strEventID = XMLTools.GetAttributeValue(obXmlRoot.Attributes, kstrXmlEnvidAttr);
                        XmlNode obXmlCmd = XMLTools.NLSelectSingleNode(obXmlRoot, kstrXMLCmdFlag, m_xmlnsMgr, kstrDefaultNsPrefix);
                        if (null != obXmlCmd)
                        {
                            m_strSequenceID = XMLTools.GetAttributeValue(obXmlCmd.Attributes, kstrXMLSeqidAttr);
                            XmlNode obXmlData = XMLTools.NLSelectSingleNode(obXmlCmd, kstrXmlDataFlag, m_xmlnsMgr, kstrDefaultNsPrefix);
                            if (null != obXmlData)
                            {
                                XmlNode obXmlFtdb = XMLTools.NLSelectSingleNode(obXmlData, kstrXmlFtdbFlag, m_xmlnsMgr, kstrDefaultNsPrefix);
                                if (null != obXmlFtdb)
                                {
                                    m_strChatRoomID = CSIPTools.GetChatRoomIDFromUri(XMLTools.GetAttributeValue(obXmlFtdb.Attributes, kstrXmlChannelUriAttr));
                                    m_strFileName = XMLTools.GetAttributeValue(obXmlFtdb.Attributes, kstrXmlFileUrlAttr);
                                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "EventID:[{0}], SequenceID:[{1}],ChatRoomUri:[{2}],FileName:[{3}]\n", m_strEventID, m_strSequenceID, m_strChatRoomID, m_strFileName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in XCCosContentCmdGetFileDownloadToken, [{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Implement Abstract functions: XCCosContent
        public override EMSIP_CONTENT_XCCOS_TYPE GetContentXCCosType()
        {
            return EMSIP_CONTENT_XCCOS_TYPE.emXCCosCmdGetFileDownloadToken;
        }
        #endregion
    }
    class XCCosContentReplyBatchJoin : XCCosContent
    {
        #region Constructors
        public XCCosContentReplyBatchJoin(XmlDocument xmlDocXCCos) : base(xmlDocXCCos)
        {

        }
        #endregion

        #region Implement Abstract functions: XCCosContent
        public override EMSIP_CONTENT_XCCOS_TYPE GetContentXCCosType()
        {
            return EMSIP_CONTENT_XCCOS_TYPE.emXCCosReplyBatchJoin;
        }
        #endregion
    }
    class XCCosContentReplyJoin : XCCosContent
    {
        #region Constructors
        public XCCosContentReplyJoin(XmlDocument xmlDocXCCos) : base(xmlDocXCCos)
        {

        }
        #endregion

        #region Implement Abstract functions: XCCosContent
        public override EMSIP_CONTENT_XCCOS_TYPE GetContentXCCosType()
        {
            return EMSIP_CONTENT_XCCOS_TYPE.emXCCosReplyJoin;
        }
        #endregion
    }
    class XCCosContentReplyGetFileToken : XCCosContent
    {
        #region Fields
        public string TokenID { get { return m_strTokenID; } }
        public string SequenceID { get { return m_strSequenceID; } }
        public string EventID { get { return m_strEventID; } }
        #endregion

        #region Members
        private readonly string m_strTokenID = "";
        private readonly string m_strSequenceID = "";
        private readonly string m_strEventID = "";
        #endregion

        #region Constructors
        public XCCosContentReplyGetFileToken(XmlDocument xmlDocXCCos) : base(xmlDocXCCos)
        {
            /*
                <xccos xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" ver="1" envid="7962449779256945056" xmlns="urn:parlano:xml:ns:xccos">
                    <rpl id="rpl:getftok" seqid="1">
                        <commandid seqid="1" envid="139132352" />
                        <resp code="200">SUCCESS_OK</resp>
                        <data>
                            <token token="effab007-2465-45c5-b62f-04e1689cdd10" serveruri="https://lync-server.lync.nextlabs.solutions/PersistentChat/MGCWebService.asmx" />
                        </data>
                    </rpl>
                </xccos>
            */
            try
            {
                if (null != xmlDocXCCos)
                {
                    XmlNode obXmlRoot = xmlDocXCCos.DocumentElement;
                    if ((null != obXmlRoot) && ((kstrXMLXCCosFlag.Equals(obXmlRoot.Name, StringComparison.OrdinalIgnoreCase))))
                    {
                        XmlNode obXmlReply = XMLTools.NLSelectSingleNode(obXmlRoot, kstrXMLReplyFlag, m_xmlnsMgr, kstrDefaultNsPrefix);
                        if (null != obXmlReply)
                        {
                            XmlNode obXmlCommandID = XMLTools.NLSelectSingleNode(obXmlReply, kstrXmlCommandIDFlag, m_xmlnsMgr, kstrDefaultNsPrefix);
                            XmlNode obXmlResponse = XMLTools.NLSelectSingleNode(obXmlReply, kstrXmlResponseFlag, m_xmlnsMgr, kstrDefaultNsPrefix);
                            XmlNode obXmlData = XMLTools.NLSelectSingleNode(obXmlReply, kstrXmlDataFlag, m_xmlnsMgr, kstrDefaultNsPrefix);
                            if ((null != obXmlCommandID) && (null != obXmlResponse) && (null != obXmlData))
                            {
                                m_strSequenceID = XMLTools.GetAttributeValue(obXmlCommandID.Attributes, kstrXMLSeqidAttr);
                                m_strEventID = XMLTools.GetAttributeValue(obXmlCommandID.Attributes, kstrXmlEnvidAttr);
                                
                                const string kstrSuccessCode = "200";
                                string strCode = XMLTools.GetAttributeValue(obXmlResponse.Attributes, kstrXmlCodeAttr);
                                if ((null != strCode) && (strCode.Equals(kstrSuccessCode, StringComparison.OrdinalIgnoreCase)))
                                {
                                    XmlNode obXmlToken = XMLTools.NLSelectSingleNode(obXmlData, kstrXmlTokenFlag, m_xmlnsMgr, kstrDefaultNsPrefix);
                                    if (null != obXmlToken)
                                    {
                                        m_strTokenID = XMLTools.GetAttributeValue(obXmlToken.Attributes, kstrXmlTokenAttr);
                                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "EventID:[{0}], SequenceID:[{1}],TokenID:[{2}]\n", m_strEventID, m_strSequenceID, m_strTokenID);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in XCCosContentReplyGetFileToken, [{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Implement Abstract functions: XCCosContent
        public override EMSIP_CONTENT_XCCOS_TYPE GetContentXCCosType()
        {
            return EMSIP_CONTENT_XCCOS_TYPE.emXCCosReplyGetFileToken;
        }
        #endregion
    }
}
