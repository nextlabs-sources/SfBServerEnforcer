using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.SIPContentHelper
{
    class SIPContentApplicationCcpXml : SIPContent
    {
        #region Construnctors
        public SIPContentApplicationCcpXml(string strContentValue, string strContentInfo) : base(strContentValue, strContentInfo)
        {
        }
        public SIPContentApplicationCcpXml(EMSIP_CONTENT_TYPE emContentType, string strContentInfo) : base(emContentType, strContentInfo)
        {
        }
        #endregion
    }
}
