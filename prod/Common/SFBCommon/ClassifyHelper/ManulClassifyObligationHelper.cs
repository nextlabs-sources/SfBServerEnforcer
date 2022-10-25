using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using SFBCommon.Common;
using SFBCommon.NLLog;
using SFBCommon.SFBObjectInfo;

namespace SFBCommon.ClassifyHelper
{
    public enum EMSFB_MANUALCLASSIFICATIONINFOTYPE
    {
        emManualClassificationInfoTypeUnknown,

        emManualClassificationInfoTypeNatural,
        emManualClassificationInfoTypeXML,
        emManualClassificationInfoTypeFile,
        emManualClassificationInfoTypeSchemaName
    }

    public class StuManulClassifyItem
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(StuManulClassifyItem));
        #endregion

        #region Fields
        public string Name { get { return m_strName; } set { m_strName = value; } }
        public bool Editable { get { return m_bEditable; } set { m_bEditable = value; } }
        public bool Mandatory { get { return m_bMandatory; } }
        public bool MultipSelect { get { return m_bMultipSelect; } }
        public string DefaultValue { get { return m_strDefaultValue; } set { m_strDefaultValue = value; } }
        public List<string> Values { get { return m_lsStrValues; }  set { m_lsStrValues=value;}}
        public string ParentName { get { return m_strParentName; } set { m_strParentName = value; } }

        public List<string> RelyOns { get { return m_lsStrRelyOns; } set { m_lsStrRelyOns = value; } }
        public int Index { get { return m_nIndex; } }
        public int ParentIndex { get { return m_nParentIndex; } }
        public int LeftBrotherIndex { get { return m_nLeftBrotherIndex; } }
        public int RightBrotherIndex { get { return m_nRightBrotherIndex; } }
        public int Level { get { return m_nLevel; } }
        #endregion

        #region Members
        private string m_strName = "";
        private bool m_bEditable = false;
        private bool m_bMandatory = true;
        private bool m_bMultipSelect = false;
        private string m_strDefaultValue = "";
        private List<string> m_lsStrValues = new List<string>();
        private List<string> m_lsStrRelyOns = new List<string>();
        private int m_nIndex = 0;                   // Current index
        private string m_strParentName = "";        // Parent name, if it is empty, it is root node
        private int m_nParentIndex = -1;            // Parent index, if ParentIndex is the same as itself, no parent
        private int m_nLeftBrotherIndex = -1;       // Left(index small) brother index, if LeftBrotherIndex is the same as itself, no left brother
        private int m_nRightBrotherIndex = -1;      // Right(index big) brother index, if RightBrotherIndex is the same as itself, no right brother
        private int m_nLevel = -1;
        #endregion

        #region Constructor
        public StuManulClassifyItem(XmlNode xmlLayer, ref int nIndex, string strParentName, int  nParentIndex, int nLeftBrotherIndex, int nRightBrotherIndex, int nLevel)
        {
            if (null != xmlLayer)
            {
                // Current layer
                m_strName = XMLTools.GetAttributeValue(xmlLayer.Attributes, ClassifyHelper.kstrXMLNameAttr, 0).ToLower().Trim(); // Ignore case
                m_bEditable = CommonHelper.ConverStringToBoolFlag(XMLTools.GetAttributeValue(xmlLayer.Attributes, ClassifyHelper.kstrXMLEditableAttr, 0), false);
                m_bMandatory = CommonHelper.ConverStringToBoolFlag(XMLTools.GetAttributeValue(xmlLayer.Attributes, ClassifyHelper.kstrXMLMandatoryAttr, 0), true);
                m_bMultipSelect = CommonHelper.ConverStringToBoolFlag(XMLTools.GetAttributeValue(xmlLayer.Attributes, ClassifyHelper.kstrXMLMultipSelectAttr, 0), false);
                m_strDefaultValue = XMLTools.GetAttributeValue(xmlLayer.Attributes, ClassifyHelper.kstrXMLDefaultAttr, 0).Trim();
                string strValues = XMLTools.GetAttributeValue(xmlLayer.Attributes, ClassifyHelper.kstrXMLValuesAttr, 0);
                List<string> lsStrOrgValues = strValues.Split(new string[] {ClassifyHelper.kstrSepValues}, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                m_lsStrValues = CommonHelper.GetStandardStringList(lsStrOrgValues, true, true, true);
                string strRelyOns = XMLTools.GetAttributeValue(xmlLayer.Attributes, ClassifyHelper.kstrXMLRelyOnAttr, 0);
                List<string> lsStrRelyOns = strRelyOns.Split(new string[] {ClassifyHelper.kstrSepValues}, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                m_lsStrRelyOns = CommonHelper.GetStandardStringList(lsStrRelyOns, true, true, true);
                m_nIndex = nIndex;
                m_strParentName = strParentName.Trim();
                m_nParentIndex = nParentIndex;
                m_nLeftBrotherIndex = nLeftBrotherIndex;
                m_nRightBrotherIndex = nRightBrotherIndex;
                m_nLevel = nLevel;
            }
        }
        public StuManulClassifyItem(string strName, string strValues, string strDefaultValue, string strParentName, string strRelyOns, bool bEditable, 
            bool bMandatory = false, bool bMultipSelect = false, int nIndex = -1, int nParentIndex = -1, int nLeftBrotherIndex = -1, int nRightBrotherIndex = -1, int nLevel = -1)
        {
            m_strName = strName.ToLower().Trim(); // Ignore case
            List<string> lsStrOrgValues = strValues.Split(new string[] { ClassifyHelper.kstrSepValues }, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            m_lsStrValues = CommonHelper.GetStandardStringList(lsStrOrgValues, true, true, true);
            m_strDefaultValue = strDefaultValue.Trim();
            
            m_strParentName = strParentName.Trim();
            List<string> lsStrRelyOns = strRelyOns.Split(new string[] { ClassifyHelper.kstrSepValues }, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            m_lsStrRelyOns = CommonHelper.GetStandardStringList(lsStrRelyOns, true, true, true);

            m_bEditable = bEditable;

            m_bMandatory = bMandatory;
            m_bMultipSelect = bMultipSelect;
            m_nIndex = nIndex;
            m_nParentIndex = nParentIndex;
            m_nLeftBrotherIndex = nLeftBrotherIndex;
            m_nRightBrotherIndex = nRightBrotherIndex;
            m_nLevel = nLevel;
        }
        #endregion

        #region Public tools
        public void OutputInfo()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Name:[{0}], Editable:[{1}], Mandatory:[{2}], MultipSelect:[{3}], DefaultValue:[{4}], Values:[{5}], RelayOn:[{6}], Index:[{7}], ParentIndex:[{8}], LeftBrotherIndex:[{9}], RightBrotherIndex:[{10}]\n", 
                m_strName, m_bEditable, m_bMandatory, m_bMultipSelect, m_strDefaultValue, string.Join(ClassifyHelper.kstrSepValues, m_lsStrValues.ToArray()), m_lsStrRelyOns.ToArray(), m_nIndex, m_nParentIndex, m_nLeftBrotherIndex, m_nRightBrotherIndex);
        }
        // bAppend, true append, false replace
        public void AddRelyOns(List<string> lsNewRelyOn, bool bAppend)
        {
            if ((null != lsNewRelyOn) && (0 < lsNewRelyOn.Count))
            {
                if (bAppend)
                {
                    m_lsStrRelyOns.AddRange(lsNewRelyOn);
                    m_lsStrRelyOns = m_lsStrRelyOns.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                }
                else
                {
                    m_lsStrRelyOns = lsNewRelyOn.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                }
            }
        }
        #endregion
    }

    public class StuManulClassifyOb
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(StuManulClassifyOb));
        #endregion

        #region Fields
        public StuManulClassifyItem ManulClassifyItem { get { return m_obManulClassifyItem; } }

        // [<parentRelonTagValue, <curTagName, curTagsDefine>>]
        public Dictionary<string, StuManulClassifyOb> ChildStuManulClassifyOb { get { return m_dicSubManulClassifyObs; } }

        public StuManulClassifyOb ParentManulClassifyOb { get { return m_obParentManulClassifyOb; } }
        #endregion

        #region Members
        StuManulClassifyItem m_obManulClassifyItem = null;

        // [<subTagName, subTagsDefine>]
        private Dictionary<string, StuManulClassifyOb> m_dicSubManulClassifyObs = new Dictionary<string, StuManulClassifyOb>();  // sub nodes

        StuManulClassifyOb m_obParentManulClassifyOb = null;
        #endregion

        #region Constructors
        public StuManulClassifyOb(XmlNode xmlParentLayer, ref int nIndex, int nParentIndex, int nLeftBrotherIndex, int nRightBrotherIndex, int nLevel, StuManulClassifyOb obParentManulClassifyOb)
        {
            if (null != xmlParentLayer)
            {
                // Current layer
                m_obParentManulClassifyOb = obParentManulClassifyOb;
                string strParentName = "";
                if ((null != obParentManulClassifyOb) && (null != obParentManulClassifyOb.ManulClassifyItem))
                {
                    strParentName = obParentManulClassifyOb.ManulClassifyItem.Name;
                }
                m_obManulClassifyItem = new StuManulClassifyItem(xmlParentLayer, ref nIndex, strParentName, nParentIndex, nLeftBrotherIndex, nRightBrotherIndex, nLevel);

                // Next layers
                XmlNodeList xmlChildLayers = xmlParentLayer.SelectNodes(ClassifyHelper.kstrXMLLayerFlag);
                if ((null != xmlChildLayers) && (0 < xmlChildLayers.Count))
                {
                    int nBrotherCount = xmlChildLayers.Count;
                    int nCurrentLeftBrotherIndex = -1;
                    int nCurrentRightBrotherIndex = -1;
                    int i = 0;
                    foreach (XmlNode obXmlChildLayer in xmlChildLayers)
                    {
                        ++nIndex;
                        if (0 == i)
                        {
                            nCurrentLeftBrotherIndex = -1;  // the first one
                        }
                        else
                        {
                            nCurrentLeftBrotherIndex = nIndex - 1;
                        }
                        if (nBrotherCount == (i+1))
                        {
                            nCurrentRightBrotherIndex = -1; // the last one
                        }
                        else
                        {
                            nCurrentRightBrotherIndex = nIndex + 1;
                        }
                        AddSubNodes(new StuManulClassifyOb(obXmlChildLayer, ref nIndex, m_obManulClassifyItem.Index, nCurrentLeftBrotherIndex, nCurrentRightBrotherIndex, nLevel+1, this));
                        ++i;
                    }
                }
            }
        }
        #endregion

        #region Public tools
        public void OutputInfo(string strParentRelayOn)
        {
            if (null != m_obParentManulClassifyOb)
            {
                int nParentIndex = m_obParentManulClassifyOb.ManulClassifyItem.Index;
                string strParentName = m_obParentManulClassifyOb.ManulClassifyItem.Name;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "CurrentPrentRelayOn:[{0}], Parent:[{1}:{2}]\n", strParentRelayOn, nParentIndex, strParentName);
            }
            if (null != m_obManulClassifyItem)
            {
                m_obManulClassifyItem.OutputInfo();
            }
            foreach (KeyValuePair<string, StuManulClassifyOb> pairSubManulClassifyOb in m_dicSubManulClassifyObs)
            {
                pairSubManulClassifyOb.Value.OutputInfo(CommonHelper.JoinList(pairSubManulClassifyOb.Value.ManulClassifyItem.RelyOns, "|"));
            }
        }
        public StuManulClassifyOb GetNodeByIndex(int nIndex)
        {
            if (null != m_obManulClassifyItem)
            {
                if (m_obManulClassifyItem.Index == nIndex)
                {
                    return this;
                }
                StuManulClassifyOb obManulClassifyOb = null;
                foreach (KeyValuePair<string, StuManulClassifyOb> pairSubManulClassifyObs in m_dicSubManulClassifyObs)
                {
                    if ((null != pairSubManulClassifyObs.Value) && (null != pairSubManulClassifyObs.Value.m_obManulClassifyItem))
                    {
                        obManulClassifyOb = pairSubManulClassifyObs.Value.GetNodeByIndex(nIndex);
                        if (null != obManulClassifyOb)
                        {
                            return obManulClassifyOb;
                        }
                    }
                }
            }
            return null;
        }
        public StuManulClassifyOb GetNextEffectiveNodeByIndex(int nIndex, Dictionary<string,string> dicExistTags)
        {
            StuManulClassifyOb obNextManulClassifyOb = null;
            while (true)
            {
                obNextManulClassifyOb = GetNodeByIndex(++nIndex);
                if (null == obNextManulClassifyOb)
                {
                    break;
                }
                else
                {
                    if (StuManulClassifyOb.IsEffectiveItem(obNextManulClassifyOb, dicExistTags))
                    {
                        break;
                    }
                    else
                    {

                    }
                }
            }
            return obNextManulClassifyOb;
        }
        #endregion

        #region Private tools
        private void AddSubNodes(StuManulClassifyOb subInNodeManulClassifyOb)
        {
            if ((null != subInNodeManulClassifyOb) && (null != subInNodeManulClassifyOb.m_obManulClassifyItem))
            {
                string strTagName = subInNodeManulClassifyOb.m_obManulClassifyItem.Name;
                StuManulClassifyOb obExistManulClassifyOb = CommonHelper.GetValueByKeyFromDir(m_dicSubManulClassifyObs, strTagName, null);
                if ((null != obExistManulClassifyOb) && (null != obExistManulClassifyOb.m_obManulClassifyItem))
                {
                    obExistManulClassifyOb.m_obManulClassifyItem.AddRelyOns(subInNodeManulClassifyOb.m_obManulClassifyItem.RelyOns, true); // Merge rely on
                    CommonHelper.AddKeyValuesToDir(m_dicSubManulClassifyObs, strTagName, obExistManulClassifyOb);
                }
                else
                {
                    CommonHelper.AddKeyValuesToDir(m_dicSubManulClassifyObs, strTagName, subInNodeManulClassifyOb);
                }
            }
        }
        #endregion

        #region Private static tools
        static private bool IsEffectiveItem(StuManulClassifyOb obItem, Dictionary<string, string> dicExistTags)
        {
            if ((null == dicExistTags) || (null == obItem))
            {
                // Parent error
                return false;
            }
            // Root node or sub node which rely on right parent tags
            StuManulClassifyOb obParentItem = obItem.m_obParentManulClassifyOb;
            if (null == obParentItem)
            {
                return true;    // Current node is root node
            }
            else
            {
                string strParentTagName = "";
                if (null != obParentItem.m_obManulClassifyItem)
                {
                    strParentTagName = obParentItem.m_obManulClassifyItem.Name;
                }
                if (!string.IsNullOrEmpty(strParentTagName))
                {
                    string strParentTagValue = CommonHelper.GetValueByKeyFromDir(dicExistTags, strParentTagName, null);
                    if (string.IsNullOrEmpty(strParentTagValue))
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Note: current item's parent item do not select\n");
                        return false;    // Current parent do not select
                    }
                    else
                    {
                        return obItem.m_obManulClassifyItem.RelyOns.Contains(strParentTagValue);
                    }
                }
            }
            return false;
        }
        #endregion
    }

    public class ManulClassifyObligationHelper : ClassifyHelper
    {
        #region Const/Read only values
        public const string kstrKeyClassificationNameHeader = "NLClassificationName";
        public const string kstrSepClassificationNameValue = "=";
        public const string kstrDefaultClassificationName = "DefaultClassification";

        private const string kstrManualClassificationInfoXMLHeader = "<?xml ";
        private const string kstrManualClassificationInfoNaturalHeader = kstrKeyClassificationNameHeader+kstrSepClassificationNameValue;
        #endregion

        #region Static public functions
        static public string GetClassificationInfoByName(string strClassificationName, bool bForceUpdateCache)
        {
            string strClassificationValue = null;
            if (!string.IsNullOrWhiteSpace(strClassificationName))
            {
                NLClassificationSchemaInfo obNLClassificationSchemaInfo = new NLClassificationSchemaInfo();
                bool bEstablished = obNLClassificationSchemaInfo.EstablishObjFormPersistentInfo(NLClassificationSchemaInfo.kstrSchemaNameFieldName, strClassificationName);
                if (bEstablished)
                {
                    strClassificationValue = obNLClassificationSchemaInfo.GetItemValue(NLClassificationSchemaInfo.kstrDataFieldName);
                }
            }
            return strClassificationValue;
        }
        #endregion

        #region Static private tools
        static private EMSFB_MANUALCLASSIFICATIONINFOTYPE GetManualClassificationInfoType(string strOrgManualClassificationInfo)
        {
            strOrgManualClassificationInfo = strOrgManualClassificationInfo.Trim();
            EMSFB_MANUALCLASSIFICATIONINFOTYPE emManualClassificationInfoType = EMSFB_MANUALCLASSIFICATIONINFOTYPE.emManualClassificationInfoTypeUnknown;
            if (!string.IsNullOrWhiteSpace(strOrgManualClassificationInfo))
            {
                if (strOrgManualClassificationInfo.StartsWith(kstrManualClassificationInfoXMLHeader, StringComparison.OrdinalIgnoreCase))
                {
                    emManualClassificationInfoType = EMSFB_MANUALCLASSIFICATIONINFOTYPE.emManualClassificationInfoTypeXML;
                }
                else if (strOrgManualClassificationInfo.StartsWith(kstrManualClassificationInfoNaturalHeader, StringComparison.OrdinalIgnoreCase))
                {
                    emManualClassificationInfoType = EMSFB_MANUALCLASSIFICATIONINFOTYPE.emManualClassificationInfoTypeNatural;
                }
                else if (strOrgManualClassificationInfo.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
                {
                    if (File.Exists(strOrgManualClassificationInfo))
                    {
                        emManualClassificationInfoType = EMSFB_MANUALCLASSIFICATIONINFOTYPE.emManualClassificationInfoTypeFile;
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Manual classification info is start with \"\\\\\" but the it is not a UNC file path or file do not exist, [{0}]\n", strOrgManualClassificationInfo);
                    }
                }
                else
                {
                    // If the manual classification info is not XML, Natural Schema or UNC path we think it is a schema name
                    emManualClassificationInfoType = EMSFB_MANUALCLASSIFICATIONINFOTYPE.emManualClassificationInfoTypeSchemaName;
                }
            }
            return emManualClassificationInfoType;
        }
        static private string GetClassificationNameFromClassificationInfo(string strClassificationInfo, EMSFB_MANUALCLASSIFICATIONINFOTYPE emManualClassificationInfoType)
        {
            string strClassificationName = "";
            strClassificationInfo = strClassificationInfo.Trim();
            if (EMSFB_MANUALCLASSIFICATIONINFOTYPE.emManualClassificationInfoTypeNatural == emManualClassificationInfoType)
            {
                string[] szStrClassificationInfoNatural = strClassificationInfo.Split(new string[] { kstrSepClassificationNameValue }, StringSplitOptions.RemoveEmptyEntries);
                if (2 == szStrClassificationInfoNatural.Length)
                {
                    strClassificationName = szStrClassificationInfoNatural[1];
                }
            }
            else if (EMSFB_MANUALCLASSIFICATIONINFOTYPE.emManualClassificationInfoTypeSchemaName == emManualClassificationInfoType)
            {
                strClassificationName = strClassificationInfo;
            }
            return strClassificationName;
        }
        #endregion

        #region Static classfiication XML trim
        static private string TrimClassificationXml(string strClassificationXml)
        {
            if (string.IsNullOrEmpty(strClassificationXml))
            {
                return strClassificationXml;
            }

            string strRetClassificationXml = strClassificationXml;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strClassificationXml);   // Load XML format string
                XmlNode obXmlNodeClassification = xmlDoc.SelectSingleNode(ClassifyHelper.kstrXMLSFBClassificationFlag);
                XmlNodeList obTopLayers = obXmlNodeClassification.SelectNodes(ClassifyHelper.kstrXMLLayerFlag);
                if (null != obTopLayers)
                {
                    XmlNode obXmlNode = null;
                    for (int i = 0; i < obTopLayers.Count; ++i)
                    {
                        obXmlNode = obTopLayers[i];
                        TrimClassificationSubNode(xmlDoc, ref obXmlNode, true, true);
                    }
                    strRetClassificationXml = xmlDoc.InnerXml;
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in TrimClassificationXml, [{0}], input info:[{1}]\n", ex.Message, strClassificationXml);
            }
            return strRetClassificationXml;
        }
        static private void TrimClassificationSubNode(XmlDocument xmlDoc, ref XmlNode obXmlNodeParent, bool bIgnoreCase, bool bNeedTrim)
        {
            XmlNode obCurMergeMainNode = null;
            XmlNode obSecondMergeNode = null;
            bool bMergeSuccess = false;
            for (int i = 0; i < obXmlNodeParent.ChildNodes.Count; ++i)
            {
                obCurMergeMainNode = obXmlNodeParent.ChildNodes[i];
                for (int j = i + 1; j < obXmlNodeParent.ChildNodes.Count; )
                {
                    obSecondMergeNode = obXmlNodeParent.ChildNodes[j];
                    bMergeSuccess = MergeNode(xmlDoc, ref obCurMergeMainNode, ref obSecondMergeNode, bIgnoreCase, bNeedTrim);
                    if (bMergeSuccess)
                    {
                        obXmlNodeParent.RemoveChild(obXmlNodeParent.ChildNodes[j]);
                    }
                    else
                    {
                        ++j;
                    }
                }
            }
            for (int i = 0; i < obXmlNodeParent.ChildNodes.Count; ++i)
            {
                obCurMergeMainNode = obXmlNodeParent.ChildNodes[i];
                TrimClassificationSubNode(xmlDoc, ref obCurMergeMainNode, bIgnoreCase, bNeedTrim);
            }
        }
        static private bool MergeNode(XmlDocument xmlDoc, ref XmlNode obXmlNodeFirst, ref XmlNode obXmlNodeSecond, bool bIgnoreCase, bool bNeedTrim)
        {
            bool bCanMerged = false;
            Dictionary<string, string> dicFirstAttr = XMLTools.GetAllAttributes(obXmlNodeFirst.Attributes);
            Dictionary<string, string> dicSecondAttr = XMLTools.GetAllAttributes(obXmlNodeSecond.Attributes);

            string strFirstTagName = CommonHelper.GetValueByKeyFromDir(dicFirstAttr, ClassifyHelper.kstrXMLNameAttr, "");
            string strSecondTagName = CommonHelper.GetValueByKeyFromDir(dicSecondAttr, ClassifyHelper.kstrXMLNameAttr, "");
            if (bNeedTrim)
            {
                strFirstTagName = strFirstTagName.Trim();
                strSecondTagName = strSecondTagName.Trim();
            }
            if (strFirstTagName.Equals(strSecondTagName, (bIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)))
            {
                // Merge tag values
                {
                    string strFirstTagValue = CommonHelper.GetValueByKeyFromDir(dicFirstAttr, ClassifyHelper.kstrXMLValuesAttr, "");
                    string strSecondTagValue = CommonHelper.GetValueByKeyFromDir(dicSecondAttr, ClassifyHelper.kstrXMLValuesAttr, "");
                    string strMergedTagValue = MergeString(strFirstTagValue, strSecondTagValue, ClassifyHelper.kstrSepValues, bIgnoreCase, bNeedTrim);
                    XMLTools.SetAttributeValue(xmlDoc, obXmlNodeFirst, ClassifyHelper.kstrXMLValuesAttr, strMergedTagValue);
                }

                // Merge editable, false > true
                {
                    bool bExceptionDefault = false;
                    string strFirstEditable = CommonHelper.GetValueByKeyFromDir(dicFirstAttr, ClassifyHelper.kstrXMLEditableAttr, bExceptionDefault.ToString());
                    string strSecondEditable = CommonHelper.GetValueByKeyFromDir(dicSecondAttr, ClassifyHelper.kstrXMLEditableAttr, bExceptionDefault.ToString());
                    bool bMergedEditable = MergeStringBoolean(strFirstEditable, strSecondEditable, bExceptionDefault, true);
                    XMLTools.SetAttributeValue(xmlDoc, obXmlNodeFirst, ClassifyHelper.kstrXMLEditableAttr, bMergedEditable.ToString());
                }

                // Merge default value, no need merge using first one
                {
                    string strMergedDefailtValue = CommonHelper.GetValueByKeyFromDir(dicFirstAttr, ClassifyHelper.kstrXMLDefaultAttr, "");
                    XMLTools.SetAttributeValue(xmlDoc, obXmlNodeFirst, ClassifyHelper.kstrXMLDefaultAttr, strMergedDefailtValue);
                }

                // Merge rely-on values
                {
                    string strFirstRelyOn = CommonHelper.GetValueByKeyFromDir(dicFirstAttr, ClassifyHelper.kstrXMLRelyOnAttr, "");
                    string strSecondRelyOn = CommonHelper.GetValueByKeyFromDir(dicSecondAttr, ClassifyHelper.kstrXMLRelyOnAttr, "");
                    string strMergedRelyOn = MergeString(strFirstRelyOn, strSecondRelyOn, ClassifyHelper.kstrSepValues, bIgnoreCase, bNeedTrim);
                    XMLTools.SetAttributeValue(xmlDoc, obXmlNodeFirst, ClassifyHelper.kstrXMLRelyOnAttr, strMergedRelyOn);
                }

                // Append sub nodes
                {
                    for (int i = obXmlNodeSecond.ChildNodes.Count - 1; i >= 0; --i)
                    {
                        // Note:
                        // Using AppendChild add sub node, obXmlNodeSecond.ChildNodes[j] will removed from obXmlNodeSecond.
                        // After append all obXmlNodeSecond child nodes to obXmlNodeFirst, the obXmlNodeSecond.ChildNodes.Count will be zero.
                        obXmlNodeFirst.AppendChild(obXmlNodeSecond.ChildNodes[i]);
                    }
                }
                bCanMerged = true;
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Merge node but the node name not same, [{0}], [{1}]\n", strFirstTagName, strSecondTagName);
            }
            return bCanMerged;
        }
        static private bool MergeStringBoolean(string strFirst, string strSecond, bool bExceptionDefault, bool bMergeLogicAdd)
        {
            bool bFirst = bExceptionDefault;
            Boolean.TryParse(strFirst, out bFirst);
            bool bSecond = bExceptionDefault;
            Boolean.TryParse(strFirst, out bSecond);
            return bMergeLogicAdd ? (bFirst && bSecond) : (bFirst || bSecond);
        }
        static private string MergeString(string strFirst, string strSecond, string strSep, bool bIgnoreCase, bool bNeedTrim)
        {
            string strRet = "";

            // Check empty or null
            if (string.IsNullOrEmpty(strFirst))
            {
                strRet = CommonHelper.GetSolidString(strSecond);
                return bNeedTrim ? strRet.Trim() : strRet;
            }
            if (string.IsNullOrEmpty(strSecond))
            {
                strRet = CommonHelper.GetSolidString(strFirst);
                return bNeedTrim ? strRet.Trim() : strRet;
            }

            if (bNeedTrim)
            {
                strFirst = strFirst.Trim();
                strSecond = strSecond.Trim();
            }

            // Merge solid string
            if (string.IsNullOrEmpty(strSep))
            {
                if (strFirst.Equals(strSecond, (bIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)))
                {
                    strRet = strFirst;
                }
                else
                {
                    strRet = strFirst + strSecond;
                }
            }
            else
            {
                // Split ==> new sz add
                HashSet<string> hashSetStrMerge = new HashSet<string>();
                TrimStringValueWithSeparator(strFirst, strSep, bIgnoreCase, bNeedTrim, ref hashSetStrMerge);
                TrimStringValueWithSeparator(strSecond, strSep, bIgnoreCase, bNeedTrim, ref hashSetStrMerge);
                strRet = string.Join(strSep, hashSetStrMerge);
            }
            return strRet;
        }
        static private void TrimStringValueWithSeparator(string strIn, string strSep, bool bIgnoreCase, bool bNeedTrim, ref HashSet<string> hashSetStrMerge)
        {
            string[] szIn = strIn.Split(new string[] { strSep }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < szIn.Length; ++i)
            {
                string strValue = bNeedTrim ? szIn[i].Trim() : szIn[i];
                hashSetStrMerge.Add(bIgnoreCase ? strValue.ToLower() : strValue);
            }
        }
        #endregion

        #region Constructors
        public ManulClassifyObligationHelper(List<StuManulClassifyItem> lsInManulClassifyItems)
        {
            SetManulClassifyObligationInfo("", EMSFB_CLASSIFYTYPE.emClassifyTypeUnknown);

            XmlDocument xmlDoc = new XmlDocument();
            XmlElement obXmlSFBClassification = CreateSFBClassificationHeader(xmlDoc, kstrXMLManulClassifyObligationAttrValue);

            List<StuManulClassifyItem> lsManulClassifyItems = new List<StuManulClassifyItem>(lsInManulClassifyItems);
            EstablishClasisfyInfo(ref lsManulClassifyItems, xmlDoc, new Dictionary<string, XmlElement>(){{"", obXmlSFBClassification}});
            Append(xmlDoc.InnerXml, false);
        }
        public ManulClassifyObligationHelper(string strManulClassifyInfo, bool bForceUpdateCache) : base()
        {
            SetManulClassifyObligationInfo("", EMSFB_CLASSIFYTYPE.emClassifyTypeUnknown);
            Append(strManulClassifyInfo, bForceUpdateCache);
        }
        #endregion

        #region Override abstract/vitual functions

        #endregion

        #region Public functions
        public List<StuManulClassifyOb> GetStuManulClassifyObs()
        {
            List<StuManulClassifyOb> lsStuManulClassify = new List<StuManulClassifyOb>();
            try
            {
                string strCurXml = GetClassifyXml();
                if (!string.IsNullOrEmpty(strCurXml))
                {
                    XmlDocument xmlCurDoc = new XmlDocument();
                    xmlCurDoc.LoadXml(strCurXml);
                    XmlNode obXmlCurSFBClassification = xmlCurDoc.SelectSingleNode(kstrXMLSFBClassificationFlag);
                    if (null != obXmlCurSFBClassification)
                    {
                        EMSFB_CLASSIFYTYPE emCurClassifyType = GetClassifyTypeByRootNode(obXmlCurSFBClassification);
                        if ((EMSFB_CLASSIFYTYPE.emManulClassifyObligation == emCurClassifyType))
                        {
                            XmlNodeList obXmlLayers = obXmlCurSFBClassification.SelectNodes(kstrXMLLayerFlag);
                            if ((null != obXmlLayers) && (0 < obXmlLayers.Count))
                            {
                                foreach (XmlNode obXmlLayer in obXmlLayers)
                                {
                                    int nIndex = 0;
                                    lsStuManulClassify.Add(new StuManulClassifyOb(obXmlLayer, ref nIndex, -1, -1, -1, 0, null));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in GetStuManulClassifyObligation, [{0}]\n", ex.Message);
            }
            return lsStuManulClassify;
        }
        public bool Append(string strManulClassifyInfo, bool bForceUpdateCache)
        {
            bool bRet = false;
            try
            {
                string strNewXml = "";

                XmlDocument xmlNewDoc = new XmlDocument();
                strManulClassifyInfo = strManulClassifyInfo.Trim();
                EMSFB_MANUALCLASSIFICATIONINFOTYPE emManualClassificationInfoType = GetManualClassificationInfoType(strManulClassifyInfo);
                switch (emManualClassificationInfoType)
                {
                case EMSFB_MANUALCLASSIFICATIONINFOTYPE.emManualClassificationInfoTypeNatural:
                case EMSFB_MANUALCLASSIFICATIONINFOTYPE.emManualClassificationInfoTypeSchemaName:
                {
                    string strClassificationName = GetClassificationNameFromClassificationInfo(strManulClassifyInfo, emManualClassificationInfoType);
                    strManulClassifyInfo = GetClassificationInfoByName(strClassificationName, bForceUpdateCache);
                    if (!string.IsNullOrWhiteSpace(strManulClassifyInfo))
                    {
                        xmlNewDoc.LoadXml(strManulClassifyInfo);   // Load XML format string
                        strNewXml = strManulClassifyInfo;
                    }
                    break;
                }
                case EMSFB_MANUALCLASSIFICATIONINFOTYPE.emManualClassificationInfoTypeXML:
                {
                    xmlNewDoc.LoadXml(strManulClassifyInfo);   // Load XML format string
                    strNewXml = strManulClassifyInfo;
                    break;
                }
                case EMSFB_MANUALCLASSIFICATIONINFOTYPE.emManualClassificationInfoTypeFile:
                {
                    xmlNewDoc.Load(strManulClassifyInfo);  // Load XML file
                    strNewXml = xmlNewDoc.InnerXml;
                    break;
                }
                default :
                {
                    return false;
                }
                }
                if (!string.IsNullOrWhiteSpace(strNewXml))
                {
                    XmlNode obXmlNewSFBClassification = xmlNewDoc.SelectSingleNode(kstrXMLSFBClassificationFlag);
                    if (null != obXmlNewSFBClassification)
                    {
                        EMSFB_CLASSIFYTYPE emClassifyType = GetClassifyTypeByRootNode(obXmlNewSFBClassification);
                        if ((EMSFB_CLASSIFYTYPE.emManulClassifyObligation == emClassifyType))
                        {
                            string strOrgExistClassifyXml = GetClassifyXml();
                            if ((!string.IsNullOrEmpty(strOrgExistClassifyXml)) && (EMSFB_CLASSIFYTYPE.emManulClassifyObligation == GetClassifyType()))
                            {
                                bRet = InnerAppendManulClassifyObligation(obXmlNewSFBClassification);
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Original XML:[{0}] is right, append new one with result:[{1}]\n", strOrgExistClassifyXml, bRet);
                            }
                            else
                            {
                                SetManulClassifyObligationInfo(strNewXml, emClassifyType);
                                bRet = true;
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Original XML:[{0}] is not right, replace\n", strOrgExistClassifyXml);
                            }
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Original XML:[{0}] is right, AfterAppend:[{1}], Status:[{2}]\n", strOrgExistClassifyXml, GetClassifyXml(), bRet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in AppendManulClassify, [{0}]\n", ex.Message);
            }
            return bRet;
        }
        public List<string> GetTagNameListFromSFBObligationXml()
        {
            List<string> tagNameList = new List<string>();
            string sfbObXmlStr = GetClassifyXml();

            if (!string.IsNullOrEmpty(sfbObXmlStr))
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(sfbObXmlStr);
                    DfsObXml2GetSFBObligationTagNames(xmlDoc.DocumentElement, tagNameList);
                }
                catch (XmlException ex)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "GetTagNameListFromSFBObligationXml() failed , error message : {0}", ex.Message);
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, " GetTagNameListFromSFBObligationXml() failed , sfbObXmlStr is null or empty ");
            }

            return tagNameList;
        }
        #endregion

        #region Private functions
        private void EstablishClasisfyInfo(ref List<StuManulClassifyItem> lsManulClassifyItems, XmlDocument xmlDoc, Dictionary<string, XmlElement> dicInParentInfo)
        {
            if ((null != lsManulClassifyItems) && (0 < lsManulClassifyItems.Count) && (null != dicInParentInfo))
            {
                Dictionary<string, XmlElement> dicCurTopLayers = new Dictionary<string, XmlElement>(StringComparer.OrdinalIgnoreCase);
                List<StuManulClassifyItem> lsCurRemoveItems = new List<StuManulClassifyItem>();
                foreach (StuManulClassifyItem obManulClassifyItem in lsManulClassifyItems)
                {
                    if (null != obManulClassifyItem)
                    {
                        string strCurItemParentName = CommonHelper.GetSolidString(obManulClassifyItem.ParentName).ToLower().Trim();
                        XmlElement obXmlCurParentLayer = CommonHelper.GetValueByKeyFromDir(dicInParentInfo, strCurItemParentName, null);
                        if (null != obXmlCurParentLayer)
                        {
                            XmlElement obXmlCurTopLayer = InnerCreateManulClassifyLayer(xmlDoc, obXmlCurParentLayer, obManulClassifyItem);
                            CommonHelper.AddKeyValuesToDir(dicCurTopLayers, obManulClassifyItem.Name.ToLower(), obXmlCurTopLayer);
                            lsCurRemoveItems.Add(obManulClassifyItem);
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current tag:[{0}] is not current top layer\n", obManulClassifyItem.Name);
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!!Error, current manual classify item is null\n");
                        lsCurRemoveItems.Add(obManulClassifyItem);
                    }
                }
                foreach (StuManulClassifyItem obManulClassifyItem in lsCurRemoveItems)
                {
                    lsManulClassifyItems.Remove(obManulClassifyItem);
                }
                if ((0 < lsManulClassifyItems.Count) && (0 < dicCurTopLayers.Count))
                {
                    EstablishClasisfyInfo(ref lsManulClassifyItems, xmlDoc, dicCurTopLayers);
                }
            }
        }
        private XmlElement InnerCreateManulClassifyLayer(XmlDocument xmlDoc, XmlNode obParentNode, StuManulClassifyItem obManulClassifyItem)
        {
            string strValues = string.Join(kstrSepValues, obManulClassifyItem.Values.Distinct());
            string strRelyOns = string.Join(kstrSepValues, obManulClassifyItem.RelyOns.Distinct());
            return CreateManualClassifyLayer(xmlDoc, obParentNode, obManulClassifyItem.Name, strValues, obManulClassifyItem.Editable.ToString(), obManulClassifyItem.DefaultValue, strRelyOns, obManulClassifyItem.Mandatory.ToString(), obManulClassifyItem.MultipSelect.ToString());
        }
        private void SetManulClassifyObligationInfo(string strXmlClassify, EMSFB_CLASSIFYTYPE emClassifyType)
        {
            strXmlClassify = ManulClassifyObligationHelper.TrimClassificationXml(strXmlClassify);
            SetClassifyInfo(strXmlClassify, emClassifyType);
        }
        private void Clean()
        {
            SetClassifyInfo("", EMSFB_CLASSIFYTYPE.emClassifyTypeUnknown);
        }
        private bool InnerAppendManulClassifyObligation(XmlNode obNewXmlSFBClassification)
        {
            bool bRet = false;
            try
            {
                // This is inner function no need check the inner data is right or not
                if (null != obNewXmlSFBClassification)
                {
                    string strCurXml = GetClassifyXml();
                    string strNewInnerXmlSFBClassification = obNewXmlSFBClassification.InnerXml;
                    if ((!string.IsNullOrEmpty(strCurXml)) && (!string.IsNullOrEmpty(strNewInnerXmlSFBClassification)))
                    {
                        bRet = InnerMergeClassification(strCurXml, obNewXmlSFBClassification);
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in InnerAppendManulClassifyObligation, [{0}]\n", ex.Message);
            }
            return bRet;
        }
        private bool InnerMergeClassification(string strCurXml, XmlNode obNewXmlSFBClassification)
        {
            bool bRet = true;
            try
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "New classify info before trim:[{0}]\n", obNewXmlSFBClassification.InnerXml);
                // Remove the same layers
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strCurXml);
                XmlNode obCurXmlSFBClassification = xmlDoc.SelectSingleNode(kstrXMLSFBClassificationFlag);
                XmlNodeList obCurXmlNodeLayers = obCurXmlSFBClassification.SelectNodes(kstrXMLLayerFlag);
                XmlNodeList obNewXmlNodeLayers = obNewXmlSFBClassification.SelectNodes(kstrXMLLayerFlag);
                foreach (XmlNode obNewNodeNew in obNewXmlNodeLayers)
                {
                    bool bFind = false;
                    foreach (XmlNode obCurNode in obCurXmlNodeLayers)
                    {
                        if (obCurNode.OuterXml.Equals(obNewNodeNew.OuterXml, StringComparison.OrdinalIgnoreCase))
                        {
                            bFind = true;
                            break;
                        }
                    }
                    if (bFind)
                    {
                        obNewXmlSFBClassification.RemoveChild(obNewNodeNew);
                    }
                }
                
                // Append
                string strNewInnerXmlSFBClassification = obNewXmlSFBClassification.InnerXml;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "New classify info after trim:[{0}]\n", strNewInnerXmlSFBClassification);
                if (!string.IsNullOrWhiteSpace(strNewInnerXmlSFBClassification))
                {
                    string strRegPattern = @"<\s*/\s*" + CommonHelper.MakeAsStandardRegularPattern(kstrXMLSFBClassificationFlag) + @"\s*>"; // ==> "</\s*SFBClassification\s*>"
                    Regex regex = new Regex(strRegPattern);
                    Match obRegMatch = regex.Match(strCurXml);
                    if (obRegMatch.Success)
                    {
                        string strWholeXml = strCurXml.Insert(obRegMatch.Index, strNewInnerXmlSFBClassification);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Match String:[{0}], Append String:[{1}], Pattern:[{2}], Index:[{3}], WholeXml:[{4}]\n", strCurXml, strNewInnerXmlSFBClassification, strRegPattern, obRegMatch.Index, strWholeXml);
                        SetManulClassifyObligationInfo(strWholeXml, EMSFB_CLASSIFYTYPE.emManulClassifyObligation); // String insert, insert the new string before the original start index
                    }
                }
            }
            catch (Exception ex)
            {
                bRet = false;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in Merge classification info, [{0}]\n", ex.Message);
            }
            return bRet;
        }
        private void DfsObXml2GetSFBObligationTagNames(XmlNode parent, List<string> tagNameList)
        {
            if (parent == null || parent.NodeType != XmlNodeType.Element)
                return;

            if (parent.Name.ToLower() == ClassifyHelper.kstrXMLSFBClassificationFlag.ToLower())
            {
                for (int i = 0; i < parent.ChildNodes.Count; i++)
                {
                    DfsObXml2GetSFBObligationTagNames(parent.ChildNodes[i], tagNameList);
                }
            }
            else if (parent.Name.ToLower() == ClassifyHelper.kstrXMLLayerFlag.ToLower())
            {
                tagNameList.Add(parent.Attributes[ClassifyHelper.kstrXMLNameAttr].Value.ToLower());
                for (int i = 0; i < parent.ChildNodes.Count; i++)
                {
                    DfsObXml2GetSFBObligationTagNames(parent.ChildNodes[i], tagNameList);
                }
            }
        }
        #endregion
    }
}
