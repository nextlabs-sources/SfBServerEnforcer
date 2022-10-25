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
using NLLyncEndpointProxy.Common;

namespace NLLyncEndpointProxy.Listener
{
    class CommandListener
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(NLLyncEndpoint));
        #endregion

        #region Members
        private readonly STUSFB_ENDPOINTPROXYTCPINFO m_stuTcpInfo = null;
        private Socket m_obSocket = null;
        private bool m_bSocketEstablishedFlag = false;
        #endregion

        #region Constructor
        public CommandListener(STUSFB_ENDPOINTPROXYTCPINFO stuTcpInfo)
        {
            SetSocketEstablishedFlag(false);
            if (null != stuTcpInfo)
            {
                m_stuTcpInfo = stuTcpInfo;
                m_stuTcpInfo.OutputInfo();
                ThreadHelper.AsynchronousInvokeHelper(true, StartListen);
            }
        }
        #endregion

        #region Listener
        public void StartListen()
        {
            const int knRebindTimes = 4;
            for (int i=0; i<knRebindTimes; ++i)
            {
                m_obSocket = EstablishSocket((int)m_stuTcpInfo.m_unPort);
                if (null == m_obSocket)
                {
                    Thread.Sleep(5 * 1000);
                }
                else
                {
                    SetSocketEstablishedFlag(true);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Successed established command listener socket whit port:[{0}]\n", m_stuTcpInfo.m_unPort);
                    break;
                }
            }

            if (null == m_obSocket)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error, cannot bind the port:[{0}], the command listener socket established falied\n", m_stuTcpInfo.m_unPort);
                return ;
            }

            //wait client 
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
                        EndPoint Remote = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));

                        byte[] data = new byte[1024];
                        int recv = m_obSocket.ReceiveFrom(data, ref Remote);
                        string strMessage = Encoding.ASCII.GetString(data, 0, recv);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Message received from {0}: {1} ", Remote.ToString(), strMessage);

                        CommandProcesser.SendMessageByCommandInfo(true, strMessage, m_obSocket, Remote);

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
        #endregion

        #region tools
        private bool GetSocketEstablishedFlag() { return m_bSocketEstablishedFlag; }
        private void SetSocketEstablishedFlag(bool bSocketCloseFlag) { m_bSocketEstablishedFlag = bSocketCloseFlag; }
        private Socket EstablishSocket(int nPort)
        {
            Socket obSocket = null;
            bool bEstablishSuccess = false;
            try
            {
                //create socket and bind local port       
                obSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, nPort);
                obSocket.Bind(ip);
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
        #endregion

    }
}
