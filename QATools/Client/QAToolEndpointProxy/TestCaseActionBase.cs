using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace NLLyncEndpointProxy
{
    class TestCaseActionBase
    {
        protected TEST_CASE_TYPE CaseType;
        protected TEST_CASE_STATUS Status;
        protected NLChatRoomManager RoomManager = null;
        protected Action<int> FinishTestCaseCallback = null;

        public TestCaseActionBase(TEST_CASE_TYPE caseType)
        {
            this.CaseType = caseType;
            Status = TEST_CASE_STATUS.ready;
        }

        public bool IsRunning()
        {
            return Status == TEST_CASE_STATUS.running;
        }

        public void Run(Action<int> finishTestCaseCallBack, NLChatRoomManager roomManager)
        {
            FinishTestCaseCallback = finishTestCaseCallBack;
            RoomManager = roomManager;
            Status = TEST_CASE_STATUS.running;

            Thread RunThread = new Thread(new ThreadStart(TestCaseWorkThread));
            RunThread.Start();
        }

        public virtual void TestCaseWorkThread()
        {
            Thread.Sleep(2000);
            Status = TEST_CASE_STATUS.finished;

            FinishTestCaseCallback(1);
        }

        public TEST_CASE_TYPE GetCaseType() { return CaseType; }
    }
}
