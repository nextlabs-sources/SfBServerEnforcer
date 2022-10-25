using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.SFBObjectInfo;
using SFBCommon.Common;

namespace TestProject
{
    class TestProjectMain
    {
        static void SFBObjInfoWorkflowTest()
        {
            // Whole work flow test
            // 1. Get data by power shell
            Console.Write("Begin to get SFB object info\n");
            List<SFBChatCategoryInfo> lsSFBObjInfo = TestProject.PowerShellTest.PowerShellTester.TestChatCategory();
            if (null != lsSFBObjInfo)
            {
                // 2. Persistent Save data
                Console.Write("Begin to persistent save SFB object info\n");
                foreach (SFBObjectInfo obSFBObjInfo in lsSFBObjInfo)
                {
                    obSFBObjInfo.PersistantSave();
                }
            }
            else
            {
                Console.Write("lsSFBObjInfo is null\n");
            }
            Console.Write("End persistent save SFB object info\n");
            Console.Write("Please check your database and then input any key to continue\n");
            Console.ReadKey();
            // 3. Establish SFB object by persistent storage data
//             List<SFBObjectInfo> lsNewSFBObjInfo = SFBObjectInfo.GetAllObjsFormPersistentInfo(EMSFB_INFOTYPE.emInfoNLChatCategory);
//             foreach (SFBObjectInfo obSFBObjInfo in lsNewSFBObjInfo)
//             {
//                 obSFBObjInfo.OutputObjInfo();
//             }
        }

        static void Main(string[] szArgs)
        {
            SFBCommon.Startup.InitSFBCommon(EMSFB_MODULE.emSFBModule_UnitTest);

            {
                // Args
                Console.Write("Args:\n");
                for (int i=0; i<szArgs.Length; ++i)
                {
                    Console.Write("\t{0}:{1}\n", i, szArgs[i]);
                }
            }

            string strTestFlag = "TestProject";
            Console.Write("Please to input any key to start test:[{0}], input \"Q/q\" exit\n", strTestFlag);
            {
                ConsoleKeyInfo obKey = Console.ReadKey();
                if ((obKey.Key == ConsoleKey.Q))
                {
                    SFBCommon.Startup.UninitSFBCommon();
                    return ;
                }
            }
            Console.Write("\nTest start running\n");


            #region Test code back
#if false   // TestProject.SFBCommonTest
            TestProject.SFBCommonTest.LogTester.Test();
            TestProject.SFBCommonTest.CommandXMLTester.Test();
            TestProject.SFBCommonTest.ConfigureFileManagerTester.Test(kstrCurModuleName);
            TestProject.CommonTest.CommonLanguageTest.testArray();
            TestProject.CommonTest.FilePathMgr.Test();
#endif

#if false   // Test regular
            TestProject.RegularTest.RegularTester.Test();
#endif

#if false   // Test PowerShell
            TestProject.PowerShellTest.PowerShellTester.TestChatRoom();
            TestProject.PowerShellTest.PowerShellTester.TestSFBUserInfo();
#endif

#if false   // Database
            TestProject.DatabaseTest.DatabaseTester.Test();
            TestProject.DatabaseTest.DatabaseTester.TestEx();
            TestProject.DatabaseTest.DatabaseTester.TestDBConnection();
            TestProject.DatabaseTest.DatabaseTester.TestSearchEx();
#endif

#if false   // Common test
            TestProject.CommonTest.DirtionaryTester.Test();
            TestProject.CommonTest.CommonTest.test();
#endif

#if false   // SFB Object info work flow test
            SFBObjInfoWorkflowTest();
#endif

#if false   // SFB Classify helper test
            TestProject.ClassifyHelperTest.ClassifyHelperTester.TestManulClassifyObligationHelper();
            TestProject.ClassifyHelperTest.ClassifyHelperTester.TestClassifyTagsHelper();
            TestProject.ClassifyHelperTest.ClassifyHelperTester.TestManulClassifyObligationHelperEx();
            TestProject.ClassifyHelperTest.ClassifyHelperTester.TestDoManulClassifyObligation();
            TestProject.ClassifyHelperTest.ClassifyHelperTester.TestTransferManulClassifyObligation();
            string strClassificationFileName = "C:\\Kim_Work\\GitCode\\SFBServerEnforcer\\bin\\SFBInstallPath\\TestCase\\ManualClassification.xml";
            ClassifyHelperTest.ClassifyHelperTester.TestTrimManualClassificationXml(strClassificationFileName);
#endif

#if false   // SFB Command helper test
            TestProject.CommandHelperTest.CommandHelperTester.Test();
            TestProject.CommandHelperTest.CommandHelperTester.NotificationCommandTest();
#endif

#if fale    // SFB Web service test
            TestProject.WebServiceTest.WebServiceTester.Test();
            TestProject.WebServiceTest.WebServiceTester.TestEx();
#endif

#if false   // SFB Object info test
            TestProject.SFBObjectTest.SFBObjTester.Test_GetDatasFromPersistentInfoWithFullSearchConditions();
#endif

#if false   // SFB Tag helper test
            TestProject.TagHelperTest.TagHelperTester.Test(szArgs[0]);
#endif
            #endregion

            TestProject.CommonTest.CommonLanguageTest.TestRegual();

            {
                Console.Write("End test\nYou can input q/Q to exit\n");
                ConsoleKeyInfo obKey = new ConsoleKeyInfo();
                while (true)
                {
                    obKey = Console.ReadKey();
                    if ((obKey.Key == ConsoleKey.Q))
                    {
                        SFBCommon.Startup.UninitSFBCommon();
                        break;
                    }
                }
            }
        }
    }
}
