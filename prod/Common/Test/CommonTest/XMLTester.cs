using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using SFBCommon.Common;

namespace TestProject.CommonTest
{
    class XMLTester
    {
        #region Const/Read only values: XML
        private const string kstrNsSPrefix = "s";
        private const string kstrDefaultNsPrefix = "defaultns";

        public const string kstrXMLEnvelopeFlag = "Envelope";
        public const string kstrXMLBodyFlag = "Body";
        public const string kstrXMLAppendChunkFlag = "AppendChunk";
        public const string kstrXMLTokenFlag = "token";
        public const string kstrXMLChannelIdFlag = "channelId";
        public const string kstrXMLUniqueFileNameFlag = "uniqueFilename";
        #endregion

        static public void Test()
        {
            string strRequestContent = 
            "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                @"<s:Body>" +
                    "<AppendChunk xmlns=\"http://www.microsoft.com\">" +
                        @"<token>2baa3978-7511-4050-8be7-f1670d8fc220</token>" +
                        @"<channelId>094f5a4a-c492-4f6b-91c2-385eba134fd3</channelId>" +
                        @"<uniqueFilename>a912898f-cf23-476b-a644-0c541d7cfdcf.txt</uniqueFilename>" +
                        @"<fileChunk>U2lwOktpbXRlc3QyLnlhbmdAbHluYy5uZXh0bGFicy5zb2x1dGlvbnMNCg==</fileChunk>" +
                        @"<offset>0</offset>" +
                        @"<fileLength>43</fileLength>" +
                    @"</AppendChunk>" +
                @"</s:Body>" +
            @"</s:Envelope>";

            XmlDocument xmlDocAppendChunk = new XmlDocument();
            xmlDocAppendChunk.LoadXml(strRequestContent);
            XmlNode obXmlRoot = xmlDocAppendChunk.DocumentElement;
            XmlNamespaceManager xnsMgr = null;
            if ((null != obXmlRoot) && (!string.IsNullOrEmpty(obXmlRoot.NamespaceURI)))
            {
                xnsMgr = new XmlNamespaceManager(xmlDocAppendChunk.NameTable);
                xnsMgr.AddNamespace(kstrNsSPrefix, obXmlRoot.NamespaceURI);
                xnsMgr.AddNamespace(kstrDefaultNsPrefix, "http://www.microsoft.com");
            }

            XmlNode obXMLBody = XMLTools.NLSelectSingleNode(obXmlRoot, kstrXMLBodyFlag, xnsMgr, kstrNsSPrefix);
            if (null != obXMLBody)
            {
                XmlNode obXMLAppendChunk = XMLTools.NLSelectSingleNode(obXMLBody, kstrXMLAppendChunkFlag, xnsMgr, kstrDefaultNsPrefix);
                if (null != obXMLAppendChunk)
                {
                    XmlNode obXmlToken = XMLTools.NLSelectSingleNode(obXMLAppendChunk, kstrXMLTokenFlag, xnsMgr, kstrDefaultNsPrefix);
                    XmlNode obXmlChanelId = XMLTools.NLSelectSingleNode(obXMLAppendChunk, kstrXMLChannelIdFlag, xnsMgr, kstrDefaultNsPrefix);
                    XmlNode obXmlUniqueFileName = XMLTools.NLSelectSingleNode(obXMLAppendChunk, kstrXMLUniqueFileNameFlag, xnsMgr, kstrDefaultNsPrefix);
                }
            }
        }
    }
}