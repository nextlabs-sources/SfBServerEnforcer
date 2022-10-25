using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.Common;

namespace TestProject.CommonTest
{
    class FilePathMgr
    {
        class STUSFB_CFGFILENAME
        {
            private string m_strParentFolderName = "";
            private string m_strPrefixName = "";
            private string m_strSpecialName = "";
            private string m_strSuffixName = "";
            private string m_strExtension = "";
            private string m_strNameSep = "";

            public STUSFB_CFGFILENAME(string strPrefixName, string strSpecialName, string strSuffixName, string strExtension, string strParentFolderName, string strNameSep)
            {
                m_strPrefixName = strPrefixName;
                m_strSpecialName = strSpecialName;
                m_strSuffixName = strSuffixName;
                m_strExtension = strExtension;
                m_strParentFolderName = strParentFolderName;
                m_strNameSep = strNameSep;
            }
            public STUSFB_CFGFILENAME(STUSFB_CFGFILENAME stuCfgFileName)
            {
                m_strPrefixName = stuCfgFileName.m_strPrefixName;
                m_strSpecialName = stuCfgFileName.m_strSpecialName;
                m_strSuffixName = stuCfgFileName.m_strSuffixName;
                m_strExtension = stuCfgFileName.m_strExtension;
                m_strParentFolderName = stuCfgFileName.m_strParentFolderName;
                m_strNameSep = stuCfgFileName.m_strNameSep;
            }

            public void SetSpecialName(string strSpecialName)
            {
                m_strSpecialName = strSpecialName;
            }
            public string GetEffectiveFilePath(string strRootFolderPath, bool bCombineParentFolder) // return an exist file path, otherwise return "";
            {
                // Make a standard root folder path
                strRootFolderPath = CommonHelper.GetStandardFolderPath(strRootFolderPath);

                string strCfgFile = strRootFolderPath + InnerGetFileName(bCombineParentFolder);
                // return (File.Exists(strCfgFile)) ? strCfgFile : "";
                return strCfgFile;
            }
            private string ConnectStringWithSeprator(string strPart1, string strPart2, string strSep)
            {
                if ((!string.IsNullOrEmpty(strPart1)) && (!string.IsNullOrEmpty(strPart2)))
                {
                    return (strPart1 + strSep + strPart2);
                }
                else
                {
                    return (strPart1 + strPart2);
                }
            }
            private string InnerGetFileName(bool bCombineParentFolder)      // level: 1 -> 2
            {
                string strFullFileName = ConnectStringWithSeprator(m_strPrefixName, m_strSpecialName, m_strNameSep);
                strFullFileName = ConnectStringWithSeprator(strFullFileName, m_strSuffixName, m_strNameSep);
                if (bCombineParentFolder)
                {
                    strFullFileName = m_strParentFolderName + "\\" + strFullFileName;
                }
                return strFullFileName;
            }
        }

        static private readonly Dictionary<EMSFB_CFGTYPE, STUSFB_CFGFILENAME> kdirCfgFilesInfo = new Dictionary<EMSFB_CFGTYPE, STUSFB_CFGFILENAME>()
        {
            {EMSFB_CFGTYPE.emCfgLog, new STUSFB_CFGFILENAME("", "", "log", ".xml", "config", "_")},
            {EMSFB_CFGTYPE.emCfgCommon, new STUSFB_CFGFILENAME("", "", "", ".xml", "config", "_")},
            {EMSFB_CFGTYPE.emCfgSipMSPL, new STUSFB_CFGFILENAME("SfbServerEnforcer", "", "", ".am", "config", "")}
        };

        static public void Test()
        {
            foreach (KeyValuePair<EMSFB_CFGTYPE, STUSFB_CFGFILENAME> pairItem in kdirCfgFilesInfo)
            {
                Console.WriteLine("Path:[{0}]\n", pairItem.Value.GetEffectiveFilePath("Root", true));
                Console.WriteLine("Path:[{0}]\n", pairItem.Value.GetEffectiveFilePath("Root", false));
            }
        }
    }
}
