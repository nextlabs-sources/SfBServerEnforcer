using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using SFBCommon.NLLog;
using SFBCommon.SFBObjectInfo;

namespace NLChatRoomAssistantWebService.Services
{
    /// <summary>
    /// Summary description for ChatRoomClassifyAddInService
    /// </summary>
    [WebService(Namespace = "https://www.nextlabs.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class ChatRoomClassifyAddInService : System.Web.Services.WebService
    {
        private static CLog logger = CLog.GetLogger(typeof(ChatRoomClassifyAddInService));

        private SFBChatRoomVariableInfo sfbChatRoomVar;

        #region .Ctor
        public ChatRoomClassifyAddInService()
        {
            this.sfbChatRoomVar = new SFBChatRoomVariableInfo();
        }
        #endregion

        #region Open API
        [WebMethod]
        public string GetChatRoomClassifyTagsByUri(string uri)
        {
            string strTagsXml = "";

            if (this.sfbChatRoomVar != null)
            {
                if (!string.IsNullOrEmpty(uri))
                {
                    if (sfbChatRoomVar.EstablishObjFormPersistentInfo(SFBChatRoomVariableInfo.kstrUriFieldName, uri))
                    {
                        strTagsXml = sfbChatRoomVar.GetItemValue(SFBChatRoomVariableInfo.kstrClassifyTagsFieldName);
                    }
                    else
                    {
                        logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "GetChatRoomClassifyTagsByUri() failed, EstablishObjFormPersistentInfo() failed.");
                    }
                }
                else
                {
                    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "GetChatRoomClassifyTagsByUri() failed, chatroom uri is {0}.", uri);
                }
            }
            else
            {
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "GetChatRoomClassifyTagsByUri() failed, sfbChatRoomVar is null.");
            }

            return strTagsXml;
        }
        #endregion

        #region Inner Tools
        
        #endregion
    }
}
