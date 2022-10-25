using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDKWrapperLib;
using SFBCommon.Common;
using SFBCommon.NLLog;

namespace TagHelper
{
    public enum EMSFB_TAGOP
    {
        emTagOpUnknown,

        emTagOpReplace,
        emTagOpAppend,
        emTagOpMerge
    }

    public class TagMain
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(TagMain));
        #endregion

        #region Const/read only values
        private const int knTagOpReplace = 1;
        private const int knTagOpAppend = 2;
        private const int knTagOpMerge = 3;
        static private readonly Dictionary<EMSFB_TAGOP, int> kdicTagOps = new Dictionary<EMSFB_TAGOP, int>()
        {
            {EMSFB_TAGOP.emTagOpUnknown, knTagOpReplace},
            {EMSFB_TAGOP.emTagOpReplace, knTagOpReplace},
            {EMSFB_TAGOP.emTagOpAppend, knTagOpAppend},
            {EMSFB_TAGOP.emTagOpMerge, knTagOpMerge}
        };
        #endregion

        #region Static members
        static private NLTagManager m_obTagManager = new NLTagManager();
        #endregion

        #region Members
        private readonly string m_kstrTagValueSeparator = "";
        private readonly bool m_kbIgnoreCaseForTagName   = true;
        private readonly bool m_kbIgnoreCaseForTagValue  = true;
        #endregion

        #region Constructor
        public TagMain(string strTagValueSeparator = "", bool bIgnoreCaseForTagName   = true, bool bIgnoreCaseForTagValue = true)
        {
            m_kstrTagValueSeparator = strTagValueSeparator;
            m_kbIgnoreCaseForTagName = bIgnoreCaseForTagName;
            m_kbIgnoreCaseForTagValue = bIgnoreCaseForTagValue;
        }
        #endregion

        #region Public tools
        public NLTag ReadTag(string strInFilePath)
        {
            NLTag obNLTag = null;
            try
            {
                if ((!string.IsNullOrEmpty(strInFilePath)) && (File.Exists(strInFilePath)))
                {
                    obNLTag = GetNewNLTag(null, EMSFB_TAGOP.emTagOpMerge);
                    m_obTagManager.ReadTag(strInFilePath, knTagOpMerge, ref obNLTag);
                }
            }
            catch (Exception ex)
            {
                obNLTag = null;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in read tags from file:[%s], [%s]\n", strInFilePath, ex.Message);
            }
            return obNLTag;
        }
        public Dictionary<string,string> ReadTagEx(string strInFilePath)
        {
            Dictionary<string,string> dicTags = null;
            try
            {
                if ((!string.IsNullOrEmpty(strInFilePath)) && (File.Exists(strInFilePath)))
                {
                    NLTag obNLTag = ReadTag(strInFilePath);
                    dicTags = GetTagDicFromNLTag(obNLTag);
                }
            }
            catch (Exception ex)
            {
                dicTags = null;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in read tags from file:[%s], [%s]\n", strInFilePath, ex.Message);
            }
            return dicTags;
        }

        public bool WriteTag(string strInFilePath, NLTag obNLTag)
        {
            bool bRet = false;
            try
            {
                if ((!string.IsNullOrEmpty(strInFilePath)) && (File.Exists(strInFilePath)))
                {
                    if (null == obNLTag)
                    {
                        bRet = true;
                    }
                    else
                    {
                        m_obTagManager.WriteTag(strInFilePath, obNLTag);
                        bRet = true;
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in write tags from file:[%s], [%s]\n", strInFilePath, ex.Message);
            }
            return bRet;
        }
        public bool WriteTag(string strInFilePath, Dictionary<string,string> dicTags)
        {
            bool bRet = false;
            try
            {
                if ((!string.IsNullOrEmpty(strInFilePath)) && (File.Exists(strInFilePath)))
                {
                    if ((null == dicTags) || (0 == dicTags.Keys.Count))
                    {
                        bRet = true;
                    }
                    else
                    {
                        NLTag obNLTag = GetNewNLTag(dicTags, EMSFB_TAGOP.emTagOpMerge);
                        m_obTagManager.WriteTag(strInFilePath, obNLTag);
                        bRet = true;
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in write tags from file:[%s], [%s]\n", strInFilePath, ex.Message);
            }
            return bRet;
        }

        public bool RemoveTag(string strInFilePath, NLTag obNLTag)
        {
            bool bRet = false;
            try
            {
                if ((!string.IsNullOrEmpty(strInFilePath)) && (File.Exists(strInFilePath)))
                {
                    if (null == obNLTag)
                    {
                        bRet = true;
                    }
                    else
                    {
                        m_obTagManager.RemoveTag(strInFilePath, obNLTag);
                        bRet = true;
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in remove tags from file:[%s], [%s]\n", strInFilePath, ex.Message);
            }
            return bRet;
        }
        public bool RemoveTag(string strInFilePath, Dictionary<string,string> dicTags)
        {
            bool bRet = false;
            try
            {
                if ((!string.IsNullOrEmpty(strInFilePath)) && (File.Exists(strInFilePath)))
                {
                    if ((null == dicTags) || (0 == dicTags.Keys.Count))
                    {
                        bRet = true;
                    }
                    else
                    {
                        NLTag obNLTag = GetNewNLTag(dicTags, EMSFB_TAGOP.emTagOpMerge);
                        m_obTagManager.RemoveTag(strInFilePath, obNLTag);
                        bRet = true;
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in remove tags from file:[%s], [%s]\n", strInFilePath, ex.Message);
            }
            return bRet;
        }

        public bool RemoveAllTag(string strInFilePath)
        {
            bool bRet = false;
            try
            {
                if ((!string.IsNullOrEmpty(strInFilePath)) && (File.Exists(strInFilePath)))
                {
                    m_obTagManager.RemoveAllTags(strInFilePath);
                    bRet = true;
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in remove tags from file:[%s], [%s]\n", strInFilePath, ex.Message);
            }
            return bRet;
        }

        bool AddNewTags(ref NLTag obNLTag, string strNewTagName, string strNewTagValue, EMSFB_TAGOP emTagOp)
        {
            bool bRet = false;
            try
            {
                if ((!string.IsNullOrEmpty(strNewTagName)) && (!string.IsNullOrEmpty(strNewTagValue)))
                {
                    obNLTag.SetTag(strNewTagName, strNewTagValue, GetTagOpNumByEnum(emTagOp));
                    bRet = true;
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in add new tags, [%s]\n", ex.Message);
            }
            return bRet;
        }
        public NLTag GetNewNLTag(Dictionary<string, string> dicTags, EMSFB_TAGOP emTagOp)
        {
            NLTag obNLTag = new NLTag();
            obNLTag.Init(m_kstrTagValueSeparator, m_kbIgnoreCaseForTagName, m_kbIgnoreCaseForTagValue);

            if (null != dicTags)
            {
                foreach (KeyValuePair<string, string> pairTag in dicTags)
                {
                    AddNewTags(ref obNLTag, pairTag.Key, pairTag.Value, emTagOp);
                }
            }
            return obNLTag;
        }
        public Dictionary<string, string> GetTagDicFromNLTag(NLTag obNLTag)
        {
            Dictionary<string, string> dicTags = null;
            try
            {
                if (null != obNLTag)
                {
                    dicTags = new Dictionary<string, string>();

                    string strTagName = "";
                    string strTagValue = "";
                    obNLTag.GetFirstTag(out strTagName, out strTagValue);

                    bool bIsEnd = false;
                    obNLTag.IsEnd(out bIsEnd);
                    while (!bIsEnd)
                    {
                        CommonHelper.AddKeyValuesToDir(dicTags, strTagName, strTagValue);

                        obNLTag.GetNextTag(out strTagName, out strTagValue);
                        obNLTag.IsEnd(out bIsEnd);
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in get tag dic from NLTag new tags, [%s]\n", ex.Message);
            }
            return dicTags;
        }
        #endregion

        #region Inner tools
        private int GetTagOpNumByEnum(EMSFB_TAGOP emTagOp)
        {
            return CommonHelper.GetValueByKeyFromDir(kdicTagOps, emTagOp, knTagOpReplace);
        }
        #endregion
    }
}
