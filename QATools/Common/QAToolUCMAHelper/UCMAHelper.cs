// Basic
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;

// UCMA
using Microsoft.Rtc.Collaboration;  // Basic namespace
using Microsoft.Rtc.Signaling;      // SipTransportType
using Microsoft.Rtc.Collaboration.ConferenceManagement; // ConferenceScheduleInformation

// Other project
using QAToolSFBCommon.NLLog;

namespace QAToolNLUCMA
{
    // A production application should have catch blocks for a number of
    // other exceptions, including FailureResponseException, ServerPolicyException, 
    // and OperationTimeoutException.
    public static class UCMAHelper
    {
        #region Values
        static private readonly char[] s_szchIllegalCharsInUserAgent = new char[]{':'};
        #endregion

        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(UCMAHelper));
        #endregion

        static private string MakeARightUserAgent(string strUserAgent)
        {
            if (!string.IsNullOrEmpty(strUserAgent))
            {
                bool bContainsIllegalChars = false;
                foreach (char kchIllegal in s_szchIllegalCharsInUserAgent)
                {
                    if (strUserAgent.Contains(kchIllegal))
                    {
                        bContainsIllegalChars = true;
                        break;
                    }
                }
                if (bContainsIllegalChars)
                {
                    string[] szValues = strUserAgent.Split(s_szchIllegalCharsInUserAgent);
                    strUserAgent = string.Join(" ", szValues);
                }
            }
            return strUserAgent;
        }

        // Note: if the user strUserAgent string contains ":", the UserEndpoint cannot create success
        public static UserEndpoint CreateUserEndPoint(string strLyncFQDN, string strUserSipUri, string strDomain, string strUserName, string strPassword, string strUserAgent)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Begin create user endpoint\n");

            // Make a right user agent string
            strUserAgent = MakeARightUserAgent(strUserAgent);

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In CreateUserEndPoint, UserAgent:[{0}]", strUserAgent);

            // Set user endpoint
            UserEndpointSettings obUserEndPointSetings = new UserEndpointSettings(strUserSipUri, strLyncFQDN);
            obUserEndPointSetings.AutomaticPresencePublicationEnabled = true;
            obUserEndPointSetings.Credential = new System.Net.NetworkCredential(strUserName, strPassword, strDomain);

            // Set client platform
            SipTransportType emSipTransportType = SipTransportType.Tls;
            ClientPlatformSettings obClientPlatformSettings = new ClientPlatformSettings(strUserAgent, emSipTransportType);

            // Create collaboration platform
            CollaborationPlatform obCollaborationPlatform = new CollaborationPlatform(obClientPlatformSettings);

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Success create user endpoint\n");

            // Create user endpoint
            return new UserEndpoint(obCollaborationPlatform, obUserEndPointSetings);
        }

        public static void StartupPlatform(UserEndpoint obUserEndpoint, AsyncCallback CBFuncStartupPlatformComplete)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Begin start up platform\n");
            IAsyncResult obAysncResult = obUserEndpoint.Platform.BeginStartup(CBFuncStartupPlatformComplete, obUserEndpoint);
            if (null == CBFuncStartupPlatformComplete)
            {
                obUserEndpoint.Platform.EndStartup(obAysncResult);
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Success start platform\n");
        }

        public static void EstablishEndpoint(UserEndpoint obUserEndpoint, AsyncCallback CBFuncEstablishUserEndpointComplete)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Begin establish endpoint \n");
            IAsyncResult obAysncResult = obUserEndpoint.BeginEstablish(CBFuncEstablishUserEndpointComplete, obUserEndpoint);
            if (null == CBFuncEstablishUserEndpointComplete)
            {
                obUserEndpoint.EndEstablish(obAysncResult);
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Success Establish endpoint\n");
        }

        public static Conversation CreateConversation(UserEndpoint obUserEndpoint, string strConversationSubject)
        {
            ConversationSettings obConversationSettings = new ConversationSettings();
            obConversationSettings.Priority = ConversationPriority.Urgent;
            obConversationSettings.Subject = strConversationSubject;
            return new Conversation(obUserEndpoint, obConversationSettings);
        }

        public static Conversation EscalateConversationToConference(Conversation obConversation, AsyncCallback CBFuncEscalateToConference)
        {
            IAsyncResult obAysncResult = obConversation.BeginEscalateToConference(CBFuncEscalateToConference, obConversation);
            if (null == CBFuncEscalateToConference)
            {
                obConversation.EndEscalateToConference(obAysncResult);
            }
            return obConversation;
        }

        public static Conference ScheduleConference(UserEndpoint obUserEndpoint, AsyncCallback CBFuncScheduleConference)
        {
            // One of the endpoints schedules the conference in advance. At schedule time, all the conference settings are set.
            // The base conference settings object, used to set the policies for the conference.
            ConferenceScheduleInformation conferenceScheduleInformation = new ConferenceScheduleInformation();

            // A closed meeting (only participants in the list of the scheduled conference can join), but requiring authentication.
            conferenceScheduleInformation.AccessLevel = ConferenceAccessLevel.Everyone;

            // This flag determines whether or not the passcode is optional for  users joining the conference.
            conferenceScheduleInformation.IsPasscodeOptional = false;

            // The verbose description of the conference.
            conferenceScheduleInformation.Description = "Auto Schedule Conference";

            // This field indicates the date and time after which the conference can be deleted.
            conferenceScheduleInformation.ExpiryTime = System.DateTime.Now.AddHours(8);

            // This property indicates if the lobby bypass for gateway participants
            // feature is enabled. If enabled for gateway participant, a 
            // participant joining from a phone will not land in the lobby if 
            // the JoinMode is GatewayParticipant.
            conferenceScheduleInformation.LobbyBypass = LobbyBypass.EnabledForGatewayParticipants;

            // This property indicates if the feature that automatic promotes 
            // participants to leader upon joining is enabled.
            conferenceScheduleInformation.AutomaticLeaderAssignment = AutomaticLeaderAssignment.SameEnterprise;

            // These two lines assign a set of modalities (here, only AudioVideo)
            // from the available MCUs to the conference. Custom modalities 
            // (and their corresponding MCUs) may be added at this time, as part
            // of the extensibility model.
            ConferenceMcuInformation audioVideoMCU = new ConferenceMcuInformation(McuType.AudioVideo);
            ConferenceMcuInformation imMCU = new ConferenceMcuInformation(McuType.InstantMessaging);
            conferenceScheduleInformation.Mcus.Add(audioVideoMCU);
            conferenceScheduleInformation.Mcus.Add(imMCU);

            // Now that the setup object is complete, schedule the conference 
            // using the conference services of the organizer’s endpoint.
            // Note: the conference organizer is considered a leader of the 
            // conference by default.
            IAsyncResult obAysncResult = obUserEndpoint.ConferenceServices.BeginScheduleConference(conferenceScheduleInformation, CBFuncScheduleConference, obUserEndpoint.ConferenceServices);
            if (null == CBFuncScheduleConference)
            {
                return obUserEndpoint.ConferenceServices.EndScheduleConference(obAysncResult);
            }
            return null;
        }

        public static void InviteRemoteUserIntoConference(Conversation obConversation, string strRemoteUserSipUri, AsyncCallback CBFuncDeliverRemoteUser)
        {
            ConferenceInvitation obConversationInvitation = new ConferenceInvitation(obConversation);
            IAsyncResult obAysncResult = obConversationInvitation.BeginDeliver(strRemoteUserSipUri, CBFuncDeliverRemoteUser, obConversationInvitation);
            if (null == CBFuncDeliverRemoteUser)
            {
                obConversationInvitation.EndDeliver(obAysncResult);
            }
        }

        public static void EstablishMessageCall(InstantMessagingCall obIMCall, string strToastMessage, string strDestinationSipUri, AsyncCallback CBFuncEstablishMessageCall)
        {
            // Establish message call
            IAsyncResult obAysncResult = null;
            ToastMessage obToastMessage = new ToastMessage(strToastMessage);
            if (string.IsNullOrEmpty(strDestinationSipUri))
            {
                obAysncResult = obIMCall.BeginEstablish(obToastMessage, null, CBFuncEstablishMessageCall, obIMCall);
            }
            else
            {
                obAysncResult = obIMCall.BeginEstablish(strDestinationSipUri, obToastMessage, null, CBFuncEstablishMessageCall, obIMCall);
            }
            if (null == CBFuncEstablishMessageCall)
            {
                obIMCall.EndEstablish(obAysncResult);
            }
        }

        public static bool SendMessage(InstantMessagingFlow obMessagingFlow, string strSendOutMessage, AsyncCallback CBFuncSendOutMessageComplete)
        {
            bool bRet = false;
            try
            {
                // Get InstantMessagingFlow and send message
                IAsyncResult obAysncResult = obMessagingFlow.BeginSendInstantMessage(strSendOutMessage, CBFuncSendOutMessageComplete, obMessagingFlow);
                if (null == CBFuncSendOutMessageComplete)
                {
                    obMessagingFlow.EndSendInstantMessage(obAysncResult);
                }
                bRet = true;
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception, SendMessage exception: " + ex.Message);
            }
            return bRet;
        }

        public static void ShutdownPlatform(CollaborationPlatform obCollaborationPlatform, AsyncCallback CBFuncShutdownPlatformComplete)
        {
            IAsyncResult obAysncResult = obCollaborationPlatform.BeginShutdown(CBFuncShutdownPlatformComplete, obCollaborationPlatform);
            if (null == CBFuncShutdownPlatformComplete)
            {
                obCollaborationPlatform.EndShutdown(obAysncResult);
            }
        }

        public static void TerminateCall(InstantMessagingCall obIMCall, AsyncCallback CBFuncTerminateCallComplete)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "TerminateCall invoked");
            IAsyncResult obAysncResult = obIMCall.BeginTerminate(CBFuncTerminateCallComplete, obIMCall);
            if (null == CBFuncTerminateCallComplete)
            {
                obIMCall.EndTerminate(obAysncResult);
            }
        }
        public static void TerminateConversation(Conversation obConversation, AsyncCallback CBFuncTerminateConversationComplete)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "TerminateCall invoked");
            IAsyncResult obAysncResult = obConversation.BeginTerminate(CBFuncTerminateConversationComplete, obConversation);
            if (null == CBFuncTerminateConversationComplete)
            {
                obConversation.EndTerminate(obAysncResult);
            }
        }
    }
}
