using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Microsoft.Win32;

using QAToolSFBCommon.NLLog;
using System.Xml;
using System.Text.RegularExpressions;
using System.Web;

/*
 * In this file include common tools and common structure which can be used in the whole SFB project
 */
namespace QAToolSFBCommon.Common
{
    public class MeetingAddrInfo
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(MeetingAddrInfo));
        #endregion

        #region Const/Readonly values
        private const string kstrSepMeetingEntryInfoParts = "/";
        private const string kstrSepMeetingSipUriParts = ";";
        private const string kstrSepMeetingSipUriConfigParts = ":";
        private const string kstrMeetingSipUriConfigPartGruuFixString = "gruu";
        private const string kstrMeetingSipUriConfigPartAppConfigFixString = "opaque=app:conf:focus:id:";
        #endregion

        #region Fields
        public string Creator { get { return m_strMeetingCreator; } }
        public string MeetingID { get { return m_strMeetingID; } }
        #endregion

        #region Members
        private bool m_bAnalysis = false;

        private string m_strMeetingEntryHeader = "";
        private string m_strMeetingEntryServer = "";
        private string m_strMeetingCreator = "";
        private string m_strMeetingID = "";
        // ";gruu;opaque=app:conf:focus:id:"
        private string m_strMeetingSipUriConfigPartFixString = string.Format("{0}{1}{2}{3}", kstrSepMeetingSipUriParts, kstrMeetingSipUriConfigPartGruuFixString, kstrSepMeetingSipUriParts, kstrMeetingSipUriConfigPartAppConfigFixString);
        #endregion

        #region Constructor
        public MeetingAddrInfo(string strMeetingAddressInfo)
        {
            m_strMeetingSipUriConfigPartFixString = EstablishMeetingSipUriConfigPartFixString(kstrMeetingSipUriConfigPartGruuFixString, kstrMeetingSipUriConfigPartAppConfigFixString);
            SetAanlysisFlag(false);
            bool bRet = false;
            const string kstrMeetingEntryInfoHeaderFlag = "https://";
            if (strMeetingAddressInfo.StartsWith(kstrMeetingEntryInfoHeaderFlag))
            {
                bRet = InitWithMeetingEntryInfo(strMeetingAddressInfo);
            }
            else
            {
                bRet = InitWithMeetingSipUri(strMeetingAddressInfo);
            }
            if (!bRet)
            {
                CleanMeetingInfo();
            }
            SetAanlysisFlag(bRet);
        }
        #endregion

        #region Public Tools
        public bool GetAnalysisFlag()
        {
            return m_bAnalysis;
        }
        public string GetMeetingLikeUri()
        {
            const string kstrMeetingUriLikeFormat = "sip:{0}@%id:{1}";
            return string.Format(kstrMeetingUriLikeFormat, m_strMeetingCreator, m_strMeetingID);
        }
        public string GetMeetingSipUri()
        {
            const string kstrMeetingUriLikeFormat = "sip:{0}{1}{2}";
            return string.Format(kstrMeetingUriLikeFormat, m_strMeetingCreator, m_strMeetingSipUriConfigPartFixString, m_strMeetingID);
        }
        #endregion

        #region Private tools
        private void SetAanlysisFlag(bool bAnalysis)
        {
            m_bAnalysis = bAnalysis;
        }
        private void CleanMeetingInfo()
        {
            m_strMeetingEntryHeader = "";
            m_strMeetingEntryServer = "";
            m_strMeetingCreator = "";
            m_strMeetingID = "";
            // ";gruu;opaque=app:conf:focus:id:"
            m_strMeetingSipUriConfigPartFixString = EstablishMeetingSipUriConfigPartFixString(kstrMeetingSipUriConfigPartGruuFixString, kstrMeetingSipUriConfigPartAppConfigFixString);
        }
        private bool InitWithMeetingEntryInfo(string strMeetingEntryInfo)
        {
            // https://meet.lync.nextlabs.solutions/kim1.yang/C42W2GMT
            bool bRet = false;
            strMeetingEntryInfo = CommonHelper.GetEffectiveUserInputString(strMeetingEntryInfo);
            string[] szStrMeetingEntryInfo = strMeetingEntryInfo.Split(new string[] { kstrSepMeetingEntryInfoParts }, StringSplitOptions.RemoveEmptyEntries);
            if ((null != szStrMeetingEntryInfo) && (4 == szStrMeetingEntryInfo.Length))
            {
                m_strMeetingEntryHeader = szStrMeetingEntryInfo[0];
                m_strMeetingEntryServer = szStrMeetingEntryInfo[1];
                string strMeetingCreatorDomain = szStrMeetingEntryInfo[1].Substring("meet.".Length);
                m_strMeetingCreator = szStrMeetingEntryInfo[2] + "@" + strMeetingCreatorDomain;
                m_strMeetingID = szStrMeetingEntryInfo[3];
                bRet = true;
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "the meeting entry info is:[{0}], split out lenght is not 4\n", strMeetingEntryInfo);
            }
            return bRet;
        }
        private bool InitWithMeetingSipUri(string strMeetingSipUri)
        {
            // sip:kim1.yang@lab11.com;gruu;opaque=app:conf:focus:id:Z30W1V7P
            bool bRet = false;
            strMeetingSipUri = CommonHelper.GetEffectiveUserInputString(strMeetingSipUri);
            if (!string.IsNullOrWhiteSpace(strMeetingSipUri))
            {
                // kim1.yang@lab11.com;gruu;opaque=app:conf:focus:id:Z30W1V7P
                string strMeetingSipUriWithoutSipHeader = CommonHelper.GetUriWithoutSipHeader(strMeetingSipUri);
                string[] szStrMeetingSipUri = strMeetingSipUriWithoutSipHeader.Split(new string[] { kstrSepMeetingSipUriParts }, StringSplitOptions.RemoveEmptyEntries);
                if ((null != szStrMeetingSipUri) && (3 == szStrMeetingSipUri.Length))
                {
                    const int nMeetingIDLength = 8;
                    string strSipUriAppConfigPart = szStrMeetingSipUri[2];
                    int nLastIndexOfConfigParts = strSipUriAppConfigPart.LastIndexOf(kstrSepMeetingSipUriConfigParts);
                    if ((0 <= nLastIndexOfConfigParts) && (strSipUriAppConfigPart.Length == (nLastIndexOfConfigParts + nMeetingIDLength + 1)))
                    {
                        m_strMeetingID = strSipUriAppConfigPart.Substring(nLastIndexOfConfigParts+1);
                        m_strMeetingSipUriConfigPartFixString = EstablishMeetingSipUriConfigPartFixString(szStrMeetingSipUri[1], strSipUriAppConfigPart.Substring(0, nLastIndexOfConfigParts+1));
                        m_strMeetingCreator = szStrMeetingSipUri[0];
                        bRet = true;
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "the meeting sip uri:[{0}], analysis meeting ID failed, config part:[{1}], index:[{2}]\n", strMeetingSipUri, szStrMeetingSipUri[1], nLastIndexOfConfigParts);
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "the meeting sip uri:[{0}], split out length is not 2 \n", strMeetingSipUri);
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "the meeting sip uri:[{0}] is empty\n", strMeetingSipUri);
            }
            return bRet;
        }
        private string EstablishMeetingSipUriConfigPartFixString(string strMeetingSipUriConfigPartGruu, string strMeetingSipUriConfigPartAppConfig)
        {
            // ";gruu;opaque=app:conf:focus:id:"
            return string.Format("{0}{1}{2}{3}", kstrSepMeetingSipUriParts, strMeetingSipUriConfigPartGruu, kstrSepMeetingSipUriParts, strMeetingSipUriConfigPartAppConfig);
        }
        #endregion
    }

    // Include common tools
    static public class CommonHelper
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(CommonHelper));
        #endregion

        #region Common values
        private const string kstrRegSFBKey = "SOFTWARE\\Nextlabs\\SFBServerEnforcer";
        private const string kstrRegItemSFBInstallPathKey = "InstallPath";
        static public readonly string kstrSFBInstallPath = GetSFBInstallPath();
        #endregion

        #region null object helper
        // Get solid string, avoid null object. If the string(strIn) is null, it will return empty string("").
        static public string GetSolidString(string strIn)
        {
            return (null == strIn) ? "" : strIn;
        }

        // Get object string value, avoid null object
        static public string GetObjectStringValue<T>(T obT)
        {
            return (null != obT) ? obT.ToString() : "";
        }

        // If strIn is null return empty string.
        // If strIn is not null trim white space and return.
        static public string GetEffectiveUserInputString(string strIn)
        {
            return (null == strIn) ? "" : strIn.Trim();
        }
        #endregion

        #region Enum helper
        static public T ConvertStringToEnum<T>(string strValue, bool bIgnoreCase, T emDefault)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), strValue, bIgnoreCase);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Convert:[{0}] to enum:[{1}] failed, please check.{2}\n", strValue, typeof(T).ToString(), ex.Message);
            }
            return emDefault;
        }
        #endregion

        #region Independence tools
        static public string GetUserNameFromUserSipUri(string strUserSipUri)
        {
            if (!string.IsNullOrEmpty(strUserSipUri))
            {
                strUserSipUri = GetUriWithoutSipHeader(strUserSipUri);
                int nAtIndex = strUserSipUri.IndexOf('@');
                if (0 < nAtIndex)
                {
                    strUserSipUri = strUserSipUri.Substring(0, nAtIndex);
                    strUserSipUri = strUserSipUri.Replace('.', ' ');
                }
            }
            return strUserSipUri;
        }
        static public string ReplaceWildcards(string strIn, Dictionary<string, string> dicWildcards, string strWildcardStartFlag, string strWildcardEndFlag, bool bNeedEncodeWildcardValue)
        {
            if (null != dicWildcards)
            {
                {
                    // Debug
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Org :\n[{0}]\n", strIn);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Start output wildcards:\n");
                    foreach (KeyValuePair<string, string> pairItem in dicWildcards)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Wildcards=>Key:[{0}],Value:[{1}],EncodeValue:[{2}]\n", pairItem.Key, pairItem.Value, (bNeedEncodeWildcardValue ? HttpUtility.HtmlEncode(pairItem.Value) : pairItem.Value));
                    }
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "End output wildcards\n");
                }
                foreach (KeyValuePair<string, string> pairWildcardItem in dicWildcards)
                {
                    string strRegPattern = CommonHelper.MakeAsStandardRegularPattern(strWildcardStartFlag + pairWildcardItem.Key + strWildcardEndFlag); // ==> "\CREATOR;"
                    Regex regex = new Regex(strRegPattern);
                    if (regex.IsMatch(strIn))
                    {
                        strIn = regex.Replace(strIn, (bNeedEncodeWildcardValue ? HttpUtility.HtmlEncode(pairWildcardItem.Value) : pairWildcardItem.Value));
                    }
                }
            }
            return strIn;
        }
        static public void SubStringBuilder(ref StringBuilder strBuilder, int nSubLength)
        {
            if (null != strBuilder)
            {
                if (strBuilder.Length >= nSubLength)
                {
                    strBuilder.Length -= nSubLength;
                }
                else
                {
                    strBuilder.Length = 0;
                }
            }
        }
        static public string JoinList<T>(List<T> lsIn, string strSepJoin)
        {
            return string.Join(strSepJoin, lsIn);
        }
        static public bool ContainsOneOfChars(string strIn, params char[] szChars)
        {
            if ((!string.IsNullOrEmpty(strIn)) && ((null != szChars)))
            {
                for (int i = 0; i < szChars.Length; ++i)
                {
                    if (strIn.Contains(szChars[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        static public bool ConverStringToBoolFlag(string strIn, bool bDefaultValue)
        {
            bool bRet = bDefaultValue;
            if (null != strIn)
            {
                try
                {
                    bRet = Convert.ToBoolean(strIn);
                }
                catch (Exception ex)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Convert:[{0}] to boolean failed, please check.{1}\n", strIn, ex.Message);
                }
            }
            return bRet;
        }
        static public string GetApplicationFile()
        {
            Assembly exeAssembly = System.Reflection.Assembly.GetEntryAssembly();
            if (null != exeAssembly)
            {
                string codeBase = exeAssembly.CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return path;
            }
            else
            {
                return string.Empty;
            }
        }
        static public string GetApplicationPath()
        {
            string strAppfile = GetApplicationFile();
            if(!strAppfile.Equals(string.Empty))
            {
                return Path.GetDirectoryName(strAppfile) + "\\";
            }
            return string.Empty;
        }
        static public string ConvertListToString(IList<string> lsMembers, string strSeparator, bool bEndWithSeparator)
        {
            string strOut = "";
            if (null != lsMembers)
            {
                int nCount = lsMembers.Count;
                if (0 < nCount)
                {
                    strOut = lsMembers[0];
                    for (int i = 1; i < nCount; ++i)
                    {
                        strOut += strSeparator + lsMembers[i];
                    }
                    if (bEndWithSeparator)
                    {
                        strOut += strSeparator;
                    }
                }
            }
            return strOut;
        }
        static public string GetStandardFolderPath(string strFolderPath)
        {
            if (null == strFolderPath)
            {
                strFolderPath = "";
            }
            else if (0 < strFolderPath.Length)
            {
                if (!strFolderPath.EndsWith("\\"))
                {
                    strFolderPath += "\\";
                }
            }
            return strFolderPath;
        }
        static public string GetStandardSipUri(string strSipUri)
        {
            if (null == strSipUri)
            {
                strSipUri = "";
            }
            else if (0 < strSipUri.Length)
            {
                const string kstrSipHeaderFlag = "Sip:";
                if (!strSipUri.StartsWith(kstrSipHeaderFlag, StringComparison.OrdinalIgnoreCase))
                {
                    strSipUri = strSipUri.Insert(0, kstrSipHeaderFlag);
                }
            }
            return strSipUri;
        }
        static public string GetUriWithoutSipHeader(string strSipUri)
        {
            if (null == strSipUri)
            {
                strSipUri = "";
            }
            else if (0 < strSipUri.Length)
            {
                const string kstrSipHeaderFlag = "Sip:";
                if (strSipUri.StartsWith(kstrSipHeaderFlag, StringComparison.OrdinalIgnoreCase))
                {
                    strSipUri = strSipUri.Substring(kstrSipHeaderFlag.Length);
                }
            }
            return strSipUri;
        }
        static private string GetSFBInstallPath()
        {
            // HKEY_LOCAL_MACHINE\SOFTWARE\Nextlabs\SFBServerEnforcer : InstallPath
            string strSFBInstallPath = ReadRegisterKey(Registry.LocalMachine, kstrRegSFBKey, kstrRegItemSFBInstallPathKey);
            if (!string.IsNullOrEmpty(strSFBInstallPath))
            {
                if (!strSFBInstallPath.EndsWith("\\"))
                {
                    strSFBInstallPath += "\\";
                }
            }
            return strSFBInstallPath;
        }
        static public string ReadRegisterKey(RegistryKey obRootRegistryKey, string strSubKeyPath, string strItemKey)
        {
            string strItemValue = "";
            RegistryKey obSubRegistryKey = null;
            try
            {
                if ((null != obRootRegistryKey) && (!string.IsNullOrEmpty(strSubKeyPath)) && (!string.IsNullOrEmpty(strItemKey)))
                {
                    obSubRegistryKey = obRootRegistryKey.OpenSubKey(strSubKeyPath);
                    if (null != obSubRegistryKey)
                    {
                        object obItemValue = obSubRegistryKey.GetValue(strItemKey);
                        if (null != obItemValue)
                        {
                            strItemValue = obItemValue.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Read register key {0}/{1}:{2} failed, {3}\n", obRootRegistryKey, strSubKeyPath, strItemKey, ex.Message);
            }
            finally
            {
                if (null != obSubRegistryKey)
                {
                    obSubRegistryKey.Close();
                }
            }
            return strItemValue;
        }
        
        static public string MakeAsStandardRegularPattern(string strInRegularFlag)
        {
            return strInRegularFlag.Replace("\\", "\\\\");
        }
        #endregion

        #region Array helper
        static public T GetArrayValueByIndex<T>(T[] szTIn, int nIndex, T tDefaultValue)
        {
            if ((0 <= nIndex) && (szTIn.Length > nIndex))
            {
                return szTIn[nIndex];
            }
            return tDefaultValue;
        }
        #endregion

        #region Dictionary helper
        static public TVALUE GetValueByKeyFromDir<TKEY, TVALUE>(Dictionary<TKEY, TVALUE> dirMaps, TKEY tKey, TVALUE tDefaultValue)
        {
            if (null != dirMaps)
            {
                if (dirMaps.ContainsKey(tKey))
                {
                    return dirMaps[tKey];
                }
            }
            return tDefaultValue;
        }
        static public void AddKeyValuesToDir<TKEY, TVALUE>(Dictionary<TKEY, TVALUE> dirMaps, TKEY tKey, TVALUE tValue)
        {
            if (null != dirMaps)
            {
                if (dirMaps.ContainsKey(tKey))
                {
                    dirMaps[tKey] = tValue;
                }
                else
                {
                    dirMaps.Add(tKey, tValue);
                }
            }
        }
        static public void RemoveKeyValuesFromDir<TKEY, TVALUE>(Dictionary<TKEY, TVALUE> dicMaps, TKEY tKey)
        {
            if (null != dicMaps)
            {
                if (dicMaps.ContainsKey(tKey))
                {
                    dicMaps.Remove(tKey);
                }
            }
        }
        static public string ConnectionDicKeyAndValues<TKEY, TVALUE>(Dictionary<TKEY, TVALUE> dicMaps, bool bRemoveEmptyItem, bool bEndWithKeySep, string strSepKeys, string strSepKeyAndValues)
        {
            if (null != dicMaps)
            {
                StringBuilder strOut = new StringBuilder();
                foreach (KeyValuePair<TKEY, TVALUE> pairItem in dicMaps)
                {
                    if ((!bRemoveEmptyItem) || (!string.IsNullOrEmpty(pairItem.Key.ToString()) && (!string.IsNullOrEmpty(pairItem.Value.ToString()))))
                    {
                        strOut.Append(pairItem.Key.ToString() + strSepKeyAndValues + pairItem.Value.ToString() + strSepKeys);
                    }
                }
                if (!bEndWithKeySep)
                {
                    SubStringBuilder(ref strOut, strSepKeys.Length);
                }
                return strOut.ToString();
            }
            return null;
        }
        static public Dictionary<string, TVALUE> DistinctDictionaryIgnoreKeyCase<TVALUE>(Dictionary<string, TVALUE> dicMaps)
        {
            Dictionary<string, TVALUE> dicCheckedMaps = new Dictionary<string, TVALUE>();
            foreach (KeyValuePair<string, TVALUE> pairItem in dicMaps)
            {
                CommonHelper.AddKeyValuesToDir(dicCheckedMaps, pairItem.Key.ToLower(), pairItem.Value);
            }
            return dicCheckedMaps;
        }
        #endregion

        #region XML help
        static public string XmlDocmentToString(XmlDocument xmlDoc)
        {
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                xmlDoc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }
        #endregion
    }
}
