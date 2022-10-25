using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.SFBObjectInfo;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.DataManagement
{
    class CEntityVariableInfoManager
    {
        public static SFBMeetingVariableInfo GetMeetingVariableInfoFromDB(string strFocusUrl)
        {
            SFBMeetingVariableInfo meetingVar = new SFBMeetingVariableInfo(SFBMeetingVariableInfo.kstrUriFieldName, strFocusUrl);
            bool bRes = meetingVar.EstablishObjFormPersistentInfo(SFBMeetingVariableInfo.kstrUriFieldName, strFocusUrl);
            if((!bRes) || string.IsNullOrWhiteSpace(meetingVar.GetItemValue(SFBMeetingVariableInfo.kstrUriFieldName)))
            {
                meetingVar.SetItem(SFBMeetingVariableInfo.kstrUriFieldName, strFocusUrl);
            }
            return meetingVar;
        }

        public static SFBChatRoomVariableInfo GetChatroomVariableInfoFromDB(string strRoomUri)
        {
            SFBChatRoomVariableInfo chatroomVar = new SFBChatRoomVariableInfo(SFBChatRoomVariableInfo.kstrUriFieldName, strRoomUri);
            bool bRes = chatroomVar.EstablishObjFormPersistentInfo(SFBChatRoomVariableInfo.kstrUriFieldName, strRoomUri);
            if((!bRes) || string.IsNullOrWhiteSpace(chatroomVar.GetItemValue(SFBChatRoomVariableInfo.kstrUriFieldName)))
            {
                chatroomVar.SetItem(SFBChatRoomVariableInfo.kstrUriFieldName, strRoomUri);
            }
            return chatroomVar;
        }

    }
}
