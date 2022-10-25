using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Rtc.Sip;

using SFBCommon.NLLog;
using SFBCommon.SFBObjectInfo;

using Nextlabs.SFBServerEnforcer.SIPComponent.SIPContentHelper;
using Nextlabs.SFBServerEnforcer.SIPComponent.NLAnalysis;
using Nextlabs.SFBServerEnforcer.PolicyHelper;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.Common
{
    public enum SIP_INVITE_TYPE
    {
        INVITE_UNKNOWN = 0,
        INVITE_IM_INVITE,                   //invite to instance message
        INVITE_CONF_INVITE,                 //invite to a conference
        INVITE_CONF_JOIN,                   //enter a conference
        INVITE_CONF_SHARE_CREATE,           //Sharer start sharing something in a conference
        INVITE_CONF_SHARE_JOIN,             //Conference participants join conference sharing application as a viewer
        INVITE_PERSISTENT_CHAT_ENDPOINT,    //establish a session with persistent chat room server
    };
    public enum SIP_SERVICE_TYPE
    {
        SERVICE_UNKNOWN = 0,
        SERVICE_CONFERENCE_CREATE, // Create conference
        SERVICE_PUBLISH_CATEGORY,//publish category, e.g. following a chat room
    };
    public enum SIP_ADDRESS_PARAMETER
    {
        EPID,
        TAG,
    };


    class CSIPTools
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(CSIPTools));
        #endregion

        #region SIP header name
        public const string SIP_HDR_CONTENTLENGTH = "content-length";
        public const string SIP_HDR_CONTENTTYPE = "content-type";
        public const string SIP_HDR_CALLID = "call-id";
        public const string SIP_HDR_TO = "to";
        public const string SIP_HDR_FROM = "from";
        public const string SIP_HDR_CONVERSTATIONID = "Ms-Conversation-ID";
        public const string SIP_HDR_SEQ = "cseq";
        public const string SIP_URI_PREFIX = "sip:";
        public const string SIP_HDR_USERAGENT = "user-agent";
        #endregion

        #region user sip parameter
        static readonly public string kStrSIPAddressParameterEpid = "epid";
        static readonly public string kStrSIPAddressParameterTag = "tag";
        static readonly public string kStrSIPAddressParameterSeparator = ";";
        #endregion

        #region Type analysis tools
        static public SIP_INVITE_TYPE GetInviteRequestType(Request request)
        {
            Header toHeader = request.AllHeaders.FindFirst(SIP_HDR_TO);
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Parse invite request type, SIP_HDR_TO:{0}\n", toHeader.Value);
            if(IsConferenceEndPoint(toHeader.Value))
            {
                if(IsConferenceFocusEndPoint(toHeader.Value))
                {
                    string strUserAgent = "";
                    Header userAgent = request.AllHeaders.FindFirst(CSIPTools.SIP_HDR_USERAGENT);
                    if(userAgent!=null)
                    {
                        strUserAgent = userAgent.Value;
                    }
                    bool bIsCASNotify = false;
                    if (!string.IsNullOrWhiteSpace(strUserAgent) && strUserAgent.Contains("Conferencing_Announcement_Service"))
                    {
                        bIsCASNotify = true;
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "the user agent is:{0}, ignore it for INVITE_CONF_JOIN.", strUserAgent);
                    }

                    if (!bIsCASNotify)
                    {
                        return SIP_INVITE_TYPE.INVITE_CONF_JOIN;
                    }
                }
                else if (IsConferenceShareEndPoint(toHeader.Value))
                {
                    return NLConferenceShareAnalyser.GetConferenceShareInviteRequestType(request);
                }
            }
            else if(IsPersistentChatEndpoint(toHeader.Value))
            {
                return SIP_INVITE_TYPE.INVITE_PERSISTENT_CHAT_ENDPOINT;
            }
            else
            {
                Header contentType = request.AllHeaders.FindFirst(SIP_HDR_CONTENTTYPE);
                if (contentType != null)
                {
                    if (contentType.Value.Equals("application/sdp"))
                    {
                        return SIP_INVITE_TYPE.INVITE_IM_INVITE;
                    }
                    else if (contentType.Value.Equals("application/ms-conf-invite+xml"))
                    {
                        string strUserAgent = "";
                        Header userAgent = request.AllHeaders.FindFirst(CSIPTools.SIP_HDR_USERAGENT);
                        if (userAgent != null)
                        {
                            strUserAgent = userAgent.Value;
                        }
                        bool bIsCASNotify = false;
                        if (!string.IsNullOrWhiteSpace(strUserAgent) && strUserAgent.Contains("AV-MCU"))
                        {
                            bIsCASNotify = true;
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "the user agent is:{0}, ignore it for INVITE_CONF_INVITE.", strUserAgent);
                        }

                        if(!bIsCASNotify)
                        {
                            return SIP_INVITE_TYPE.INVITE_CONF_INVITE;
                        }
                    }
                }
            }

            return SIP_INVITE_TYPE.INVITE_UNKNOWN;
        }
        static public SIP_SERVICE_TYPE GetServiceRequestType(Request request)
         {
            try
            {
                Header contentType = request.AllHeaders.FindFirst(SIP_HDR_CONTENTTYPE);
                
                if (contentType.Value.Equals("application/cccp+xml"))
                {
                    Header toHeader = request.AllHeaders.FindFirst(SIP_HDR_TO);
                    if (toHeader.Value.Contains("app:conf:focusfactory"))
                    {
                        return SIP_SERVICE_TYPE.SERVICE_CONFERENCE_CREATE;
                    }
                }
                else if(contentType.Value.Equals("application/msrtc-category-publish+xml"))
                {
                    return SIP_SERVICE_TYPE.SERVICE_PUBLISH_CATEGORY;
                }
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, ex.ToString());
            }
     

            return SIP_SERVICE_TYPE.SERVICE_UNKNOWN;
         }
        #endregion

        #region Conference tools
        static public bool IsConferenceEndPoint(string strEndPoint)
        {
            return strEndPoint.Contains("opaque=app:conf");
        }
        static public bool IsConferenceFocusEndPoint(string strEndpoint)
        {
            return strEndpoint.Contains("opaque=app:conf:focus");
        }
        static public bool IsConferenceShareEndPoint(string strEndpoint)
        {//meeting sharing
            return strEndpoint.Contains("opaque=app:conf:applicationsharing");
        }
        static public bool IsConferenceChannelEndPoint(string strEndpoint)
        {
            return (IsConferenceEndPoint(strEndpoint) && (!IsConferenceFocusEndPoint(strEndpoint)));
        }

        static public string GetConferenceFocusUriAccordingShareUri(string strConferenceShareUri)
        {
            // sip:user.sfb@lync.nextlabs.solutions;gruu;opaque=app:conf:applicationsharing:id:MZVS9M47
            // sip:user.sfb@lync.nextlabs.solutions;gruu;opaque=app:conf:focus:id:10G857WW
            return strConferenceShareUri.Replace("opaque=app:conf:applicationsharing", "opaque=app:conf:focus");
        }
        static public string GetConferenceIDFromUri(string strUri)
        {
            string strID = "";
            int nPos = strUri.LastIndexOf("id:", StringComparison.OrdinalIgnoreCase);
            if ((nPos > 0) && (strUri.Length > nPos + 3))
            {
                if (strID.EndsWith(">"))
                {
                    strID = strUri.Substring(nPos + 3, strUri.Length - nPos - 4);
                }
                else
                {
                    strID = strUri.Substring(nPos + 3);
                }

            }
            return strID;
        }
        static public string GetConfIDFromConfEntry(string strConfEntry)
        {
            if (strConfEntry == null)
                return null;

            int nIndex = strConfEntry.LastIndexOf(':');
            if (nIndex != -1)
            {
                return strConfEntry.Substring(nIndex + 1);
            }

            return null;
        }
        static public string GetMsgDenyBeforeManualClassifyDoneByAction(EMSFB_ACTION emAction)
        {
            switch (emAction)
            {
            case EMSFB_ACTION.emMeetingInvite:
            {
                return SIPComponentConfig.kstrMeetingInviteMsgDenyBeforeManualClassifyDone;
            }
            case EMSFB_ACTION.emMeetingJoin:
            {
                return SIPComponentConfig.kstrMeetingJoinMsgDenyBeforeManualClassifyDone;
            }
            case EMSFB_ACTION.emMeetingShare:
            {
                return SIPComponentConfig.kstrMeetingShareCreateMsgDenyBeforeManualClassifyDone;
            }
            case EMSFB_ACTION.emMeetingShareJoin:
            {
                return SIPComponentConfig.kstrMeetingShareJoinMsgDenyBeforeManualClassifyDone;
            }
            default:
            {
                break;
            }
            }
            return "";
        }
        #endregion

        #region Chat room tools
        static public bool IsPersistentChatEndpoint(string strEndpoint)
        {
            //persistent chat endpoint: sip:GC-1-PersistentChatService-2@lync.nextlabs.solutions
            //here we may make a wrong decision by lookup keyword: "-PersistentChatService-"
            //the right way is get persistent chat endpoint by PS command: Get-CsPersistentChatEndpoint
            return strEndpoint.Contains("-PersistentChatService-"); 
        }
        static public string GetChatRoomIDFromUri(string strRoomUri)
        {
            string strRoomID = strRoomUri;
            int nPos = strRoomUri.LastIndexOf('/');
            if (nPos > 0)
            {
                strRoomID = strRoomUri.Substring(nPos + 1);
            }

            return strRoomID;
        }
        static public string GetChatRoomDomainFromUri(string strRoomUri)
        {
            //ma-chan://lync.nextlabs.solutions/58384137-ac90-43f3-b045-9bc0fc408d2b
            int nPosBegin = strRoomUri.IndexOf("//");
            if (nPosBegin > 0)
            {
                int nPosEnd = strRoomUri.IndexOf("/", nPosBegin + 2);
                if (nPosEnd > 0)
                {
                    return strRoomUri.Substring(nPosBegin + 2, nPosEnd - nPosBegin - 2);
                }
            }

            return "";
        }
        #endregion

        #region Common tools
        static public string GetUserUriFromRequestHeader(Header obSipUserHeader)
        {
            if (null != obSipUserHeader)
            {
                return CSIPTools.GetUserAtHost(obSipUserHeader.Value);
            }
            return "";
        }
        static public Response CreateDenySIPResponse(Request obRequest)
        {
            return obRequest.CreateResponse(600);
        }
        static public string GetUriFromSipAddrHdr(string strHdr)
        {
            if (strHdr == null) 
                return null;

            string uri = strHdr;
            int index1 = strHdr.IndexOf('<');
            if (index1 != -1)
            {
                int index2 = strHdr.IndexOf('>');
                ///address, extract uri
                uri = strHdr.Substring(index1 + 1, index2 - index1 - 1);
                return uri;
            }

            return uri;
        }
        static public string GetUserAtHost(string sipAddressHeader)
        {
            if (sipAddressHeader == null) return null;

            string uri = null;

            /// If the header has < > present, then extract the uri
            /// else treat the input as uri
            int index1 = sipAddressHeader.IndexOf('<');

            if (index1 != -1)
            {
                int index2 = sipAddressHeader.IndexOf('>');
                ///address, extract uri
                uri = sipAddressHeader.Substring(index1 + 1, index2 - index1 - 1);
            }
            else
            {
                uri = sipAddressHeader;
            }

            ///chop off all parameters. we assume that there is no
            ///semicolon in the user part (which is allowed in some cases!)
            index1 = uri.IndexOf(';');
            if (index1 != -1)
            {
                uri = uri.Substring(0, index1);
            }

            ///we will process only SIP uri (thus no sips or tel)
            ///and wont accept those without user names
            if (uri.StartsWith(CSIPTools.SIP_URI_PREFIX, StringComparison.OrdinalIgnoreCase) == false ||
                uri.IndexOf('@') == -1)
                return null;

            ///now we have sip:user@host most likely, with some exceptions that
            /// are ignored
            ///  1) user part contains semicolon separated user parameters
            ///  2) user part also has the password (as in sip:user:pwd@host)
            ///  3) some hex escaped characters are present in user part
            ///  4) the host part also has the port (Contact header for example)

            return uri.Substring(CSIPTools.SIP_URI_PREFIX.Length /* uri.Substring(4) */);
        }
        static public string GetUserParameterFromSIPAddress(string strSipAddr, SIP_ADDRESS_PARAMETER emParameter)
        {
            //check argument
            string strParameterName = emParameter == SIP_ADDRESS_PARAMETER.EPID ? kStrSIPAddressParameterEpid :
                (emParameter == SIP_ADDRESS_PARAMETER.TAG ? kStrSIPAddressParameterTag : "");
            if(string.IsNullOrWhiteSpace(strSipAddr) || string.IsNullOrWhiteSpace(strParameterName))
            {
                return "";
            }

            strParameterName += "=";

            //
            string strParameterValue = "";
            int nPosBegin = strSipAddr.IndexOf(strParameterName, StringComparison.OrdinalIgnoreCase);
            if (nPosBegin > 0)
            {
                int nPosEnd = strSipAddr.IndexOf(kStrSIPAddressParameterSeparator, nPosBegin, StringComparison.OrdinalIgnoreCase);
                if(nPosEnd<0)
                {
                    strParameterValue = strSipAddr.Substring(nPosBegin);
                }
                else
                {
                    strParameterValue = strSipAddr.Substring(nPosBegin, nPosEnd - nPosBegin);
                }
            }

            return strParameterValue;
        }
        #endregion
    }
}
