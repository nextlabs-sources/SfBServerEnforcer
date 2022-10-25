using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SFBCommon.NLLog;
using SFBCommon.Common;

namespace SFBCommon.ClassifyHelper
{
    public enum EMSFB_CLASSIFYTYPE
    {
        emClassifyTypeUnknown,

        emManulClassifyObligation,
        emClassifyTags
    }

    public abstract class ClassifyHelper
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(ClassifyHelper));
        #endregion

        #region Const/Read only value, Classify XML info
        public const string kstrSepValues = "|";
        public const string kstrSepTags = ";";
        public const string kstrSepTagsKeyAndValue = "=";

        public const string kstrXMLSFBClassificationFlag = "Classification";
        public const string kstrXMLLayerFlag = "Tag";
        
        public const string kstrXMLTypeAttr = "type";
        public const string kstrXMLNameAttr = "name";
        public const string kstrXMLEditableAttr = "editable";
        public const string kstrXMLDefaultAttr = "default";
        public const string kstrXMLValuesAttr = "values";
        public const string kstrXMLMandatoryAttr = "mandatory";
        public const string kstrXMLMultipSelectAttr = "multipSelect";
        public const string kstrXMLRelyOnAttr = "relyOn";

        public const string kstrXMLManulClassifyObligationAttrValue = "manual";
        public const string kstrXMLClassifyTagsAttrValue = "tags";
        #endregion

        #region Static public tools
        static public EMSFB_CLASSIFYTYPE GetClassifyTypeFromClassifyXml(string strXmlClassify)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strXmlClassify);
            XmlNode obXmlSFBClassification = xmlDoc.SelectSingleNode(kstrXMLSFBClassificationFlag);
            if (null != obXmlSFBClassification)
            {
                return GetClassifyTypeByRootNode(obXmlSFBClassification);
            }
            return EMSFB_CLASSIFYTYPE.emClassifyTypeUnknown;
        }
        static protected EMSFB_CLASSIFYTYPE GetClassifyTypeByRootNode(XmlNode obXmlRootNode)
        {
            EMSFB_CLASSIFYTYPE emClassifyType = EMSFB_CLASSIFYTYPE.emClassifyTypeUnknown;
            if (null != obXmlRootNode)
            {
                string strClassifyType = XMLTools.GetAttributeValue(obXmlRootNode.Attributes, kstrXMLTypeAttr, 0);
                if (kstrXMLManulClassifyObligationAttrValue.Equals(strClassifyType, StringComparison.OrdinalIgnoreCase))
                {
                    emClassifyType = EMSFB_CLASSIFYTYPE.emManulClassifyObligation;
                }
                else if (kstrXMLClassifyTagsAttrValue.Equals(strClassifyType, StringComparison.OrdinalIgnoreCase))
                {
                    emClassifyType = EMSFB_CLASSIFYTYPE.emClassifyTags;
                }
            }
            return emClassifyType;
        }
        #endregion

        #region Members
        private EMSFB_CLASSIFYTYPE m_emClassifyType = EMSFB_CLASSIFYTYPE.emClassifyTypeUnknown;
        private string m_strXmlClassify = "";
        #endregion

        #region Constructors
        public ClassifyHelper()
        {
            SetClassifyInfo("", EMSFB_CLASSIFYTYPE.emClassifyTypeUnknown); 
        }
        #endregion

        #region abstract/virtual functions

        #endregion

        #region Public functions
        public string GetClassifyXml() { return m_strXmlClassify; }
        public EMSFB_CLASSIFYTYPE GetClassifyType() { return m_emClassifyType; }
        #endregion

        #region Protect functions
        protected XmlElement CreateSFBClassificationHeader(XmlDocument xmlDoc, string strType)
        {
            XmlNode obDecNode = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
            xmlDoc.AppendChild(obDecNode);
            return XMLTools.CreateElement(xmlDoc, xmlDoc, kstrXMLSFBClassificationFlag, "", kstrXMLTypeAttr, strType);
        }
        protected XmlElement CreateClassifyTagLayer(XmlDocument xmlDoc, XmlNode obParentNode, string strName, string strValues)
        {
            return XMLTools.CreateElement(xmlDoc, obParentNode, kstrXMLLayerFlag, "", kstrXMLNameAttr, strName, kstrXMLValuesAttr, strValues);
        }
        protected XmlElement CreateManualClassifyLayer(XmlDocument xmlDoc, XmlNode obParentNode, string strName, string strValues, string strEditable, string strDefaultValue, string strRelyOnValues, string strMandatory, string strMultiSelect)
        {
            return XMLTools.CreateElement(xmlDoc, obParentNode, kstrXMLLayerFlag, "", kstrXMLNameAttr, strName, kstrXMLValuesAttr, strValues, kstrXMLEditableAttr, strEditable, kstrXMLDefaultAttr, strDefaultValue, kstrXMLRelyOnAttr, strRelyOnValues, kstrXMLMandatoryAttr, strMandatory, kstrXMLMultipSelectAttr, strMultiSelect);
        }
        protected void SetClassifyInfo(string strXmlClassify, EMSFB_CLASSIFYTYPE emClassifyType)
        {
            m_strXmlClassify = strXmlClassify;
            m_emClassifyType = emClassifyType;
        }
        #endregion
    }
}
