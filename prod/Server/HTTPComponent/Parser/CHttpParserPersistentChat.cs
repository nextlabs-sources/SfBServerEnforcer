using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Diagnostics;

using Nextlabs.SFBServerEnforcer.HTTPComponent.ResponseFilters;

namespace Nextlabs.SFBServerEnforcer.HTTPComponent.Parser
{
    class CHttpParserPersistentChat : CHttpParserBase
    {
        public CHttpParserPersistentChat(HttpRequest request): base(request, REQUEST_TYPE.REQUEST_CHATROOM_MAIN)
        {
        }

        protected override ResponseFilter CreateResponseFilter(HttpResponse httpResponse)
        {
            if((null != httpResponse) && (null != httpResponse.Filter))
            {
                return new ResponseFilterPersistentChat(httpResponse);
            }
            return null;
        }

    }
}
