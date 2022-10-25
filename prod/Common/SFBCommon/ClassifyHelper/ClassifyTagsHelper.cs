using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SFBCommon.Common;
using SFBCommon.NLLog;

namespace SFBCommon.ClassifyHelper
{
    public class ClassifyTagsHelper : ClassifyHelper
    {
        #region Fields
        public Dictionary<string, string> ClassifyTags { get { return m_dicClassifyTags; } }
        #endregion

        #region Members
        private Dictionary<string,string> m_dicClassifyTags = new Dictionary<string,string>();
        #endregion

        #region Constructors
        public ClassifyTagsHelper(string strXmlClassifyTags) : base()
        {
            try
            {
                if (!string.IsNullOrEmpty(strXmlClassifyTags))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(strXmlClassifyTags);
                    XmlNode obXmlSFBClassification = xmlDoc.SelectSingleNode(kstrXMLSFBClassificationFlag);
                    if (null != obXmlSFBClassification)
                    {
                        EMSFB_CLASSIFYTYPE emClassifyType = GetClassifyTypeByRootNode(obXmlSFBClassification);
                        if ((EMSFB_CLASSIFYTYPE.emClassifyTags == emClassifyType))
                        {
                            XmlNodeList obXmlLayers = obXmlSFBClassification.SelectNodes(kstrXMLLayerFlag);
                            if ((null != obXmlLayers) && (0 < obXmlLayers.Count))
                            {
                                Dictionary<string, string> dicClassifyTags = new Dictionary<string, string>();
                                foreach (XmlNode obXmlLayer in obXmlLayers)
                                {
                                    string strTagName = XMLTools.GetAttributeValue(obXmlLayer.Attributes, kstrXMLNameAttr);
                                    string strTagValues = XMLTools.GetAttributeValue(obXmlLayer.Attributes, kstrXMLValuesAttr);
                                    CommonHelper.AddKeyValuesToDir(dicClassifyTags, strTagName.ToLower(), strTagValues);
                                }
                                SetClassifyTagsInfo(strXmlClassifyTags, EMSFB_CLASSIFYTYPE.emClassifyTags, dicClassifyTags);
                            }
                            else
                            {
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "no layers in classify tags xml {0}\n", strXmlClassifyTags);
                            }
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "wrong type {0} in the classify tags xml {1}\n", emClassifyType, strXmlClassifyTags);
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "no flag {0} in the classify tags xml {1}\n", kstrXMLSFBClassificationFlag, strXmlClassifyTags);
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "the classify tags is null or empty, {0}\n", strXmlClassifyTags);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in ClassifyTagsHelper constructor(1), [{0}]\n", ex.Message);
            }
        }
        public ClassifyTagsHelper(params string[] szStrClassifyTags): base()
        {
            try
            {
                if ((null != szStrClassifyTags) && (0 == (szStrClassifyTags.Length % 2)))
                {
                    Dictionary<string, string> dicClassifyTags = new Dictionary<string, string>();
                    for (int i=1; i<szStrClassifyTags.Length; i += 2)
                    {
                        CommonHelper.AddKeyValuesToDir(dicClassifyTags, szStrClassifyTags[i-1].ToLower(), szStrClassifyTags[i]);
                    }
                    string strXmlClassifyTags = EstablishClassifyXmlWithClassifyTags(dicClassifyTags);
                    if (!string.IsNullOrEmpty(strXmlClassifyTags))
                    {
                        SetClassifyTagsInfo(strXmlClassifyTags, EMSFB_CLASSIFYTYPE.emClassifyTags, dicClassifyTags);
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!!Exception in ClassifyTagsHelper constructor(2), {0}\n", ex.Message);
            }
        }
        public ClassifyTagsHelper(Dictionary<string,string> dicClassifyTags) : base()
        {
            try
            {
                if ((null != dicClassifyTags) && (0 < dicClassifyTags.Count))
                {
                    Dictionary<string,string> dicCheckedCaseClassifyTags = CommonHelper.DistinctDictionaryIgnoreKeyCase(dicClassifyTags);
                    string strXmlClassifyTags = EstablishClassifyXmlWithClassifyTags(dicCheckedCaseClassifyTags);
                    if (!string.IsNullOrEmpty(strXmlClassifyTags))
                    {
                        SetClassifyTagsInfo(strXmlClassifyTags, EMSFB_CLASSIFYTYPE.emClassifyTags, dicCheckedCaseClassifyTags);
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!!Exception in ClassifyTagsHelper constructor(3), {0}\n", ex.Message);
            }
        }
        #endregion

        #region Override abstract/virtual functions

        #endregion

        #region Public functions
        public void OutputInfo()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "ClassifyType:[{0}], ClassifyXml:[{1}]\n", GetClassifyType(), GetClassifyXml());
            Dictionary<string,string> dicClassifyTags = ClassifyTags;
            if (null != dicClassifyTags)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Tags Start \n");
                foreach(KeyValuePair<string,string> pairTagNameAndValue in dicClassifyTags)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\tTagName:[{0}], TagValue:[{1}]\n", pairTagNameAndValue.Key, pairTagNameAndValue.Value);
                }
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Tags End \n");
            }
        }

        public string GetStringTags(string strSepTagsKeyAndValue = ClassifyTagsHelper.kstrSepTagsKeyAndValue, string strSepTags = ClassifyTagsHelper.kstrSepTags)
        {
            string strStringTags = "";
            foreach (KeyValuePair<string, string> pairTag in m_dicClassifyTags)
            {
                if ((!string.IsNullOrWhiteSpace(pairTag.Key)) && (!string.IsNullOrWhiteSpace(pairTag.Value)))
                {
                    strStringTags += pairTag.Key;
                    strStringTags += strSepTagsKeyAndValue;
                    strStringTags += pairTag.Value;
                    strStringTags += strSepTags;
                }
            }
            return strStringTags;
        }
        #endregion

        #region Private tools
        protected void SetClassifyTagsInfo(string strXmlClassify, EMSFB_CLASSIFYTYPE emClassifyType, Dictionary<string, string> dicClassifyTags)
        {
            SetClassifyInfo(strXmlClassify, emClassifyType);
            m_dicClassifyTags = dicClassifyTags;
        }
        private string EstablishClassifyXmlWithClassifyTags(Dictionary<string,string> dicClassifyTags)
        {
            if ((null != dicClassifyTags) && (0 < dicClassifyTags.Count))
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement obXmlSFBClassification = CreateSFBClassificationHeader(xmlDoc, kstrXMLClassifyTagsAttrValue);
                foreach (KeyValuePair<string,string> pairTagNameAndValue in dicClassifyTags)
                {
                    CreateClassifyTagLayer(xmlDoc, obXmlSFBClassification, pairTagNameAndValue.Key, pairTagNameAndValue.Value);
                }
                return xmlDoc.InnerXml;
            }
            return "";
        }
        #endregion
    }
}
