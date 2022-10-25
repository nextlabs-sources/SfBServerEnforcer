using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SFBCommon.NLLog;

namespace TestProject.CommonTest
{
    class CommonLanguageTest
    {
        class TestObject
        {
            public string m_strTest = "";
        }

        public static void TestRegual()
        {
            string strContent = "<body style=\"overflow: auto\">\r\n" +
                                "<!--This div define 2 same text with different font style, by checking the offsetwidth of these 2 span, we can detect if the\r\n" +
                                "Ignore font styles specified on webpages checked in IE's Accessbility settings, only when this checked, the 2 span has  \r\n" + 
                                "the same offsetwidth otherwise it's different because of the different font styles--> \r\n" +
                                "<div style=\"visibility:hidden; position: absolute\"> \r\n" +
                                "<span id=\"font1\" style=\"font-family: 'Times New Roman'\">Text 1</span> \r\n" +
                                "<span id=\"font2\" style=\"font-family: Webdings\">Text 1</span> \r\n" +
                                "</div> \r\n" +
                                "<div id=\"silverlightControlHost\" align=\"center\"> \r\n" +
                                "<object id=\"silverlightObj\" data=\"data:application/x-silverlight-2,\" type=\"application/x-silverlight-2\"\r\n" +
                                "width=\"100%\" height=\"100%\"> \r\n";

            // add style                                     (<\\s{0,1}div\\s)[^>]*?id\\s*=\\s*\"silverlightControlHost\"(?:>|(?:\\s[^>]*?>))
            const string kstrSilverLightControlHostPatten = "(<\\s{0,1}div\\s)[^>]*?id\\s*=\\s*\"silverlightControlHost\"(?:>|(?:\\s+[^>]*?>))";
            Regex obReg = new Regex(kstrSilverLightControlHostPatten, RegexOptions.IgnoreCase);
            Match obMatch = obReg.Match(strContent);
            if (obMatch.Success)
            {
                int nInsertIndex = obMatch.Index + obMatch.Groups[1].Length;
                const string kstrStyleExteralSetting = " style=\"height: calc(100% - 20px);\" ";
                // const string kstrXapObjectExteralSetting = "\r\n <param name=\"wmode\" value=\"opaque\" />";
                // const string kstrXapObjectExteralSetting = "\r\n <param name=\"windowless\" value=\"true\" />";
                strContent = strContent.Insert(nInsertIndex, kstrStyleExteralSetting);
            }
        }

        public static void TestRegual_1()
        {
            string strContent = " <div id=\"silverlightControlHost\" align=\"center\"><object id=\"silverlightObj\"\r\n data=\"data:application/x-silverlight-2,\" type=\"application/x-silverlight-2\" width=\"100%\" height=\"100%\"><param name></param>";

            // add parameters in XAP object
            {
                const string kstrXapObjectPattern = "<object\\s[\\s\\S]+>";
                Regex reg = new Regex(kstrXapObjectPattern);
                Match obMatch = reg.Match(strContent);

                Console.WriteLine("Index:[{0}]\n", obMatch.Index);
            }

            {
                const string kstrXapObjectPattern = "<\\s{0,1}object\\s(.|[\r\n])+?>";
                Regex reg = new Regex(kstrXapObjectPattern);
                Match obMatch = reg.Match(strContent);

                Console.WriteLine("Index:[{0}]\n", obMatch.Index);
            }
        }

        public static void TestArray()
        {
            TestObject[] szTestObj = new TestObject[3]
            {
                new TestObject(),
                new TestObject(),
                new TestObject()
            };

            int nIndex = 1;
            TestObject obTestObj1 = szTestObj[nIndex];
            obTestObj1.m_strTest = "Test1";

            Console.WriteLine("szTestObj[{0}]:[{1}], obTestObj1:[{2}]\n", nIndex, szTestObj[nIndex].m_strTest, obTestObj1.m_strTest);

            TestObject obTestObj2 = obTestObj1;
            obTestObj2.m_strTest = "Test2";
            Console.WriteLine("szTestObj[{0}]:[{1}], obTestObj1:[{2}],obTestObj2:[{3}]\n", nIndex, szTestObj[nIndex].m_strTest, obTestObj1.m_strTest, obTestObj2.m_strTest);

        }

        public static void Test()
        {
            bool createdNew = false;
            Mutex m_Mutex = new Mutex(true, "Global\\Nextlabs.SFBServerEnforcer.SIPComponent", out createdNew);
            if (!createdNew)
            {
                //already running
                Console.WriteLine("Mutex already create\n");
            }
            else
            {
                Console.WriteLine("Create mutex success\n");
            }
        }

    }
}
