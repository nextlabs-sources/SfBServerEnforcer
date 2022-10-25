using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Diagnostics;

namespace Nextlabs.SFBServerEnforcer.HTTPComponent.ResponseFilters
{
    class ResponseFilterRoomFormJS : ResponseFilter
    {
        public ResponseFilterRoomFormJS(HttpResponse response)
            : base(response)
        {

        }

        public override void Close()
        {
            //get string content
            m_streamContentBuf.Position = 0;
            StreamReader contentReader = new StreamReader(m_streamContentBuf);
            String strContent = contentReader.ReadToEnd();

            //added call back
            strContent = strContent.Replace("this.On_RMCreateRoomCallBack();", "this.On_NLRMCreateRoomCallBack();");
            strContent = strContent.Replace("this.OnRM_GetNewRoomInfoCallback();", "this.OnRM_NLGetNewRoomInfoCallback();");
            strContent = strContent.Replace("this.On_RMModifyRoomCallBack();", "this.On_NLRMModifyRoomCallBack();");
            

            //modify ShowEditRoom
            strContent = strContent.Replace("this.ShowEditRoom(xml);", "this.NLShowEditRoom(xml);");

            //modify View Card page.
            strContent = strContent.Replace("this.ShowReadOnlyRoom(xml);", "this.NLShowReadOnlyRoom(xml);");

            //write back
            WriteToOldStream(strContent);
            m_oldFilter.Close();

            //log
            theBaseLog.OutputLog(SFBCommon.NLLog.EMSFB_LOGLEVEL.emLogLevelDebug, "Close:Content{0}\n", strContent);

        }
    }
}
