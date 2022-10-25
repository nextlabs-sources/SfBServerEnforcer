using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using SFBCommon.Common;
using SFBCommon.NLLog;
using SFBCommon.SFBObjectInfo;
using SFBCommon.ClassifyHelper;

namespace SFBCommon.CommandHelper
{
    public class ClassifyCommandHelper : CommandHelper
    {
        #region Static functions
        static public bool IsClassifyManager(STUSFB_CLASSIFYCMDINFO stuClassifyCmdInfo)
        {
            if (null != stuClassifyCmdInfo)
            {
                if (EMSFB_COMMAND.emCommandClassifyMeeting == stuClassifyCmdInfo.m_emCommandType)
                {
                    SFBMeetingVariableInfo obSFBVariableInfo = new SFBMeetingVariableInfo();
                    bool bEstablished = obSFBVariableInfo.EstablishObjFormPersistentInfo(SFBMeetingVariableInfo.kstrUriFieldName, stuClassifyCmdInfo.m_strSFBObjUri);
                    if (bEstablished)
                    {
                        string[] szStrClassifyManagers = obSFBVariableInfo.GetClassifyManagers();
                        if (null != szStrClassifyManagers)
                        {
                            string strCurUserStandardSipUri = CommonHelper.GetStandardSipUri(stuClassifyCmdInfo.m_strUserSipUri);
                            foreach (string strClassifyManager in szStrClassifyManagers)
                            {
                                if (strCurUserStandardSipUri.Equals(CommonHelper.GetStandardSipUri(strClassifyManager), StringComparison.OrdinalIgnoreCase))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Currently do not support to classify managers in proxy module for chat room\n");
                }
            }
            return false;
        }
        static public ManulClassifyObligationHelper GetClassifyObligationInfo(STUSFB_CLASSIFYCMDINFO stuClassifyCmdInfo)
        {
            if (null != stuClassifyCmdInfo)
            {
                if (EMSFB_COMMAND.emCommandClassifyMeeting == stuClassifyCmdInfo.m_emCommandType)
                {
                    NLMeetingInfo obSFBObjectInfo = new NLMeetingInfo();
                    bool bEstablished = obSFBObjectInfo.EstablishObjFormPersistentInfo(NLMeetingInfo.kstrUriFieldName, stuClassifyCmdInfo.m_strSFBObjUri);
                    if (bEstablished)
                    {
                        return obSFBObjectInfo.GetManulClassifyObligation();
                    }
                }
                else
                {
                    throw new Exception("Currently do not support to do classify in proxy module for chat room\n");
                }
            }
            return null;
        }
        static public ClassifyTagsHelper GetClassifyTagsInfo(STUSFB_CLASSIFYCMDINFO stuClassifyCmdInfo)
        {
            if (null != stuClassifyCmdInfo)
            {
                if (EMSFB_COMMAND.emCommandClassifyMeeting == stuClassifyCmdInfo.m_emCommandType)
                {
                    SFBMeetingVariableInfo obSFBObjectInfo = new SFBMeetingVariableInfo();
                    bool bEstablished = obSFBObjectInfo.EstablishObjFormPersistentInfo(SFBMeetingVariableInfo.kstrUriFieldName, stuClassifyCmdInfo.m_strSFBObjUri);
                    if (bEstablished)
                    {
                        return obSFBObjectInfo.GetClassifyTags();
                    }
                }
                else
                {
                    throw new Exception("Currently do not support to do classify in proxy module for chat room\n");
                }
            }
            return null;
        }
        #endregion

        #region Fields
        public STUSFB_CLASSIFYCMDINFO ClassifyCmdInfo { get { return m_StuClassifyCmdInfo; } }
        #endregion

        #region Members
        private STUSFB_CLASSIFYCMDINFO m_StuClassifyCmdInfo = new STUSFB_CLASSIFYCMDINFO( EMSFB_COMMAND.emCommandUnknown, "", "");
        private string m_strXmlClassifyCommandInfo = "";
        #endregion

        #region Constructors
        // emCommandType is EMSFB_COMMAND.emCommandClassifyMeeting or EMSFB_COMMAND.emCommandClassifyChatRoom
        public ClassifyCommandHelper(EMSFB_COMMAND emCommandType, string strUserSipUri, string strSFBObjUri)
        {
            try
            {
                if ((IsSupportCommandType(emCommandType)) && (!string.IsNullOrEmpty(strUserSipUri)) && (!string.IsNullOrEmpty(strSFBObjUri)))
                {
                    EstablishClassifyCommandXml(emCommandType, strUserSipUri, strSFBObjUri);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!!Exception in ClassifyCommandHelper constructor(1) with CommandType:[{0}], UserSipUri:[{1}], SFBObjUri:[{2}], exception message:{3}\n", emCommandType, strUserSipUri, strSFBObjUri, ex.Message);
            }
        }
        public ClassifyCommandHelper(string strXmlClassifyCommandInfo)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strXmlClassifyCommandInfo);

                // Select Message Info
                XmlNode obXMLMessageInfo = xmlDoc.SelectSingleNode(kstrXMLMessageInfoFlag);
                if (null != obXMLMessageInfo)
                {
                    // Get command type, ID, user sip URI, SFB Object Uri
                    string strCommandId = XMLTools.GetAttributeValue(obXMLMessageInfo.Attributes, kstrXMLIdAttr, 0);
                    string strCommandType = XMLTools.GetAttributeValue(obXMLMessageInfo.Attributes, kstrXMLTypeAttr, 0);
                    EMSFB_COMMAND emCommandType = ConvertStringCommandType(strCommandType);
                    if (IsSupportCommandType(emCommandType))
                    {
                        string strUserSipUri = XMLTools.GetXMLNodeText(obXMLMessageInfo.SelectSingleNode(kstrXMLUserSipUriFlag));
                        string strSFBObjUri = XMLTools.GetXMLNodeText(obXMLMessageInfo.SelectSingleNode(kstrXMLSFBObjUriFlag));

                        m_StuClassifyCmdInfo= new STUSFB_CLASSIFYCMDINFO(emCommandType, strUserSipUri, strSFBObjUri);
                        m_strXmlClassifyCommandInfo = strXmlClassifyCommandInfo;

                        SetCommandID(strCommandId);
                        SetAanlysisFlag(true);
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception, EndpointProxyInfoCommandHelper constructor(3), [{0}], [{1}]\n", ex.Message, strXmlClassifyCommandInfo);
            }
        }
        #endregion

        #region Public functions
        public string GetCommandXml()
        {
            return m_strXmlClassifyCommandInfo;
        }
        #endregion

        #region Private functions
        private bool IsSupportCommandType(EMSFB_COMMAND emCommandType)
        {
            return ((EMSFB_COMMAND.emCommandClassifyChatRoom == emCommandType) || (EMSFB_COMMAND.emCommandClassifyMeeting == emCommandType));
        }
        private string EstablishClassifyCommandXml(EMSFB_COMMAND emCommandType, string strUserSipUri, string strSFBObjUri)
        {

            string strCommandType = CommonHelper.GetValueByKeyFromDir(kdicCommandTypes, emCommandType, "");
            string strCommandId = Guid.NewGuid().ToString();

            XmlDocument xmlDoc = new XmlDocument();
            XmlElement obXmlMessageInfo = CreateCommandMessageInfoHeader(xmlDoc, strCommandType, strCommandId, EMSFB_STATUS.emStatusOK.ToString());
            XMLTools.CreateElement(xmlDoc, obXmlMessageInfo, kstrXMLUserSipUriFlag, strUserSipUri);
            XMLTools.CreateElement(xmlDoc, obXmlMessageInfo, kstrXMLSFBObjUriFlag, strSFBObjUri);

            m_StuClassifyCmdInfo = new STUSFB_CLASSIFYCMDINFO(emCommandType, strUserSipUri, strSFBObjUri);
            m_strXmlClassifyCommandInfo = xmlDoc.InnerXml;
            SetCommandID(strCommandId);
            SetAanlysisFlag(true);
            return xmlDoc.InnerXml;
        }
        #endregion
    }
}
