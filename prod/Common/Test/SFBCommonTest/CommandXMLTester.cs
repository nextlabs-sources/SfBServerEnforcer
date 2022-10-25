using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.NLLog;
using SFBCommon.CommandHelper;

namespace TestProject.SFBCommonTest
{
    class CommandXMLTester: Logger
    {
        static public void Test()
        {
            string strCommandXML = "";
            string strCommandID = "";
            STUSFB_MESSAGEINFOHEADER stuEMessageInfoHeader = new STUSFB_MESSAGEINFOHEADER(EMSFB_COMMAND.emCommandUnknown, "", EMSFB_STATUS.emStatusUnknown);

            EndpointProxyInfoCommandHelper obEndpointInfo = new EndpointProxyInfoCommandHelper();
            strCommandXML = obEndpointInfo.GetCommandXml(EMSFB_COMMAND.emCommandGetEndpointProxyInfo);
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, strCommandXML);
            strCommandID = CommandHelper.GetCommandID(strCommandXML);
            stuEMessageInfoHeader = CommandHelper.GetMessageInfoHeader(strCommandXML);

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "begin NotifyCommandHelper");
            NotifyCommandHelper obNotifyInfo = new NotifyCommandHelper("\'s\"trDesSipUri", "&strToastMessage", "&amp;strMessageHeader", "/>strMessageBody");
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, obNotifyInfo.StrXmlNotifyInfo);
            List<BasicNotificationInfo> lsBasicNotificationInfo = obNotifyInfo.LsBasicNotificationInfo;

            strCommandID = CommandHelper.GetCommandID(strCommandXML);
            stuEMessageInfoHeader = CommandHelper.GetMessageInfoHeader(strCommandXML);

            foreach (BasicNotificationInfo obBasicNotificationInfo in lsBasicNotificationInfo)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "UserSipUri:[{0}],Header:[{1}],ToastMessage:[{2}],Body:[{3}]", obBasicNotificationInfo.SzStrDesSipUris.ToString(), obBasicNotificationInfo.StrMessageHeader, obBasicNotificationInfo.StrToastMessage, obBasicNotificationInfo.StrMessageBody);
            }

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Command ID:[{0}], Status:[{1}]\n", strCommandID, stuEMessageInfoHeader.m_emStatus);
        }

        static public void TestEx()
        {
            string strCommandXML = "";
            string strCommandID = "";
            STUSFB_MESSAGEINFOHEADER stuEMessageInfoHeader = new STUSFB_MESSAGEINFOHEADER(EMSFB_COMMAND.emCommandUnknown, "", EMSFB_STATUS.emStatusUnknown);

            EndpointProxyInfoCommandHelper obEndpointInfo = new EndpointProxyInfoCommandHelper();
            strCommandXML = obEndpointInfo.GetCommandXml(EMSFB_COMMAND.emCommandGetEndpointProxyInfo);
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, strCommandXML);
            strCommandID = CommandHelper.GetCommandID(strCommandXML);
            stuEMessageInfoHeader = CommandHelper.GetMessageInfoHeader(strCommandXML);

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "begin NotifyCommandHelper");
            string strNotifyXML = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                                  "<MessageInfo type=\"Notify\">" +
                                    "<Notification>" +
                                        "<DesSipUri>\\CREATOR;;Sip:KimTest1.yang@lync.nextlabs.solutions</DesSipUri>" +
                                        "<ToastMessage></ToastMessage>" +
                                        "<Body>" +
                                            "A test notify message \\CHATROOMNAME;. Current \\TYPE; creator is \\CREATOR;, This message is send by Nextlabs enforcer enpoint proxy. The message will tell you why you are blocked by the enforcer." +
                                        "</Body>" +
                                     "</Notification>" +
                                     "<Notification>" +
                                        "<DesSipUri>\\CREATOR;Sip:KimTest1.yang@lync.nextlabs.solutions</DesSipUri>" +
                                        "<Body>" +
                                            "A test notify message. Current \\TYPE; creator is \\CREATOR;, This message is send by Nextlabs enforcer enpoint proxy. The message will tell you why you are blocked by the enforcer." +
                                         "</Body>" +
                                     "</Notification>" +
                                  "</MessageInfo>";
            Dictionary<string, string> dicWildcards = new Dictionary<string,string>()
            {
                {CommandHelper.kstrWildcardType, "MyTests"},
                {CommandHelper.kstrWildcardCreator, "Creators"},
                {CommandHelper.kstrWildcardChatRoomName , "Chat&Name"}
            };
            NotifyCommandHelper obNotifyInfo = new NotifyCommandHelper(strNotifyXML, dicWildcards);
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, obNotifyInfo.StrXmlNotifyInfo);
            List<BasicNotificationInfo> lsBasicNotificationInfo = obNotifyInfo.LsBasicNotificationInfo;

            foreach (BasicNotificationInfo obBasicNotificationInfo in lsBasicNotificationInfo)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "UserSipUri:[{0}],Header:[{1}],ToastMessage:[{2}],Body:[{3}]", obBasicNotificationInfo.SzStrDesSipUris.ToString(), obBasicNotificationInfo.StrMessageHeader, obBasicNotificationInfo.StrToastMessage, obBasicNotificationInfo.StrMessageBody);
            }

            strCommandID = CommandHelper.GetCommandID(strCommandXML);
            stuEMessageInfoHeader = CommandHelper.GetMessageInfoHeader(strCommandXML);

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Command ID:[{0}], Status:[{1}]\n", strCommandID, stuEMessageInfoHeader.m_emStatus);
        }
    }
}
