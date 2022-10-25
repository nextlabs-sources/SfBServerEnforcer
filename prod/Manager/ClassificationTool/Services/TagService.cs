using ClassificationTool.Models;
using Newtonsoft.Json;
using SFBCommon.Common;
using SFBCommon.NLLog;
using SFBCommon.SFBObjectInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassificationTool.Services
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class TagService
    {
        #region logger
        private static readonly CLog theLog = CLog.GetLogger(typeof(TagService));
        #endregion

        #region members
        private Dictionary<string, Tag> m_dicTags;  // ignore case
        #endregion

        #region singleton
        private static readonly TagService m_instance = new TagService();
        public static TagService GetInstance()
        {
            return m_instance;
        }
        #endregion

        #region .ctor
        private TagService() { }
        #endregion

        #region properties

        #endregion

        #region public methods
        public void InitTagInfoFromPersistentInfo()
        {
            List<SFBObjectInfo> objList = SFBObjectInfo.GetAllObjsFrommPersistentInfo(SFBCommon.Common.EMSFB_INFOTYPE.emInfoNLTagLabel);
            if (objList != null)
            {
                m_dicTags = ParseTagListFromSFBObjectList(objList);
            }
        }
        public void UpdateTagListFromDatabase()
        {
            m_dicTags = null;
            InitTagInfoFromPersistentInfo();
        }
        public string GetTagsInJSON()
        {
            string strTagList = "";

            try
            {
                List<Tag> lsValidTag = new List<Tag>();
                foreach (KeyValuePair<string, Tag> pairTag in m_dicTags)
                {
                    if (EMSFB_EntityState.Deleted != pairTag.Value.State)
                    {
                        lsValidTag.Add(pairTag.Value);
                    }
                }
                strTagList = JsonConvert.SerializeObject(lsValidTag);
            }
            catch (Exception e)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "GetTagsInJSON failed, Error: {0}, StackTrace: {1}", e.Message, e.StackTrace);
            }

            return strTagList;
        }
        public void CacheTag(string strTag)
        {
            Tag newTag = null;

            if(!string.IsNullOrEmpty(strTag))
            {
                try
                {
                    newTag = JsonConvert.DeserializeObject<Tag>(strTag);
                }
                catch (Exception e)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "CacheTag -> DeserializeObject failed, tag string: {0}, error: {1}, stacktrace: {2}", strTag, e.Message, e.StackTrace);
                }

                if((newTag != null) && (!string.IsNullOrEmpty(newTag.TagName)))
                {
                    CommonHelper.AddKeyValuesToDir(m_dicTags, newTag.TagName.ToLower(), newTag);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "CacheTag failed, tag string: {0}", strTag);
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "CacheTag failed, tag string: null or empty");
            }
        }
        public void SaveChanges()
        {
            Tag obCurTag = null;
            List<string> lsKeys = new List<string>(m_dicTags.Keys.ToList());
            foreach (string strKey in lsKeys)
            {
                obCurTag = CommonHelper.GetValueByKeyFromDir(m_dicTags, strKey, null);
                if (null != obCurTag)
                {
                    switch (obCurTag.State)
                    {
                    case EMSFB_EntityState.Modified:
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! logic error, for tag service no modify operation, it is add new and delete old\n");
                        break;
                    }
                    case EMSFB_EntityState.Added:
                    {
                        if (AddTagToDB(obCurTag))
                        {
                            obCurTag.State = EMSFB_EntityState.Unchanged;
                        }
                        break;
                    }
                    case EMSFB_EntityState.Deleted:
                    {
                        if (DeleteTagInDB(obCurTag))
                        {
                            CommonHelper.RemoveKeyValuesFromDir(m_dicTags, strKey);
                        }
                        break;
                    }
                    default:
                    {
                        break;
                    }
                    }
                }
            }
        }
        #endregion

        #region inner tools
        private Dictionary<string, Tag> ParseTagListFromSFBObjectList(List<SFBObjectInfo> sfbObjList)
        {
            Dictionary<string, Tag> dicTempTags = new Dictionary<string, Tag>();
            foreach (SFBObjectInfo sfbObj in sfbObjList)
            {
                Tag tempTag = new Tag();
                tempTag.TagName = sfbObj.GetItemValue(NLTagLabelInfo.kstrTagNameFieldName);
                tempTag.TagValue = sfbObj.GetItemValue(NLTagLabelInfo.kstrTagValuesFieldName);
                tempTag.DefaultValue = sfbObj.GetItemValue(NLTagLabelInfo.kstrDefaultValueFieldName);
                tempTag.Editable = sfbObj.GetItemValue(NLTagLabelInfo.kstrEditableFieldName);
                tempTag.MultiSelect = sfbObj.GetItemValue(NLTagLabelInfo.kstrMultiSelectFieldName);
                tempTag.Mandatory = sfbObj.GetItemValue(NLTagLabelInfo.kstrMandatoryFieldName);

                CommonHelper.AddKeyValuesToDir(dicTempTags, tempTag.TagName.ToLower(), tempTag);
            }
            return dicTempTags;
        }
        private bool AddTagToDB(Tag tag)
        {
            NLTagLabelInfo nlTag = new NLTagLabelInfo();
            Dictionary<string, string> tagDict = ParseTagToDict(tag);
            nlTag.UpdateInfo(tagDict);
            return nlTag.PersistantSave();
        }
        private bool DeleteTagInDB(Tag tag)
        {
            NLTagLabelInfo nlTag = new NLTagLabelInfo();
            Dictionary<string, string> tagDict = ParseTagToDict(tag);
            nlTag.UpdateInfo(tagDict);
            return nlTag.PersistantSave();
        }
        private Dictionary<string, string> ParseTagToDict(Tag tag)
        {
            Dictionary<string, string> resultDict = new Dictionary<string, string>();

            CommonHelper.AddKeyValuesToDir(resultDict, Tag.TAG_NAME, tag.TagName);
            CommonHelper.AddKeyValuesToDir(resultDict, Tag.TAG_VALUE, tag.TagValue);
            CommonHelper.AddKeyValuesToDir(resultDict, Tag.DEFAULT_VALUE, tag.DefaultValue);
            CommonHelper.AddKeyValuesToDir(resultDict, Tag.EDITABLE, tag.Editable);
            CommonHelper.AddKeyValuesToDir(resultDict, Tag.MULTI_SELECT, tag.MultiSelect);
            CommonHelper.AddKeyValuesToDir(resultDict, Tag.MANDATORY, tag.Mandatory);

            EMSFB_EntityState objState = tag.State;
            string isDeprecated = "false";

            if (objState == EMSFB_EntityState.Deleted)
            {
                isDeprecated = "true";
            }

            CommonHelper.AddKeyValuesToDir(resultDict, Schema.SCHEMA_DEPRECATED, isDeprecated);

            return resultDict;
        }
        #endregion
    }
}
