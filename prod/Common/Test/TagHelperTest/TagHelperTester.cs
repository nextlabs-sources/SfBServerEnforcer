using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TagHelper;

namespace TestProject.TagHelperTest
{
    class TagHelperTester
    {
        #region Public functions
        static public void Test(string strFilePath)
        {
            
            TagMain obTagMain = new TagMain();

            {
                Console.Write("Begin read tags\n");
                // ReadTag
                Dictionary<string, string> dicTags = obTagMain.ReadTagEx(strFilePath);
                if (null != dicTags)
                {
                    foreach (KeyValuePair<string, string> pairTag in dicTags)
                    {
                        Console.Write("TagName:[{0}], TagValue:[{1}]\n", pairTag.Key, pairTag.Value);
                    }
                }
                else
                {
                    Console.Write("Read tags failed\n");
                }
                Console.Write("End read tags\n");
            }

            {
                Console.Write("Begin write tags\n");
                // Write tag
                Dictionary<string, string> dicNewTags = new Dictionary<string,string>()
                {
                    {"strTagName1", "strTagValue1"},
                    {"strTagName2", "strTagValue2"},
                    {"strTagName3", "strTagValue3"}
                };
                bool bRet = obTagMain.WriteTag(strFilePath, dicNewTags);
                if (bRet)
                {
                    Dictionary<string, string> dicTags = obTagMain.ReadTagEx(strFilePath);
                    if (null != dicTags)
                    {
                        foreach (KeyValuePair<string, string> pairTag in dicTags)
                        {
                            Console.Write("TagName:[{0}], TagValue:[{1}]\n", pairTag.Key, pairTag.Value);
                        }
                    }
                    else
                    {
                        Console.Write("Read tags failed\n");
                    }
                }
                else
                {
                    Console.Write("Write tags failed\n");
                }
                Console.Write("End write tags\n");
            }

            {
                Console.Write("Begin remove tags\n");

                // Remove tags
                Dictionary<string, string> dicNewTags = new Dictionary<string, string>()
                {
                    {"strTagName1", "strTagValue1"}
                };
                bool bRet = obTagMain.RemoveTag(strFilePath, dicNewTags);
                if (bRet)
                {
                    Dictionary<string, string> dicTags = obTagMain.ReadTagEx(strFilePath);
                    if (null != dicTags)
                    {
                        foreach (KeyValuePair<string, string> pairTag in dicTags)
                        {
                            Console.Write("TagName:[{0}], TagValue:[{1}]\n", pairTag.Key, pairTag.Value);
                        }
                    }
                    else
                    {
                        Console.Write("Read tags failed\n");
                    }
                }
                else
                {
                    Console.Write("Remove tags failed\n");
                }
                Console.Write("End remove tags\n");
            }

            {
                Console.Write("Begin remove tags\n");

                // Remove all tags
                bool bRet = obTagMain.RemoveAllTag(strFilePath);
                if (bRet)
                {
                    Dictionary<string, string> dicTags = obTagMain.ReadTagEx(strFilePath);
                    if (null != dicTags)
                    {
                        foreach (KeyValuePair<string, string> pairTag in dicTags)
                        {
                            Console.Write("TagName:[{0}], TagValue:[{1}]\n", pairTag.Key, pairTag.Value);
                        }
                    }
                    else
                    {
                        Console.Write("Read tags failed\n");
                    }
                }
                else
                {
                    Console.Write("Remove all tags failed\n");
                }
                Console.Write("End remove all tags\n");
            }
        }
        #endregion
    }
}
