using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using System.Xml.Serialization;

using QAToolSFBCommon.NLLog;

namespace QAToolSFBCommon.Common
{
    public static class NLXmlSerializerHelper
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(NLXmlSerializerHelper));
        #endregion

        public static void SaveToXmlFile(string strFilePath, object obSource, Type type = null, string xmlRootName = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(strFilePath) && obSource != null)
                {
                    type = type != null ? type : obSource.GetType();

                    using (StreamWriter writer = new StreamWriter(strFilePath))
                    {
                        System.Xml.Serialization.XmlSerializer xmlSerializer = string.IsNullOrWhiteSpace(xmlRootName) ?
                            new System.Xml.Serialization.XmlSerializer(type) :
                            new System.Xml.Serialization.XmlSerializer(type, new XmlRootAttribute(xmlRootName));
                        xmlSerializer.Serialize(writer, obSource);
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in SaveToXmlFile, [{0}]\n", ex.Message);
            }
        }

        public static T LoadFromXml<T>(string strFilePath)
        {
            T result = default(T);
            try
            {
                if (File.Exists(strFilePath))
                {
                    using (StreamReader reader = new StreamReader(strFilePath))
                    {
                        System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                        result = (T)Convert.ChangeType(xmlSerializer.Deserialize(reader), typeof(T));
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in LoadFromXml, [{0}]\n", ex.Message);
            }
            return result;
        }
    }
}
