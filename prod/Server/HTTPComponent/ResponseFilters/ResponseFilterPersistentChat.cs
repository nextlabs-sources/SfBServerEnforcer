using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Web;

namespace Nextlabs.SFBServerEnforcer.HTTPComponent.ResponseFilters
{
    class ResponseFilterPersistentChat : ResponseFilter
    {
        public ResponseFilterPersistentChat(HttpResponse response)
            : base(response)
        {
        }

        public override void Close()
        {
            try
            {
                //get string content
                m_streamContentBuf.Position = 0;
                StreamReader contentReader = new StreamReader(m_streamContentBuf);
                String strContent = contentReader.ReadToEnd();

                //added customer ui
                int nPos = strContent.IndexOf("Delete this chat room </span>", StringComparison.OrdinalIgnoreCase);
                if (nPos != -1)
                {
                    int nLineEndPos = strContent.IndexOf("</tr>", nPos, StringComparison.OrdinalIgnoreCase);
                    if (nLineEndPos != -1)
                    {
                        string strHtml = "<tr><td class=\"RoomBtItemSpacing\">";
                        strHtml += "<input id=\"cbRoomNeedEnforce\" type=\"checkbox\" /><span id=\"lblNxlControlRoom\" class=\"RoomDisableText\" > Set this room controlled by Nextlabs Skype for Business Enforcer</span>";
                        
                        strHtml += "<img id=\"imgRoomEnforce\" height=\"32\" width=\"32\" src=\"NLResources/room_enforce.png\">";
                        strHtml += "<img id=\"imgRoomNotEnforce\" height=\"32\" width=\"32\" src=\"NLResources/room_notEnforce.png\">&nbsp;";
                        strHtml += "<span id=\"lblNxlRoomEnforceStatusDesc\" class=\"RoomDisableText\" >NextLabs Enforcement is enabled on this chat room</span>";
                        strHtml += "</td></tr>";
                        strContent = strContent.Insert(nLineEndPos + 5, strHtml);
                    }
                }

                //add javascript
                nPos = strContent.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
                if(nPos!=-1)
                {
                    nPos = strContent.LastIndexOf("</script>", nPos, StringComparison.OrdinalIgnoreCase);
                    if(nPos!=-1)
                    {
                        strContent = strContent.Insert(nPos + "</script>".Length, "\r\n<script src=\"NLJScripts/NLRoomForm.js\" type=\"text/javascript\"></script>");
                    }
                }


                //modified the onclick event on "Create button"
                strContent = strContent.Replace("mainWnd.CreateRoom();", "mainWnd.NLCreateRoom();");
               
                //modify the onclick event on "Create A New Room" button
                strContent = strContent.Replace("mainWnd.ShowCreatePage();", "mainWnd.NLShowCreatePage();");

                //modify category change event
                strContent = strContent.Replace("mainWnd.CategoryChange()", "mainWnd.NLCategoryChange()"); 

                //modify commit change button, used for modify chat room
                strContent = strContent.Replace("mainWnd.CommitChanges()", "mainWnd.NLCommitChanges()");

                //write back
                WriteToOldStream(strContent);
                m_oldFilter.Close();

                //log
                theBaseLog.OutputLog(SFBCommon.NLLog.EMSFB_LOGLEVEL.emLogLevelDebug, "Content:{0}", strContent);
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
      
        }
    }
}
