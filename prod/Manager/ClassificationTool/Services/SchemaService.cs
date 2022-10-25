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
using SFBCommon.ClassifyHelper;

namespace ClassificationTool.Services
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class SchemaService
    {
        #region logger
        private static readonly CLog logger = CLog.GetLogger(typeof(SchemaService));
        #endregion

        #region singleton
        private static readonly SchemaService m_instance = new SchemaService();
        public static SchemaService GetInstance()
        {
            return m_instance;
        }
        #endregion

        #region memebers
        private List<Schema> m_schemaList;
        #endregion

        #region .ctor
        private SchemaService() { }
        #endregion

        #region properities
        public List<Schema> SchemaList
        {
            get
            {
                if (m_schemaList == null)
                {
                    List<SFBObjectInfo> objList = SFBObjectInfo.GetAllObjsFrommPersistentInfo(SFBCommon.Common.EMSFB_INFOTYPE.emInfoNLClassificationSchema);
                    if(objList != null)
                    {
                        m_schemaList = ParseSchemaListFromSFBObjectList(objList);
                    }
                    else
                    {
                        m_schemaList = new List<Schema>();
                    }
                }
                return m_schemaList;
            }
        }
        #endregion

        #region public methods
        public string GetSchemasInJSON()
        {
            string strSchemas = "";

            try
            {
                List<Schema> validSchemaList = this.SchemaList.Where(s => s.State != EMSFB_EntityState.Deleted).ToList();
                strSchemas = JsonConvert.SerializeObject(validSchemaList);
            }
            catch (Exception e)
            {
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "GetSchemasInJSON failed, Error: {0}, StackTrace: {1}", e.Message, e.StackTrace);
            }
            
            return strSchemas;
        }

        public void UpdateSchemaListFromDatabase()
        {
            m_schemaList = null;
        }

        public void CacheSchema(string strSchema)
        {
            Schema newSchema = null;

            if (!string.IsNullOrEmpty(strSchema))
            {
                try
                {
                    newSchema = JsonConvert.DeserializeObject<Schema>(strSchema);
                }
                catch (Exception e)
                {
                    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "CacheSchema -> DeserializeObject failed, schema string: {0}, error: {1}, stacktrace: {2}", strSchema, e.Message, e.StackTrace);
                }

                if (newSchema != null && !string.IsNullOrEmpty(newSchema.SchemaName))
                {
                    bool isExist = false;

                    for (int i = 0; i < SchemaList.Count; i++)
                    {
                        if (SchemaList[i].SchemaName == newSchema.SchemaName)
                        {
                            isExist = true;
                            SchemaList[i] = newSchema;
                        }
                    }

                    if (!isExist)
                    {
                        SchemaList.Add(newSchema);
                    }
                }
                else
                {
                    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "CacheSchema failed, schema string: {0}", strSchema);
                }
            }
            else
            {
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "CacheSchema failed, schema string: null or empty");
            }
        }

        public void SaveChanges()
        {
            for(int i = SchemaList.Count - 1; i >= 0; i--)
            {
                switch (SchemaList[i].State)
                {
                    case EMSFB_EntityState.Modified:
                        {
                            UpdateSchemaInDB(SchemaList[i]);
                            break;
                        }
                    case EMSFB_EntityState.Added:
                        {
                            AddSchemaToDB(SchemaList[i]);
                            break;
                        }
                    case EMSFB_EntityState.Deleted:
                        {
                            DeleteSchemaInDB(SchemaList[i]);
                            break;
                        }
                    default:
                        break;
                }
            }
        }


        #endregion

        #region inner tools
        private List<Schema> ParseSchemaListFromSFBObjectList(List<SFBObjectInfo> sfbObjList)
        {
            List<Schema> schemaList = new List<Schema>();

            foreach (SFBObjectInfo sfbObj in sfbObjList)
            {
                Schema tempSchema = new Schema();
                tempSchema.SchemaName = sfbObj.GetItemValue(NLClassificationSchemaInfo.kstrSchemaNameFieldName);
                tempSchema.SchemaData = sfbObj.GetItemValue(NLClassificationSchemaInfo.kstrDataFieldName);
                tempSchema.SchemaDescription = sfbObj.GetItemValue(NLClassificationSchemaInfo.kstrDescriptionFieldName);
                schemaList.Add(tempSchema);
            }
            
            return schemaList;
        }

        private bool AddSchemaToDB(Schema schema)
        {
            bool result = false;

            NLClassificationSchemaInfo nlSchema = new NLClassificationSchemaInfo();
            Dictionary<string, string> schemaDict = ParseSchemaToDict(schema);
            nlSchema.UpdateInfo(schemaDict);
            result = nlSchema.PersistantSave();

            if (result)
            {
                schema.State = EMSFB_EntityState.Unchanged;
            }

            return result;
        }

        private bool DeleteSchemaInDB(Schema schema)
        {
            bool result = false;

            NLClassificationSchemaInfo nlSchema = new NLClassificationSchemaInfo();
            Dictionary<string, string> schemaDict = ParseSchemaToDict(schema);
            nlSchema.UpdateInfo(schemaDict);
            result = nlSchema.PersistantSave();

            if (result)
            {
                SchemaList.Remove(schema);
            }

            return result;
        }

        private bool UpdateSchemaInDB(Schema schema)
        {
            bool result = false;

            NLClassificationSchemaInfo nlSchema = new NLClassificationSchemaInfo();
            Dictionary<string, string> schemaDict = ParseSchemaToDict(schema);
            nlSchema.UpdateInfo(schemaDict);
            result = nlSchema.PersistantSave();

            if (result)
            {
                schema.State = EMSFB_EntityState.Unchanged;
            }

            return result;
        }

        private Dictionary<string, string> ParseSchemaToDict(Schema schema)
        {
            Dictionary<string, string> resultDict = new Dictionary<string, string>();

            // Trim classification XML
            ManulClassifyObligationHelper obManulClassifyObligationHelper = new ManulClassifyObligationHelper(schema.SchemaData, true);
            string strTrimedClassificationInfo = obManulClassifyObligationHelper.GetClassifyXml();

            CommonHelper.AddKeyValuesToDir(resultDict, Schema.SCHEMA_NAME, schema.SchemaName);
            CommonHelper.AddKeyValuesToDir(resultDict, Schema.SCHEMA_DATA, strTrimedClassificationInfo);
            CommonHelper.AddKeyValuesToDir(resultDict, Schema.SCHEMA_DESCRIPTION, schema.SchemaDescription);

            EMSFB_EntityState objState = schema.State;
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
