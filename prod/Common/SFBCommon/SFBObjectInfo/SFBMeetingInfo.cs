using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;

namespace SFBCommon.SFBObjectInfo
{
    public enum EMSFB_MeetingType
    {
        Unknown=0,
        ClientMeeting,
        ScheduleStatic,
        ScheduleDynamic
    };

    public class SFBMeetingInfo : SFBObjectInfo
    {
        #region Static values: field names
        public static readonly string kstrUriFieldName = "uri";
        public static readonly string kstrCreatorFieldName = "creator";
        public static readonly string kstrCreateTimeFieldName = "createtime";
        public static readonly string kstrEntryInformationFieldName = "entryinformation";
        public static readonly string kstrMeetingTypeFieldName = "meetingtype";
        public static readonly string kstrExpiryTimeFieldName = "expirytime";
        #endregion

        #region Constructors
        public SFBMeetingInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public SFBMeetingInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public SFBMeetingInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region public function
        public EMSFB_MeetingType GetSfbMeetingType()
        {
            string strMeetingType = GetItemValue(kstrMeetingTypeFieldName);
            if(string.IsNullOrWhiteSpace(strMeetingType))
            {
                return  EMSFB_MeetingType.Unknown;
            }
            else if(strMeetingType.Equals(EMSFB_MeetingType.Unknown.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return EMSFB_MeetingType.Unknown;
            }
            else if (strMeetingType.Equals(EMSFB_MeetingType.ScheduleStatic.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return EMSFB_MeetingType.ScheduleStatic;
            }
            else if (strMeetingType.Equals(EMSFB_MeetingType.ScheduleDynamic.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return EMSFB_MeetingType.ScheduleDynamic;
            }
            else if (strMeetingType.Equals(EMSFB_MeetingType.ClientMeeting.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return EMSFB_MeetingType.ClientMeeting;
            }
            else
            {
                return EMSFB_MeetingType.Unknown;
            }
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoSFBMeeting;
        }
        #endregion
    }
}