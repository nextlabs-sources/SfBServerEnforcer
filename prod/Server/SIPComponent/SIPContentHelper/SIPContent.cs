using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.NLLog;
using SFBCommon.Common;
using System.Text.RegularExpressions;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.SIPContentHelper
{
    public enum EMSIP_CONTENT_TYPE
    {
        emContent_Unknown,

        emContent_Application_Sdp,
        emContent_Text_Plain,
        emContent_Application_Ccp_Xml
    }

    abstract class SIPContent
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(SIPContent));
        #endregion

        #region Static values
        static private readonly string m_kstrContentFlag_MultipartAlternative = "multipart/alternative";
        static private readonly string m_kstrContentFlag_MultipartAlternative_WithEndFlag = "multipart/alternative;";
        static private readonly string m_kstrContentFlag_ContentType = "Content-Type";
        static private readonly Dictionary<EMSIP_CONTENT_TYPE, string> s_kdicContentTypeAndFlag = new Dictionary<EMSIP_CONTENT_TYPE,string>()
        {
            {EMSIP_CONTENT_TYPE.emContent_Application_Sdp, "application/sdp"},
            {EMSIP_CONTENT_TYPE.emContent_Text_Plain, "text/plain"},
            {EMSIP_CONTENT_TYPE.emContent_Application_Ccp_Xml, "application/cccp+xml"}
        };
        static private readonly Dictionary<string, EMSIP_CONTENT_TYPE> s_kdicContentFlagAndType = new Dictionary<string,EMSIP_CONTENT_TYPE>()
        {
            {"application/sdp", EMSIP_CONTENT_TYPE.emContent_Application_Sdp},
            {"text/plain", EMSIP_CONTENT_TYPE.emContent_Text_Plain},
            {"application/cccp+xml", EMSIP_CONTENT_TYPE.emContent_Application_Ccp_Xml}
        };
        #endregion

        #region Static functions
        static public string GetStandardContentTypeFlag(string strContentFlag, string strContentInfo)
        {
            string strStandardContentTypeFlag = strContentFlag;
            if (string.IsNullOrEmpty(strContentFlag) || 
                strContentFlag.Equals(m_kstrContentFlag_MultipartAlternative, StringComparison.OrdinalIgnoreCase) ||
                strContentFlag.StartsWith(m_kstrContentFlag_MultipartAlternative_WithEndFlag, StringComparison.OrdinalIgnoreCase))
            {
                // Get content type flag from content info
                // For SFB 2019, meeting share create and join event, the request content flag will be multipart/alternative
                // For this content flag, we need check the content info to get content-type, not just use the content flag
                if (String.IsNullOrEmpty(strContentInfo))
                {
                    // Ignore
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Try to get standard content by flag:[{0}] with content info:[{1}], but the content info is empty\n", strStandardContentTypeFlag, strContentInfo);                    
                }
                else
                {
                    string strContentTypeFlagFromContent = InnerGetFirstContentValueByKey(strContentInfo, m_kstrContentFlag_ContentType, ":");
                    if (!String.IsNullOrEmpty(strContentTypeFlagFromContent))
                    {
                        strStandardContentTypeFlag = strContentTypeFlagFromContent;
                    }
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Get standard content type flag:[{0}](out content type flag:[{1}]) from content info:[{2}]\n", strContentTypeFlagFromContent, strStandardContentTypeFlag, strContentInfo);                    
                }
            }
            return strStandardContentTypeFlag;
        }
        static public EMSIP_CONTENT_TYPE GetContentTypeByFlag(string strContentFlag)
        {
            return CommonHelper.GetValueByKeyFromDir(s_kdicContentFlagAndType, strContentFlag.ToLower(), EMSIP_CONTENT_TYPE.emContent_Unknown);
        }
        static public string GetContentFlagByType(EMSIP_CONTENT_TYPE emContentType)
        {
            return CommonHelper.GetValueByKeyFromDir(s_kdicContentTypeAndFlag, emContentType, "");;
        }
        static public SIPContent CreateSIPContentObjByFlag(string strContentFlag, string strContentInfo)
        {
            strContentFlag = GetStandardContentTypeFlag(strContentFlag, strContentInfo);
            EMSIP_CONTENT_TYPE emContentType = GetContentTypeByFlag(strContentFlag);
            return CreateSIPContentObjByType(emContentType, strContentInfo);
        }
        static public string InnerGetFirstContentValueByKey(string strContentInfo, string strContentKey, string strSep)
        {
            string strContentValue = null;

            const string kstrGroupNameContentValue = "GroupNameContentValue";
            string strRegPattern = CommonHelper.MakeAsStandardRegularPattern("^" + strContentKey + strSep + "(?<" + kstrGroupNameContentValue + ">(.+))$"); // ==> "^a=connection:(?<name>(.+))$"
            Regex regex = new Regex(strRegPattern, RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(strContentInfo);
            if ((null != matches) && (0 < matches.Count))
            {
                Match obMatch = matches[0];
                strContentValue = obMatch.Groups[kstrGroupNameContentValue].Value.Trim();
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Cannot match conten:[{0}] with pattern:[{1}]. (Key:[{2}], Sep:[3])\n", strContentInfo, strRegPattern, strContentKey, strSep);
            }
            return strContentValue;
        }
        static public SIPContent CreateSIPContentObjByType(EMSIP_CONTENT_TYPE emContentType, string strContentInfo)
        {
            SIPContent obSIPContent = null;
            switch (emContentType)
            {
            case EMSIP_CONTENT_TYPE.emContent_Application_Sdp:
            {
                obSIPContent = new SIPContentApplicationSdp(emContentType, strContentInfo);
                break;
            }
            case EMSIP_CONTENT_TYPE.emContent_Text_Plain:
            {
                obSIPContent = new SIPContentTextPlain(emContentType, strContentInfo);
                break;
            }
            case EMSIP_CONTENT_TYPE.emContent_Application_Ccp_Xml:
            {
                obSIPContent = new SIPContentApplicationCcpXml(emContentType, strContentInfo);
                break;
            }
            default:
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "SIPContent type:[{0}] doesn't spport\n", emContentType);
                break;
            }
            }
            return obSIPContent;
        }
        
        #endregion

        #region Members, private
        private readonly EMSIP_CONTENT_TYPE m_emContentType = EMSIP_CONTENT_TYPE.emContent_Unknown;
        private readonly string m_strContentFlag = "";
        #endregion

        #region Members, protected
        protected string m_strContentInfo = "";
        #endregion

        #region Constructor
        public SIPContent(string strContentFlag, string strContentInfo)
        {
            if (!string.IsNullOrEmpty(strContentFlag))
            {
                m_strContentInfo = CommonHelper.GetSolidString(strContentInfo);
                m_strContentFlag = strContentFlag = GetStandardContentTypeFlag(strContentFlag, strContentInfo);
                m_emContentType = GetContentTypeByFlag(m_strContentFlag);
                
            }
        }
        public SIPContent(EMSIP_CONTENT_TYPE emContentType, string strContentInfo)
        {
            if (EMSIP_CONTENT_TYPE.emContent_Unknown != emContentType)
            {
                m_strContentInfo = CommonHelper.GetSolidString(strContentInfo);
                m_emContentType = emContentType;
                m_strContentFlag = GetContentFlagByType(m_emContentType);
            }
        }
        #endregion

        #region Abstract

        #endregion

        #region Public functions
        public EMSIP_CONTENT_TYPE GetContentType()
        {
            return m_emContentType;
        }
        public string GetContentFlag()
        {
            return m_strContentFlag;
        }
        public string GetContentInfo()
        {
            return m_strContentInfo;
        }
        #endregion
    }
}
