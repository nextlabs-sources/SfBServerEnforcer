using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.NLLog;

namespace Nextlabs.SFBServerEnforcer.SIPComponent
{
    class CIMCall
    {
        public string From { get { return m_strFrom; } }
        public string To { get { return m_strTo; } }
        public string CallID { get { return m_strCallID; } }
        public string ConversationID { get { return m_strConversationID; } }

        protected string m_strFrom;
        protected string m_strTo;
        protected string m_strCallID;
        protected string m_strConversationID;

        public static CLog theLog = CLog.GetLogger(typeof(CIMCall));
 
        public CIMCall(string strFrom, string strTo, string strCallID, string strConversationID)
        {
            m_strFrom = strFrom;
            m_strTo = strTo;
            m_strCallID = strCallID;
            m_strConversationID = strConversationID;
        }

        public void Bye(string byeUser)
        {
            CIMCallManager.GetIMCallManager().RemoveIMCall(this);
        }
    }

    class CIMCallManager
    {
        protected List<CIMCall> m_lstIMCalls;
        static protected CIMCallManager m_imCallMgr;

        public CIMCallManager()
        {
            m_lstIMCalls = new List<CIMCall>();
        }
        static CIMCallManager()
        {
            CIMCallManager.m_imCallMgr = new CIMCallManager();
        }

        static public CIMCallManager GetIMCallManager()
        {
            return m_imCallMgr;
        }


        public void AddIMCall(CIMCall imCall)
        {
            CIMCall.theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Enter AddIMCall, from:{0}, to:{1}, callid:{2}, convID:{3}", imCall.From, imCall.To, imCall.CallID, imCall.ConversationID);
            //find if the call exist

            //
            lock(this)
            {
                m_lstIMCalls.Add(imCall);
            }
        }

        public void RemoveIMCall(CIMCall imCall)
        {
            lock(this)
            {
                m_lstIMCalls.Remove(imCall);
            }
        }

        public CIMCall GetIMCallByCallID(string strCallID)
        {
            lock(this)
            {
                foreach(CIMCall imCall in m_lstIMCalls)
                {
                    if(imCall.CallID.Equals(strCallID, StringComparison.OrdinalIgnoreCase))
                    {
                        return imCall;
                    }
                }

                return null;
            }
        }

        public CIMCall GetIMCallByConversationID(string strConvID)
        {
            lock (this)
            {
                foreach (CIMCall imCall in m_lstIMCalls)
                {
                    if (imCall.ConversationID.Equals(strConvID, StringComparison.OrdinalIgnoreCase))
                    {
                        return imCall;
                    }
                }

                return null;
            }
        }

        
         

    }
}
