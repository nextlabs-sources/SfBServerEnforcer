using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChatRoomClassifyAddIn.Helpers
{
    public class ThreadHelper
    {
        public static void WorkInBackground(Action target)
        {
            ThreadPool.QueueUserWorkItem(state => { target(); });
        }

        public static void WorkInBackground(Action<object> target, object state)
        {
            ThreadPool.QueueUserWorkItem(obj => { target(obj); }, state);
        }
    }
}
