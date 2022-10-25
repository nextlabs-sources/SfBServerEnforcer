using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.Common;

namespace TestProject.CommonTest
{
    class CFlag
    {
        private string m_strFlag = "";
        public CFlag(string strFlag)
        {
            m_strFlag = strFlag;
            Console.Write("CFlag constructor: {0}", m_strFlag);
        }
        ~CFlag()
        {
            Console.Write("CFlag destructor: {0}", m_strFlag);
        }
    }

    class GolbalValueConstruct
    {
        public CFlag m_flagMember = new CFlag("common member");

        public static CFlag m_flagStatic = new CFlag("common static");
        
        public static readonly CFlag m_flagReadOnlyStatic = new CFlag("read only static");

        // public const CFlag m_flagConst = new CFlag("const");     // C# 语法不支持

    }

    class CommonTest
    {
        #region Const/Read only values
        public static readonly EMSFB_MODULE[] g_szEmModules = new EMSFB_MODULE[]
            {
                EMSFB_MODULE.emSFBModule_Unknown,
                EMSFB_MODULE.emSFBModule_HTTPComponent,
                EMSFB_MODULE.emSFBModule_NLLyncEndpointProxy_Agent,
                EMSFB_MODULE.emSFBModule_NLLyncEndpointProxy_Assistant,
                EMSFB_MODULE.emSFBModule_SFBControlPanel,
                EMSFB_MODULE.emSFBModule_SIPComponent,
                EMSFB_MODULE.emSFBModule_MaintainTool,
                EMSFB_MODULE.emSFBModule_NLAssistantWebService,
                EMSFB_MODULE.emSFBModule_UnitTest,
            };
        public static readonly EMSFB_CFGTYPE[] g_szCftTypes = new EMSFB_CFGTYPE[]
            {
                EMSFB_CFGTYPE.emCfgLog,
                EMSFB_CFGTYPE.emCfgCommon,
                EMSFB_CFGTYPE.emCfgSipMSPL,
                EMSFB_CFGTYPE.emCfgClassifyInfo
            };
        #endregion

        static public void test()
        {
            string strTestString = "11;22;33;44;55;66;77;88;99;00;11;22;33;";
            string[] szStrDesSipUris = strTestString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> lsStrDesSipUris = szStrDesSipUris.ToList();
            string[] szNewStrDesSipUris = lsStrDesSipUris.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();   // Remove repetition items

            Console.WriteLine("szStrDesSipUris length:[{0}], szNewStrDesSipUris length:[{1}]\n", szStrDesSipUris.Length, szNewStrDesSipUris.Length);
        }

        static public void ConfigFileTest()
        {
            foreach (EMSFB_MODULE emModule in g_szEmModules)
            {
                foreach (EMSFB_CFGTYPE emCfgType in g_szCftTypes)
                {
                    string strCfgFile = ConfigureFileManager.GetCfgFilePath(emCfgType, emModule);
                    Console.WriteLine("Module:[{0}], CfgType:[{1}], File:[{2}]\n", emModule, emCfgType, strCfgFile);
                }
            }

        }
    }
}
