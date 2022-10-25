using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.CommandHelper;

namespace TestProject.CommandHelperTest
{
    class CommandHelperTester
    {
        public const string kstrUserSipUri = "sip:kim.yang@nextlabs.com";
        public const string kstrSFBObjSipUri = "sip:SFBObjSipUri";

        public const string kstrNotifyInfoNatural = "NLRECIPIENTS:\\CREATOR;NLMESSAGE:this is a notify message which will be send to \\CREATOR;.NLRECIPIENTS:\\INVITEE;NLMESSAGE:this is a notify message which will be send to \\INVITEE;.";

        public const string kstrNotifyInfoXML = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                                                "<MessageInfo type=\"Notify\">" +
                                                    "<Notification>" +
                                                        "<DesSipUri>\\CREATOR;;Sip:KimTest1.yang@lync.nextlabs.solutions</DesSipUri>" +
                                                        "<Body>" +
                                                            "A test notify message. \\TYPE; creator is \\CREATOR;, This message is send by Nextlabs enforcer enpoint proxy. The message will tell you why you are blocked by the enforcer." +
                                                        "</Body>" +
                                                    "</Notification>" +
                                                    "<Notification>" +
                                                        "<DesSipUri>\\CREATOR;;Sip:KimTest1.yang@lync.nextlabs.solutions</DesSipUri>" +
                                                        "<Body>" +
                                                            "A test notify message. \\TYPE; creator is \\CREATOR;, This message is send by Nextlabs enforcer enpoint proxy. The message will tell you why you are blocked by the enforcer." +
                                                        "</Body>" +
                                                    "</Notification>" +
                                                "</MessageInfo>";

        public static Dictionary<string, string> s_dicWildcards = new Dictionary<string,string>()
        {
            { CommandHelper.kstrWildcardAnchorKey, "USERPLACEHOLDER" },
            { CommandHelper.kstrWildcardNLClassifyTagsName, "NLCLASSIFYTAGS"},

            { CommandHelper.kstrWildcardType, "TYPE"},
            { CommandHelper.kstrWildcardCreator, "CREATOR"},
            { CommandHelper.kstrWildcardSfbObjUri, "SFBOBJURI"},
            { CommandHelper.kstrWildcardParticipates, "PARTICIPANTS"},
            { CommandHelper.kstrWildcardInviter, "INVITER"},
            { CommandHelper.kstrWildcardInvitee, "INVITEE"},
            { CommandHelper.kstrWildcardPolicyName, "POLICYNAME"},
        
            { CommandHelper.kstrWildcardChatRoomManagers, "CHATROOMMANAGERS"},
            { CommandHelper.kstrWildcardChatRoomMembers, "CHATROOMMEMBERS"},
            { CommandHelper.kstrWildcardChatRoomName, "CHATROOMNAME"}
        };

        static public void Test()
        {
            ClassifyCommandHelper obClassifyCommandHelpersServer = new ClassifyCommandHelper(EMSFB_COMMAND.emCommandClassifyMeeting, kstrUserSipUri, kstrSFBObjSipUri);
            {
                bool bAnalysisFlag = obClassifyCommandHelpersServer.GetAanlysisFlag();
                string strCommandID = obClassifyCommandHelpersServer.GetCommandID();
                string strCommandXml = obClassifyCommandHelpersServer.GetCommandXml();

                STUSFB_CLASSIFYCMDINFO obClassifyCmdInfoServer = obClassifyCommandHelpersServer.ClassifyCmdInfo;

                Console.Write("AnalysisFlag:[{0}], CommandID:[{1}]\n CommandXml:\n[{2}]\n", bAnalysisFlag, strCommandID, strCommandXml);
                Console.Write("ClassifyCommandType:[{0}], UserSipUri:[{1}], SFBObjUri:[{2}]\n", obClassifyCmdInfoServer.m_emCommandType, obClassifyCmdInfoServer.m_strUserSipUri, obClassifyCmdInfoServer.m_strSFBObjUri);
            }

            ClassifyCommandHelper obClassifyCommandHelpersClient = new ClassifyCommandHelper(obClassifyCommandHelpersServer.GetCommandXml());
            {
                bool bAnalysisFlag = obClassifyCommandHelpersClient.GetAanlysisFlag();
                string strCommandID = obClassifyCommandHelpersClient.GetCommandID();
                string strCommandXml = obClassifyCommandHelpersClient.GetCommandXml();

                STUSFB_CLASSIFYCMDINFO obClassifyCmdInfoClient = obClassifyCommandHelpersClient.ClassifyCmdInfo;

                Console.Write("AnalysisFlag:[{0}], CommandID:[{1}]\n CommandXml:\n[{2}]\n", bAnalysisFlag, strCommandID, strCommandXml);
                Console.Write("ClassifyCommandType:[{0}], UserSipUri:[{1}], SFBObjUri:[{2}]\n", obClassifyCmdInfoClient.m_emCommandType, obClassifyCmdInfoClient.m_strUserSipUri, obClassifyCmdInfoClient.m_strSFBObjUri);
            }

        }

        static public void NotificationCommandTest()
        {
            {
                NotifyCommandHelper obNotifyCommandHelper = new NotifyCommandHelper(kstrNotifyInfoNatural, s_dicWildcards);
                Console.Write("AnalysisFlag:[{0}], CommandID:[{1}]\n CommandXml:\n[{2}]\n", obNotifyCommandHelper.GetAanlysisFlag(), obNotifyCommandHelper.GetCommandID(), obNotifyCommandHelper.StrXmlNotifyInfo);
                foreach (BasicNotificationInfo obBasicNotificationInfo in obNotifyCommandHelper.LsBasicNotificationInfo)
                {
                    Console.Write("Des:[{0}]\nBody:[{1}]\nHeader:[{2}]\nToastMessage:[{3}]\n", string.Join(";", obBasicNotificationInfo.SzStrDesSipUris), obBasicNotificationInfo.StrMessageBody, obBasicNotificationInfo.StrMessageHeader, obBasicNotificationInfo.StrToastMessage);
                }
            }

            {
                NotifyCommandHelper obNotifyCommandHelper = new NotifyCommandHelper(kstrNotifyInfoXML, s_dicWildcards);
                Console.Write("AnalysisFlag:[{0}], CommandID:[{1}]\n CommandXml:\n[{2}]\n", obNotifyCommandHelper.GetAanlysisFlag(), obNotifyCommandHelper.GetCommandID(), obNotifyCommandHelper.StrXmlNotifyInfo);
                foreach (BasicNotificationInfo obBasicNotificationInfo in obNotifyCommandHelper.LsBasicNotificationInfo)
                {
                    Console.Write("Des:[{0}]\nBody:[{1}]\nHeader:[{2}]\nToastMessage:[{3}]\n", string.Join(";", obBasicNotificationInfo.SzStrDesSipUris), obBasicNotificationInfo.StrMessageBody, obBasicNotificationInfo.StrMessageHeader, obBasicNotificationInfo.StrToastMessage);
                }
            }
        }
    }
}
