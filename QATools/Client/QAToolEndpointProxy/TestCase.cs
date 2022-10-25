using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NLLyncEndpointProxy
{
    class TestCase
    {
        private NLLyncEndpoint m_userEndPoint = null;
        private NLChatRoomManager m_roomManager = null;
        private List<TestCaseActionBase> m_lstActions = new List<TestCaseActionBase>();

        public TestCase(NLLyncEndpoint endpoint)
        {
            m_userEndPoint = endpoint;
        }

        public void AddTestCaseAction(TestCaseActionBase action)
        {
            m_lstActions.Add(action);
        }

        public bool ConnectToChatRoomServer()
        {
            //connect to skype server
            m_userEndPoint.ConnectToServer();
            if(!m_userEndPoint.GetEndpointEstablishFalg())
            {
                Trace.WriteLine("Failed to connect to skype server.");
                return false;
            }

            //connect to chat room server
            if(null == m_roomManager)
            {
                m_roomManager = new NLChatRoomManager(m_userEndPoint.GetUserEndPoint());
            }

            return m_roomManager.ConnectToRoomServer();
        }

        public void DisConnectFromRoomServer()
        {
            if(null!=m_roomManager)
            {
                m_roomManager.LeaveAllChatRoom();
                m_roomManager.DisconnectFromRoomServer();
            }

            if(null!=m_userEndPoint)
            {
                m_userEndPoint.CloseNLUserEndpoint();
            }
            
        }

        public void Run(Action<int> finishTestCaseCallBack)
        {
            foreach(TestCaseActionBase testCase in m_lstActions)
            {
                testCase.Run(finishTestCaseCallBack, m_roomManager);
            }
        }

        public bool IsRunning()
        {
            foreach (TestCaseActionBase testCase in m_lstActions)
            {
                if(testCase.IsRunning())
                {
                    return true;
                }
            }
            return false;
        }


    }
}
