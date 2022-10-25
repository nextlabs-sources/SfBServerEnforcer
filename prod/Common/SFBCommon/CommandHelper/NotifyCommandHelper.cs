using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Web;

using SFBCommon.Common;
using SFBCommon.NLLog;

namespace SFBCommon.CommandHelper
{
    public enum EMSFB_NOTIFYINFOTYPE
    {
        emNotifyInfoTypeUnknown,

        emNotifyInfoTypeNormal,
        emNotifyInfoTypeNatural,
        emNotifyInfoTypeXML
    }

    public class BasicNotificationInfo
    {
        #region Fields
        public string[] SzStrDesSipUris { get { return m_szStrDesSipUris;} }
        public string StrToastMessage { get { return m_strToastMessage;} }
        public string StrMessageHeader { get { return m_strMessageHeader;} }
        public string StrMessageBody { get { return m_strMessageBody;} }
        #endregion

        #region Members
        private string[] m_szStrDesSipUris = null;
        private string m_strToastMessage = "";
        private string m_strMessageHeader = "";
        private string m_strMessageBody = "";
        #endregion

        #region Constructors
        public BasicNotificationInfo(XmlNode obXMLNotification)
        {
            // Select DesSipUri
            string strWholeDesSipUris = XMLTools.GetXMLNodeText(obXMLNotification.SelectSingleNode(CommandHelper.kstrXMLDesSipUriFlag));
            SetDesSipUris(strWholeDesSipUris);

            // Select ToastMessage, header, body
            string strToastMessage = XMLTools.GetXMLNodeText(obXMLNotification.SelectSingleNode(CommandHelper.kstrXMLToastMessageFlag));
            string strMessageHeader = XMLTools.GetXMLNodeText(obXMLNotification.SelectSingleNode(CommandHelper.kstrXMLHeaderFlag));
            string strMessageBody = XMLTools.GetXMLNodeText(obXMLNotification.SelectSingleNode(CommandHelper.kstrXMLBodyFlag));
            
            SetToastMessage(strToastMessage);
            SetMessageHeader(strMessageHeader);
            SetMessageBody(strMessageBody);
        }
        #endregion

        #region Private tools
        private void SetDesSipUris(string strWholeDesSipUris)
        {
            string[] szStrDesSipUris = strWholeDesSipUris.Split(new char[]{CommandHelper.kchSepDesSipUri}, StringSplitOptions.RemoveEmptyEntries);
            List<string> lsStrDesSipUris = szStrDesSipUris.ToList();
            m_szStrDesSipUris = lsStrDesSipUris.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();   // Remove repetition items
        }
        private void SetToastMessage(string strToastMessage)
        {
            m_strToastMessage = strToastMessage;
        }
        private void SetMessageHeader(string strMessageHeader)
        {
            m_strMessageHeader = strMessageHeader;
        }
        private void SetMessageBody(string strMessageBody)
        {
            m_strMessageBody = strMessageBody;
        }
        #endregion
    }

    public class NotifyCommandHelper : CommandHelper
    {
        #region Const/Read only string
        public const string kstrWildcardStartFlag = "\\";
        public const string kstrWildcardEndFlag = ";";

        public const string kstrNaturalNLRecipientsFlag = "NLRECIPIENTS:";
        public const string kstrNaturalNLMessageFlag = "NLMESSAGE:";

        private const string kstrNotifyInfoXMLHeader = "<?xml ";
        private const string kstrNotifyInfoNaturalHeader = kstrNaturalNLRecipientsFlag;
        #endregion

        #region Static public tools
        // Note, the return value only contains the anchor wildcards values.
        // for example, the anchor wildcards is "USERPLACEHOLDER=KIMTESTA" and "USERPLACEHOLDER=KIMTESTB", the return value will be list<string> as {"KIMTESTA", "KIMTESTB"}.
        static public List<string> GetWildcardAnchorInfo(string strXmlNotifyInfo)
        {
            List<string> lsWildcardAnchorValues = new List<string>();
            try
            {
                if (null != strXmlNotifyInfo)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Org notification XML:\n[{0}]\n", strXmlNotifyInfo);

                    string strRegPattern = CommonHelper.MakeAsStandardRegularPattern(kstrWildcardStartFlag + kstrWildcardAnchorKey + kchSepWildcardAnchorKeyAndValue + "([A-Z]+)" + kstrWildcardEndFlag); // ==> "\kstrWildcardAnchorKey=xxx;"
                    Regex regex = new Regex(strRegPattern);
                    MatchCollection obRegMatchs = regex.Matches(strXmlNotifyInfo);
                    foreach (Match obRegMatch in obRegMatchs)
                    {
                        GroupCollection obRegGroups = obRegMatch.Groups;
                        if ((null != obRegGroups) && (2 == obRegGroups.Count))
                        {
                            string strWildcardAnchorValue = obRegGroups[1].Value;
                            if (!string.IsNullOrEmpty(strWildcardAnchorValue))
                            {
                                lsWildcardAnchorValues.Add(strWildcardAnchorValue);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in GetAnchorWildcardInfo, [{0}]\n[{1}]\n", ex.Message, strXmlNotifyInfo);
            }
            return lsWildcardAnchorValues.Distinct().ToList();
        }
        static public EMSFB_NOTIFYINFOTYPE GetNotifyInfoType(string strOrgNotifyInfo)
        {
            EMSFB_NOTIFYINFOTYPE emNotifyInfoType = EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeUnknown;
            if (!string.IsNullOrWhiteSpace(strOrgNotifyInfo))
            {
                if (strOrgNotifyInfo.StartsWith(kstrNotifyInfoXMLHeader, StringComparison.OrdinalIgnoreCase))
                {
                    emNotifyInfoType = EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeXML;
                }
                else if (strOrgNotifyInfo.StartsWith(kstrNotifyInfoNaturalHeader, StringComparison.OrdinalIgnoreCase))
                {
                    emNotifyInfoType = EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNatural;
                }
                else
                {
                    emNotifyInfoType = EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNormal;
                }
            }
            return emNotifyInfoType;
        }
        #endregion

        #region Static private tools
        static private string AnalysisSFBNotificationXML(string strSFBNotificationXML, Dictionary<string, string> dicWildcards)
        {
            return CommonHelper.ReplaceWildcards(strSFBNotificationXML, dicWildcards, kstrWildcardStartFlag, kstrWildcardEndFlag, true);
        }
        #endregion

        #region Fields
        public List<BasicNotificationInfo> LsBasicNotificationInfo 
        { 
            get
            {
                if (null == m_lsBasicNotificationInfo)
                {
                    m_lsBasicNotificationInfo = GetBasicNotificationInfo(m_strXmlNotifyInfo);
                    if (null == m_lsBasicNotificationInfo)
                    {
                        m_lsBasicNotificationInfo = new List<BasicNotificationInfo>();
                    }
                }
                return m_lsBasicNotificationInfo;
            }
        }
        public string StrXmlNotifyInfo { get { return m_strXmlNotifyInfo; } }
        #endregion

        #region Members
        private string m_strXmlNotifyInfo = "";
        private List<BasicNotificationInfo> m_lsBasicNotificationInfo = null;
        #endregion

        #region Constructors
        public NotifyCommandHelper(string strXmlNotifyInfo)
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

                    // Get basic notification infos
                    m_lsBasicNotificationInfo = GetBasicNotificationInfo(obXMLMessageInfo);

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
        public NotifyCommandHelper(string strSFBNotifyInfo, Dictionary<string, string> dicWildcards, string strNotifyID = null, EMSFB_STATUS emStatus = EMSFB_STATUS.emStatusOK)
        {
            try
            {
                string strSFBNotificationXML = strSFBNotifyInfo;
                EMSFB_NOTIFYINFOTYPE emNotifyInfo = GetNotifyInfoType(strSFBNotifyInfo);
                if (EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeXML != emNotifyInfo)
                {
                    if (EMSFB_NOTIFYINFOTYPE.emNotifyInfoTypeNatural == emNotifyInfo)
                    {
                        strSFBNotificationXML = ConvertNotifyInfoFromNaturalToXML(strSFBNotificationXML);
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "The notify info:[{0}] with type is:[{1}] and invoker invoke an wrong constructor\n", strSFBNotifyInfo, emNotifyInfo);
                        return;
                    }
                }
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "The notify org info:[{0}] with type is:[{1}], XML notify info:[{2}]\n", strSFBNotifyInfo, emNotifyInfo, strSFBNotificationXML);

                m_strXmlNotifyInfo = AnalysisSFBNotificationXML(strSFBNotificationXML, dicWildcards);

                if (string.IsNullOrEmpty(strNotifyID))
                {
                    strNotifyID = (Guid.NewGuid()).ToString();
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(m_strXmlNotifyInfo);

                // Select notify and add "id" and "status" attributes
                XmlNode obXMLMessageInfo = xmlDoc.SelectSingleNode(kstrXMLMessageInfoFlag);
                if (null != obXMLMessageInfo)
                {
                    XmlElement obXMLElementMessageInfo = obXMLMessageInfo as XmlElement;
                    if (null != obXMLElementMessageInfo)
                    {
                        obXMLElementMessageInfo.SetAttribute(kstrXMLIdAttr, strNotifyID);
                        obXMLElementMessageInfo.SetAttribute(kstrXMLStatusAttr, emStatus.ToString());
                    }
                }

                m_strXmlNotifyInfo = xmlDoc.InnerXml;

                SetCommandID(strNotifyID);
                SetAanlysisFlag(true);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in NotifyCommandInfoHelper constructor(2), [{0}]\n[{1}]\n", ex.Message, strSFBNotifyInfo);
            }
        }
        public NotifyCommandHelper(string strDesSipUri, string strToastMessage, string strMessageHeader, string strMessageBody, string strID = null, EMSFB_STATUS emStatus = EMSFB_STATUS.emStatusOK)
        {
            try
            {
                if (string.IsNullOrEmpty(strID))
                {
                    strID = (Guid.NewGuid()).ToString();
                }
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement obXMLMessageInfo = CreateCommandMessageInfoHeader(xmlDoc, kstrCommandNotify, strID, emStatus.ToString());
                XmlElement obXMLNotification = XMLTools.CreateElement(xmlDoc, obXMLMessageInfo, kstrXMLNotificationFlag, "");

                XMLTools.CreateElement(xmlDoc, obXMLNotification, kstrXMLDesSipUriFlag, strDesSipUri);
                XMLTools.CreateElement(xmlDoc, obXMLNotification, kstrXMLToastMessageFlag, strToastMessage);
                XMLTools.CreateElement(xmlDoc, obXMLNotification, kstrXMLHeaderFlag, strMessageHeader);
                XMLTools.CreateElement(xmlDoc, obXMLNotification, kstrXMLBodyFlag, strMessageBody);

                m_strXmlNotifyInfo = xmlDoc.InnerXml;

                SetCommandID(strID);
                SetAanlysisFlag(true);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in NotifyCommandInfoHelper constructor(3), [{0}]\n[{1},{2},{3},{4}]\n", ex.Message, strDesSipUri, strToastMessage, strMessageHeader, strMessageBody);
            }
        }
        #endregion

        #region Public tools
        
        #endregion

        #region Private tools
        private string ConvertNotifyInfoFromNaturalToXML(string strNotifyInfoNatural)
        {
            /*
             * NLRECIPIENTS:\CREATOR;
             * NLMESSAGE:this is a notify message which will be send to \CREATOR;.
             * NLRECIPIENTS:\INVITEE;
             * NLMESSAGE:this is a notify message which will be send to \INVITEE;.
            */
            string strNotifyInfoXML = "";
            if (!string.IsNullOrWhiteSpace(strNotifyInfoNatural))
            {
                strNotifyInfoNatural = strNotifyInfoNatural.Trim();
                string[] szStrNotifyInfoNatural = strNotifyInfoNatural.Split(new string[]{ kstrNaturalNLRecipientsFlag }, StringSplitOptions.RemoveEmptyEntries);
                if ((null != szStrNotifyInfoNatural) && (0 < szStrNotifyInfoNatural.Length))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    XmlElement obXMLMessageInfo = CreateCommandMessageInfoHeader(xmlDoc, kstrCommandNotify, (Guid.NewGuid()).ToString(), EMSFB_STATUS.emStatusOK.ToString());
                    for (int i=0; i<szStrNotifyInfoNatural.Length; ++i)
                    {
                        string[] szNotifyInfoItem = szStrNotifyInfoNatural[i].Split(new string[]{ kstrNaturalNLMessageFlag }, StringSplitOptions.RemoveEmptyEntries);
                        if ((null != szNotifyInfoItem) && (2 == szNotifyInfoItem.Length))
                        {
                            XmlElement obXMLNotification = XMLTools.CreateElement(xmlDoc, obXMLMessageInfo, kstrXMLNotificationFlag, "");
                            XMLTools.CreateElement(xmlDoc, obXMLNotification, kstrXMLDesSipUriFlag, szNotifyInfoItem[0].Trim());
                            XMLTools.CreateElement(xmlDoc, obXMLNotification, kstrXMLToastMessageFlag, "");
                            XMLTools.CreateElement(xmlDoc, obXMLNotification, kstrXMLHeaderFlag, "");
                            XMLTools.CreateElement(xmlDoc, obXMLNotification, kstrXMLBodyFlag, szNotifyInfoItem[1].Trim());
                        }
                    }
                    strNotifyInfoXML = xmlDoc.InnerXml;
                }
            }
            return strNotifyInfoXML;
        }
        private List<BasicNotificationInfo> GetBasicNotificationInfo(string strXmlNotifyInfo)
        {
            List<BasicNotificationInfo> lsBasicNotificationInfo = new List<BasicNotificationInfo>();
            try
            {
                if (!string.IsNullOrEmpty(strXmlNotifyInfo))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(strXmlNotifyInfo);

                    XmlNode obXMLMessageInfo = xmlDoc.SelectSingleNode(kstrXMLMessageInfoFlag);
                    lsBasicNotificationInfo = GetBasicNotificationInfo(obXMLMessageInfo);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in GetBasicNotificationInfo(1), [{0}]\n[{1}]\n", ex.Message, strXmlNotifyInfo);
            }
            return lsBasicNotificationInfo;
        }
        private List<BasicNotificationInfo> GetBasicNotificationInfo(XmlNode obXMLMessageInfo)
        {
            List<BasicNotificationInfo> lsBasicNotificationInfo = new List<BasicNotificationInfo>();
            try
            {
                if (null != obXMLMessageInfo)
                {
                    // Select notification nodes
                    XmlNodeList obXMLNotifications = obXMLMessageInfo.SelectNodes(kstrXMLNotificationFlag);
                    if (null != obXMLNotifications)
                    {
                        foreach (XmlNode obXMLNotification in obXMLNotifications)
                        {
                            lsBasicNotificationInfo.Add(new BasicNotificationInfo(obXMLNotification));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in GetBasicNotificationInfo(2), [{0}]\n", ex.Message);
            }
            return lsBasicNotificationInfo;
        }
        #endregion
    }
}
