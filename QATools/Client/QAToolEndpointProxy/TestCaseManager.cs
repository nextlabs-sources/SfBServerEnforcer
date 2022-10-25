using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Diagnostics;
using QAToolSFBCommon.Common;

namespace NLLyncEndpointProxy
{
    enum TEST_CASE_TYPE
    {
        Unknown,
        ChatRoomUploadFile,
        ChatRoomDownloadFile,
    };

    enum TEST_CASE_STATUS
    {
        ready,
        running,
        finished,
    };

    class TestCaseManager
    {
        private List<TestCase> m_lstTestCase = new List<TestCase>();
        private static TestCaseManager m_instance = null;

        public static TestCaseManager GetInstance()
        {
            if(null==m_instance)
            {
                m_instance = new TestCaseManager();
            }
            return m_instance;
        }


        private TestCaseManager()
        { }

        public int GetTestCaseCount()
        {
            return m_lstTestCase.Count;
        }

        public void LoadTestCaseFromFile(string strCaseFile)
        {
            m_lstTestCase.Clear();

            //open xml
            XmlDocument xmldoc = new XmlDocument();
            try
            {
                xmldoc.Load(strCaseFile);
            }
            catch(Exception ex)
            {
                Trace.WriteLine(string.Format("LoadTestCaseFromFile exception:{0}", ex.ToString()));
            }
            
            //read
            try
            {
                XmlNodeList testCaseNodeList = xmldoc.GetElementsByTagName("TestCase");
                foreach (XmlNode caseNode in testCaseNodeList)
                {
                    int nChildNodeCount = caseNode.ChildNodes.Count;
                    if(nChildNodeCount<2)
                    {
                        Trace.WriteLine("Error TestCase node, child node < 2");
                        continue;
                    }

                    //read user info
                    XmlNode nodeUserInfo = caseNode.FirstChild;
                    if(!nodeUserInfo.Name.Equals("user", StringComparison.OrdinalIgnoreCase))
                    {
                        Trace.WriteLine("Error, the first child node of testcase is not User.");
                        continue;
                    }

               
                    string strServerFQDN = nodeUserInfo.Attributes["serverfqdn"].Value;
                    string strUserName =   nodeUserInfo.Attributes["username"].Value;
                    string strUserDomain=  nodeUserInfo.Attributes["userdomain"].Value;
                    string strUserPwd =    nodeUserInfo.Attributes["password"].Value;
                    string strUserUri =    nodeUserInfo.Attributes["useruri"].Value;

                    Trace.WriteLine(string.Format("Find user:FQDN={0}, UserName={1}, Domain={2}, Pwd={3}, Uri={4}", strServerFQDN, strUserName, strUserDomain, strUserPwd, strUserUri));
                    STUSFB_ENDPOINTPROXYACCOUNT account = new STUSFB_ENDPOINTPROXYACCOUNT(EMSFB_ENDPOINTTTYPE.emTypePerformanceTester,
                                                                                           strServerFQDN,
                                                                                           strUserDomain,
                                                                                           strUserName,
                                                                                           strUserPwd,
                                                                                           strUserUri);
                    NLLyncEndpoint nlEndPoint = new NLLyncEndpoint(account);

                    //create new test case
                    TestCase newCase = new TestCase(nlEndPoint);


                    //load actions
                    for(int nAction=1; nAction<nChildNodeCount; nAction++)
                    {
                        XmlNode nodeAction = caseNode.ChildNodes.Item(nAction);

                        if(nodeAction.Name.Equals("action", StringComparison.OrdinalIgnoreCase))
                        {
                            TEST_CASE_TYPE caseType = GetCaseType(nodeAction.Attributes["type"].Value);
                            string strChatRoom = nodeAction.SelectSingleNode("ChatRoom").InnerText;
                            XmlNode filesNode = nodeAction.SelectSingleNode("Files");
                            string strFileFolder = filesNode.InnerText;
                            string strFileCount = filesNode.Attributes["count"].Value;
                            string strLogFile = nodeAction.SelectSingleNode("result").InnerText;

                            Trace.WriteLine(string.Format("Find case, type:{0}, room:{1}, files:{2}, count:{3}, log:{4}", caseType.ToString(), strChatRoom, strFileFolder, strFileCount, strLogFile));

                            if (caseType == TEST_CASE_TYPE.ChatRoomUploadFile)
                            {
                                TestCaseActionChatRoomUploadFile roomUploadCase = new TestCaseActionChatRoomUploadFile(strChatRoom, strFileFolder, int.Parse(strFileCount), strLogFile);
                                newCase.AddTestCaseAction(roomUploadCase);
                            }
                            else if (caseType == TEST_CASE_TYPE.ChatRoomDownloadFile)
                            {
                                TestCaseActionChatRoomDownloadFile roomDownloadCase = new TestCaseActionChatRoomDownloadFile(strChatRoom, strFileFolder, int.Parse(strFileCount), strLogFile);
                                newCase.AddTestCaseAction(roomDownloadCase);
                            }
                        }
                        else
                        {
                            Trace.WriteLine("Node error!!!, it name is not action");
                        }
                    }


                    m_lstTestCase.Add(newCase);
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine(string.Format("LoadTestCaseFromFile exception on read test case. {0}", ex.ToString()));
            }
        }

        public bool ConnectToRoomServer()
        {
            foreach (TestCase testCase in m_lstTestCase)
            {
                if (!testCase.ConnectToChatRoomServer())
                {
                    return false;
                }
            }

            return true;
        }

        public void DisConnectFromRoomServer()
        {
            foreach (TestCase testCase in m_lstTestCase)
            {
                testCase.DisConnectFromRoomServer();
            }
        }

        public void RunCases(Action<int> finishCasesCallBack)
        {
            //run test case
            foreach(TestCase testCase in m_lstTestCase)
            {
                testCase.Run(finishCasesCallBack);
            }
        }

        public bool IsAllCaseFinish()
        {
            foreach (TestCase testCase in m_lstTestCase)
            {
               if(testCase.IsRunning())
               {
                   return false;
               }
            }

            return true;
        }

        private TEST_CASE_TYPE GetCaseType(string strCaseType)
        {
            if(strCaseType.Equals("ChatRoomDownloadFile", StringComparison.OrdinalIgnoreCase) )
            {
                return TEST_CASE_TYPE.ChatRoomDownloadFile;
            }
            else if(strCaseType.Equals("ChatRoomUploadFile", StringComparison.OrdinalIgnoreCase))
            {
                return TEST_CASE_TYPE.ChatRoomUploadFile;
            }
            else
            {
                return TEST_CASE_TYPE.Unknown;
            }
        }
    }
}
