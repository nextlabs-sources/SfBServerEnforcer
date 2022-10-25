using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;
using SFBCommon.NLLog;
using SFBCommon.ClassifyHelper;

namespace SFBCommon.SFBObjectInfo
{
    abstract public class SFBObjectClassifyTags : SFBObjectInfo
    {
        #region Members
        private string m_strCurClassifyTagsFieldName = "";
        #endregion

        #region Constructors
        public SFBObjectClassifyTags(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public SFBObjectClassifyTags(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public SFBObjectClassifyTags(Dictionary<string, string> dicInfos) : base(dicInfos)
        {
        }
        #endregion

        #region Protected functions
        protected void SetClassifyTagsFieldName(string strClassifyTagsFieldName)
        {
            m_strCurClassifyTagsFieldName = strClassifyTagsFieldName;
        }
        #endregion

        #region classify tags operator
        public ClassifyTagsHelper GetClassifyTags()
        {
            string strClassifyTagsInfo = GetItemValue(m_strCurClassifyTagsFieldName);
            if (!string.IsNullOrEmpty(strClassifyTagsInfo))
            {
                return new ClassifyTagsHelper(strClassifyTagsInfo);
            }
            return null;
        }
        public Dictionary<string, string> GetDictionaryTags()
        {
            ClassifyTagsHelper obClassifyTagsHelper = GetClassifyTags();
            if (null != obClassifyTagsHelper)
            {
                return obClassifyTagsHelper.ClassifyTags;
            }
            return new Dictionary<string, string>();
        }
        public string GetStringTags()
        {
            Dictionary<string, string> dicTags = GetDictionaryTags();
            string strStringTags = "";
            foreach (KeyValuePair<string, string> pairTag in dicTags)
            {
                if ((!string.IsNullOrWhiteSpace(pairTag.Key)) && (!string.IsNullOrWhiteSpace(pairTag.Value)))
                {
                    strStringTags += pairTag.Key;
                    strStringTags += "=";
                    strStringTags += pairTag.Value;
                    strStringTags += ClassifyTagsHelper.kstrSepTags;
                }
            }
            return strStringTags;
        }
        public void AddedNewTags(Dictionary<string, string> dicNewTags)
        {
            if ((null != dicNewTags) && (dicNewTags.Count > 0))
            {
                //get exist tags
                Dictionary<string, string> dicOldTags = this.GetDictionaryTags();

                //merge
                foreach (KeyValuePair<string, string> pairNewTag in dicNewTags)
                {
                    if ((!string.IsNullOrWhiteSpace(pairNewTag.Key)) && (!string.IsNullOrWhiteSpace(pairNewTag.Value)))
                    {
                        CommonHelper.AddKeyValuesToDir(dicOldTags, pairNewTag.Key.ToLower(), pairNewTag.Value);
                    }
                }

                //update the exist tags
                this.SetDictionaryTags(dicOldTags);
            }
        }
        public void SetDictionaryTags(Dictionary<string, string> dicTags)
        {
            ClassifyTagsHelper classifyTagsHelper = new ClassifyTagsHelper(dicTags);
            string strTagsXml = classifyTagsHelper.GetClassifyXml();

            SetItem(m_strCurClassifyTagsFieldName, strTagsXml);
        }
        #endregion
    }
}
