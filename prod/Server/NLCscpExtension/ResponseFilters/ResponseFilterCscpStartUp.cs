using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using SFBCommon.NLLog;

namespace NLCscpExtension.ResponseFilters
{
    class ResponseFilterCscpStartUp : ResponseFilter
    {
        #region Constructors
        public ResponseFilterCscpStartUp(HttpResponse obHttpResponse): base(obHttpResponse)
        {
        }
        #endregion

        #region Override response filter
        override public void Close()
        {
            try
            {
                //get string content
                m_streamContentBuf.Position = 0;
                StreamReader contentReader = new StreamReader(m_streamContentBuf);
                String strContent = contentReader.ReadToEnd();
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Cscp start up response, lenght:[{0}], content:[{1}]\n", strContent.Length, strContent);


                // find silver light control host and reset height
                // <div style="height: calc(100% - 100px);" align="center" id="silverlightControlHost">
                const string kstrSilverLightControlHostPatten = "(<\\s{0,1}div\\s)[^>]*?id\\s*=\\s*\"silverlightControlHost\"(?:>|(?:\\s+[^>]*?>))";
                Regex obReg = new Regex(kstrSilverLightControlHostPatten, RegexOptions.IgnoreCase);
                Match obMatch = obReg.Match(strContent);
                if (obMatch.Success)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Index:[{0}], GroupLength:[{1}]\n", obMatch.Index, obMatch.Groups[1].Length);

                    int nInsertIndex = obMatch.Index + obMatch.Groups[1].Length;
                    const string kstrStyleExteralSetting = "style=\"width: 100%; height: calc(100% - 50px);\" ";
                    // const string kstrXapObjectExteralSetting = "\r\n <param name=\"wmode\" value=\"opaque\" />";
                    // const string kstrXapObjectExteralSetting = "\r\n <param name=\"windowless\" value=\"true\" />";
                    strContent = strContent.Insert(nInsertIndex, kstrStyleExteralSetting);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Cannot find specify div\n");
                }

                // added NEXTLABS extension link
                // Note: can add this dive but hide by silver light object, it take up the whole window.
                const string kstrExtentPostionFlag = "</div>";
                int nPos = strContent.LastIndexOf(kstrExtentPostionFlag, StringComparison.OrdinalIgnoreCase);
                if (-1 != nPos)
                {
                    // const string kstrNLExtentionLink = "<div><a style=\"position:fixed;right:400px;top:300px;z-index:9999\" href=\"./kimtest.html\">NEXTLABS Control Panel</a></div>";
                    const string kstrNLExtentionLink = "<div style=\"width: 100%; height: 50px; bottom: 0px; font-size: 20px; position: fixed; background-color: rgb(230, 241, 244);\" ><a href=\"./kimtest.html\">NEXTLABS Control Panel</a></div>";
                    strContent = strContent.Insert(nPos + kstrExtentPostionFlag.Length, kstrNLExtentionLink);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Cannot file the space to add NL extension link\n");
                }
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Cscp start up response content with NL extention:[{0}]\n", strContent);

                //write back
                WriteToOldStream(strContent);
                m_oldFilter.Close();
            }
            catch(Exception ex)
            {
                theLog.OutputLog(SFBCommon.NLLog.EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in Cscp Startup response filter close, [{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Inner tools
        #endregion

        #region back up code
#if false
        const string kstrXapObjectPattern = "<\\s{0,1}object\\s[\\s\\S]+?>";
        Regex obReg = new Regex(kstrXapObjectPattern, RegexOptions.IgnoreCase);
        Match obMatch = obReg.Match(strContent);
        if (obMatch.Success)
        {
            int nInsertIndex = strContent.IndexOf("<param", obMatch.Index + obMatch.Length);
            nInsertIndex = strContent.IndexOf(">", nInsertIndex) + 1;
            const string kstrXapObjectExteralSetting = "\r\n <param name=\"windowless\" value=\"true\" />\r\n <param name=\"wmode\" value=\"opaque\" />"; // no used, parm windowless analysis failedin IE
            // const string kstrXapObjectExteralSetting = "\r\n <param name=\"wmode\" value=\"opaque\" />";
            // const string kstrXapObjectExteralSetting = "\r\n <param name=\"windowless\" value=\"true\" />";
            strContent = strContent.Insert(nInsertIndex, kstrXapObjectExteralSetting);
        }
        else
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "match xap object area failed\n");
        }
#endif
        #endregion
    }
}
