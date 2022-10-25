using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.NLLog;
using SFBCommon.Common;

namespace SFBCommon.SFBObjectInfo
{
    public class SFBUserInfo : SFBObjectInfo
    {
        #region Const values: field names
        public const string kstrSipAddress = "sipaddress";   // Key
        public const string kstrFullName = "fullname";
        public const string kstrDisplayName = "displayname";
        public const string kstrRegistrarPool = "registrarpool";
        public const string kstrSamAccountName = "samaccountname";
        public const string kstrUserIdentity = "useridentity";
        #endregion

        #region Const values: power shell command
        private const string kstrPSCmdGetCsUser = "Get-CsUser";
        private const string kstrPSCmdParamLdapFilter = "-LdapFilter";
        #endregion

        #region Constructors
        public SFBUserInfo(Microsoft.Rtc.Management.ADConnect.Schema.OCSADUser obOCSADUser)
        {
            InitSFBUserInfoByPSOCSADUser(obOCSADUser);
        }
        public SFBUserInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)   // [fullname, test.sfb@test.com, displayname, test sfb]
        {

        }
        public SFBUserInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public SFBUserInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoSFBUserInfo;
        }
        #endregion

        #region Tools
        private void InitSFBUserInfoByPSOCSADUser(Microsoft.Rtc.Management.ADConnect.Schema.OCSADUser obOCSADUser)
        {
            {
                // Debug
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "OCSADUser Info:\n\tIdentity:[{0}]\n\tHostingProvider:[{1}]\n\tRegistrarPool:[{2}]\n\tSipAddress:[{3}]\n\tLineURI:[{4}]\n\tHomeServer:[{5}]\n\tDisplayName:[{6}]\n\tSamAccountName:[{7}]\n", 
                    obOCSADUser.Identity, obOCSADUser.HostingProvider, obOCSADUser.RegistrarPool, obOCSADUser.SipAddress, obOCSADUser.LineURI, obOCSADUser.HomeServer, obOCSADUser.DisplayName, obOCSADUser.SamAccountName);
            }
            InnerSetItem(kstrSipAddress, CommonHelper.GetUriWithoutSipHeader(CommonHelper.GetObjectStringValue(obOCSADUser.SipAddress)));
            InnerSetItem(kstrFullName, GetUserFullName()); // Empty, this version using sip address as the full name, but this is not real full name in AD, we need connect the SamAccount and domain info as the full name.
            InnerSetItem(kstrDisplayName, CommonHelper.GetObjectStringValue(obOCSADUser.DisplayName));
            InnerSetItem(kstrRegistrarPool, CommonHelper.GetObjectStringValue(obOCSADUser.RegistrarPool));
            InnerSetItem(kstrSamAccountName, CommonHelper.GetObjectStringValue(obOCSADUser.SamAccountName));
            InnerSetItem(kstrUserIdentity, CommonHelper.GetObjectStringValue(obOCSADUser.Identity));            
        }
        #endregion

        #region Inner tools
        private string GetUserFullName()
        {
            string strSipAddress = GetItemValue(kstrSipAddress);
            if(string.IsNullOrWhiteSpace(strSipAddress))
            {
                strSipAddress = "";
            }
            return strSipAddress;
        }
        #endregion

        #region Powershell functions
        static public List<SFBUserInfo> GetAllCurSFBUserInfo(string strDisplayName)
        {
            List<SFBUserInfo> lsSFBUserInfo = null;
            Collection<PSObject> collectionPSSFBUserInfos = null;
            if (string.IsNullOrEmpty(strDisplayName))
            {
                collectionPSSFBUserInfos = PowerShellHelper.RunPowershell(kstrLyncPSModuleName, kstrPSCmdGetCsUser);
            }
            else
            {
                collectionPSSFBUserInfos = PowerShellHelper.RunPowershell(kstrLyncPSModuleName, kstrPSCmdGetCsUser, kstrPSCmdParamLdapFilter, "DisplayName=" + strDisplayName);
            }
            if (null != collectionPSSFBUserInfos)
            {
                lsSFBUserInfo = new List<SFBUserInfo>();
                foreach (PSObject obPSObject in collectionPSSFBUserInfos)
                {
                    Microsoft.Rtc.Management.ADConnect.Schema.OCSADUser obOCSADUser = obPSObject.BaseObject as Microsoft.Rtc.Management.ADConnect.Schema.OCSADUser;
                    if (null != obOCSADUser)
                    {
                        lsSFBUserInfo.Add(new SFBUserInfo(obOCSADUser));
                    }
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "failed to invoke:[{0}] with paramters:[{1}]\n", kstrPSCmdGetCsUser, strDisplayName);
            }
            return lsSFBUserInfo;
        }
        #endregion
    }
}
