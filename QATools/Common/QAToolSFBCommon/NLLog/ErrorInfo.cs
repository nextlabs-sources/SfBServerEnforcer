using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QAToolSFBCommon.NLLog
{
    public class ErrorInfo
    {
        public int m_nErrorCode;
        public string m_strErrorMessage;

        public ErrorInfo(int nErrorCode, string strErrorMessage)
        {
            m_nErrorCode = nErrorCode;
            m_strErrorMessage = strErrorMessage;
        }
    }
}
