using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Diagnostics;
using System.Xml;
using SFBCommon.SFBObjectInfo;
using SFBCommon.NLLog;

using Nextlabs.SFBServerEnforcer.HTTPComponent.Data;
using Nextlabs.SFBServerEnforcer.HTTPComponent.RequestFilters;
using Nextlabs.SFBServerEnforcer.HTTPComponent.ResponseFilters;

namespace Nextlabs.SFBServerEnforcer.HTTPComponent.Parser
{
    class CHttpParserManageHandler : CHttpParserBase
    {
        #region Construnctors
        public CHttpParserManageHandler(HttpRequest request) : base(request, REQUEST_TYPE.REQUEST_MANAGER_HANDLE)
        {
        }
        #endregion

        #region Override: CHttpParserBase public
        public override void PreRequestHandler(object source, EventArgs e)
        {
            try
            {
                HttpApplication application = (HttpApplication)source;
                HttpContext context = application.Context;

                //replace request filter, this must be placed at the entry, before any other code.
                ReplaceRequestFilter(context.Request);
                RequestFilterManageHandler requestFilter = m_newRequestFilter as RequestFilterManageHandler;
                requestFilter.SetHttpApplication(application);
                

                //replace response filter
                ReplaceResponseFilter(context.Response);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, ex.ToString());
            }
        }
        public override void EndRequest(object source, EventArgs e)
        {
            try
            {
                RequestFilterManageHandler requestFilter = m_newRequestFilter as RequestFilterManageHandler;
                if (requestFilter.GetManageHandleType() == MANAGEHANDLE_TYPE.MANAGEHANDLE_CREATEROOM)
                {
                    HttpApplication application = (HttpApplication)source;
                    HttpContext context = application.Context;

                    //get response status
                    ResponseFilterManageHandler responseFilter = m_newResponseFilter as ResponseFilterManageHandler;
                    if (responseFilter.GetRoomOperatorResult().bSuccess)
                    {
                        RequestFilterManageHandler.AddDelyProcessChatRoomInfo(requestFilter.GetCurrentChatRoomInfo());
                    }
                }
                else if (requestFilter.GetManageHandleType() == MANAGEHANDLE_TYPE.MANAGEHANDLE_GETALLROOM)
                {
                    EndRequestForGetAllRoom();
                }
                else if (requestFilter.GetManageHandleType() == MANAGEHANDLE_TYPE.MANAGEHANDLE_MODIFYROOM)
                {
                    EndRequestForModifyRoom(source, e);
                }
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "exception on CHttpParserManageHandler::EndRequest:" + ex.ToString());
            }
            base.EndRequest(source, e);
        }
        #endregion

        #region Override: CHttpParserBase protected
        protected override RequestFilter CreateRequestFilter(HttpRequest request)
        {
            if (m_newRequestFilter == null)
            {
                m_newRequestFilter = new RequestFilterManageHandler(request);
            }

            return m_newRequestFilter;
        }

        protected override ResponseFilter CreateResponseFilter(HttpResponse httpResponse)
        {
            if (m_newResponseFilter == null)
            {
                RequestFilterManageHandler reqFilter = m_newRequestFilter as RequestFilterManageHandler;
                m_newResponseFilter = new ResponseFilterManageHandler(httpResponse, reqFilter);
            }

            return m_newResponseFilter;
        }
        #endregion

        #region Inner tools
        private void EndRequestForModifyRoom(object source, EventArgs e)
        {
            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;

            //get response status
            ResponseFilterManageHandler responseFilter = m_newResponseFilter as ResponseFilterManageHandler;
            if (responseFilter.GetRoomOperatorResult().bSuccess)
            {
                RequestFilterManageHandler requestFilter = m_newRequestFilter as RequestFilterManageHandler;

                //save
                requestFilter.GetCurrentChatRoomInfo().SaveToDataBase();
            }
        }
        private void EndRequestForGetAllRoom()
        {
            ResponseFilterManageHandler responseFilter = m_newResponseFilter as ResponseFilterManageHandler;
            RequestFilterManageHandler requestFilter = m_newRequestFilter as RequestFilterManageHandler;

            List<LandResultRoom> lstLandingResult = responseFilter.GetLandingResult();
            foreach(LandResultRoom landRoom in lstLandingResult)
            {
                HttpChatRoomInfo httpChatRoomInfo = RequestFilterManageHandler.GetDelayProcessChatRoomInfoByName(landRoom.Name);
                if(httpChatRoomInfo!=null)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Find delay process room" + landRoom.Name);
                    httpChatRoomInfo.SetRoomID(landRoom.ChatRoomGuid);

                    httpChatRoomInfo.SaveToDataBase();

                    RequestFilterManageHandler.RemoveFromDelyProcessChatRoomInfo(httpChatRoomInfo);
                }
            }

        }
        #endregion
    }
}
