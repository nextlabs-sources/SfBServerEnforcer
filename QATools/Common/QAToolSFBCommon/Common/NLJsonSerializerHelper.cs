using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;

using QAToolSFBCommon.NLLog;

namespace QAToolSFBCommon.Common
{
    public static class NLJsonSerializerHelper
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(NLJsonSerializerHelper));
        #endregion

        public static string GetJsonStrngFromObj<T>(T t)
        {
            string jsonString = "";
            MemoryStream obMemoryStream = null;
            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                obMemoryStream = new MemoryStream();
                ser.WriteObject(obMemoryStream, t);
                jsonString = Encoding.UTF8.GetString(obMemoryStream.ToArray());
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in GetJsonStrngFromObj, [{0}]\n", ex.Message);
            }
            finally
            {
                if (null != obMemoryStream)
                {
                    obMemoryStream.Close();
                }
            }
            return jsonString;
        }

        public static T LoadFromJson<T>(string strJson)
        {
            T tRead = default(T);
            try
            {
                if (!string.IsNullOrEmpty(strJson))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                    MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(strJson));
                    tRead = (T)serializer.ReadObject(mStream);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in LoadFromJson. InputJson:[{0}], ObjectType:[{1}], [{2}]\n", strJson, typeof(T), ex.Message);
            }
            return tRead;
        }
    }
}
