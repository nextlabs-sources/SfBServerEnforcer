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
    abstract public class SFBObjectClassifyInfo : SFBObjectClassifyTags
    {
        #region Static values: field names
        public static readonly string kstrUriFieldName = "uri";
        public static readonly string kstrParticipatesFieldName = "participants";
        public static readonly string kstrClassifyTagsFieldName = "classifytags";
        public static readonly string kstrDoneManulClassifyFieldName = "donemanulclassify";
        public static readonly string kstrClassifyManagersFieldName = "classifymanagers";
        #endregion

        #region Constructors
        public SFBObjectClassifyInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
            SetClassifyTagsFieldName(kstrClassifyTagsFieldName);
        }
        public SFBObjectClassifyInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
            SetClassifyTagsFieldName(kstrClassifyTagsFieldName);
        }
        public SFBObjectClassifyInfo(Dictionary<string, string> dicInfos) : base(dicInfos)
        {
            SetClassifyTagsFieldName(kstrClassifyTagsFieldName);
        }
        #endregion

#if false // This code will be delete later
        #region classify tags operator
        public ClassifyTagsHelper GetClassifyTags()
        {
            string strClassifyTagsInfo = GetItemValue(kstrClassifyManagersFieldName);
            if (!string.IsNullOrEmpty(strClassifyTagsInfo))
            {
                return new ClassifyTagsHelper(strClassifyTagsInfo);
            }
            return null;
        }
        public Dictionary<string, string> GetDictionaryTags()
        {
            string strTagsXml = GetItemValue(kstrClassifyTagsFieldName);
            if ((null != strTagsXml) && (!string.IsNullOrWhiteSpace(strTagsXml)))
            {
                ClassifyTagsHelper classifyTagsHelper = new ClassifyTagsHelper(strTagsXml);
                return classifyTagsHelper.ClassifyTags;
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

            SetItem(kstrClassifyTagsFieldName, strTagsXml);
        }
        #endregion
#endif
        #region classify tags logic flag operator
        public bool IsManualClassifyDone()
        {
            string strManualClassifyDone = this.GetItemValue(SFBMeetingVariableInfo.kstrDoneManulClassifyFieldName);
            return (!string.IsNullOrWhiteSpace(strManualClassifyDone)) &&
                   strManualClassifyDone.Equals(SFBMeetingVariableInfo.kstrDoneManulClassifyYes);
        }
        #endregion


        #region classify manager operator
        public string[] GetClassifyManagers()
        {
            string strClassifyManagers = GetItemValue(kstrClassifyManagersFieldName);
            if (!string.IsNullOrEmpty(strClassifyManagers))
            {
                List<string> lsClassifyManagers = strClassifyManagers.Split(new string[] { kstrMemberSeparator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                return lsClassifyManagers.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            }
            return null;
        }
        public void AddedClassifyManager(string strUser)
        {
            string strManager = strUser + SFBObjectInfo.kstrMemberSeparator;
            string strExistManagers = this.GetItemValue(SFBMeetingVariableInfo.kstrClassifyManagersFieldName);
            if ((!string.IsNullOrWhiteSpace(strExistManagers)))
            {
                if (-1 == strExistManagers.IndexOf(strManager, StringComparison.OrdinalIgnoreCase))
                {
                    strExistManagers += strManager;
                    this.SetItem(SFBMeetingVariableInfo.kstrClassifyManagersFieldName, strExistManagers);
                }
            }
            else
            {
                this.SetItem(SFBMeetingVariableInfo.kstrClassifyManagersFieldName, strManager);
            }
        }
        public void RemoveClassifyManager(string strUser)
        {
            string strManager = strUser + SFBObjectInfo.kstrMemberSeparator;
            string strExistManagers = this.GetItemValue(SFBMeetingVariableInfo.kstrClassifyManagersFieldName);
            if ((!string.IsNullOrWhiteSpace(strExistManagers)))
            {
                int nPos = strExistManagers.IndexOf(strManager, StringComparison.OrdinalIgnoreCase);
                if (-1 != nPos)
                {
                    strExistManagers = strExistManagers.Remove(nPos, strManager.Length);
                    this.SetItem(SFBMeetingVariableInfo.kstrClassifyManagersFieldName, strExistManagers);
                }
            }
        }

        #endregion
    }
}
