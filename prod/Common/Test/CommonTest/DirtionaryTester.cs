using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.CommonTest
{
    class DirtionaryTester
    {
        static public Dictionary<string, string> dirInfos = new Dictionary<string,string>()
        {
            {"key1", "value1"},
            {"key2", "value2"},
            {"key3", "value3"},
            {"key4", "value4"}
        };

        static public void Test()
        {
            KeyValuePair<string,string>[] szArray = dirInfos.ToArray();
            foreach (var obItem in szArray)
            {
                Console.Write(obItem.Key + ":" + obItem.Value + "\n");
            }
        }
    }
}
