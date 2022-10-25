using ClassificationTool.Models;
using ClassificationTool.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassificationTool
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class ServiceProvider
    {
        private TagService m_tagService = null;
        private SchemaService m_schemaService = null;

        public ServiceProvider()
        {
            m_tagService = TagService.GetInstance();
            m_schemaService = SchemaService.GetInstance();
        }

        #region properties
        public List<Schema> SchemaList
        {
            get { return m_schemaService.SchemaList; }
        }
        #endregion

        #region public methods
        public void UpdateTagListFromDatabase()
        {
            m_tagService.UpdateTagListFromDatabase();
        }

        public string GetTagsInJSON()
        {
            return m_tagService.GetTagsInJSON();
        }

        public void CacheTag(string strTag)
        {
            m_tagService.CacheTag(strTag);
        }

        public void SaveTagChanges()
        {
            m_tagService.SaveChanges();
        }

        public string GetSchemasInJSON()
        {
            return m_schemaService.GetSchemasInJSON();
        }

        public void UpdateSchemaListFromDatabase()
        {
            m_schemaService.UpdateSchemaListFromDatabase();
        }

        public void CacheSchema(string strSchema)
        {
            m_schemaService.CacheSchema(strSchema);
        }

        public void SaveSchemaChanges()
        {
            m_schemaService.SaveChanges();
        }
        #endregion
    }
}
