using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

using SFBCommon.Common;
using SFBCommon.CommandHelper;

namespace TestProject.RegularTest
{
    class RegularTester
    {
        static public void Test()
        {
            string strContent = "v=0\n" +
                                "a=connection:new\n" +
                                "a=x-applicationsharing-session-id:1\n" +
                                "a=x-applicationsharing-role:sharer\n" +
                                "a=x-applicationsharing-media-type:rdp\n";

            string strRegPattern = CommonHelper.MakeAsStandardRegularPattern("^a=connection:(?<ContentValue>(.+))$"); // ==> "^a=connection:(.+)$"
            Regex regex = new Regex(strRegPattern, RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(strContent);
            if ((null != matches) && (0 < matches.Count))
            {
                Console.Write("Matched count:[{0}]\n", matches.Count);
                for (int i = 0; i < matches.Count; ++i)
                {
                    Console.Write("Index:{0}, groups count:[{1}], base value:[{2}], ContentValue:[{3}]\n", i, matches[i].Groups.Count, matches[i].Value, matches[i].Groups["ContentValue"].Value);
                    for (int j=0; j<matches[i].Groups.Count; ++j)
                    {
                        Console.Write("\tGroup: Index:{0}, value:[{1}]\n", j, matches[i].Groups[j].Value);
                    }
                }
            }
        }

        static public void TestEx()
        {
            // string strText = "<a class=\"mb-wp-more-link\"></a><a class=\"mb-wp-more-link\"></a>";
            // string strText = "<a class=\"mb-wp-more-link\" href=\"aaa?a\">more...</a><br /><br /><a class=\"mb-wp-more-link\" href=\"bbb\">DocumentsList</a>";
#if false
            string strText1 = "<a class=\"mb-wp-more-link\" href=\"http://emily-sp13.qapf1.qalab01.nextlabs.com/_layouts/15/mobile/viewa.aspx?List=48b10d2a%2D3a85%2D4aa2%2Da149%2De52da2d85ac1&amp;View=3778e97d%2D6d14%2D495e%2D91b5%2Dea87a9c21107\">more...</a>";
            string strText2 = "<br /><br />";
            string strText3 = "<a class=\"mb-wp-title-link\" href=\"http://emily-sp13.qapf1.qalab01.nextlabs.com/_layouts/15/mobile/viewa.aspx?List=da214a48%2Db962%2D4530%2Db7a7%2D849614f50fbe&amp;View=bb73ac0e%2D48a9%2D461b%2Db695%2Dfb89f294d787\">DocumentsList</a>";
            string strText = strText1 + strText2 + strText3;
#endif
            string strText1 = "<a class=\"mb-wp-more-link\" href=\"http://emily-sp13.qapf1.qalab01.nextlabs.com/_layouts/15/mobile/viewa.aspx?List=48b10d2a%2D3a85%2D4aa2%2Da149%2De52da2d85ac1&amp;View=3778e97d%2D6d14%2D495e%2D91b5%2Dea87a9c21107\">more...</a>";
            string strText2 = "<br /><br />";
            string strText3 = "<a class=\"mb-wp-title-link\" href=\"http://emily-sp13.qapf1.qalab01.nextlabs.com/_layouts/15/mobile/viewa.aspx?List=da214a48%2Db962%2D4530%2Db7a7%2D849614f50fbe&amp;View=bb73ac0e%2D48a9%2D461b%2Db695%2Dfb89f294d787\">DocumentsList</a>";
            string strText = strText1 + strText2 + strText3;

            string strRegPatten = "<a\\s+([^>]*?)(\\s*)class=\"mb-wp-more-link\"([^>]*?)>[^<>]*?</a>";

            Regex re = new Regex(strRegPatten);
            MatchCollection matches = re.Matches(strText);

            Console.Write("Count:[{0}]",  matches.Count);
        }

        static public void TestWildcardAnchor()
        {
            string strXmlNotifyInfo = "";
            try
            {
                strXmlNotifyInfo = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><MessageInfo type=\"Notify\">" +
                                    @"<Notification><DesSipUri>\CREATOR;;\USERPLACEHOLDER=KIMTEST;;\USERPLACEHOLDER=KIMTESTWW;;Sip:Kim\USERPLACEHOLDER=KIMTESsssT;Test1.yang@lync.nextlabs.solutions</DesSipUri>" +
                                    @"<Body>A test notify message. \TYPE; creator is \CREATOR;, This me\USERPLACEHOLDER=KIMTEST;ssage is send by Nextlabs enforcer enpoint proxy. The message will tell you why you are blocked by the enforcer.</Body>" +
                                    @"</Notification><Notification><DesSipUri>\CREATOR;;Sip:KimTest1.yang@lync.nextlabs.solutions;\USERPLACEHOLDER=KIAAAMTEST;</DesSipUri>" + 
                                    @"<Body>A test notify message. \TYPE; \USERPLACEHOLDER=KIMTEST; creator is \CREATOR;, This message is send by Nextlabs enforcer enpoint proxy. The message will tell you why you are blocked by the enforcer.</Body>" +
                                    @"</Notification></MessageInfo>";

                if (null != strXmlNotifyInfo)
                {
                    Console.Write("Org notification XML:\n[{0}]\n", strXmlNotifyInfo);

                    List<string> lsWildcardAnchorValues = NotifyCommandHelper.GetWildcardAnchorInfo(strXmlNotifyInfo);
                    foreach (string strWildcardAnchorValue in lsWildcardAnchorValues)
                    {
                        Console.Write("[{0}:{1}]\n", CommandHelper.kstrWildcardAnchorKey, strWildcardAnchorValue);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write( "Exception in GetAnchorWildcardInfo, [{0}]\n[{1}]\n", ex.Message, strXmlNotifyInfo);
            }
        }
    }
}
