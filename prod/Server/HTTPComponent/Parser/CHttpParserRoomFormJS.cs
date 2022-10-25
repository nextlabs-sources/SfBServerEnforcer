using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Nextlabs.SFBServerEnforcer.HTTPComponent.ResponseFilters;

namespace Nextlabs.SFBServerEnforcer.HTTPComponent.Parser
{
    class CHttpParserRoomFormJS : CHttpParserBase
    {
        public CHttpParserRoomFormJS(HttpRequest request): base(request, REQUEST_TYPE.REQUEST_CHATROOM_MAIN)
        {
        }

        #region Override: CHttpParserBase
        override protected ResponseFilter CreateResponseFilter(HttpResponse httpResponse)
        {
            if((null != httpResponse) && (null != httpResponse.Filter))
            {
                return new ResponseFilterRoomFormJS(httpResponse);
            }
            return null;
        }
        #endregion
    }
}
