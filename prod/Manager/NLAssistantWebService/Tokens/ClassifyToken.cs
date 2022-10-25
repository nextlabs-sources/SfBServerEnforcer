using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SFBCommon.NLLog;
using SFBCommon.Common;

namespace NLAssistantWebService.Tokens
{
    public enum EMSFB_TOKENTYPE
    {
        emTokenType_unknown,

        enTokenType_ClassifyToken
    }

    public class ClassifyToken
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(ClassifyToken));
        #endregion

        #region Const/Read only values
        private const string kstrSepTokenMembers = "--";
        private const uint kunDefaultExpireTime = 10 * 60;
        private static readonly uint kunExpireTime = kunDefaultExpireTime;   // (s)
        #endregion

        #region Static constructor
        static ClassifyToken()
        {
            try
            {
                string strExpireTime = NLConfigurationHelper.s_obConfigInfo.GetRuntimeConfigInfoByKeyFlag(ConfigureFileManager.kstrXMLNLAssistantClassifyTokenExpiryTimeFlag, "600");
                if (!UInt32.TryParse(strExpireTime, out kunExpireTime))
                {
                    kunExpireTime = kunDefaultExpireTime;
                }
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Token expire time:[{0}]\n", kunExpireTime);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in ClassifyToken static construct, [{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Fields
        public string TokenID 
        { 
            get
            {
                // KimNeedUpdate: base64编码并加密
                return m_strTokenID; 
            } 
        }
        public EMSFB_TOKENTYPE TokenType { get { return m_emTokenType; } }
        #endregion

        #region Members
        private string m_strTokenID = "";
        private DateTime m_dtStartTime = new DateTime(0);
        private DateTime m_dtLastModifyTime = new DateTime(0);
        private uint m_unExpiryTime = kunExpireTime;
        private EMSFB_TOKENTYPE m_emTokenType = EMSFB_TOKENTYPE.enTokenType_ClassifyToken;
        #endregion

        #region Constructors
        public ClassifyToken()
        {
            InitToken();
        }
        public ClassifyToken(string strStringToken)
        {
            InitTokenWithStringToken(strStringToken);
        }
        #endregion

        #region Public functions
        public bool IsValidToken()
        {
            if (!string.IsNullOrEmpty(m_strTokenID))
            {
                DateTime dtExpiryTime = m_dtLastModifyTime.AddSeconds(m_unExpiryTime); // Get the expire time
                return (DateTime.Compare(dtExpiryTime, DateTime.UtcNow) > 0);
            }
            return false;
        }
        public bool RefreshToken()
        {
            bool bRet = false;
            if (IsValidToken())
            {
                m_dtLastModifyTime = DateTime.UtcNow;
                bRet = true;
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Refresh token failed, because current token is invalid\n");
            }
            return bRet;
        }
        public string GetStringToken()
        {
            return m_strTokenID + kstrSepTokenMembers + 
                   m_dtStartTime.Ticks.ToString() + kstrSepTokenMembers +
                   m_dtLastModifyTime.Ticks.ToString() + kstrSepTokenMembers +
                   m_unExpiryTime.ToString() + kstrSepTokenMembers +
                   m_emTokenType.ToString();
        }
        #endregion

        #region Private tools
        private void InitToken()
        {
            try
            {
                m_strTokenID = Guid.NewGuid().ToString();
                m_dtLastModifyTime = m_dtStartTime = DateTime.UtcNow;
                m_unExpiryTime = kunExpireTime;
                m_emTokenType = EMSFB_TOKENTYPE.enTokenType_ClassifyToken;
            }
            catch (Exception ex)
            {
                SetDefaultTokenValues();
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in InitToken, [{0}]\n", ex.Message);
            }
        }
        private bool InitTokenWithStringToken(string strStringToken)
        {
            bool bRet = false;
            try
            {
                if (!string.IsNullOrEmpty(strStringToken))
                {
                    string[] szTokenItems = strStringToken.Split(new string[] { kstrSepTokenMembers }, StringSplitOptions.RemoveEmptyEntries);
                    if (5 == szTokenItems.Length)
                    {
                        m_strTokenID = szTokenItems[0];
                        m_dtStartTime = new DateTime(long.Parse(szTokenItems[1]));
                        m_dtLastModifyTime = new DateTime(long.Parse(szTokenItems[2]));
                        m_unExpiryTime = uint.Parse(szTokenItems[3]);
                        m_emTokenType = (EMSFB_TOKENTYPE)Enum.Parse(typeof(EMSFB_TOKENTYPE), szTokenItems[4], true);
                        bRet = true;
                    }
                }
            }
            catch (Exception ex)
            {
                SetDefaultTokenValues();
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in InitTokenWithStringToken, [{0}], [{1}]\n", ex.Message, strStringToken);
            }
            return bRet;
        }
        private void SetDefaultTokenValues()
        {
            m_strTokenID = "";
            m_dtStartTime = new DateTime(0);
            m_dtLastModifyTime = new DateTime(0);
            m_unExpiryTime = kunExpireTime;
            m_emTokenType = EMSFB_TOKENTYPE.enTokenType_ClassifyToken;
        }
        #endregion
    }
}