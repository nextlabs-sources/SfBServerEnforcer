using System;
using System.Net;
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
    public class LogHelper
    {
        public static void Log(string msg)
        {
            MessageBox.Show(msg);
        }

        public static void Log(string type, Exception e)
        {
            Log(string.Format("-------{0}------- \n-------Message------- \n{1} \n-------StackTrace-------\n{2}",type, e.Message, e.StackTrace));
        }
    }
}
