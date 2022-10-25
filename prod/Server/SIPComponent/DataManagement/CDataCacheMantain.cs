using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SFBCommon.NLLog;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.DataManagement
{
    class CDataCacheMantain
    {
        static private CDataCacheMantain m_inst;

        AutoResetEvent m_eventStop;
        Thread m_threadMantain;
        static private CLog theLog = CLog.GetLogger("CDataCacheMantain");

        public static CDataCacheMantain GetInstance()
        {
            if(m_inst==null)
            {
                m_inst = new CDataCacheMantain();
            }
            return m_inst;
        }


        public void Start()
        {
            m_threadMantain = new Thread(new ThreadStart(DataCacheMantainThread));

            // Start the thread
            m_threadMantain.Start();
        }

        public void Stop()
        {
            m_eventStop.Set();
            m_threadMantain.Join();
        }

        private CDataCacheMantain()
        {
            m_eventStop = new AutoResetEvent(false);
        }

        //Maintain data cache, current we simply clear data once everyday at ~24:00:00
        public void DataCacheMantainThread()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Data cache mantain thread start.");
            while(true)
            {
                bool bWaitResult = false;
                try
                {
                  bWaitResult =  m_eventStop.WaitOne(1*60*60*1000); //one hour
                }
                catch(Exception /*ex*/)
                {

                }          

                if(bWaitResult)
                {//stop event signal
                    break;
                }
                else
                {
                    DateTime dateTime = DateTime.Now;
                    if(dateTime.Hour==0)
                    {
                        CConferenceManager.GetConferenceManager().ClearDataCache();
                        CChatRoomManager.GetInstance().ClearDataCache();
                    }
                }
            }

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Data cache mantain thread exit.");
        }

    }
}
