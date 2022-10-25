using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Configuration;
using System.Xml;
using System.Threading;
using SFBCommon.CommandHelper;
using SFBCommon.NLLog;
using Nextlabs.SFBServerEnforcer.PolicyHelper;
using SFBCommon.SFBObjectInfo;
using SFBCommon.Common;

using Nextlabs.SFBServerEnforcer.SIPComponent.Common;
using Nextlabs.SFBServerEnforcer.SIPComponent.DataManagement;
using Nextlabs.SFBServerEnforcer.SIPComponent.Policy;

namespace Nextlabs.SFBServerEnforcer.SIPComponent
{
    class ContactRequest
    {
        public string m_strRequestGuid;
        public AutoResetEvent m_notifyEvent;
        public string m_strResponse;
    }

    class ContactToEndpointProxy
    {
        protected Socket m_serverSocket;
        protected IPEndPoint m_serverEndpoint;

        protected string m_strProxySipName="";
        
        static protected ContactToEndpointProxy m_agentEndpointContact;
        static protected ContactToEndpointProxy m_assistantEndpointContact;

        private Object m_lockLstRequest = new object();
        private List<ContactRequest> m_lstRequest;

        public string ProxySipName { get { return m_strProxySipName; } set { m_strProxySipName = value; } }

        private static CLog theLog = CLog.GetLogger("SIPComponent:ContactToEndpointProxy");

        Thread m_threadRecvData;
        Thread m_threadGetEndpointProxyInfo;

        public bool IsEndpointProxy(string strUser)
        {
           return (m_strProxySipName.IndexOf(strUser, StringComparison.OrdinalIgnoreCase)!=-1);
        }

        static ContactToEndpointProxy()
        {
            m_agentEndpointContact = new ContactToEndpointProxy(EMSFB_ENDPOINTTTYPE.emTypeAgent);
            m_assistantEndpointContact = new ContactToEndpointProxy(EMSFB_ENDPOINTTTYPE.emTypeAssistant);
        }
        public static ContactToEndpointProxy GetAgentProxyContact()
        {
            return m_agentEndpointContact;
        }
        public static ContactToEndpointProxy GetAssistantProxyContact()
        {
            return m_assistantEndpointContact;
        }

        protected ContactToEndpointProxy(EMSFB_ENDPOINTTTYPE emEndpointType) 
        {
            m_lstRequest = new List<ContactRequest>();

            //read Endpointproxy IP and Port
            try
            {
                STUSFB_ENDPOINTPROXYTCPINFO stuEndpointProxyTcpInfo = SIPComponentConfig.GetEndpointProxyTcpInfo(emEndpointType);
                string strProxyIP = stuEndpointProxyTcpInfo.m_strAddr;
                int nPort = (int)stuEndpointProxyTcpInfo.m_unPort;

                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "EndpointProxy Config addr={0}, port={1}", strProxyIP, nPort);

                IPAddress[] addresslist = Dns.GetHostAddresses(strProxyIP);
                foreach(IPAddress address in addresslist)
                {
                    if(address.AddressFamily == AddressFamily.InterNetwork) //IPv4
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "EndpointProxy parse result IP={0}", address.ToString() );
                        m_serverEndpoint = new IPEndPoint(address, nPort);
                        break;
                    }
                }

            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception happen on ContactToEndpointProxy CreateSocket," + ex.ToString());
                return;
            }

            //create UDP socket and bind
            try
            {
                m_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPEndPoint localIPEndPoint = new IPEndPoint(IPAddress.Any, 0 /*0 means any port*/ );
                m_serverSocket.Bind(localIPEndPoint);
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception happen on ContactToEndpointProxy create and bind socket," + ex.ToString());
                return;
            }


            //start a thread to receive data from that socket
            m_threadRecvData = new Thread(new ThreadStart(this.RecvData));
            m_threadRecvData.Start();  
 
        }
        protected ContactToEndpointProxy(ContactToEndpointProxy con) { }

        protected void SendMessage(string strMsg)
        {
            if(null != m_serverEndpoint)
            {
                byte[] data = Encoding.ASCII.GetBytes(strMsg);
                m_serverSocket.SendTo(data, data.Length, SocketFlags.None, m_serverEndpoint);
            }
        }

        protected void RecvData()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Enter RecvData.");

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = sender as EndPoint;
            byte[] data = new byte[2048];

            while (true)
            {
                int nRecv = 0;
                try
                {
                    nRecv = m_serverSocket.ReceiveFrom(data, ref Remote);
                }
                catch (ObjectDisposedException)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "ContactToEndpointProxy.RecvData thread abort.");
                    break;
                }
                catch (Exception ex)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Exception on m_serverSocket.ReceiveFrom:, may be the Endpoint is not start. {0}", ex.ToString());
                }
               
                if (nRecv > 0)
                {
                    string strRecv = Encoding.ASCII.GetString(data, 0, nRecv);
                    string strCmdGuid = SFBCommon.CommandHelper.CommandHelper.GetCommandID(strRecv);

                    ContactRequest reqeust = GetRequestByID(strCmdGuid);
                    if(reqeust!=null)
                    {
                        if(reqeust.m_notifyEvent != null)
                        {
                            reqeust.m_strResponse = strRecv;
                            reqeust.m_notifyEvent.Set();
                        }
                    }
                    else
                    {
                        //process message here

                    }
                }
            }

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Exit RecvData.");
        }

        public void Stop()
        {
            //abort recv data thread.
            if (m_serverSocket != null)
            {
                m_serverSocket.Close();
            }
            if (m_threadRecvData != null)
            {
                m_threadRecvData.Join();
                m_threadRecvData = null;
            }

            //abort get endpointproxy info thread
            if (m_threadGetEndpointProxyInfo != null)
            {
                m_threadGetEndpointProxyInfo.Abort();
                m_threadGetEndpointProxyInfo.Join();
                m_threadGetEndpointProxyInfo = null;
            }
        }

  
        public void SendMessageToUser(string strToUser, string strHeader, string strBody)
        {
            NotifyCommandHelper notifyCmdHelp = new NotifyCommandHelper(strToUser, "", strHeader, strBody);
            SendMessage(notifyCmdHelp.StrXmlNotifyInfo);

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Send notify: message={0}",  notifyCmdHelp.StrXmlNotifyInfo);
        }

        public void SendMessageToUser(string strNotifyXml, CConference conf, SFBMeetingVariableInfo meetingVar, string strInviter, string strInvitee, string strPolicyName)
        {
            List<string> lstDistinctParticipant = null;
            if(null!=meetingVar)
            {
                lstDistinctParticipant = SFBCommon.SFBObjectInfo.SFBParticipantManager.GetDistinctParticipantsAsList(meetingVar);
            }
            string strParticipate = (lstDistinctParticipant == null) ? "" : CommonHelper.JoinList(lstDistinctParticipant, SFBObjectClassifyInfo.kstrMemberSeparator);

            //construct diction
            Dictionary<string, string> dicWildcard = new Dictionary<string, string>();
            dicWildcard.Add(CommandHelper.kstrWildcardType, "meeting");
            dicWildcard.Add(CommandHelper.kstrWildcardCreator,  conf.Creator);
            dicWildcard.Add(CommandHelper.kstrWildcardSfbObjUri, conf.FocusUri);
            dicWildcard.Add(CommandHelper.kstrWildcardParticipates, strParticipate );
            dicWildcard.Add(CommandHelper.kstrWildcardInviter,  strInviter);
            dicWildcard.Add(CommandHelper.kstrWildcardInvitee, strInvitee);
            dicWildcard.Add(CommandHelper.kstrWildcardPolicyName, strPolicyName);
            dicWildcard.Add(CommandHelper.kstrWildcardNLClassifyTagsName, meetingVar.GetStringTags());

            //Parse wildcard anchor. this used to send message to special users(e.g. user.department="QA")
            if(!string.IsNullOrWhiteSpace(strParticipate))
            {
                List<string> lsWildcardAnchor = NotifyCommandHelper.GetWildcardAnchorInfo(strNotifyXml);
                if ((lsWildcardAnchor != null) && (lsWildcardAnchor.Count > 0))
                {
                    Dictionary<string, string> dicWildcardValue = GetWildcardAnchorValue(lsWildcardAnchor, lstDistinctParticipant);
                    AddedWildcardAnchorValue(dicWildcard, dicWildcardValue);
                }
            }
     

            //send message
            NotifyCommandHelper notifyCmdHelp = new NotifyCommandHelper(strNotifyXml, dicWildcard);
            SendMessage(notifyCmdHelp.StrXmlNotifyInfo);

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Send notify: message{0}", notifyCmdHelp.StrXmlNotifyInfo);

        }

        public void SendMessageToUser(string strNotifyXml, CChatRoom chatRoom, SFBChatRoomVariableInfo roomVar, string strInviter, string strInvitee, string strPolicyName)
        {
            List<string> lstDistinctParticipant = null;
            if (roomVar!= null)
            {
                lstDistinctParticipant = SFBParticipantManager.GetDistinctParticipantsAsList(roomVar);
            }
            string strParticipate = (lstDistinctParticipant == null) ? "" : CommonHelper.JoinList(lstDistinctParticipant, SFBObjectClassifyInfo.kstrMemberSeparator);

            //construct diction
            Dictionary<string, string> dicWildcard = new Dictionary<string, string>();
            dicWildcard.Add(CommandHelper.kstrWildcardType, "chatroom");
            dicWildcard.Add(CommandHelper.kstrWildcardCreator, chatRoom.CreatorUri);
            dicWildcard.Add(CommandHelper.kstrWildcardSfbObjUri, chatRoom.ID);
            dicWildcard.Add(CommandHelper.kstrWildcardParticipates, strParticipate );
            dicWildcard.Add(CommandHelper.kstrWildcardInviter, strInviter);
            dicWildcard.Add(CommandHelper.kstrWildcardInvitee,  strInvitee);
            dicWildcard.Add(CommandHelper.kstrWildcardChatRoomName, chatRoom.Name);
            dicWildcard.Add(CommandHelper.kstrWildcardChatRoomMembers, SFBCommon.Common.CommonHelper.GetSolidString(chatRoom.Members));   // For open chat room, the members property maybe null
            dicWildcard.Add(CommandHelper.kstrWildcardChatRoomManagers, SFBCommon.Common.CommonHelper.GetSolidString(chatRoom.Managers)); // Not all chat room has managers, the managers property maybe null
            dicWildcard.Add(CommandHelper.kstrWildcardPolicyName, strPolicyName);
            dicWildcard.Add(CommandHelper.kstrWildcardNLClassifyTagsName, roomVar.GetStringTags());

            //Parse wildcard anchor. this used to send message to special users(e.g. user.department="QA")
            if (!string.IsNullOrWhiteSpace(strParticipate))
            {
                List<string> lsWildcardAnchor = NotifyCommandHelper.GetWildcardAnchorInfo(strNotifyXml);
                if ((lsWildcardAnchor != null) && (lsWildcardAnchor.Count > 0))
                {
                    Dictionary<string, string> dicWildcardValue = GetWildcardAnchorValue(lsWildcardAnchor, lstDistinctParticipant);
                    AddedWildcardAnchorValue(dicWildcard, dicWildcardValue);
                }
            }

            //send message
            NotifyCommandHelper notifyCmdHelp = new NotifyCommandHelper(strNotifyXml, dicWildcard);
            SendMessage(notifyCmdHelp.StrXmlNotifyInfo);

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Send notify: message{0}", notifyCmdHelp.StrXmlNotifyInfo);

        }

        public void GetEndpointProxyInfoThread()
        {
           theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Enter GetEndpointProxyInfoThread.");
           EndpointProxyInfoCommandHelper endpointInfoCmdHelp = new EndpointProxyInfoCommandHelper();
           string strReqXml = endpointInfoCmdHelp.GetCommandXml(EMSFB_COMMAND.emCommandGetEndpointProxyInfo);

           //save request before send
           ContactRequest req = new ContactRequest();
           req.m_strRequestGuid = endpointInfoCmdHelp.GetCommandID();
           req.m_notifyEvent = new AutoResetEvent(false);
           AddRequest(req);

           while(true)
           {
               //send message
               SendMessage(strReqXml);

               //wait response
               if (req.m_notifyEvent.WaitOne(5 * 1000))
               {
                   EndpointProxyInfoCommandHelper endpointInfoCmdHelpResponse = new EndpointProxyInfoCommandHelper(req.m_strResponse);
                   STUSFB_ENDPOINTINFO endpointInfo = endpointInfoCmdHelpResponse.GetEndpointProxyInfo();
                   m_strProxySipName = endpointInfo.m_strSipUri;

                   theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Get EndpointProxy information success. name:" + m_strProxySipName);

                   RemoveRequest(req);
                   break;
               }
               else
               {//timeout
                   theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Haven't get EndpointProxy information, will try again.");
               }
           }

           theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Exit GetEndpointProxyInfoThread.");

        }

        //we start a thread to get endpointproxy information, because if endpointproxy haven't started, we must wait untill it start.
        public void InitEndpointProxyInfo()
        {
            m_threadGetEndpointProxyInfo = new Thread(this.GetEndpointProxyInfoThread);
            m_threadGetEndpointProxyInfo.Start();
        }

        public void AddRequest(ContactRequest req)
        {
            lock (m_lockLstRequest)
            {
                m_lstRequest.Add(req);
            }
        }
        
        public void RemoveRequest(ContactRequest req)
        {
            lock (m_lockLstRequest)
            {
                m_lstRequest.Remove(req);
            }
        }

        private ContactRequest GetRequestByID(string strID)
        {
            lock(m_lockLstRequest)
            {
                foreach (ContactRequest request in m_lstRequest)
                {
                    if (request.m_strRequestGuid.Equals(strID, StringComparison.OrdinalIgnoreCase))
                    {
                        return request;
                    }
                }
            }
            return null;
        }


        private Dictionary<string, string> GetWildcardAnchorValue(List<string> lstWildcardAnchor, List<string> lstParticipate)
        {
            //map each participate with every WildcardAnchor
            KeyValuePair<string, string>[] arrayBaseRequestData = new KeyValuePair<string, string>[lstWildcardAnchor.Count * lstParticipate.Count];
            int nBaseDataIndex = 0;
            foreach(string strWildcard in lstWildcardAnchor)
            {
                foreach(string strParticipate in lstParticipate)
                {
                    arrayBaseRequestData[nBaseDataIndex] = new KeyValuePair<string, string>(strWildcard, strParticipate);
                    nBaseDataIndex++;
                }
            }

            //create query policy
            PolicyResult[] arrayPolicyResult = null;
            int nPolicyQueryResult = CPolicy.Instance().QueryPolicyForWildcardAnchorMulti(arrayBaseRequestData, out arrayPolicyResult);

            //match policy
            if(nPolicyQueryResult!=0)
            {
                Dictionary<string, string> dicWildcardValue = new Dictionary<string, string>();
                for (int nIndexResult = 0; nIndexResult < arrayPolicyResult.Length; nIndexResult++)
                {
                    if(arrayPolicyResult[nIndexResult].IsAllow())
                    {
                        string strValue = null;
                        dicWildcardValue.TryGetValue(arrayBaseRequestData[nIndexResult].Key, out strValue);
                        if (string.IsNullOrWhiteSpace(strValue))
                        {
                            dicWildcardValue[arrayBaseRequestData[nIndexResult].Key] = "";
                        }

                        dicWildcardValue[arrayBaseRequestData[nIndexResult].Key] += arrayBaseRequestData[nIndexResult].Value;
                        dicWildcardValue[arrayBaseRequestData[nIndexResult].Key] += SFBObjectInfo.kstrMemberSeparator;
                    }     
                }
                return dicWildcardValue;
            }

            return null;
        }

        private void AddedWildcardAnchorValue(Dictionary<string,string> dicWildcard, Dictionary<string,string> dicWildcardValue )
        {
            if(dicWildcardValue!=null)
            {
                foreach(KeyValuePair<string,string> pairValue in dicWildcardValue)
                {
                    string strKey = CommandHelper.kstrWildcardAnchorKey + CommandHelper.kchSepWildcardAnchorKeyAndValue + pairValue.Key;
                    SFBCommon.Common.CommonHelper.AddKeyValuesToDir(dicWildcard, strKey, pairValue.Value);
                }
            }
        }

        public void NotifyUserToClassifyMeeting(string strToUser, CConference conf)
        {
            ClassifyCommandHelper clsCmdHelper = new ClassifyCommandHelper(EMSFB_COMMAND.emCommandClassifyMeeting, CSIPTools.SIP_URI_PREFIX + strToUser, conf.FocusUri);
            string strNotifyXml = clsCmdHelper.GetCommandXml();
            this.SendMessage(strNotifyXml);
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Send meeting manual classify notify:{0}", strNotifyXml);
        }
    }
}
