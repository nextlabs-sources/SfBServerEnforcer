using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SFBBaseCommon
{
    class Common
    {
        #region Public values
        static public readonly string kstrSFBInstallPath = GetSFBInstallPath();
        #endregion

        #region Private values
        private const string kstrRegSFBKey = "SOFTWARE\\Nextlabs\\SFBServerEnforcer";
        private const string kstrRegItemSFBInstallPathKey = "InstallPath";
        #endregion

        #region Log
        static public void OutputTraceLog(string strFormat, params object[] szArgs)
        {
            System.Diagnostics.Trace.TraceInformation(strFormat, szArgs);
        }
        #endregion

        static public string GetSFBInstallPath()
        {
            // HKEY_LOCAL_MACHINE\SOFTWARE\Nextlabs\SFBServerEnforcer : InstallPath
            string strSFBInstallPath = ReadRegisterKey(Registry.LocalMachine, kstrRegSFBKey, kstrRegItemSFBInstallPathKey);
            if (!string.IsNullOrEmpty(strSFBInstallPath))
            {
                if (!strSFBInstallPath.EndsWith("\\"))
                {
                    strSFBInstallPath += "\\";
                }
            }
            return strSFBInstallPath;
        }
        static public string ReadRegisterKey(RegistryKey obRootRegistryKey, string strSubKeyPath, string strItemKey)
        {
            string strItemValue = "";
            RegistryKey obSubRegistryKey = null;
            try
            {
                if ((null != obRootRegistryKey) && (!string.IsNullOrEmpty(strSubKeyPath)) && (!string.IsNullOrEmpty(strItemKey)))
                {
                    obSubRegistryKey = obRootRegistryKey.OpenSubKey(strSubKeyPath);
                    if (null != obSubRegistryKey)
                    {
                        object obItemValue = obSubRegistryKey.GetValue(strItemKey);
                        if (null != obItemValue)
                        {
                            strItemValue = obItemValue.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.OutputTraceLog("Read register key {0}/{1}:{2} failed, {3}\n", obRootRegistryKey, strSubKeyPath, strItemKey, ex.Message);
            }
            finally
            {
                if (null != obSubRegistryKey)
                {
                    obSubRegistryKey.Close();
                }
            }
            return strItemValue;
        }

    }
}
