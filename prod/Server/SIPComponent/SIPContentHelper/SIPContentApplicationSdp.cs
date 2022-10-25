using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using SFBCommon.Common;
using SFBCommon.NLLog;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.SIPContentHelper
{
    public enum EMSIP_CONNECTION_TYPE
    {
        emConnection_Unknown,

        emConnection_New,
        emConnection_Existing
    }
    public enum EMSIP_USER_ROLE
    {
        emRole_Unknown,

        emRole_Sharer,
        emRole_Viewer
    }

    class SIPContentApplicationSdp : SIPContent
    {
        #region Const/Read onlye values: flags, all flags using lowercase
        private const string kstrSepFirstKeyAndValue = "=";
        private const string kstrSepSecondKeyAndValue = ":";
        #endregion

        #region Const/Read only values: content key
        public const string kstrContentKeyConnection = "a=connection";
        public const string kstrContentKeyApplicationSharingRole = "a=x-applicationsharing-role";
        #endregion

        #region Const/Read only values: content value
        private static readonly Dictionary<string, EMSIP_CONNECTION_TYPE> s_kdicConnectionFlagAndType = new Dictionary<string, EMSIP_CONNECTION_TYPE>()
        {
            {"new", EMSIP_CONNECTION_TYPE.emConnection_New},
            {"existing", EMSIP_CONNECTION_TYPE.emConnection_Existing}
        };

        private static readonly Dictionary<string, EMSIP_USER_ROLE> s_kdicUserRoleFlagAndType = new Dictionary<string,EMSIP_USER_ROLE>()
        {
            {"sharer", EMSIP_USER_ROLE.emRole_Sharer},
            {"viewer", EMSIP_USER_ROLE.emRole_Viewer}
        };
        #endregion

        #region Static functions
        static public EMSIP_CONNECTION_TYPE GetConnectionType(string strConnectionFlag)
        {
            return CommonHelper.GetValueByKeyFromDir(s_kdicConnectionFlagAndType, strConnectionFlag, EMSIP_CONNECTION_TYPE.emConnection_Unknown);
        }
        static public EMSIP_USER_ROLE GetUserRoleType(string strUserRoleFlag)
        {
            return CommonHelper.GetValueByKeyFromDir(s_kdicUserRoleFlagAndType, strUserRoleFlag, EMSIP_USER_ROLE.emRole_Unknown);
        }
        #endregion

        #region Construnctors
        public SIPContentApplicationSdp(string strContentValue, string strContentInfo) : base(strContentValue, strContentInfo)
        {
        }
        public SIPContentApplicationSdp(EMSIP_CONTENT_TYPE emContentType, string strContentInfo) : base(emContentType, strContentInfo)
        {

        }
        #endregion

        #region Public functions
        public string GetFirstContentValueByKey(string strContentKey)
        {
            string strContentValue = null;
            if (!string.IsNullOrEmpty(strContentKey))
            {
                string strSep = GetSepForContentKey(strContentKey);
                strContentValue = InnerGetFirstContentValueByKey(m_strContentInfo, strContentKey, strSep);
            }
            return strContentValue;
        }
        #endregion

        #region Private tools
        private string GetSepForContentKey(string strContentKey)
        {
            string strOutSep = null;

            strContentKey = strContentKey.ToLower();
            bool bContainsFirstSep = strContentKey.Contains(kstrSepFirstKeyAndValue);
            bool bContainsSecondSep = strContentKey.Contains(kstrSepSecondKeyAndValue);
            if (bContainsFirstSep)
            {
                strOutSep = bContainsSecondSep ? null : kstrSepSecondKeyAndValue;
            }
            else
            {
                strOutSep = bContainsSecondSep ? null : kstrSepFirstKeyAndValue;
            }
            return strOutSep;
        }
        #endregion
    }
}
