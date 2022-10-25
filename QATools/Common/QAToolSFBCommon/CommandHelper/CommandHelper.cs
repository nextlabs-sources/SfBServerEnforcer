using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using QAToolSFBCommon.Common;
using QAToolSFBCommon.NLLog;

namespace QAToolSFBCommon.CommandHelper
{
    #region Lync server command info
    public enum EMSFB_STATUS
    {
        emStatusUnknown,

        emStatusOK,
        emStatusWaiting,
        emStatusFailed
    }
    public enum EMSFB_COMMAND
    {
        emCommandUnknown = 0,

        emCommandGetEndpointProxyInfo,
        emCommandEndpointProxyInfo
    };
    public class STUSFB_ENDPOINTINFO
    {
        public string m_strSipUri = "";

        public STUSFB_ENDPOINTINFO(string strParamSipUri)
        {
            m_strSipUri = strParamSipUri;
        }
        public STUSFB_ENDPOINTINFO(STUSFB_ENDPOINTINFO stuParamProxyEndpointInfo)
        {
            m_strSipUri = stuParamProxyEndpointInfo.m_strSipUri;
        }
    }
    public class STUSFB_CLASSIFYCMDINFO
    {
        public EMSFB_COMMAND m_emCommandType = EMSFB_COMMAND.emCommandUnknown;
        public string m_strUserSipUri = "";
        public string m_strSFBObjUri = "";     // meeting or chat room URI

        public STUSFB_CLASSIFYCMDINFO(EMSFB_COMMAND emCommandType, string strUserSipUri, string strSFBObjUri)
        {
            m_emCommandType = emCommandType;
            m_strUserSipUri = strUserSipUri;
            m_strSFBObjUri = strSFBObjUri;
        }
        public STUSFB_CLASSIFYCMDINFO(STUSFB_CLASSIFYCMDINFO stuParamClassifyCmdInfo)
        {
            m_emCommandType = stuParamClassifyCmdInfo.m_emCommandType;
            m_strUserSipUri = stuParamClassifyCmdInfo.m_strUserSipUri;
            m_strSFBObjUri = stuParamClassifyCmdInfo.m_strSFBObjUri;
        }
    }
    public class STUSFB_MESSAGEINFOHEADER
    {
        public EMSFB_COMMAND m_emCommandType = EMSFB_COMMAND.emCommandUnknown;
        public string m_strID = "";
        public EMSFB_STATUS m_emStatus = EMSFB_STATUS.emStatusUnknown;

        public STUSFB_MESSAGEINFOHEADER(EMSFB_COMMAND emCommandType, string strID, EMSFB_STATUS emStatus)
        {
            m_emCommandType = emCommandType;
            m_strID = strID;
            m_emStatus = emStatus;
        }
        public STUSFB_MESSAGEINFOHEADER(STUSFB_MESSAGEINFOHEADER stuParamMessageInfoHeader)
        {
            m_emCommandType = stuParamMessageInfoHeader.m_emCommandType;
            m_strID = stuParamMessageInfoHeader.m_strID;
            m_emStatus = stuParamMessageInfoHeader.m_emStatus;
        }
    }
    #endregion

    public abstract class CommandHelper
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(CommandHelper));
        #endregion

        #region Separators, Wildcards
        public const char kchSepDesSipUri = ';';
        public const char kchSepWildcardAnchorKeyAndValue = '=';

        public const string kstrWildcardAnchorKey = "USERPLACEHOLDER";
        public const string kstrWildcardNLClassifyTagsName = "NLCLASSIFYTAGS";

        public const string kstrWildcardType = "TYPE";
        public const string kstrWildcardCreator = "CREATOR";
        public const string kstrWildcardSfbObjUri = "SFBOBJURI";
        public const string kstrWildcardParticipates = "PARTICIPANTS";
        public const string kstrWildcardInviter = "INVITER";
        public const string kstrWildcardInvitee = "INVITEE";
        public const string kstrWildcardPolicyName = "POLICYNAME";
        
        public const string kstrWildcardChatRoomManagers = "CHATROOMMANAGERS";
        public const string kstrWildcardChatRoomMembers = "CHATROOMMEMBERS";
        public const string kstrWildcardChatRoomName = "CHATROOMNAME";
        private const string kstrWildcardChatCategoryName = "CHATCATEGORYNAME";
        #endregion

        #region message types, EMNLPROXY_COMAND
        public const string kstrCommandGetEndpointProxyInfo = "getEndpointProxyInfo";
        public const string kstrCommandEndpointProxyInfo = "endpointProxyInfo";

        public static readonly Dictionary<EMSFB_COMMAND, string> kdicCommandTypes = new Dictionary<EMSFB_COMMAND, string>()
        {
            { EMSFB_COMMAND.emCommandUnknown, "" },

            { EMSFB_COMMAND.emCommandGetEndpointProxyInfo, kstrCommandGetEndpointProxyInfo },
            { EMSFB_COMMAND.emCommandEndpointProxyInfo, kstrCommandEndpointProxyInfo }
        };
        #endregion

        #region Command XML Flag & Attr 
        public const string kstrXMLMessageInfoFlag = "MessageInfo";
        public const string kstrXMLNotificationFlag = "Notification";
        public const string kstrXMLDesSipUriFlag = "DesSipUri";
        public const string kstrXMLToastMessageFlag = "ToastMessage";
        public const string kstrXMLHeaderFlag = "Header";
        public const string kstrXMLBodyFlag = "Body";
        public const string kstrXMLUserSipUriFlag = "UserSipUri";
        public const string kstrXMLTagsFlag = "Tags";
        public const string kstrXMLTagFlag = "Tag";
        public const string kstrXMLSFBObjUriFlag = "SFBObjUri";

        public const string kstrXMLTypeAttr = "type";
        public const string kstrXMLIdAttr = "id";
        public const string kstrXMLStatusAttr = "status";
        public const string kstrXMLTagNameAttr = "TagName";
        public const string kstrXMLTagValueAttr = "TagValue";
        #endregion

        #region Flags
        private bool m_bAnalysisSuccess = false;
        private string m_strCommandID = "";
        #endregion

        #region Constructors
        public CommandHelper() { }
        #endregion

        #region Public functions
        public bool GetAanlysisFlag() { return m_bAnalysisSuccess; }
        public string GetCommandID() { return m_strCommandID; }
        #endregion

        #region Private functions
        protected void SetAanlysisFlag(bool bSuccess) { m_bAnalysisSuccess = bSuccess; }
        protected void SetCommandID(string strstrCommandID) { m_strCommandID = strstrCommandID; }
        #endregion

        #region Protected functions
        protected XmlElement CreateCommandMessageInfoHeader(XmlDocument xmlDoc, string strType, string strGuid, string strStatus)
        {
            XmlNode obDecNode = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
            xmlDoc.AppendChild(obDecNode);
            return XMLTools.CreateElement(xmlDoc, xmlDoc, kstrXMLMessageInfoFlag, "", kstrXMLTypeAttr, strType, kstrXMLIdAttr, strGuid, kstrXMLStatusAttr, strStatus);
        }
        #endregion

        #region Static function
        static public STUSFB_MESSAGEINFOHEADER GetMessageInfoHeader(string strCommandXML)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strCommandXML);

            // Select MessageInfo to get message type
            XmlNode obXMLMessageInfo = xmlDoc.SelectSingleNode(kstrXMLMessageInfoFlag);
            if (null != obXMLMessageInfo)
            {
                string strCommandType = XMLTools.GetAttributeValue(obXMLMessageInfo.Attributes, kstrXMLTypeAttr, 0);
                EMSFB_COMMAND emCommandType = ConvertStringCommandType(strCommandType);

                string strCommandId = XMLTools.GetAttributeValue(obXMLMessageInfo.Attributes, kstrXMLIdAttr, 0);

                string strCommandStatus = XMLTools.GetAttributeValue(obXMLMessageInfo.Attributes, kstrXMLStatusAttr, 0);
                EMSFB_STATUS emCommandStatus = ConvertStringCommandStatus(strCommandStatus);

                return new STUSFB_MESSAGEINFOHEADER(emCommandType, strCommandId, emCommandStatus);
            }
            return null;
        }
        static public EMSFB_COMMAND GetCommandType(string strCommandXML)
        {
            EMSFB_COMMAND emCommandType = EMSFB_COMMAND.emCommandUnknown;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strCommandXML);

            // Select MessageInfo to get message type
            XmlNode obXMLMessageInfo = xmlDoc.SelectSingleNode(kstrXMLMessageInfoFlag);
            if (null != obXMLMessageInfo)
            {
                string strCommandType = XMLTools.GetAttributeValue(obXMLMessageInfo.Attributes, kstrXMLTypeAttr, 0);
                emCommandType = ConvertStringCommandType(strCommandType);
            }

            return emCommandType;
        }
        static public string GetCommandID(string strCommandXML)
        {
            string strCommandId = "";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strCommandXML);

            // Select MessageInfo to get message type
            XmlNode obXMLMessageInfo = xmlDoc.SelectSingleNode(kstrXMLMessageInfoFlag);
            if (null != obXMLMessageInfo)
            {
                strCommandId = XMLTools.GetAttributeValue(obXMLMessageInfo.Attributes, kstrXMLIdAttr, 0);
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug,"Message id is:[{0}]", strCommandId);

            return strCommandId;
        }
        static public EMSFB_STATUS GetCommandStatus(string strCommandXML)
        {
            EMSFB_STATUS emCommandStatus = EMSFB_STATUS.emStatusUnknown;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strCommandXML);

            // Select MessageInfo to get message type
            XmlNode obXMLMessageInfo = xmlDoc.SelectSingleNode(kstrXMLMessageInfoFlag);
            if (null != obXMLMessageInfo)
            {
                string strCommandStatus = XMLTools.GetAttributeValue(obXMLMessageInfo.Attributes, kstrXMLStatusAttr, 0);
                emCommandStatus = ConvertStringCommandStatus(strCommandStatus);
            }
            return emCommandStatus;
        }

        static protected EMSFB_COMMAND ConvertStringCommandType(string strCommandType)
        {
            EMSFB_COMMAND emCommandType = EMSFB_COMMAND.emCommandUnknown;
            if (kstrCommandGetEndpointProxyInfo.Equals(strCommandType, StringComparison.OrdinalIgnoreCase))
            {
                emCommandType = EMSFB_COMMAND.emCommandGetEndpointProxyInfo;
            }
            else if (kstrCommandEndpointProxyInfo.Equals(strCommandType, StringComparison.OrdinalIgnoreCase))
            {
                emCommandType = EMSFB_COMMAND.emCommandEndpointProxyInfo;
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug,"Convert result :[in={0}], [out={1}]", strCommandType, emCommandType);
            return emCommandType;
        }
        static protected EMSFB_STATUS ConvertStringCommandStatus(string strCommandStatus)
        {
            EMSFB_STATUS emCommandStatus = EMSFB_STATUS.emStatusUnknown;
            if (!string.IsNullOrEmpty(strCommandStatus))
            {
                if (Enum.IsDefined(typeof(EMSFB_STATUS), strCommandStatus))
                {
                    emCommandStatus = (EMSFB_STATUS)Enum.Parse(typeof(EMSFB_STATUS), strCommandStatus);
                }
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug,"Convert result :[in={0}], [out={1}]", strCommandStatus, emCommandStatus);
            return emCommandStatus;
        }
        #endregion
    }
}
