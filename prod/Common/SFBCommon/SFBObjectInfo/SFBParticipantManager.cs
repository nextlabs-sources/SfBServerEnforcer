using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFBCommon.SFBObjectInfo
{
   public class SFBParticipantManager
    {
        #region const string 
        public static readonly string kstrParticipantParameterSeparator = ",";
        public static readonly string kstrParticipantSeparator = ";";
        #endregion


        #region public chat room participants interfact

        //added participant with parameter
        static public void AddedParticipantWithParameter(SFBChatRoomVariableInfo roomVar, string strUserName, string strEpid, string strTag)
        {
            string strParticipants = roomVar.GetItemValue(SFBChatRoomVariableInfo.kstrParticipatesFieldName);
            AddedParticipantWithParameter(ref strParticipants, strUserName, strEpid, strTag);
            roomVar.SetItem(SFBChatRoomVariableInfo.kstrParticipatesFieldName, strParticipants);
        }

        //remove participant with parameter
        static public void RemoveParticipantWithParameter(SFBChatRoomVariableInfo roomVar, string strUserName, string strEpid, string strTag)
        {
            string strParticipants = roomVar.GetItemValue(SFBChatRoomVariableInfo.kstrParticipatesFieldName);
            RemoveParticipantWithParameter(ref strParticipants, strUserName, strEpid, strTag);
            roomVar.SetItem(SFBChatRoomVariableInfo.kstrParticipatesFieldName, strParticipants);
        }

        //remove participant without parameter
        static public void RemoveParticipant(SFBChatRoomVariableInfo roomVar, string strUserName)
        {
            string strParticipants = roomVar.GetItemValue(SFBChatRoomVariableInfo.kstrParticipatesFieldName);
            RemoveParticipant(ref strParticipants, strUserName);
            roomVar.SetItem(SFBChatRoomVariableInfo.kstrParticipatesFieldName, strParticipants);
        }

        //get participant without parameter
        static public List<string> GetDistinctParticipantsAsList(SFBChatRoomVariableInfo roomVar)
        {
            //get all participants
            string strParticipants = roomVar.GetItemValue(SFBChatRoomVariableInfo.kstrParticipatesFieldName);
            return GetDistinctParticipantsAsList(strParticipants);
        }
        #endregion 

        #region public meeting participants interfact

        static public void AddedParticipantWithParameter(SFBMeetingVariableInfo meetingVar, string strUserName, string strEpid, string strTag)
        {
            string strParticipants = meetingVar.GetItemValue(SFBChatRoomVariableInfo.kstrParticipatesFieldName);
            AddedParticipantWithParameter(ref strParticipants, strUserName, strEpid, strTag);
            meetingVar.SetItem(SFBChatRoomVariableInfo.kstrParticipatesFieldName, strParticipants);
        }

        //remove participant with parameter
        static public void RemoveParticipantWithParameter(SFBMeetingVariableInfo meetingVar, string strUserName, string strEpid, string strTag)
        {
            string strParticipants = meetingVar.GetItemValue(SFBChatRoomVariableInfo.kstrParticipatesFieldName);
            RemoveParticipantWithParameter(ref strParticipants, strUserName, strEpid, strTag);
            meetingVar.SetItem(SFBChatRoomVariableInfo.kstrParticipatesFieldName, strParticipants);
        }

        //remove participant without parameter
        static public void RemoveParticipant(SFBMeetingVariableInfo meetingVar, string strUserName)
        {
            string strParticipants = meetingVar.GetItemValue(SFBChatRoomVariableInfo.kstrParticipatesFieldName);
            RemoveParticipant(ref strParticipants, strUserName);
            meetingVar.SetItem(SFBChatRoomVariableInfo.kstrParticipatesFieldName, strParticipants);
        }

        //get participant without parameter
        static public List<string> GetDistinctParticipantsAsList(SFBMeetingVariableInfo meetingVar)
        {
            //get all participants
            string strParticipants = meetingVar.GetItemValue(SFBChatRoomVariableInfo.kstrParticipatesFieldName);
            return GetDistinctParticipantsAsList(strParticipants);
        }

        #endregion

      
        #region private tool
        static private void AddedParticipantWithParameter(ref string refStrParticipants, string strUserName, string strEpid, string strTag)
        {
            //added
            string strParticipant = ConstructParticipantItem(strUserName, strEpid, strTag); 
            if (string.IsNullOrWhiteSpace(refStrParticipants))
            {
                refStrParticipants = strParticipant;
            }
            else if (!ParticipantExist(refStrParticipants, strParticipant))
            {
                refStrParticipants += strParticipant;
            }
        }

        static private void RemoveParticipantWithParameter(ref string refStrParticipants, string strUserName, string strEpid, string strTag)
        {
            if(!string.IsNullOrWhiteSpace(refStrParticipants))
            {
                string strParticipant = ConstructParticipantItem(strUserName, strEpid, strTag);
                while(true)//may be multiple participant need to be remove
                {
                    int nPos = refStrParticipants.IndexOf(strParticipant);
                    if (nPos >= 0)
                    {
                        refStrParticipants = refStrParticipants.Remove(nPos, strParticipant.Length);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        static private void RemoveParticipant(ref string refStrParticipants, string strUserName)
        {
            if (!string.IsNullOrWhiteSpace(refStrParticipants))
            {
                int nFindIndex = 0;
                while (nFindIndex<refStrParticipants.Length)//may be multiple participant need to be remove
                {
                    int nPosBegin = refStrParticipants.IndexOf(strUserName, nFindIndex);
                    if (nPosBegin >= 0)
                    {
                        int nPosEnd = refStrParticipants.IndexOf(kstrParticipantSeparator, nPosBegin);
                        if(nPosEnd<=0)
                        {
                            refStrParticipants = refStrParticipants.Remove(nPosBegin);
                            break;
                        }
                        else
                        {
                            refStrParticipants = refStrParticipants.Remove(nPosBegin, nPosEnd - nPosBegin + 1);
                            nFindIndex = nPosEnd + 1;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        static private string ConstructParticipantItem(string strUserName, string strEpid, string strTag)
        {
            string strParticipant = strUserName;
            strParticipant += kstrParticipantParameterSeparator;
            strParticipant += strEpid;
            strParticipant += kstrParticipantParameterSeparator;
            strParticipant += strTag;
            strParticipant += kstrParticipantSeparator;
            return strParticipant;
        }

        static private string ExtractUserNameFromParticipantItem(string strParticipantItem)
        {
            int nPos = strParticipantItem.IndexOf(kstrParticipantParameterSeparator);
            if(nPos>0)
            {
                return strParticipantItem.Substring(0, nPos);
            }
            else
            {
                return strParticipantItem;
            }
        }

        static private bool ParticipantExist(string strParticipants, string strParticipant)
        {
            if (string.IsNullOrWhiteSpace(strParticipants))
            {
                return false;
            }
            else
            {
                return strParticipants.IndexOf(strParticipant, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

       static private List<string> ConvertParticipantsFromStringToList(string strParticipants)
        {
            if (!string.IsNullOrWhiteSpace(strParticipants))
            {
                string[] arrayStrSep = new string[1];
                arrayStrSep[0] = kstrParticipantSeparator;

                string[] arrayValues = strParticipants.Split(arrayStrSep, StringSplitOptions.RemoveEmptyEntries);
                if ((arrayValues != null))
                {
                    List<string> lstParticipants = new List<string>(arrayValues.Length + 2);
                    foreach (string strV in arrayValues)
                    {
                        lstParticipants.Add(strV);
                    }
                    return lstParticipants;
                }
            }

            return null;
        }

        static private List<string> GetDistinctParticipantsAsList(string strParticipants)
       {
           List<string> lstFullParticipants = ConvertParticipantsFromStringToList(strParticipants);
            if(null!=lstFullParticipants)
            {
                List<string> lstDistinctParticipants = new List<string>();
                foreach(string strFullParticipant in lstFullParticipants)
                {
                    string strUserName = ExtractUserNameFromParticipantItem(strFullParticipant);
                    if(!SFBCommon.Common.CommonHelper.ListStringContains(lstDistinctParticipants, strUserName))
                    {
                        lstDistinctParticipants.Add(strUserName);
                    }
                }
                return lstDistinctParticipants;
            }
            return new List<string>(); ;
       }

        #endregion





    }
}
