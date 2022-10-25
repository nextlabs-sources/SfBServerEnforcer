using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaintainTool
{
    enum EMSFB_MAINTAINTYPE
    {
        emMaintainType_unknown,

        emMaintainType_establishUserInfoMapping,
        
        emMaintainType_neatenMeetingInfo,   // include SFB/NL meeting info
        emMaintainType_neatenChatRoomInfo,  // include SFB/NL chat room info and SFB/NL chat category info
    }

    class CmdInfoWihtEstablishUserInfoMapping
    {
        #region Const values
        public const string kstrCmdParamTypeVlaue = "SetupUserMapping";
        public const string kstrCmdParamDisplayNameFlag = "-user";
        
        public const string kstrCmdParamHelpInfoPromptHeader = "Configure the mapping of user SIP account and user full name";
        public const string kstrCmdParamHelpInfoFullCommandHeader = "SFBEADMIN [-type SetupUserMapping] [-user DisplayName]";
        public const string kstrCmdParamHelpInfoTypeCommandHeader = "-type SetupUserMapping";
        public const string kstrCmdParamHelpInfoTypeCommandBody = "Sync user SIP accounts from Skype for Business server to NextLabs Enforcer database. Only SIP login account name and user full name are stored in our database, which are used for security policy enforcement.";
        public const string kstrCmdParamHelpInfoUserCommandHeader = "-user DisplayName";
        public const string kstrCmdParamHelpInfoUserCommandBody = "Example, SFBEADMIN –user “Carol Welch”. It queries SIP login account name of “Carol Welch” and syncs into our NextLabs Enforcer database.";
        #endregion

        #region Fields
        public string DisplayName { get { return m_strDisplayName; } }
        #endregion

        #region members
        private string m_strDisplayName = null;
        #endregion

        public CmdInfoWihtEstablishUserInfoMapping(string[] szCmdAgrs)
        {
            // Maintain Establish user info persistent mapping
            // Command: -type EstablishUserInfoMapping [-DisplayName "Kim yang"]

            m_strDisplayName = null;
            if ((null != szCmdAgrs) && (0 == (szCmdAgrs.Length%2)))
            {
                for (int i=1; i<szCmdAgrs.Length; i+=2)
                {
                    if (kstrCmdParamDisplayNameFlag.Equals(szCmdAgrs[i-1], StringComparison.OrdinalIgnoreCase))
                    {
                        m_strDisplayName = szCmdAgrs[i];
                        break;
                    }
                }
            }
        }
        public CmdInfoWihtEstablishUserInfoMapping(string strParamDisplayName)
        {
            m_strDisplayName = strParamDisplayName;
        }
        public CmdInfoWihtEstablishUserInfoMapping(CmdInfoWihtEstablishUserInfoMapping obCmdInfoWihtEstablishUserInfoMapping)
        {
            m_strDisplayName = obCmdInfoWihtEstablishUserInfoMapping.m_strDisplayName;
        }
    }
}
