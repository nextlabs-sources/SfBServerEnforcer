using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TestProject;
using SFBCommon.Common;
using SFBCommon.NLLog;

namespace TestProject.SFBCommonTest
{
    class CommonFunctionTest
    {
        static public void Test()
        {
            string strWholeDesSipUris = "A:B:C:D:E:F:G:a:b:c:d:e:f:g";
            string[] szOrgStrDesSipUris = strWholeDesSipUris.Split(new char[]{':'}, StringSplitOptions.RemoveEmptyEntries);
            List<string> lsOrgStrDesSipUris = szOrgStrDesSipUris.ToList();
            string[] szDistinctStrDesSipUris = lsOrgStrDesSipUris.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();   // Remove repetition items

            CommonTools.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "OrgString:[{0}], OrgSz:[{1}], DistinctSz:[{2}]\n", strWholeDesSipUris, string.Join(":", szOrgStrDesSipUris), string.Join(":", szDistinctStrDesSipUris));
        }

        static public void Test_StandardSipUri()
        {
            string strUser1 = "sip:user1.test@test.com";
            string strUser2 = "Sip:user2.test@test.com";
            string strUser3 = "user3.test@test.com";

            string strSipUser1 = CommonHelper.GetStandardSipUri(strUser1);
            string strSipUser2 = CommonHelper.GetStandardSipUri(strUser2);
            string strSipUser3 = CommonHelper.GetStandardSipUri(strUser3);

            string strWithoutSipUserUri1 = CommonHelper.GetUriWithoutSipHeader(strUser1);
            string strWithoutSipUserUri2 = CommonHelper.GetUriWithoutSipHeader(strUser2);
            string strWithoutSipUserUri3 = CommonHelper.GetUriWithoutSipHeader(strUser3);

            Console.WriteLine("Uris:[{0}],[{1}], [{2}]\n", strUser1, strSipUser1, strWithoutSipUserUri1);
            Console.WriteLine("Uris:[{0}],[{1}], [{2}]\n", strUser2, strSipUser2, strWithoutSipUserUri2);
            Console.WriteLine("Uris:[{0}],[{1}], [{2}]\n", strUser3, strSipUser3, strWithoutSipUserUri3);
        }
    }
}
