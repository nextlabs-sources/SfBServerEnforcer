using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SFBCommon.NLLog;
using SFBCommon.Common;
using SFBCommon.SFBObjectInfo;

namespace NLAssistantWebService.Tokens
{
    public class TokenManager
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(TokenManager));
        #endregion

        #region Members
        // Cannot use simple memory to cache the token, bug 40106
        // If do not consider load balance which install the service in different machine, we can consider to use global shared memory to cache the token.
        // private Dictionary<string, ClassifyToken> m_dicClassifyTokens = new Dictionary<string, ClassifyToken>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Constructors
        public TokenManager()
        {

        }
        #endregion

        #region Public functions
        public ClassifyToken GetNewUserToken(string strUserSipUri, EMSFB_TOKENTYPE emTokenType = EMSFB_TOKENTYPE.enTokenType_ClassifyToken)
        {
            ClassifyToken obClassifyToken = null;
            if (!string.IsNullOrEmpty(strUserSipUri))
            {
                strUserSipUri = strUserSipUri.ToLower();
                obClassifyToken = InnerCreateNewToken(strUserSipUri);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "GetNewUserToken, user:[{0}], tokenID:[{1}], type:[{2}]\n", strUserSipUri, obClassifyToken.TokenID, obClassifyToken.TokenType);
            }
            return obClassifyToken;
        }
        public bool CheckUserToken(string strUserSipUri, string strTokenID, bool bRefresh, EMSFB_TOKENTYPE emTokenType = EMSFB_TOKENTYPE.enTokenType_ClassifyToken)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "CheckUserToken, user:[{0}], tokenID:[{1}], refresh:[{2}], type:[{3}]\n", strUserSipUri, strTokenID, bRefresh, emTokenType);
            if ((!string.IsNullOrEmpty(strUserSipUri)) && (!string.IsNullOrEmpty(strTokenID)))
            {
                strUserSipUri = strUserSipUri.ToLower();
                ClassifyToken obClassifyToken = GetAndCheckUserToken(strUserSipUri, strTokenID, emTokenType);
                if ((null != obClassifyToken))
                {
                    if (bRefresh)
                    {
                        InnerRefreshToken(strUserSipUri, ref obClassifyToken);
                    }
                    return obClassifyToken.IsValidToken();
                }
            }
            return false;
        }
        public bool RefreshUserToken(string strUserSipUri, string strTokenID, EMSFB_TOKENTYPE emTokenType = EMSFB_TOKENTYPE.enTokenType_ClassifyToken)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "RefreshUserToken, user:[{0}], tokenID:[{1}], type:[{2}]\n", strUserSipUri, strTokenID, emTokenType);
            if ((!string.IsNullOrEmpty(strUserSipUri)) && (!string.IsNullOrEmpty(strTokenID)))
            {
                strUserSipUri = strUserSipUri.ToLower();
                ClassifyToken obClassifyToken = GetAndCheckUserToken(strUserSipUri, strTokenID, emTokenType);
                if (null != obClassifyToken)
                {
                    return InnerRefreshToken(strUserSipUri, ref obClassifyToken);
                }
            }
            return false;
        }
        public void DeleteUserToken(string strUserSipUri)
        {
            if (!string.IsNullOrEmpty(strUserSipUri))
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "DeleteUserToken, user:[{0}]\n", strUserSipUri);
                strUserSipUri = strUserSipUri.ToLower();
                InnerDeleteUserToken(strUserSipUri);
            }
        }
        #endregion

        #region Inner tools, we need update the cache info to database when we create, refresh, delete the token
        private ClassifyToken GetUserToken(string strUserSipUri, bool bCreateNEW, EMSFB_TOKENTYPE emTokenType = EMSFB_TOKENTYPE.enTokenType_ClassifyToken)
        {
            ClassifyToken obClassifyToken = null;
            if (!string.IsNullOrEmpty(strUserSipUri))
            {
                obClassifyToken = InnerGetUserToken(strUserSipUri);
                if ((null == obClassifyToken) || (!obClassifyToken.IsValidToken()))
                {
                    if (bCreateNEW)
                    {
                        obClassifyToken = InnerCreateNewToken(strUserSipUri);
                    }
                    else
                    {
                        obClassifyToken = null;
                    }
                }
                else
                {
                    if (bCreateNEW)
                    {
                        InnerRefreshToken(strUserSipUri, ref obClassifyToken);
                    }
                }
            }
            return obClassifyToken;
        }
        private ClassifyToken GetAndCheckUserToken(string strUserSipUri, string strTokenID, EMSFB_TOKENTYPE emTokenType = EMSFB_TOKENTYPE.enTokenType_ClassifyToken)
        {
            ClassifyToken obClassifyToken = GetUserToken(strUserSipUri, false, emTokenType);
            if ((null != obClassifyToken))
            {
                // ID is correct and token is valid
                if ((obClassifyToken.TokenID.Equals(strTokenID)) && (obClassifyToken.IsValidToken()))
                {
                    return obClassifyToken;
                }
            }
            return null;

        }

        private ClassifyToken InnerGetUserToken(string strUserSipUri)
        {
            return GetClassifyTokenFromPersistentInfo(strUserSipUri);
        }
        private ClassifyToken InnerCreateNewToken(string strUserSipUri)
        {
            ClassifyToken obClassifyToken = null;
            if (!string.IsNullOrEmpty(strUserSipUri))
            {
                obClassifyToken = new ClassifyToken();
                UpdatePersistentTokenInfo(strUserSipUri, obClassifyToken);
            }
            return obClassifyToken;
        }
        private bool InnerRefreshToken(string strUserSipUri, ref ClassifyToken obClassifyToken)
        {
            bool bRet = false;
            if ((!string.IsNullOrEmpty(strUserSipUri)) && (null != obClassifyToken))
            {
                if (obClassifyToken.RefreshToken())
                {
                    bRet = UpdatePersistentTokenInfo(strUserSipUri, obClassifyToken);
                }
            }
            return bRet;
        }
        private void InnerDeleteUserToken(string strUserSipUri)
        {
            if (!string.IsNullOrEmpty(strUserSipUri))
            {
                UpdatePersistentTokenInfo(strUserSipUri, null);
            }
        }
        private ClassifyToken GetClassifyTokenFromPersistentInfo(string strUserSipUri)
        {
            NLAssistantServiceInfo obNLAssistantServiceInfo = new NLAssistantServiceInfo();
            obNLAssistantServiceInfo.EstablishObjFormPersistentInfo(NLAssistantServiceInfo.kstrUriFieldName, strUserSipUri);

            string strStringToken = obNLAssistantServiceInfo.GetItemValue(NLAssistantServiceInfo.kstrAssistantTokenFieldName);
            if (!string.IsNullOrEmpty(strStringToken))
            {
                return new ClassifyToken(strStringToken);
            }
            return null;
        }
        private bool UpdatePersistentTokenInfo(string strUserSipUri, ClassifyToken obClassifyToken)
        {
            bool bRet = false;
            if (!string.IsNullOrEmpty(strUserSipUri))
            {
                string strStringToken = "";
                if (null != obClassifyToken)
                {
                    strStringToken = obClassifyToken.GetStringToken();
                }
                NLAssistantServiceInfo obNLAssistantServiceInfo = new NLAssistantServiceInfo(NLAssistantServiceInfo.kstrUriFieldName, strUserSipUri, NLAssistantServiceInfo.kstrAssistantTokenFieldName, strStringToken);
                bRet = obNLAssistantServiceInfo.PersistantSave();
            }
            return bRet;
        }
        #endregion
    }
}