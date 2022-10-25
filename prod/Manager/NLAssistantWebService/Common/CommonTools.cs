using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SFBCommon.Common;
using SFBCommon.NLLog;
using SFBCommon.SFBObjectInfo;
using Nextlabs.SFBServerEnforcer.PolicyHelper;

namespace NLAssistantWebService.Common
{
    static class CommonTools
    {
        public static bool IsSFBMeetingAction(EMSFB_ACTION emActionIn)
        {
            return (EMSFB_ACTION.emMeetingCreate == emActionIn) ||
                (EMSFB_ACTION.emMeetingInvite == emActionIn) ||
                (EMSFB_ACTION.emMeetingJoin == emActionIn) ||
                (EMSFB_ACTION.emMeetingShare == emActionIn) ||
                (EMSFB_ACTION.emMeetingShareJoin == emActionIn);
        }

        public static string GetFuzzyUriByEntryInfo(string strEntryInfo)
        {
            string uri = "";

            if (string.IsNullOrEmpty(strEntryInfo))
            {
                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n GetFuzzyUriByEntryInfo failed , entryInfo is null \n");
            }
            else
            {
                MeetingEntryInfo obEntryInfo = new MeetingEntryInfo(strEntryInfo);
                uri = obEntryInfo.GetMeetingLikeUri();               
            }

            return uri;
        }

        public static string GetCreatorFromSipUri(string sipUri)
        {
            string creatorStr = "";

            if (!string.IsNullOrEmpty(sipUri))
            {
                string headFlag = "sip:";
                creatorStr = sipUri.Substring(headFlag.Length);
            }
            else
            {
                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n GetCreatorFromSipUri failed , sipUri is null \n");
            }

            return creatorStr;
        }

        public static SFBMeetingInfo GetSFBMeetingObjectByEntryInfo(string strEntryInfo)
        {
            SFBMeetingInfo obSFBMeetingInfoRet = null;
            string strFuzzyUri = GetFuzzyUriByEntryInfo(strEntryInfo);
            if (!string.IsNullOrEmpty(strFuzzyUri))
            {
                List<SFBObjectInfo> lsResultSFBObject = SFBObjectInfo.GetObjsFromPersistentInfoByLikeValue(EMSFB_INFOTYPE.emInfoSFBMeeting, SFBMeetingInfo.kstrUriFieldName, strFuzzyUri);
                if (1 == lsResultSFBObject.Count)
                {
                    obSFBMeetingInfoRet = lsResultSFBObject[0] as SFBMeetingInfo;
                }
                else
                {
                    XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "\n GetObjsFromPersistentInfoByLikeValue() failed , result.Count != 1 \n");
                }
            }
            else
            {
                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "\n GetFuzzyUriByEntryInfo failed , entryInfo : {0} \n", strEntryInfo);
            }
            return obSFBMeetingInfoRet;
        }
        public static string GetMeetingSipUriStrByEntryInfo(string strEntryInfo)
        {
            string strSipUrl = "";
            SFBMeetingInfo obSFBMeetingInfo = GetSFBMeetingObjectByEntryInfo(strEntryInfo);
            if (null != obSFBMeetingInfo)
            {
                strSipUrl = obSFBMeetingInfo.GetItemValue(SFBMeetingInfo.kstrUriFieldName);
            }
            else
            {
                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "\n Get the SFBMeetingObject by entry info:{0} failed \n", strEntryInfo);
            }
            return strSipUrl;
        }
    }
}