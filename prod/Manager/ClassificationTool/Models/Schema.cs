using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.SFBObjectInfo;
using Newtonsoft.Json;

namespace ClassificationTool.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Schema
    {
        #region Property Name Definitions
        //can't use definitions in NLClassificationSchema, JsonProperty only accepts variables declared by const
        public const string SCHEMA_NAME = "schemaname";
        public const string SCHEMA_DATA = "data";
        public const string SCHEMA_DESCRIPTION = "description";
        public const string SCHEMA_DEPRECATED = "deprecated";
        public const string STATE = "state";
        #endregion

        #region members
        private string m_schemaName = "";
        private string m_schemaData = "";
        private string m_schemaDescription = "";
        private EMSFB_EntityState m_state = EMSFB_EntityState.Unchanged;
        #endregion

        #region properties
        [JsonProperty(propertyName: SCHEMA_NAME)]
        public string SchemaName
        {
            get { return m_schemaName; }
            set
            {
                if(!string.IsNullOrEmpty(value))
                {
                    m_schemaName = value.ToLower();
                }
            }
        }

        [JsonProperty(propertyName: SCHEMA_DATA)]
        public string SchemaData
        {
            get { return m_schemaData; }
            set
            {
                if(!string.IsNullOrEmpty(value))
                {
                    m_schemaData = value;
                }
            }
        }

        [JsonProperty(propertyName: SCHEMA_DESCRIPTION)]
        public string SchemaDescription
        {
            get { return m_schemaDescription; }
            set
            {
                if(!string.IsNullOrEmpty(value))
                {
                    m_schemaDescription = value.ToLower();
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
