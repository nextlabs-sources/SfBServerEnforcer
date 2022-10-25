using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Rtc.Sip;
using System.Threading;
using System.Diagnostics;
using SFBCommon.NLLog;

namespace Nextlabs.SFBServerEnforcer.SIPComponent
{
    class CSessionManager
    {
        /// objects needed for connecting to the server
        protected ApplicationManifest m_applicationManifest;
        protected ServerAgent serverAgent;

        /// the Event manager thread implements the main message pump for receiving SIP messages and  delivers it to various handlers. 
        protected Thread eventManagerThread;
        protected AutoResetEvent eventManagerQuit;

        private static CLog theLog = CLog.GetLogger("SIPComponent.CSessionManager");

        public delegate void DisconnectEventListeners(string strReason);
        public DisconnectEventListeners DisconnectListeners;
        public CSessionManager()
        {
            eventManagerQuit = new AutoResetEvent(false);
        }

        public void ConnectToServer(string manifestFile, string applicationName)
        {
            ///load and compile the application manifest
            m_applicationManifest = ApplicationManifest.CreateFromFile(manifestFile);
            if (m_applicationManifest == null)
            {
                throw new Exception(String.Format("The manifest file {0} was not found", manifestFile));
            }

            try
            {
                m_applicationManifest.Compile();
                ///try to connect to server
                serverAgent = new ServerAgent(this, m_applicationManifest);
            }
            catch (CompilerErrorException cee)
            {

                ///collapse all compiler errors into one, and return it
                StringBuilder sb = new StringBuilder(1024, 1024);
                foreach (string errorMessage in cee.ErrorMessages)
                {
                    if (errorMessage.Length + sb.Length + 2 < sb.MaxCapacity)
                    {
                        sb.Append(errorMessage);
                        sb.Append("\r\n");
                    }
                    else
                    {
                        ///compiler returns really large error message
                        ///so just return what we can accomodate
                        sb.Append(errorMessage.Substring(0, sb.MaxCapacity - sb.Length - 1));
                        break;
                    }
                }

                throw new Exception(sb.ToString());
            }
            catch (Exception e)
            {
                if (m_applicationManifest != null)
                {
                    m_applicationManifest = null;
                }
                throw e;
            }


            ///hook the connection dropped event handler
            serverAgent.ConnectionDropped += new ConnectionDroppedEventHandler(this.ConnectionDroppedHandler); //ConnectionDroppedEventHandler

            ///start the eventManager thread after making sure one is 
            ///not already running
            eventManagerQuit.Reset();
            eventManagerThread = new Thread(new ThreadStart(EventManagerHandler));
            eventManagerThread.Start();

            return;
        }


        /// This callback will be invoked by ServerAgent when we are
        /// disconnected by the server due to some external reason
        protected void ConnectionDroppedHandler(object sender, ConnectionDroppedEventArgs cde)
        {
            ///stop event manager and cleanup
            InternalDisconnect();

            ///notify all listeners who want to know that we lost
            ///the server connection
            string reason = String.Format("Reason: {0}", cde.Reason);
            this.DisconnectListeners(reason);

            return;
        }

        /// <summary>
        /// Disconnect from the Live Communications Server, cleanup
        /// </summary>
        /// <remarks> If we are connected to the server,
        /// to disconnect and cleanup is to dispose the
        /// server agent object. 
        protected void InternalDisconnect()
        {
            if (serverAgent == null)
                return; ///already gone

            if (eventManagerThread != null)
            {
                ///first stop our event manager thread
                eventManagerQuit.Set();
                eventManagerThread.Join(1000 /* upto a second */);
                eventManagerThread = null;
            }

            if (serverAgent != null)
            {
                ///remove the connection to server
                ServerAgent serverAgentToDispose = serverAgent;
                serverAgent = null;
                serverAgentToDispose.Dispose();

            }

            m_applicationManifest = null;
            return;
        }

        /// Event manager routine. Listen for events from the server, and dispatch them.
        protected virtual void EventManagerHandler()
        {
            ///Wait on the serverAgent event notification handle 
            ///and the eventManager exit notification handle. 
            WaitHandle[] waitHandle = new WaitHandle[2];
            waitHandle[0] = serverAgent.WaitHandle;
            waitHandle[1] = this.eventManagerQuit;

            WaitCallback wcb = new WaitCallback(serverAgent.ProcessEvent);

            while (true)
            {

                int handleSignalled = WaitHandle.WaitAny(waitHandle);

                ///are we asked to quit ?
                if (handleSignalled == 1)
                {
                    Trace.Write("Event manager exiting");
                    return;
                }

                ///an event was received from the server, Dispatch it
                ThreadPool.QueueUserWorkItem(wcb);
            }
        }

        /// disconnect from server, cleanup
        public void Disconnect()
        {
            InternalDisconnect();
            return;
        }


        /// This function receives SIP responses,
        public void ResponseHandler(object sender, ResponseReceivedEventArgs e)
        {
            try
            {
                CSIPParser.GetParser().ParseSIPResponse(e.Response);
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, ex.ToString());
            }
 

            ///done with state management, so forward the response
            e.ClientTransaction.ServerTransaction.SendResponse(e.Response);

            return;
        }

        /// <summary>
        /// This function receives SIP requests, updates
        /// session state variables, and proxies the request
        /// to the default request uri
        /// </summary>
        /// <remarks>
        /// The request handler's name must be the name of the 
        /// function that is given in the SPL Dispatch function 
        /// for SIP requests. 
        /// </remarks>
        /// <param name="sender">not used</param>
        /// <param name="e">the request state</param>
        public void RequestHandler(object sender, RequestReceivedEventArgs e)
        {
            Response ourResponse = null;
           
            try
            {
                CSIPParser.GetParser().ParseSIPRequest(e.Request, ref ourResponse);
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, ex.ToString());
                ourResponse = e.Request.CreateResponse(500);
            }


            ///we are done, proxy
            if(ourResponse != null)
            {
                e.ServerTransaction.SendResponse(ourResponse);
            }
            else
            {
                e.ServerTransaction.EnableForking = false;
                ClientTransaction ct = e.ServerTransaction.CreateBranch();
                ct.SendRequest(e.Request);
            }


            return;
        }
    }
}
