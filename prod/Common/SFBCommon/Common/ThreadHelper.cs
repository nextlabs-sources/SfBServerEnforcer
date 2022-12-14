using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFBCommon.Common
{
    public class ThreadHelper
    {
        #region Asynchronous invoke helper: Thread
        static public Thread AsynchronousThreadInvokeHelper(bool bAsynchronous, Action pFuncAction)
        {
            Thread obThread = null;
            if (bAsynchronous)
            {
                obThread = new Thread(new ThreadStart(pFuncAction));
                obThread.Start();
            }
            else
            {
                pFuncAction();
            }
            return obThread;
        }
        static public Thread AsynchronousThreadInvokeHelper(bool bAsynchronous, Action<object> pFuncAction, object tyInfo)
        {
            Thread obThread = null;
            if (bAsynchronous)
            {
                obThread = new Thread(new ParameterizedThreadStart(pFuncAction));
                obThread.Start(tyInfo);
            }
            else
            {
                pFuncAction(tyInfo);
            }
            return obThread;
        }
        #endregion
        #region Asynchronous invoke helper: Task
        static public void AsynchronousInvokeHelper(bool bAsynchronous, Action pFuncAction)
        {
            if (bAsynchronous)
            {
                Task obTask = new Task(pFuncAction);
                obTask.Start();
            }
            else
            {
                pFuncAction();
            }
        }
        static public void AsynchronousInvokeHelper(bool bAsynchronous, Action<object> pFuncAction, object tyInfo)
        {
            if (bAsynchronous)
            {
                Task obTask = new Task(pFuncAction, tyInfo);
                obTask.Start();
            }
            else
            {
                pFuncAction(tyInfo);
            }
        }
        #endregion
        #region Asynchronous invoke helper: Tread pool
        static public void AsynchronousTheadPoolInvokeHelper(bool bAsynchronous, WaitCallback pCallBack, object obState)
        {
            if (bAsynchronous)
            {
                ThreadPool.QueueUserWorkItem(pCallBack, obState);
            }
            else
            {
                pCallBack(obState);
            }
        }
        #endregion
    }
}
