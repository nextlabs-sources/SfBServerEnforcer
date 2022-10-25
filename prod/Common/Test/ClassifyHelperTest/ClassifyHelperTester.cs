using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.NLLog;
using SFBCommon.ClassifyHelper;
using SFBCommon.Common;
using System.Xml;

namespace TestProject.ClassifyHelperTest
{
    class ClassifyHelperTester
    {
        public const string kstrXmlManulClassifyObligations = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + 
                                                   "<Classification type = \"manual\">" + 
                                                        "<Tag name=\"itar\" editable=\"false\" default=\"yes\" values=\"yes|no\">" + 
                                                            "<Tag name=\"level\" editable=\"false\" default=\"1\" values=\"1|2|3|4|5\" relyOn=\"yes\">" + 
                                                                "<Tag name=\"classify\" editable=\"false\" default=\"yes\" values=\"yes|no\" relyOn=\"4|5\"/>" + 
                                                            "</Tag>" + 
                                                            "<Tag name=\"kaka\" editable=\"false\" default=\"1\" values=\"1|2|3|4|5\" relyOn=\"yes\"/>" +
                                                        "</Tag>" + 
                                                        "<Tag name=\"description\" editable=\"false\" default=\"yes\" values=\"protected meeting|normal meeting\"></Tag>" + 
                                                    "</Classification>";
        public static readonly string kstrNewXmlManulClassifyObligations = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + 
                                                    "<Classification type = \"manual\">" + 
                                                        "<Tag name=\"tag\" editable=\"false\" default=\"A\" values=\"A|B\"></Tag>" +
                                                    "</Classification>";

        public const string kstrXmlClassifyTags = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                                    "<Classification type = \"tags\">" +
                                                        "<Tag name=\"itar\" values=\"yes\"/>" +
                                                        "<Tag name=\"level\" values=\"5\"/>" +
                                                        "<Tag name=\"classify\" values=\"yes\"/>" +
                                                        "<Tag name=\"description\" values=\"protected meeting\"/>" +
                                                        "<Tag name=\"Itar\" values=\"yes\"/>" +
                                                        "<Tag name=\"Level\" values=\"5\"/>" +
                                                    "</Classification>";


        static public void TestManulClassifyObligationHelper()
        {
#if true
            {
                ManulClassifyObligationHelper obManulClassifyHelper = new ManulClassifyObligationHelper(kstrXmlManulClassifyObligations, false);
                if (EMSFB_CLASSIFYTYPE.emManulClassifyObligation == obManulClassifyHelper.GetClassifyType())
                {
                    {
                        bool bAppendSuccess = obManulClassifyHelper.Append(kstrXmlManulClassifyObligations, false);
                        if (!bAppendSuccess)
                        {
                            Console.Write("Append new classify obligation info failed A, {0}\n", kstrNewXmlManulClassifyObligations);
                        }
                        Console.Write("Append result: \n[\n{0}\n]\n", obManulClassifyHelper.GetClassifyXml());
                    }
                    {
                        bool bAppendSuccess = obManulClassifyHelper.Append(kstrNewXmlManulClassifyObligations, false);
                        if (!bAppendSuccess)
                        {
                            Console.Write("Append new classify obligation info failed A, {0}\n", kstrNewXmlManulClassifyObligations);
                        }
                        Console.Write("Append result: \n[\n{0}\n]\n", obManulClassifyHelper.GetClassifyXml());
                    }

                    {
                        bool bAppendSuccess = obManulClassifyHelper.Append(kstrXmlManulClassifyObligations, false);
                        if (!bAppendSuccess)
                        {
                            Console.Write("Append new classify obligation info failed A, {0}\n", kstrNewXmlManulClassifyObligations);
                        }
                        Console.Write("Append result: \n[\n{0}\n]\n", obManulClassifyHelper.GetClassifyXml());
                    }
                }
                if (EMSFB_CLASSIFYTYPE.emManulClassifyObligation == obManulClassifyHelper.GetClassifyType())
                {
                    List<StuManulClassifyOb> lsStuManulClassifyObs = obManulClassifyHelper.GetStuManulClassifyObs();
                    foreach (StuManulClassifyOb stuManulClassifyOb in lsStuManulClassifyObs)
                    {
                        stuManulClassifyOb.OutputInfo("");
                    }
                }
//                 Dictionary<string,string> dicClassificationInfo = CommonCfgMgr.GetInstance().GetAllClassificationInfo(true);
//                 foreach (KeyValuePair<string,string> pairClassify in dicClassificationInfo)
//                 {
//                     Console.Write("ClassificationName:[{0}], Content:[{1}]\n", pairClassify.Key, pairClassify.Value);
//                 }
            }
#endif

#if false
            {
                string strClassifyFilePath = ConfigureFileManager.GetCfgFilePath(EMSFB_CFGTYPE.emCfgClassifyInfo, EMSFB_MODULE.emSFBModule_SFBControlPanel);
                Console.Write("Classify info file path: {0}\n", strClassifyFilePath);
                ManulClassifyObligationHelper obManulClassifyHelper = new ManulClassifyObligationHelper(strClassifyFilePath, true);
                if (EMSFB_CLASSIFYTYPE.emManulClassifyObligation == obManulClassifyHelper.GetClassifyType())
                {
                    List<StuManulClassifyOb> lsStuManulClassifyObs = obManulClassifyHelper.GetStuManulClassifyObs();
                    foreach (StuManulClassifyOb stuManulClassifyOb in lsStuManulClassifyObs)
                    {
                        stuManulClassifyOb.OutputInfo("");
                    }
#endif

#if false
                    bool bAppendSuccess = obManulClassifyHelper.Append(strClassifyFilePath, true);
                    if (!bAppendSuccess)
                    {
                        Console.Write("Append new classify obligation info failed, {0}", strClassifyFilePath);
                    }
#endif

#if false
                if (EMSFB_CLASSIFYTYPE.emManulClassifyObligation == obManulClassifyHelper.GetClassifyType())
                {
                    List<StuManulClassifyOb> lsStuManulClassifyObs = obManulClassifyHelper.GetStuManulClassifyObs();
                    foreach (StuManulClassifyOb stuManulClassifyOb in lsStuManulClassifyObs)
                    {
                        stuManulClassifyOb.OutputInfo(true);
                    }
                }
#endif
        }

        static public void TestClassifyTagsHelper()
        {
            Dictionary<string,string> dicClassifyTags = new Dictionary<string,string>();
            {
                ClassifyTagsHelper obClassifyTagHelper = new ClassifyTagsHelper(kstrXmlClassifyTags);
                dicClassifyTags = obClassifyTagHelper.ClassifyTags;
                obClassifyTagHelper.OutputInfo();
            }

            {
                ClassifyTagsHelper obClassifyTagHelper = new ClassifyTagsHelper("Tagname1", "TagValue1", "Tagname2", "TagValue2", "TagName3", "TagValue3", "TagName1", "TagValue1", "TagName2", "TagValue2");
                dicClassifyTags = obClassifyTagHelper.ClassifyTags;
                obClassifyTagHelper.OutputInfo();
            }

            {
                CommonHelper.AddKeyValuesToDir(dicClassifyTags, "NewTagName1", "NewTagValue1");
                CommonHelper.AddKeyValuesToDir(dicClassifyTags, "NewTagName2", "NewTagValue2");
                CommonHelper.AddKeyValuesToDir(dicClassifyTags, "NewTagname1", "NewTagValue1");
                CommonHelper.AddKeyValuesToDir(dicClassifyTags, "NewTagname2", "NewTagValue2");
                ClassifyTagsHelper obClassifyTagHelper = new ClassifyTagsHelper(dicClassifyTags);
                dicClassifyTags = obClassifyTagHelper.ClassifyTags;
                obClassifyTagHelper.OutputInfo();
            }
        }

        static public void TestManulClassifyObligationHelperEx()
        {
            string strClassifyFilePath = ConfigureFileManager.GetCfgFilePath(EMSFB_CFGTYPE.emCfgClassifyInfo, EMSFB_MODULE.emSFBModule_SFBControlPanel);
            Console.Write("Classify info file path: {0}\n", strClassifyFilePath);
            ManulClassifyObligationHelper obManulClassifyHelper = new ManulClassifyObligationHelper(strClassifyFilePath, true);
            if (EMSFB_CLASSIFYTYPE.emManulClassifyObligation == obManulClassifyHelper.GetClassifyType())
            {
                List<StuManulClassifyOb> lsStuManulClassifyObs = obManulClassifyHelper.GetStuManulClassifyObs();
                foreach (StuManulClassifyOb stuManulClassifyOb in lsStuManulClassifyObs)
                {
                    // stuManulClassifyOb.OutputInfo("", false);
                    int i = -1;
                    while (true)
                    {
                        StuManulClassifyOb obManulClassify = stuManulClassifyOb.GetNodeByIndex(++i);
                        if (null != obManulClassify)
                        {
                            obManulClassify.OutputInfo("");
                        }
                        else
                        {
                            Console.Write("Item counts:[{0}]\n", i);
                            break;
                        }
                    }
                }
            }
        }

        static public void TestTrimManualClassificationXml(string strClassificationFileName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(strClassificationFileName);   // Load XML format string
            string strClassificationInfo = xmlDoc.InnerXml;

            ManulClassifyObligationHelper obManulClassifyObligationHelper = new ManulClassifyObligationHelper(strClassificationInfo, true);
            string strTrimedClassificationInfo = obManulClassifyObligationHelper.GetClassifyXml();
            EMSFB_CLASSIFYTYPE emClassifyType = obManulClassifyObligationHelper.GetClassifyType();
            Console.Write("Type:[{0}]\nOld:[\n{1}\n]\nTrimed:[\n{2}\n]\n", emClassifyType, strClassificationInfo, strTrimedClassificationInfo);
        }

        static public void TestDoManulClassifyObligation()
        {
#if true
            string strClassifyFilePath = ConfigureFileManager.GetCfgFilePath(EMSFB_CFGTYPE.emCfgClassifyInfo, EMSFB_MODULE.emSFBModule_SFBControlPanel);
            Console.Write("Classify info file path: {0}\n", strClassifyFilePath);
            ManulClassifyObligationHelper obManulClassifyHelper = new ManulClassifyObligationHelper(strClassifyFilePath, true);
#else
            ManulClassifyObligationHelper obManulClassifyHelper = new ManulClassifyObligationHelper(kstrXmlManulClassifyObligations, false);
#endif
            if (EMSFB_CLASSIFYTYPE.emManulClassifyObligation == obManulClassifyHelper.GetClassifyType())
            {
                StuManulClassifyItem obManulClassifyItem = null;
                Dictionary<string,string> dicExistTag = new Dictionary<string,string>();
                List<StuManulClassifyOb> lsStuManulClassifyObs = obManulClassifyHelper.GetStuManulClassifyObs();
                foreach (StuManulClassifyOb stuManulClassifyOb in lsStuManulClassifyObs)
                {
                    StuManulClassifyOb obCurManulClassify = stuManulClassifyOb; // Root
                    obCurManulClassify.OutputInfo("");
                    int i = -1;
                    while (true)
                    {
                        if (null != obCurManulClassify)
                        {
                            ++i;
                            obManulClassifyItem = obCurManulClassify.ManulClassifyItem;
                            if ((null != obManulClassifyItem) && (null != obManulClassifyItem.Values) && (null != obManulClassifyItem.Name))
                            {
                                // Show tags for user
                                Console.Write("Please select tags for:[{0}]\n", obManulClassifyItem.Name);
                                for (int iValue=0; iValue<obManulClassifyItem.Values.Count; ++iValue)
                                {
                                    Console.Write("{0}:[{1}]\n", iValue+1, obManulClassifyItem.Values[iValue]);
                                }
                                Console.Write("{0}:[{1}]\n", 0, "stop and exit classify");
                                while (true)
                                {
                                    bool bSelectRight = false;
                                    try
                                    {
                                        string strUserSelect = Console.ReadLine().Trim();
                                        int nUserSelect = int.Parse(strUserSelect);
                                        if ((0 <= nUserSelect) && (obManulClassifyItem.Values.Count >= nUserSelect))
                                        {
                                            if (0 == nUserSelect)
                                            {
                                                // Stop
                                                Console.Write("Stop to exit\n");
                                                return ;
                                            }
                                            else
                                            {
                                                CommonHelper.AddKeyValuesToDir(dicExistTag, obManulClassifyItem.Name, obManulClassifyItem.Values[nUserSelect-1]);
                                                bSelectRight = true;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // Exception here, maybe user do not input right select index
                                        Console.Write("Exception, maybe user do not input right select index, [{0}]\n", ex.Message);
                                    }
                                    if (bSelectRight)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        Console.Write("Invalid select, please reselect the tag value\n");
                                    }
                                }
                                obCurManulClassify = stuManulClassifyOb.GetNextEffectiveNodeByIndex(obCurManulClassify.ManulClassifyItem.Index, dicExistTag);
                            }
                        }
                        else
                        {
                            Console.Write("Effective counts:[{0}]\n", i);
                            break;
                        }
                    }
                    foreach (KeyValuePair<string,string> pairTagNameAndValues in dicExistTag)
                    {
                        Console.Write("Tag[{0} = {1}]\n", pairTagNameAndValues.Key, pairTagNameAndValues.Value);
                    }
                }
            }
        }

        static public void TestManualClassificationTree()
        {
            {
                List<StuManulClassifyItem> lsManulClassifyItems = new List<StuManulClassifyItem>()
                        { 
                            new StuManulClassifyItem("RootA", "1|2|3|4|5", "1", "", "", false),
                            new StuManulClassifyItem("RootB", "1|2|3|4|5", "1", "", "", false),
                            new StuManulClassifyItem("RootC", "1|2|3|4|5", "1", "", "", false)
                        };
                ManulClassifyObligationHelper obManulClassifyHelper = new ManulClassifyObligationHelper(lsManulClassifyItems);
                TransferManulClassification(obManulClassifyHelper);
            }
            {
                List<StuManulClassifyItem> lsManulClassifyItems = new List<StuManulClassifyItem>()
                        { 
                            new StuManulClassifyItem("RootA", "1|2|3|4|5", "1", "", "", false),
                            new StuManulClassifyItem("A_1", "1|2|3|4|5", "1", "RootA", "1|2", false),
                            new StuManulClassifyItem("A_2", "1|2|3|4|5", "1", "RootA", "3|4|5", false),
                            new StuManulClassifyItem("A_3", "1|2|3|4|5", "1", "RootA", "3", false),
                            new StuManulClassifyItem("A_3", "1|2|3|4|5", "1", "RootA", "1|2|3|4|5", false),

                            new StuManulClassifyItem("RootB", "1|2|3|4|5", "1", "", "", false),
                            new StuManulClassifyItem("RootC", "1|2|3|4|5", "1", "", "", false),

                            new StuManulClassifyItem("RootC", "1|2|3|4|5", "1", "WroongParent", "1", false)
                        };
                ManulClassifyObligationHelper obManulClassifyHelper = new ManulClassifyObligationHelper(lsManulClassifyItems);
                TransferManulClassification(obManulClassifyHelper);
            }
            {
            /*
                <?xml version="1.0" encoding="utf-8" ?>
                <Classification type = "manual">
                    <Tag name="itar" editable="false" default="yes" values="yes|no">
                        <Tag name="level" editable="false" default="1" values="1|2|3|4|5" relyOn="yes">
                            <Tag name="classify" editable="false" default="yes" values="yes|no" relyOn="4|5"/>
                        </Tag>
                        <Tag name="NoItar" editable="false" default="1" values="1|2|3|4|5" relyOn="no">
                            <Tag name="level" editable="false" default="1" values="1|2|3|4|5" relyOn="1|2"/>
                        </Tag>
                    </Tag>
                    <Tag name="description" editable="false" default="yes" values="protected meeting|normal meeting"></Tag>
                </Classification>
             */
                List<StuManulClassifyItem> lsManulClassifyItems = new List<StuManulClassifyItem>()
                        { 
                            new StuManulClassifyItem("Itar", "yes|no", "yes", "", "", false),
                            new StuManulClassifyItem("level", "1|2|3|4|5", "1", "itar", "yes", false),
                            new StuManulClassifyItem("classify", "yes|no", "1", "level", "4|5", false),
                            new StuManulClassifyItem("NoItar", "1|2|3|4|5", "1", "Itar", "no", false),
                            new StuManulClassifyItem("level", "1|2|3|4|5", "1", "NoItar", "1|2", false),

                            new StuManulClassifyItem("RootB", "1|2|3|4|5", "1", "", "", false),
                            new StuManulClassifyItem("RootC", "1|2|3|4|5", "1", "", "", false),

                            new StuManulClassifyItem("RootC", "1|2|3|4|5", "1", "WroongParent", "1", false)
                        };
                ManulClassifyObligationHelper obManulClassifyHelper = new ManulClassifyObligationHelper(lsManulClassifyItems);
                TransferManulClassification(obManulClassifyHelper);
            }
        }

        static public void TransferManulClassification(ManulClassifyObligationHelper obManulClassifyHelper)
        {
            if ((null != obManulClassifyHelper) && (EMSFB_CLASSIFYTYPE.emManulClassifyObligation == obManulClassifyHelper.GetClassifyType()))
            {
                Console.Write("ClassificationXML:\n[\n\t{0}\n]\n", obManulClassifyHelper.GetClassifyXml());
                List<StuManulClassifyOb> lsStuManulClassifyObs = obManulClassifyHelper.GetStuManulClassifyObs();
                foreach (StuManulClassifyOb stuManulClassifyOb in lsStuManulClassifyObs)
                {
                    int i = 0;
                    StuManulClassifyOb obNextStuManulClassifyOb = stuManulClassifyOb.GetNodeByIndex(i);
                    while (null != obNextStuManulClassifyOb)
                    {
                        string strParentName = "";
                        if (null != obNextStuManulClassifyOb.ParentManulClassifyOb)
                        {
                            strParentName = obNextStuManulClassifyOb.ParentManulClassifyOb.ManulClassifyItem.Name;
                        }
                        Console.Write("{0}:{1}, RelyOn:[{2}] parent:[{3},{4},{5}]\n", i, obNextStuManulClassifyOb.ManulClassifyItem.Name, CommonHelper.JoinList(obNextStuManulClassifyOb.ManulClassifyItem.RelyOns, "|"), 
                            strParentName, obNextStuManulClassifyOb.ManulClassifyItem.ParentName, obNextStuManulClassifyOb.ManulClassifyItem.ParentIndex);

                        obNextStuManulClassifyOb = stuManulClassifyOb.GetNodeByIndex(++i);
                    }
                    Console.Write("Count:[{0}]\n", i);
                }
            }
        }
    }
}
