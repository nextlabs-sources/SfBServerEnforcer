using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Rtc.Collaboration;

using QAToolSFBCommon.NLLog;
using QAToolSFBCommon.WebServiceHelper;
using QAToolSFBCommon.Common;

namespace NLLyncEndpointProxy.Common
{
    static class EndpointProxyCommonTools
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(EndpointProxyCommonTools));
        #endregion

        #region static/const values
        public const char kchSepSipUri = ';';
        #endregion

        static public string GetIDFromSipUri(string strConferenceSipUri)
        {
            if (!string.IsNullOrEmpty(strConferenceSipUri))
            {
                string strFlag = ":id:";
                int nIndex = strConferenceSipUri.LastIndexOf(strFlag, StringComparison.OrdinalIgnoreCase);
                return strConferenceSipUri.Substring(nIndex + strFlag.Length);
            }
            return "";
        }
        static public string GetConferenceOwnerSipUri(string strConferenceSipUri)
        {
            if (!string.IsNullOrEmpty(strConferenceSipUri))
            {
                /*
                 * sip:abraham.lincoln@lync.nextlabs.solutions;gruu;opaque=app:conf:focus:id:B43LT101
                 * sip:abraham.lincoln@lync.nextlabs.solutions;gruu;opaque=app:conf:chat:id:B43LT101
                 * sip:abraham.lincoln@lync.nextlabs.solutions;gruu;opaque=app:conf:audio-video:id:B43LT101
                */
                string strFlag = ";";
                int nIndex = strConferenceSipUri.IndexOf(strFlag, StringComparison.OrdinalIgnoreCase);
                return strConferenceSipUri.Substring(0, nIndex);
            }
            return "";
        }
        static public string GetConferenceChatSipUri(string strConferenceSipUri)
        {
            /*
             * sip:abraham.lincoln@lync.nextlabs.solutions;gruu;opaque=app:conf:focus:id:B43LT101
             * sip:abraham.lincoln@lync.nextlabs.solutions;gruu;opaque=app:conf:chat:id:B43LT101
             * sip:abraham.lincoln@lync.nextlabs.solutions;gruu;opaque=app:conf:audio-video:id:B43LT101
            */
            return strConferenceSipUri.Replace(":focus:", ":chat:");
        }
        static public string GetCallRemoteUserSipUri(InstantMessagingCall obIMCall)
        {
            if ((null != obIMCall) && (null != obIMCall.RemoteEndpoint))
            {
                string strRemoteEndpointUri = obIMCall.RemoteEndpoint.Uri;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "IM call: remote uri:{0}\n", strRemoteEndpointUri);
                string[] szUriInfos = strRemoteEndpointUri.Split(new char[] { kchSepSipUri }, StringSplitOptions.RemoveEmptyEntries);
                if ((null != szUriInfos) && (0 < szUriInfos.Length))
                {
                    return szUriInfos[0];
                }
            }
            return "";
        }

        static public string GetAgentAutoReplyInfo(string strBaseAutoReplay, string strUserSipUri)
        {
            string strAutoReply = "";
            try
            {
                Dictionary<string, string> dicAgentAutoSendWildcards = new Dictionary<string, string>()
                {
                    {NLLyncEndpointProxyConfigInfo.kstrWildcardUserDisplayName, CommonHelper.GetUserNameFromUserSipUri(strUserSipUri)}
                };
                strAutoReply = CommonHelper.ReplaceWildcards(strBaseAutoReplay, dicAgentAutoSendWildcards, NLLyncEndpointProxyConfigInfo.kstrConfigWildcardStartFlag, NLLyncEndpointProxyConfigInfo.kstrConfigWildcardEndFlag, false);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in GetAgentAutoReplyInfo, [{0}]\n", ex.Message);
            }
            return strAutoReply;
        }

        static public string GetClassifyMeetingAutoReplyInfo(string strBaseAutoReplay, string strUserSipUri, string strMeetingUri, out bool bAllSuccess)
        {
            string strAutoReply = "";
            bAllSuccess = false;
            try
            {
                ClassifyMeetingServiceResponseInfo obClassifyMeetingWebServiceResponseInfo = ClassifyMeetingWebServiceHelper.GetClassifyMeetingUrlFromAssistantWebService(
                    NLLyncEndpointProxyConfigInfo.s_endpointProxyConfigInfo.ClassifyAssitantWebServiceAddr, strUserSipUri, strMeetingUri, ClassifyMeetingWebServiceHelper.kstrUserAgent_Assistant);
                if (null != obClassifyMeetingWebServiceResponseInfo)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Uri:[{0}], Status:[{1}]\n", obClassifyMeetingWebServiceResponseInfo.strClassifyMeetingUrl, obClassifyMeetingWebServiceResponseInfo.strStatus);
                    Dictionary<string, string> dicAssistantAutoSendWildcards = new Dictionary<string, string>()
                    {
                        {NLLyncEndpointProxyConfigInfo.kstrWildcardUserDisplayName, CommonHelper.GetUserNameFromUserSipUri(strUserSipUri)},
                        {NLLyncEndpointProxyConfigInfo.kstrWildcardClassifyUrl, obClassifyMeetingWebServiceResponseInfo.strClassifyMeetingUrl}
                    };
                    bAllSuccess = ClassifyMeetingServiceResponseInfo.kstrStatusSuccess.Equals(obClassifyMeetingWebServiceResponseInfo.strStatus, StringComparison.OrdinalIgnoreCase);
                    if (!bAllSuccess)
                    {
                        strBaseAutoReplay = NLLyncEndpointProxyConfigInfo.s_endpointProxyConfigInfo.ClassifyAssistantUnknownError.m_strErrorMsg;
                    }
                    strAutoReply = CommonHelper.ReplaceWildcards(strBaseAutoReplay, dicAssistantAutoSendWildcards, NLLyncEndpointProxyConfigInfo.kstrConfigWildcardStartFlag, NLLyncEndpointProxyConfigInfo.kstrConfigWildcardEndFlag, false);
                    bAllSuccess &= (!string.IsNullOrEmpty(strAutoReply));
                }
            }
            catch (Exception ex)
            {
                bAllSuccess = false;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in GetClassifyMeetingAutoReplyInfo, [{0}]\n", ex.Message);
            }
            return strAutoReply;
        }
    }
}
