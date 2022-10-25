using Newtonsoft.Json;
using SFBCommon.SFBObjectInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassificationTool.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Tag
    {
        #region Members
        private string m_tagName = "";
        private string m_tagValue = "";
        private string m_defaultValue = "";
        private string m_editable = "";
        private string m_multiSelect = "false";
        private string m_mandatory = "false";
        private EMSFB_EntityState m_state = EMSFB_EntityState.Unchanged;
        #endregion
        #region Property Name Definitions
        public const string STATE = "state";
        public const string TAG_NAME = "tagname";
        public const string TAG_VALUE = "tagvalues";
        public const string DEFAULT_VALUE = "defaultvalue";
        public const string EDITABLE = "editable";
        public const string MULTI_SELECT = "multiselect";
        public const string MANDATORY = "mandatory";
        public const string DEPRECATED = "deprecated";
        #endregion

        #region Properites
        [JsonProperty(propertyName: TAG_NAME)]
        public string TagName 
        {
            get
            {
                return m_tagName;
            }
            set
            {
                if(!string.IsNullOrEmpty(value))
                {
                    m_tagName = value.ToLower();
                }
            }
        }

        [JsonProperty(propertyName: TAG_VALUE)]
        public string TagValue
        {
            get
            {
                return m_tagValue;
            }
            set
            {
                if(!string.IsNullOrEmpty(value))
                {
                    m_tagValue = value.ToLower();
                }
            }
        }

        [JsonProperty(propertyName: DEFAULT_VALUE)]
        public string DefaultValue
        {
            get
            {
                return m_defaultValue;
            }
            set
            {
                if(!string.IsNullOrEmpty(value))
                {
                    m_defaultValue = value.ToLower();
                }
            }
        }

        [JsonProperty(propertyName: EDITABLE)]
        public string Editable
        {
            get
            {
                return m_editable;
            }
            set
            {
                if(!string.IsNullOrEmpty(value))
                {
                    m_editable = value.ToLower();
                }
            }
        }

        [JsonProperty(propertyName: MULTI_SELECT)]
        public string MultiSelect
        {
            get
            {
                return m_multiSelect;
            }
            set
            {
                if(!string.IsNullOrEmpty(value))
                {
                    m_multiSelect = value.ToLower();
                }
            }
        }

        [JsonProperty(propertyName: MANDATORY)]
        public string Mandatory
        {
            get
            {
                return m_mandatory;
            }
            set
            {
                if(!string.IsNullOrEmpty(value))
                {
                    m_mandatory = value.ToLower();
                }
            }
        }

        [JsonProperty(propertyName: STATE)]
        public EMSFB_EntityState State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }
        #endregion
    }
}
