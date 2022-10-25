﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Microsoft.Win32;

using SFBCommon.NLLog;
using System.Xml;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections;

/*
 * In this file include common tools and common structure which can be used in the whole SFB project
 */
namespace SFBCommon.Common
{
    public class MeetingEntryInfo
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(MeetingEntryInfo));
        #endregion

        #region Const/Readonly values
        private const string kstrSepMeetingEntryInfoParts = "/";
        #endregion

        #region Members
        private string m_strMeetingEntryHeader = "";
        private string m_strMeetingEntryServer = "";
        private string m_strMeetingCreator = "";
        private string m_strMeetingID = "";
        #endregion

        #region Constructor
        public MeetingEntryInfo(string strMeetingEntryInfo)
        {
            // https://meet.lync.nextlabs.solutions/kim1.yang/C42W2GMT
            string[] szStrMeetingEntryInfo = strMeetingEntryInfo.Split(new string[] { kstrSepMeetingEntryInfoParts }, StringSplitOptions.RemoveEmptyEntries);
            if ((null != szStrMeetingEntryInfo) && (4 == szStrMeetingEntryInfo.Length))
            {
                m_strMeetingEntryHeader = szStrMeetingEntryInfo[0];
                m_strMeetingEntryServer = szStrMeetingEntryInfo[1];
                m_strMeetingCreator = szStrMeetingEntryInfo[2];
                m_strMeetingID = szStrMeetingEntryInfo[3];
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "the meeting entry info is:[{0}], split out lenght is not 4\n", strMeetingEntryInfo);
            }
        }
        #endregion

        #region Public Tools
        public string GetMeetingLikeUri()
        {
            const string kstrMeetingUriLikeFormat = "sip:{0}@%id:{1}";
            return string.Format(kstrMeetingUriLikeFormat, m_strMeetingCreator, m_strMeetingID);
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
        #endregion

        #region Convert helper
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
        static public int ConvertStringToInt(string strValue, int nDefault)
        {
            try
            {
                return Int32.Parse(strValue);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Convert:[{0}] to int failed, use default value:[{1}], please check.{2}\n", strValue, nDefault, ex.Message);
            }
            return nDefault;
        }
        static public bool ConvertStringToBoolean(string strValue, bool bDefault)
        {
            try
            {
                if ((strValue.Equals("0", StringComparison.OrdinalIgnoreCase)) ||
                    (strValue.Equals("no", StringComparison.OrdinalIgnoreCase)) ||
                    (strValue.Equals("false", StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
                else if ((strValue.Equals("1", StringComparison.OrdinalIgnoreCase)) ||
                         (strValue.Equals("yes", StringComparison.OrdinalIgnoreCase)) ||
                         (strValue.Equals("true", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
                else
                {
                    return bDefault;
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Convert:[{0}] to boolean failed, use default value:[{1}], please check.{2}\n", strValue, bDefault, ex.Message);
            }
            return bDefault;
        }
        static public string[] ConvertStringToArray(string strValue, string strSep, bool bIgnoreCase, StringSplitOptions emStringSplitOption)
        {
            string[] szValueRet = null;
            if (!String.IsNullOrEmpty(strValue))
            {
                if (String.IsNullOrEmpty(strSep))
                {
                    szValueRet = new string[] { strValue };
                }
                else
                {
                    if (bIgnoreCase)
                    {
                        szValueRet = strValue.ToLower().Split(new string[] { strSep.ToLower() }, emStringSplitOption);
                    }
                    else
                    {
                        szValueRet = strValue.Split(new string[] { strSep }, emStringSplitOption);
                    }                   
                }
            }
            return szValueRet;
        }
        static public void ConvertArrayToCollection<TCollection, TItem>(TItem[] szValue, ref TCollection setValueRefRet) where TCollection : ICollection<TItem>
        {
            // HashSet<string>
            if (null != szValue)
            {
                if (null == setValueRefRet)
                {
                    setValueRefRet = default(TCollection);
                }
                foreach (TItem strItem in szValue)
                {
                    setValueRefRet.Add(strItem);
                }
            }
        }
        #endregion

        #region Independence tools
        static public string GetTabsStrByTabNum(int nTabNumbers)
        {
            string strTabs = "";
            for (int i = 0; i < nTabNumbers; ++i)
            {
                strTabs += "\t";
            }
            return strTabs;
        }
        static public string[] SplitStringByLength(string strIn, int nSepCharNum)
        {
            List<string> lsSnippets = new List<string>();
            if (!(string.IsNullOrEmpty(strIn)) && (0 < nSepCharNum))
            {
                int nSnippets = (strIn.Length / (nSepCharNum)) + 1;
                for (int i = 0; i < nSnippets; ++i)
                {
                    string strCurSnippet = "";
                    int nCurStartIndex = i * nSepCharNum;
                    if (i == (nSnippets - 1))
                    {
                        strCurSnippet = strIn.Substring(nCurStartIndex); // the last one
                    }
                    else
                    {
                        strCurSnippet = strIn.Substring(nCurStartIndex, nSepCharNum);
                    }

                    lsSnippets.Add(strCurSnippet);
                }
            }
            return lsSnippets.ToArray();
        }
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

        #region List helper
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
        static public string JoinList<T>(List<T> lsIn, string strSepJoin)
        {
            return string.Join(strSepJoin, lsIn);
        }
        static public List<string> GetStandardStringList(List<string> lsStrIn, bool bNeedTrim, bool bConverNullToEmpty, bool bRemovedNullOrWitheSpaceItem)
        {
            List<string> lsOut = null;
            if (null != lsStrIn)
            {
                lsOut = new List<string>();
                foreach (string strItem in lsStrIn)
                {
                    if (null == strItem)
                    {
                        lsOut.Add(bConverNullToEmpty ? "" : null);
                    }
                    else
                    {
                        lsOut.Add((bNeedTrim ? strItem.Trim() : strItem));
                    }
                }
                if (bRemovedNullOrWitheSpaceItem)
                {
                    lsOut.RemoveAll(new Predicate<string>(string.IsNullOrWhiteSpace));
                }
            }
            return lsOut;
        }
        static public bool ListStringContains(List<string> lstSrc, string strDest, StringComparison emStringComparison = StringComparison.OrdinalIgnoreCase)
        {
            foreach (string strSrc in lstSrc)
            {
                if (strSrc.Equals(strDest, emStringComparison))
                {
                    return true;
                }
            }
            return false;
        }
        static public void ListStringRemove(List<string> lstSrc, string strDest, StringComparison emStringComparison = StringComparison.OrdinalIgnoreCase)
        {
            foreach (string strSrc in lstSrc)
            {
                if (strSrc.Equals(strDest, emStringComparison))
                {
                    lstSrc.Remove(strSrc);
                    break;
                }
            }
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
