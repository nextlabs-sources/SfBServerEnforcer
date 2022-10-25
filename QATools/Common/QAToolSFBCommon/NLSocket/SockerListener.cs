using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Threading;

// Other project
using QAToolSFBCommon.Common;
using QAToolSFBCommon.NLLog;

// Current project

namespace QAToolSFBCommon.NLSocket
{
    class SocketListener
    {
        #region Delegate
        public delegate void RecivedSocketMessaged(Socket obSocket, EndPoint obSoketRemoteEndPoint, string strMessage);
        #endregion

        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(SocketListener));
        #endregion

        #region Members
        private readonly int m_nPort = 8001;
        private Socket m_obSocket = null;
        private bool m_bSocketEstablishedFlag = false;
        private RecivedSocketMessaged m_pFuncRecivedSocketMessaged = null;
        #endregion

        #region Constructor
        public SocketListener(int nPort/*0 means any port*/, int nRebindTimes = 4, int nRebindInterval = 5000)
        {
            SetSocketEstablishedFlag(false);
            m_nPort = nPort;
            if (!GetSocketEstablishedFlag())
            {
                m_obSocket = EstablishSocketEx(nRebindTimes, nRebindInterval);
                if (null == m_obSocket)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error, cannot bind the port:[{0}], the command listener socket established falied\n", m_nPort);
                    return;
                }
                else
                {
                    SetSocketEstablishedFlag(true);
                }
            }
        }
        #endregion

        #region Public functions
        public bool GetSocketEstablishedFlag() { return m_bSocketEstablishedFlag; }
        public bool StartListen(RecivedSocketMessaged pFuncRecivedSocketMessaged)
        {
            if (GetSocketEstablishedFlag())
            {
                m_pFuncRecivedSocketMessaged = pFuncRecivedSocketMessaged;
                ThreadHelper.AsynchronousThreadInvokeHelper(true, WorkListener);
                return true;
            }
            return false;
        }
        public void StopListen()
        {
            if (GetSocketEstablishedFlag())
            {
                SetSocketEstablishedFlag(false);
                if (null != m_obSocket)
                {
                    m_obSocket.Close();
                    m_obSocket = null;
                }
            }
        }
        public void SendMessage(IPEndPoint obDesIPEndpoint, string strMessage, SocketFlags emSocketFlags)
        {
            if (GetSocketEstablishedFlag())
            {
                byte[] data = Encoding.ASCII.GetBytes(strMessage);
                if (SocketFlags.Broadcast == emSocketFlags)
                {
                    m_obSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                }
                m_obSocket.SendTo(data, data.Length, emSocketFlags, obDesIPEndpoint);
            }
        }
        #endregion

        #region tools
        private void SetSocketEstablishedFlag(bool bSocketCloseFlag) { m_bSocketEstablishedFlag = bSocketCloseFlag; }
        private Socket EstablishSocketEx(int nRebindTimes, int nRebindInterval)
        {
            Socket obSocket = null;
            for (int i = 0; i < nRebindTimes; ++i)
            {
                obSocket = EstablishSocket(m_nPort);
                if (null == obSocket)
                {
                    Thread.Sleep(nRebindInterval);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Successed established command listener socket whit port:[{0}]\n", m_nPort);
                    break;
                }
            }
            return obSocket;
        }
        private Socket EstablishSocket(int nPort)
        {
            Socket obSocket = null;
            bool bEstablishSuccess = false;
            try
            {
                //create socket and bind local port       
                obSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Any, nPort);
                obSocket.Bind(ipEndpoint);
                bEstablishSuccess = true;
            }
            catch (ArgumentNullException exArgumentNullException)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Exception when we try to create a socket with point:[{0}], ex:{1}\n", nPort, exArgumentNullException.Message);
            }
            catch (SocketException exSocketException)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Exception when we try to create a socket with point:[{0}], ex:{1}\n", nPort, exSocketException.Message);
            }
            catch (ObjectDisposedException exObjectDisposedException)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Exception when we try to create a socket with point:[{0}], ex:{1}\n", nPort, exObjectDisposedException.Message);
            }
            catch (SecurityException exSecurityException)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Exception when we try to create a socket with point:[{0}], ex:{1}\n", nPort, exSecurityException.Message);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Exception when we try to create a socket with point:[{0}], ex:{1}\n", nPort, ex.Message);
            }
            if (!bEstablishSuccess)
            {
                try
                {
                    if (null != obSocket)
                    {
                        obSocket.Close();
                        obSocket = null;
                    }
                }
                catch (Exception ex)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Establish socket failed but the object is not null, need close. But close with an exception, maybe no need care. port:[{0}], message:{1}\n", nPort, ex.Message);
                }
            }
            return obSocket;
        }
        private void WorkListener()
        {
            while (true)
            {
                try
                {
                    if (!GetSocketEstablishedFlag())
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "The socket object is null, the process stopped\n");
                        break;
                    }
                    else
                    {
                        //recv data
                        EndPoint obIPEndPointRemote = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));

                        byte[] data = new byte[1024];
                        int recv = m_obSocket.ReceiveFrom(data, ref obIPEndPointRemote);
                        string strMessage = Encoding.ASCII.GetString(data, 0, recv);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Message received from {0}: {1} ", obIPEndPointRemote.ToString(), strMessage);

                        if (null != m_pFuncRecivedSocketMessaged)
                        {
                            m_pFuncRecivedSocketMessaged(m_obSocket, obIPEndPointRemote, strMessage);
                        }

                        // reply
                        // newsock.SendTo(data, data.Length, SocketFlags.None, Remote);
                    }
                }
                catch (Exception ex)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in start listen command: {0}", ex.Message);
                }
            }
        }
        private IPEndPoint GetIPEndpoint(string strAddr, int nPort)  // strAddr can be as IP address or FQDN address
        {
            IPEndPoint obIPEndpoint = null;
            if (!string.IsNullOrEmpty(strAddr))
            {
                IPAddress[] addresslist = Dns.GetHostAddresses(strAddr);
                foreach(IPAddress address in addresslist)
                {
                    if(address.AddressFamily == AddressFamily.InterNetwork) //IPv4
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "EndpointProxy parse result IP={0}", address.ToString() );
                        obIPEndpoint = new IPEndPoint(address, nPort);
                        break;
                    }
                }
            }
            return obIPEndpoint;
        }
        #endregion

    }
}
