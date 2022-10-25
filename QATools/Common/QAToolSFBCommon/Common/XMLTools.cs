using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Web;

using QAToolSFBCommon.NLLog;

namespace QAToolSFBCommon.Common
{
    static public class XMLTools
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(XMLTools));
        #endregion

        // nType: 0(InnerText), 1(InnerXml), 2(OuterXml)
        static public string GetXMLSingleNodeText(XmlNode obParentNode, string strChidNodeName, int nType = 0)
        {
            if (null != obParentNode)
            {
                return GetXMLNodeText(obParentNode.SelectSingleNode(strChidNodeName), nType);
            }
            return "";
        }
        // nType: 0(InnerText), 1(InnerXml), 2(OuterXml)
        static public string GetXMLNodeText(XmlNode obNode, int nType = 0)
        {
            if (null != obNode)
            {
                if (0 == nType)
                {
                    return HttpUtility.HtmlDecode(obNode.InnerText);
                }
                else if (1 == nType)
                {
                    return HttpUtility.HtmlDecode(obNode.InnerXml);
                }
                else if (2 == nType)
                {
                    return HttpUtility.HtmlDecode(obNode.OuterXml);
                }
            }
            return "";
        }
        static public XmlElement CreateElement(XmlDocument xmlDoc, XmlNode obParentNode, string strName, string strValue, params string[] szStrAttr)
        {
            XmlElement obElement = xmlDoc.CreateElement(strName);
            int nLength = szStrAttr.Length - (szStrAttr.Length % 2);
            for (int i = 0; i < nLength; i += 2)
            {
                obElement.SetAttribute(szStrAttr[i], HttpUtility.HtmlEncode(szStrAttr[i + 1]));
            }
            obElement.InnerText = HttpUtility.HtmlEncode(strValue);
            obParentNode.AppendChild(obElement);
            return obElement;
        }
        // nType: 0(InnerText), 1(InnerXml), 2(OuterXml)
        static public string GetAttributeValue(XmlAttributeCollection xmlAttributes, string strAttrName, int nType = 0)
        {
            string strAttrValue = "";
            if (null != xmlAttributes)
            {
                XmlAttribute xmlAttr = xmlAttributes[strAttrName];
                if (null != xmlAttr)
                {
                    if (0 == nType)
                    {
                        strAttrValue = HttpUtility.HtmlDecode(xmlAttr.InnerText);
                    }
                    else if (1 == nType)
                    {
                        strAttrValue = HttpUtility.HtmlDecode(xmlAttr.InnerXml);
                    }
                    else if (2 == nType)
                    {
                        strAttrValue = HttpUtility.HtmlDecode(xmlAttr.OuterXml);
                    }
                }
            }
            return strAttrValue;
        }
        // nType: value 0(InnerText), 1(InnerXml), 2(OuterXml)
        static public Dictionary<string, string> GetAllAttributes(XmlAttributeCollection xmlAttributes, int nType = 0)
        {
            Dictionary<string, string> dirAttribures = new Dictionary<string,string>();
            if (null != xmlAttributes)
            {
                foreach (XmlAttribute xmlAttr in xmlAttributes)
                {
                    if (0 == nType)
                    {
                        dirAttribures.Add(xmlAttr.Name, HttpUtility.HtmlDecode(xmlAttr.InnerText));
                    }
                    else if (1 == nType)
                    {
                        dirAttribures.Add(xmlAttr.Name, HttpUtility.HtmlDecode(xmlAttr.InnerXml));
                    }
                    else if (2 == nType)
                    {
                        dirAttribures.Add(xmlAttr.Name, HttpUtility.HtmlDecode(xmlAttr.OuterXml));
                    }
                }
            }
            return dirAttribures;
        }
        static public Dictionary<string, string> GetAllSubNodesInfo(XmlNode obParentNode)
        {
            Dictionary<string, string> dirSubNodesInfo = new Dictionary<string,string>();
            if (null != obParentNode)
            {
                foreach (XmlNode obSubNode in obParentNode.ChildNodes)
                {
                    dirSubNodesInfo.Add(obSubNode.Name, GetXMLNodeText(obSubNode));
                }
            }
            return dirSubNodesInfo;
        }
        static public Dictionary<string, STUSFB_ERRORMSG> GetAllErrorMsgFromSubNodes(XmlNode obParentNode)
        {
            Dictionary<string, STUSFB_ERRORMSG> dirErrorMsgInfo = new Dictionary<string, STUSFB_ERRORMSG>();
            if (null != obParentNode)
            {
                foreach (XmlNode obSubNode in obParentNode.ChildNodes)
                {
                    int nErrorCode = 0;
                    try
                    {
                        string strErrorCode = GetAttributeValue(obSubNode.Attributes, ConfigureFileManager.kstrXMLCodeAttr);
                        if (!string.IsNullOrEmpty(strErrorCode))
                        {
                            nErrorCode = Int32.Parse(strErrorCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        nErrorCode = 0;
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Maybe the error code is not number in the configure file, please check, {0}\n", ex.Message);
                    }

                    STUSFB_ERRORMSG stuErrorMsg = new STUSFB_ERRORMSG(GetXMLNodeText(obSubNode),nErrorCode);
                    dirErrorMsgInfo.Add(obSubNode.Name, stuErrorMsg);
                }
            }
            return dirErrorMsgInfo;
        }
    }
}
