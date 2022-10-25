using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDKWrapperLib;
using SFBCommon.SFBObjectInfo;
using SFBCommon.Common;

namespace Nextlabs.SFBServerEnforcer.PolicyHelper
{
    static class PolicyTools
    {
        public static void SeprateAttribute(string strValues, string strSeprator, string strKey, CEAttres theSrcAttres)
        {
            string[] arrayStrSep = new string[1];
            arrayStrSep[0] = strSeprator;

            string[] arrayValues = strValues.Split(arrayStrSep, StringSplitOptions.RemoveEmptyEntries);
            if ((arrayValues != null))
            {
                foreach (string strV in arrayValues)
                {
                    theSrcAttres.add_attre(strKey, strV);
                }
            }
        }
        public static bool IgnoreAttribute(SFBObjectInfo obj, string strAttrName)
        {
            bool bIgnore = false;
            switch (obj.GetSFBInfoType())
            {
            case EMSFB_INFOTYPE.emInfoSFBChatRoomVariable:
            bIgnore = strAttrName.Equals(SFBChatRoomVariableInfo.kstrParticipatesFieldName);
            break;

            case EMSFB_INFOTYPE.emInfoSFBMeetingVariable:
            bIgnore = strAttrName.Equals(SFBMeetingVariableInfo.kstrParticipatesFieldName);
            break;

            default:
            bIgnore = false;
            break;
            }

            return bIgnore;
        }
        public static bool NeedSeprateAttribute(SFBObjectInfo obj, string strAttrName)
        {
            bool bNeedSeprate = false;
            switch (obj.GetSFBInfoType())
            {
            case EMSFB_INFOTYPE.emInfoSFBChatRoom:
            bNeedSeprate = strAttrName.Equals(SFBChatRoomInfo.kstrManagersFieldName) ||
                           strAttrName.Equals(SFBChatRoomInfo.kstrMembersFieldName);
            break;

            default:
            bNeedSeprate = false;
            break;
            }

            return bNeedSeprate;
        }
    }
}
