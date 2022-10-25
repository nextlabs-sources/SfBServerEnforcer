using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NLCscpExtension.ResponseFilters;
using SFBCommon.NLLog;

namespace NLCscpExtension.Parser
{
    class CHttpParserCscpStartUp : CHttpParserBase
    {
        #region Constructors
        public CHttpParserCscpStartUp(HttpRequest obHttpRequest) : base(obHttpRequest)
        {

        }
        #endregion

        #region Override CHttpParserBase
        override protected ResponseFilter CreateResponseFilter(HttpResponse httpResponse)
        {
            if ((null != httpResponse) && (null != httpResponse.Filter))
            {
                return new ResponseFilterCscpStartUp(httpResponse);
            }
            return null;
        }
        #endregion
    }
}
