using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Debug
using QAToolSFBCommon.NLLog;

namespace QAToolSFBCommon.Common
{
    public class FileOpHelper : Logger
    {
        static public bool SaveToFile(string strFilePath, FileMode emFileMode, string strFileContent)
        {
            bool bRet = false;
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = new FileStream(strFilePath, emFileMode);
                sw = new StreamWriter(fs);
                sw.Write(strFileContent);
                sw.Flush();
                bRet = true;
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in SaveObjectInfoIntoFile: [{0}]\n", ex.Message);
            }
            finally
            {
                if (null != sw)
                {
                    sw.Close();
                }
                if (null != fs)
                {
                    fs.Close();
                }
            }
            return bRet;
        }
        static public string ReadAllFileContent(string strFilePath, FileMode emFileMode)
        {
            string strFileContent = "";
            FileStream fs = null;
            StreamReader sr = null;
            try
            {
                fs = new FileStream(strFilePath, emFileMode);
                sr = new StreamReader(fs);
                strFileContent = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in SaveObjectInfoIntoFile: [{0}]\n", ex.Message);
            }
            finally
            {
                if (null != sr)
                {
                    sr.Close();
                }
                if (null != fs)
                {
                    fs.Close();
                }
            }
            return strFileContent;
        }
    }
}
