using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

// Other project
using QAToolSFBCommon.Common;
using QAToolSFBCommon.CommandHelper;
using QAToolSFBCommon.NLLog;

// Current project
using NLLyncEndpointProxy.Common;

namespace NLLyncEndpointProxy.Listener
{
    class STUSFB_SERVERCOMMANDINFO
    {
        public NLLyncEndpoint m_obNLUserEndpoint;
        public string m_strCommandXML;
        public Socket m_obSocket;
        public EndPoint m_obSoketRemoteEndPoint;

        public STUSFB_SERVERCOMMANDINFO(NLLyncEndpoint obNLUserEndpoint, string strCommandXML, Socket obSocket, EndPoint obSoketRemoteEndPoint)
        {
            m_obNLUserEndpoint = obNLUserEndpoint;
            m_strCommandXML = strCommandXML;
            m_obSocket = obSocket;
            m_obSoketRemoteEndPoint = obSoketRemoteEndPoint;
        }
        public STUSFB_SERVERCOMMANDINFO(STUSFB_SERVERCOMMANDINFO stuServerCommandInfo)
        {
            m_obNLUserEndpoint = stuServerCommandInfo.m_obNLUserEndpoint;
            m_strCommandXML = stuServerCommandInfo.m_strCommandXML;
            m_obSocket = stuServerCommandInfo.m_obSocket;
            m_obSoketRemoteEndPoint = stuServerCommandInfo.m_obSoketRemoteEndPoint;
        }
    }

    class CommandProcesser : Logger
    {
        #region Const/Readonly values
        private static Dictionary<EMSFB_COMMAND, WaitCallback> kdicCommandAndCallBackMapping = new Dictionary<EMSFB_COMMAND,WaitCallback>()
        {
            {EMSFB_COMMAND.emCommandUnknown, null},

            {EMSFB_COMMAND.emCommandGetEndpointProxyInfo, CommandProcesser.CallBackSendEndpointInfoToLyncServer},
            {EMSFB_COMMAND.emCommandEndpointProxyInfo, null},   // This the command flag the return info for GetEndpointProxyInfo, this command processed in SIP server side
        };
        #endregion

        public static void SendMessageByCommandInfo(bool bAsynchronous, string strCommandXML, Socket obSocket, EndPoint obSoketRemoteEndPoint)
        {
            try
            {
                STUSFB_SERVERCOMMANDINFO stuServerCommandInfo = new STUSFB_SERVERCOMMANDINFO(NLLyncEndpointProxyMain.s_obNLLyncEndpointProxyObj.CurLyncEndpoint, strCommandXML, obSocket, obSoketRemoteEndPoint);
                EMSFB_COMMAND emCommand = CommandHelper.GetCommandType(strCommandXML);
                WaitCallback pCallBack = CommonHelper.GetValueByKeyFromDir(kdicCommandAndCallBackMapping, emCommand, null);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Command type is:[{0}], call back:[{1}]\n", emCommand, pCallBack);
                if (null != pCallBack)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Command type is:[{0}], call back function:[{1}]\n", emCommand, pCallBack);
                    ThreadHelper.AsynchronousTheadPoolInvokeHelper(bAsynchronous, pCallBack, stuServerCommandInfo);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in SendMessageByCommandInfo, [{0}]\n", ex.Message);
            }
        }

        #region Command call back functions


        static private void CallBackSendEndpointInfoToLyncServer(object obServerCommandInfo)    // object is STUSFB_SERVERCOMMANDINFO
        {
            try
            {
                STUSFB_SERVERCOMMANDINFO stuServerCommandInfo = obServerCommandInfo as STUSFB_SERVERCOMMANDINFO;
                NLLyncEndpoint obNLUserEndpoint = stuServerCommandInfo.m_obNLUserEndpoint;
                Socket obSocket = stuServerCommandInfo.m_obSocket;
                EndPoint obSoketRemoteEndPoint = stuServerCommandInfo.m_obSoketRemoteEndPoint;
                string strCommandXML = stuServerCommandInfo.m_strCommandXML;
                string strEndpointProxyInfo = "";
                try
                {
                    string strID = CommandHelper.GetCommandID(strCommandXML);
                    EndpointProxyInfoCommandHelper obEndpointProxyInfoHelper = new EndpointProxyInfoCommandHelper(new STUSFB_ENDPOINTINFO(obNLUserEndpoint.GetUserSipURI()), strID, EMSFB_STATUS.emStatusOK);
                    strEndpointProxyInfo = obEndpointProxyInfoHelper.GetCommandXml(EMSFB_COMMAND.emCommandEndpointProxyInfo);
                }
                catch (Exception ex)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception, SendTagsToLyncServerListener, [{0}]\n", ex.Message);
                }
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "strUserSipUri: [{0}]\n", strEndpointProxyInfo);
                byte[] data = Encoding.ASCII.GetBytes(strEndpointProxyInfo);
                obSocket.SendTo(data, data.Length, SocketFlags.None, obSoketRemoteEndPoint);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in SendEndpointProxyNameToLyncServerListener, [{0}]\n", ex.Message);
            }
        }

        #endregion

        #region Private tools
        
        #endregion

        
    }
}
