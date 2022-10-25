using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Collections.Specialized;

using SFBCommon.NLLog;
using SFBCommon.Common;

using Nextlabs.SFBServerEnforcer.HTTPComponent.Common;

namespace Nextlabs.SFBServerEnforcer.HTTPComponent.RequestFilters
{
    public enum EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE
    {
        emUnknown,
        
        emRequestUniqueFilenameWithExtension,
        emGetMaxFileSizeInKB,
        emGetMaxRequestContentInKB,
        emAppendChunk,
        emGetFileSize,
        emDownloadChunk
    }

    class RequestFilterChatRoomAttachment : RequestFilter
    {
        #region Const/Read only values: HTTP Header
        private const string kstrHeaderSoapAction = "SOAPAction";

        private const string kstrSoapActionValueRequestUniqueFilenameWithExtension = "www.microsoft.com/RequestUniqueFilenameWithExtension";
        private const string kstrSoapActionValueGetMaxFileSizeInKB = "www.microsoft.com/GetMaxFileSizeInKB";
        private const string kstrSoapActionValueGetMaxRequestContentInKB = "www.microsoft.com/GetMaxRequestContentInKB";
        private const string kstrSoapActionValueAppendChunk = "www.microsoft.com/AppendChunk";
        private const string kstrSoapActionValueGetFileSize = "www.microsoft.com/GetFileSize";
        private const string kstrSoapActionValueDownloadChunk = "www.microsoft.com/DownloadChunk";
        #endregion

        #region Const/Read only values: XML
        private const string kstrNsSPrefix = "s";
        private const string kstrDefaultNsPrefix = "defaultns";

        private const string kstrXMLEnvelopeFlag = "Envelope";
        private const string kstrXMLBodyFlag = "Body";
        private const string kstrXMLAppendChunkFlag = "AppendChunk";
        private const string kstrXMLDownloadChunkFlag = "DownloadChunk";
        private const string kstrXMLTokenFlag = "token";
        private const string kstrXMLChannelIdFlag = "channelId";
        private const string kstrXMLUniqueFileNameFlag = "uniqueFilename";
        private const string kstrXMLReposFileNameFlag = "reposFileName";
        #endregion

        #region Fields
        public EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE AttachmentRequestType { get { return m_emAttachmentRequestType; } }
        public string Token { get { return m_strToken; } }
        public string ChannelId { get { return m_strChannelId; } }
        public string FileName { get { return m_strFileName; } }
        #endregion

        #region Members
        private EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE m_emAttachmentRequestType = EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE.emUnknown;
        private string m_strToken = "";
        private string m_strChannelId = "";
        private string m_strFileName = "";
        #endregion

        #region Constructor
        public RequestFilterChatRoomAttachment(HttpRequest httpRequest) : base(httpRequest)
        {
            InitAttachmentRequestInfo();
        }
        #endregion

        #region Override: RequestFilterManageHandler protected functions, do donot invoked the filter read function in attachment upload/download case
//         override protected void DoRequestFilter()
//         {
//             theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Do request filter\n");
// 
//         }
        #endregion

        #region Initialize
        private void InitAttachmentRequestInfo()
        {
            m_emAttachmentRequestType = EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE.emUnknown;
            string strCurSoapActionValue = GetSoapActionUriFromHeader(m_httpRequest.Headers, true);
            if (!string.IsNullOrEmpty(strCurSoapActionValue))
            {
                if (kstrSoapActionValueRequestUniqueFilenameWithExtension.Equals(strCurSoapActionValue, StringComparison.OrdinalIgnoreCase))
                {
                    m_emAttachmentRequestType = EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE.emRequestUniqueFilenameWithExtension;
                }
                else if (kstrSoapActionValueGetMaxFileSizeInKB.Equals(strCurSoapActionValue, StringComparison.OrdinalIgnoreCase))
                {
                    m_emAttachmentRequestType = EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE.emGetMaxFileSizeInKB;
                }
                else if (kstrSoapActionValueGetMaxRequestContentInKB.Equals(strCurSoapActionValue, StringComparison.OrdinalIgnoreCase))
                {
                    m_emAttachmentRequestType = EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE.emGetMaxRequestContentInKB;
                }
                else if (kstrSoapActionValueAppendChunk.Equals(strCurSoapActionValue, StringComparison.OrdinalIgnoreCase))
                {
                    m_emAttachmentRequestType = EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE.emAppendChunk;
                }
                else if (kstrSoapActionValueGetFileSize.Equals(strCurSoapActionValue, StringComparison.OrdinalIgnoreCase))
                {
                    m_emAttachmentRequestType = EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE.emGetFileSize;
                }
                else if (kstrSoapActionValueDownloadChunk.Equals(strCurSoapActionValue, StringComparison.OrdinalIgnoreCase))
                {
                    m_emAttachmentRequestType = EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE.emDownloadChunk;
                }
                else
                {
                    m_emAttachmentRequestType = EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE.emUnknown;
                }
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current soap action is:[{0}], with type:[{1}]\n", strCurSoapActionValue, m_emAttachmentRequestType);

            if (EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE.emAppendChunk == m_emAttachmentRequestType)
            {
                string strContent = CHttpTools.GetRequestBody(m_httpRequest);
                AnalysisAttachmentAppendChunkRequestBody(strContent);
            }
            else if (EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE.emDownloadChunk == m_emAttachmentRequestType)
            {
                string strContent = CHttpTools.GetRequestBody(m_httpRequest);
                AnalysisAttachmentDownloadChunkRequestBody(strContent);
            }
        }
        #endregion

        #region Inner tools
        private void AnalysisAttachmentDownloadChunkRequestBody(string strRequestContent)
        {
            /*
            <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                <s:Body>
                    <DownloadChunk xmlns="http://www.microsoft.com">
                        <token>3f2900af-52f7-40d6-91df-a211e72b7980</token>
                        <channelId>094f5a4a-c492-4f6b-91c2-385eba134fd3</channelId>
                        <reposFileName>6d042d7b-50d1-4117-9bf0-5a8c0b11e842.txt</reposFileName>
                        <offset>0</offset>
                        <bufferSize>43</bufferSize>
                    </DownloadChunk>
                </s:Body>
            </s:Envelope>
            */
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Request content:[{0}]\n", strRequestContent);
            if (!string.IsNullOrEmpty(strRequestContent))
            {
                try
                {
                    XmlDocument xmlDocAppendChunk = new XmlDocument();
                    xmlDocAppendChunk.LoadXml(strRequestContent);
                    XmlNamespaceManager xnsMgr = CreateXMLNameSpaceForAppendChunckRequestContent(xmlDocAppendChunk);

                    XmlNode obXmlRoot = xmlDocAppendChunk.DocumentElement;
                    XmlNode obXMLBody = XMLTools.NLSelectSingleNode(obXmlRoot, kstrXMLBodyFlag, xnsMgr, kstrNsSPrefix);
                    if (null != obXMLBody)
                    {
                        XmlNode obXMLDownloadChunk = XMLTools.NLSelectSingleNode(obXMLBody, kstrXMLDownloadChunkFlag, xnsMgr, kstrDefaultNsPrefix);
                        if (null != obXMLDownloadChunk)
                        {
                            m_strToken = XMLTools.GetXMLNodeText(XMLTools.NLSelectSingleNode(obXMLDownloadChunk, kstrXMLTokenFlag, xnsMgr, kstrDefaultNsPrefix));
                            m_strChannelId = XMLTools.GetXMLNodeText(XMLTools.NLSelectSingleNode(obXMLDownloadChunk, kstrXMLChannelIdFlag, xnsMgr, kstrDefaultNsPrefix));
                            m_strFileName = XMLTools.GetXMLNodeText(XMLTools.NLSelectSingleNode(obXMLDownloadChunk, kstrXMLReposFileNameFlag, xnsMgr, kstrDefaultNsPrefix));

                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Analysis attachment append chunk, token:[{0}], chanelId:[{1}], FileName:[{2}]\n", m_strToken, m_strChannelId, m_strFileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception on AnalysisAttachmentAppendChunkRequestBody, {0}", ex.Message);
                }
            }
        }
        private void AnalysisAttachmentAppendChunkRequestBody(string strRequestContent)
        {
            /*
            <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                <s:Body>
                    <AppendChunk xmlns="http://www.microsoft.com">
                        <token>2baa3978-7511-4050-8be7-f1670d8fc220</token>
                        <channelId>094f5a4a-c492-4f6b-91c2-385eba134fd3</channelId>
                        <uniqueFilename>a912898f-cf23-476b-a644-0c541d7cfdcf.txt</uniqueFilename>
                        <fileChunk>U2lwOktpbXRlc3QyLnlhbmdAbHluYy5uZXh0bGFicy5zb2x1dGlvbnMNCg==</fileChunk>
                        <offset>0</offset>
                        <fileLength>43</fileLength>
                    </AppendChunk>
                </s:Body>
            </s:Envelope>
            */
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Request content:[{0}]\n", strRequestContent);
            if (!string.IsNullOrEmpty(strRequestContent))
            {
                try
                {
                    XmlDocument xmlDocAppendChunk = new XmlDocument();
                    xmlDocAppendChunk.LoadXml(strRequestContent);
                    XmlNamespaceManager xnsMgr = CreateXMLNameSpaceForAppendChunckRequestContent(xmlDocAppendChunk);

                    XmlNode obXmlRoot = xmlDocAppendChunk.DocumentElement;
                    XmlNode obXMLBody = XMLTools.NLSelectSingleNode(obXmlRoot, kstrXMLBodyFlag, xnsMgr, kstrNsSPrefix);
                    if (null != obXMLBody)
                    {
                        XmlNode obXMLAppendChunk = XMLTools.NLSelectSingleNode(obXMLBody, kstrXMLAppendChunkFlag, xnsMgr, kstrDefaultNsPrefix);
                        if (null != obXMLAppendChunk)
                        {
                            m_strToken = XMLTools.GetXMLNodeText(XMLTools.NLSelectSingleNode(obXMLAppendChunk, kstrXMLTokenFlag, xnsMgr, kstrDefaultNsPrefix));
                            m_strChannelId = XMLTools.GetXMLNodeText(XMLTools.NLSelectSingleNode(obXMLAppendChunk, kstrXMLChannelIdFlag, xnsMgr, kstrDefaultNsPrefix));
                            m_strFileName = XMLTools.GetXMLNodeText(XMLTools.NLSelectSingleNode(obXMLAppendChunk, kstrXMLUniqueFileNameFlag, xnsMgr, kstrDefaultNsPrefix));

                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Analysis attachment append chunk, token:[{0}], chanelId:[{1}], uniqueFileName:[{2}]\n", m_strToken, m_strChannelId, m_strFileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception on AnalysisAttachmentAppendChunkRequestBody, {0}", ex.Message);
                }
            }
        }
        private XmlNamespaceManager CreateXMLNameSpaceForAppendChunckRequestContent(XmlDocument xmlDocAppendChunk)
        {
            XmlNamespaceManager xnsMgr = new XmlNamespaceManager(xmlDocAppendChunk.NameTable);
            xnsMgr.AddNamespace(kstrNsSPrefix, "http://schemas.xmlsoap.org/soap/envelope/");
            xnsMgr.AddNamespace(kstrDefaultNsPrefix, "http://www.microsoft.com");
            return xnsMgr;
        }
        private string GetSoapActionUriFromHeader(NameValueCollection obHeader, bool bRemovePortocol)
        {
            string strCurSoapActionValue = m_httpRequest.Headers[kstrHeaderSoapAction];
            if (!string.IsNullOrEmpty(strCurSoapActionValue))
            {
                // Trim remove empty chars and '"'
                strCurSoapActionValue = strCurSoapActionValue.Trim();
                strCurSoapActionValue = strCurSoapActionValue.TrimStart('"');
                strCurSoapActionValue = strCurSoapActionValue.TrimEnd('"');

                if (bRemovePortocol)
                {
                    const string kstrProtocolSepFlag = "://";
                    int nIndex = strCurSoapActionValue.IndexOf(kstrProtocolSepFlag);
                    if (-1 != nIndex)
                    {
                        strCurSoapActionValue = strCurSoapActionValue.Substring(nIndex+kstrProtocolSepFlag.Length);
                    }
                }
            }
            return strCurSoapActionValue;
        }
        #endregion

    }
}
