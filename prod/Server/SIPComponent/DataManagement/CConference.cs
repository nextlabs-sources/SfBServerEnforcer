using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.SFBObjectInfo;
using SFBCommon.Database;
using SFBCommon.NLLog;
using System.Threading;

using SFBCommon.Common;
using Nextlabs.SFBServerEnforcer.SIPComponent.Common;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.DataManagement
{
    class CConference
    {
        #region Logger
        public static CLog theLog = CLog.GetLogger("SIPComponent:CConference");
        #endregion

        #region get set
        public CIMCall PreIMCall { get { return m_preIMCall; } set { m_preIMCall = value; } }

        public string Creator { get { return m_sfbMeetingInfo.GetItemValue(SFBMeetingInfo.kstrCreatorFieldName); }  }

        public string FocusUri 
        { 
            get { return m_sfbMeetingInfo.GetItemValue(SFBMeetingInfo.kstrUriFieldName); } 
            set { m_sfbMeetingInfo.SetItem(SFBMeetingInfo.kstrUriFieldName, value);
                  m_nlMeetingInfo.SetItem(NLMeetingInfo.kstrUriFieldName, value);
            }
        }

        public string ExpireTime
        {
            get { return m_sfbMeetingInfo.GetItemValue(SFBMeetingInfo.kstrExpiryTimeFieldName); }
            set { m_sfbMeetingInfo.SetItem(SFBMeetingInfo.kstrExpiryTimeFieldName, value); }
        }

        public string CreateTime
        {
            get { return m_sfbMeetingInfo.GetItemValue(SFBMeetingInfo.kstrCreateTimeFieldName); }
            set { m_sfbMeetingInfo.SetItem(SFBMeetingInfo.kstrCreateTimeFieldName, value);  }
        }

        public EMSFB_MeetingType SFBMeetingType
        {
            get { return m_sfbMeetingInfo.GetSfbMeetingType(); }
            set { m_sfbMeetingInfo.SetItem(SFBMeetingInfo.kstrMeetingTypeFieldName, value.ToString()); }
        }

        public string MeetingID { get { return m_strMeetingID; } set { m_strMeetingID = value; } }
        public string CreateCallID { get { return m_strCreateCallID; } set { m_strCreateCallID = value; } }

        public string Enforcer { get { return m_nlMeetingInfo.GetItemValue(NLMeetingInfo.kstrEnforcerFieldName); }
            set { m_nlMeetingInfo.SetItem(NLMeetingInfo.kstrEnforcerFieldName, value);  }
        }

        public string ManualTagXml { get { return m_nlMeetingInfo.GetItemValue(NLMeetingInfo.kstrManulClassifyObsFieldName);  }
            set { m_nlMeetingInfo.SetItem(NLMeetingInfo.kstrManulClassifyObsFieldName, value); }
        }

        public string ForceManualTag
        {
            get { return m_nlMeetingInfo.GetItemValue(NLMeetingInfo.kstrForceManulClassifyFieldName); }
            set { m_nlMeetingInfo.SetItem(NLMeetingInfo.kstrForceManulClassifyFieldName, value); }
        }

        public SFBMeetingInfo sfbMeetingInfo { get { return m_sfbMeetingInfo; } }
        public NLMeetingInfo nlMeetingInfo { get { return m_nlMeetingInfo; } }
        #endregion

        #region Members
        private string m_strMeetingID; //id, e.g.V28VT9Z7, is a part of meeting URI

        private CIMCall m_preIMCall;
        private string m_strCreateCallID;

        //sub object, store meeting informations
        private SFBMeetingInfo m_sfbMeetingInfo = new SFBMeetingInfo();
        private NLMeetingInfo m_nlMeetingInfo = new NLMeetingInfo();
        #endregion

        #region Constructors
        public CConference(string strCreator, string strMeetingID)
        {
            m_sfbMeetingInfo.SetItem(SFBMeetingInfo.kstrCreatorFieldName, strCreator);
            m_strMeetingID = strMeetingID;
            m_preIMCall = null;
            CreateCallID = "";
        }
        public CConference(SFBMeetingInfo sfbMeetingInfo, NLMeetingInfo nlMeetingInfo)
        {
            m_sfbMeetingInfo = sfbMeetingInfo;
            m_nlMeetingInfo = nlMeetingInfo;

            m_strMeetingID = CSIPTools.GetConferenceIDFromUri(FocusUri);
            m_preIMCall = null;
            CreateCallID = "";
        }
        public CConference()
        {
        }
        #endregion

        public bool IsCreator(string strUser)
        {
            if (strUser == null)
                return false;

            return this.Creator.Equals(strUser, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsConvertedFromIMCall()
        {
            return (m_preIMCall != null);
        }

        public bool IsTheOldSpeaker(string strUser)
        {
            if((strUser==null) || (m_preIMCall==null))
            {
                return false;
            }
            else
            {
                return m_preIMCall.From.Equals(strUser, StringComparison.OrdinalIgnoreCase) ||
                       m_preIMCall.To.Equals(strUser, StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool NeedManualClassify()
        {
            return !string.IsNullOrWhiteSpace(ManualTagXml);
        }

        public bool IsForceManualClassify()
        {
            string strForce = ForceManualTag;
            if(!string.IsNullOrWhiteSpace(strForce))
            {
               return ForceManualTag.Equals(PolicyHelper.PolicyMain.KStrObAttributeForceClassifyYes, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public bool NeedEnforce()
        {
            string strEnforcerStatus = this.Enforcer;
            if(strEnforcerStatus.Equals(NLMeetingInfo.kstrEnforceAutoYes, StringComparison.OrdinalIgnoreCase) ||
               strEnforcerStatus.Equals(NLMeetingInfo.kstrEnforceManualYes, StringComparison.OrdinalIgnoreCase) )
            {
                //auto yes / manual yes
                return true;
            }
            else if(strEnforcerStatus.Equals(NLMeetingInfo.kstrEnforceAutoNo, StringComparison.OrdinalIgnoreCase) ||
                    strEnforcerStatus.Equals(NLMeetingInfo.kstrEnforceManualNo, StringComparison.OrdinalIgnoreCase) )
            {
                //auto no / manual no
                return false;
            }
            else if(strEnforcerStatus.Equals(NLMeetingInfo.kstrEnforceManualNotSet, StringComparison.OrdinalIgnoreCase) )
            {
                //need manul set, but haven't set right know.
                return true;
            }
            else //if(strEnforcerStatus.Equals(NLMeetingInfo.kstrEnforceNA, StringComparison.OrdinalIgnoreCase))
            {
                //not set to any value, need a default value for this status.
                return false;
            }
         
        }

        public void SaveConferenceInfo()
        {
            try
            {
                m_sfbMeetingInfo.PersistantSave();
                m_nlMeetingInfo.PersistantSave();
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception on SaveConferenceInfo:" + ex.ToString());
            }

        }

        public bool LoadFromDB(string strFocusUri)
        {
            m_sfbMeetingInfo = new SFBMeetingInfo();
            bool bRest = m_sfbMeetingInfo.EstablishObjFormPersistentInfo(SFBMeetingInfo.kstrUriFieldName, strFocusUri);
            if(bRest)
            {
                m_nlMeetingInfo = new NLMeetingInfo();
                bRest = m_nlMeetingInfo.EstablishObjFormPersistentInfo(NLMeetingInfo.kstrUriFieldName, strFocusUri);
            }
            return bRest;
        }

        public bool IsValid()
        {
            if(m_sfbMeetingInfo==null)
            {
                return false;
            }

            string strUri = m_sfbMeetingInfo.GetItemValue(SFBMeetingInfo.kstrUriFieldName);
            bool bValidUri = !string.IsNullOrEmpty(strUri);

            return bValidUri;
        }
    }

    class CConferenceManager
    {
        #region Logger
        public static CLog theLog = CLog.GetLogger("SIPComponent:CConferenceManager");
        #endregion

        #region Members
        private Dictionary<string, SFBMeetingShareInfo> m_dicConferenceShareInfo = new Dictionary<string,SFBMeetingShareInfo>();
        private List<CConference> m_lstConference;
        private List<CConference> m_lstConfPendingCreate;//conference that we just received request message and waiting for response message 
        private object m_lockLstConfPendingCreate = new object();
        private ReaderWriterLockSlim m_rwLockLstConference = new ReaderWriterLockSlim(); // no-recursion lock
        private ReaderWriterLockSlim m_rwLockLstConferenceShare = new ReaderWriterLockSlim(); // no-recursion lock
        #endregion

        #region Singleton
        static public CConferenceManager GetConferenceManager()
        {
            return s_conferenceMgr;
        }
        static private CConferenceManager s_conferenceMgr = new CConferenceManager();
        private CConferenceManager(CConferenceManager mgr) { }
        private CConferenceManager()
        {
            m_lstConference = new List<CConference>();
            m_lstConfPendingCreate = new List<CConference>();
        }
        #endregion

        #region Conference share info
        public string GetSharerInfoByConferenceShareUri(string strConferenceShareUri)
        {
            string strConferenceSharer = "";
            if (!string.IsNullOrEmpty(strConferenceShareUri))
            {
                SFBMeetingShareInfo obSFBMeetingShareInfo = null;
                try
                {
                    m_rwLockLstConferenceShare.EnterReadLock();
                    obSFBMeetingShareInfo = CommonHelper.GetValueByKeyFromDir(m_dicConferenceShareInfo, strConferenceShareUri, null);
                }
                finally
                {
                    m_rwLockLstConferenceShare.ExitReadLock();
                }
                if (null == obSFBMeetingShareInfo)
                {
                    obSFBMeetingShareInfo = new SFBMeetingShareInfo();
                    bool bEstablished = obSFBMeetingShareInfo.EstablishObjFormPersistentInfo(SFBMeetingShareInfo.kstrShareUriFieldName, strConferenceShareUri);
                    if (bEstablished)
                    {
                        strConferenceSharer = obSFBMeetingShareInfo.GetItemValue(SFBMeetingShareInfo.kstrSharerFieldName);

                        try
                        {
                            m_rwLockLstConferenceShare.EnterWriteLock();
                            CommonHelper.AddKeyValuesToDir(m_dicConferenceShareInfo, strConferenceShareUri, obSFBMeetingShareInfo);
                        }
                        finally
                        {
                            m_rwLockLstConferenceShare.ExitWriteLock();
                        }
                    }
                }
                else
                {
                    strConferenceSharer = obSFBMeetingShareInfo.GetItemValue(SFBMeetingShareInfo.kstrSharerFieldName);
                }
            }
            return strConferenceSharer;
        }
        public void SetConferenceShareInfo(string strConferenceShareUri, string strConferenceSharer)
        {
            if ((!string.IsNullOrEmpty(strConferenceShareUri)) && (!string.IsNullOrEmpty(strConferenceSharer)))
            {
                SFBMeetingShareInfo obSFBMeetingShareInfo = new SFBMeetingShareInfo(SFBMeetingShareInfo.kstrShareUriFieldName, strConferenceShareUri, SFBMeetingShareInfo.kstrSharerFieldName, strConferenceSharer);
                CommonHelper.AddKeyValuesToDir(m_dicConferenceShareInfo, strConferenceShareUri, obSFBMeetingShareInfo);
                obSFBMeetingShareInfo.PersistantSave();

                try
                {
                    m_rwLockLstConferenceShare.EnterWriteLock();
                    CommonHelper.AddKeyValuesToDir(m_dicConferenceShareInfo, strConferenceShareUri, obSFBMeetingShareInfo);
                }
                finally
                {
                    m_rwLockLstConferenceShare.ExitWriteLock();
                }
            }
        }
        #endregion

        public void AddPendingCreateConference(CConference conf)
        {
            lock(m_lockLstConfPendingCreate)
            {
                m_lstConfPendingCreate.Add(conf);
            }
        }

        public CConference PopupPendingCreateConference(string strConfID)
        {
            lock(m_lockLstConfPendingCreate)
            {
                foreach(CConference conf in m_lstConfPendingCreate)
                {
                    if(conf.MeetingID.Equals(strConfID, StringComparison.OrdinalIgnoreCase))
                    {
                        m_lstConfPendingCreate.Remove(conf);
                        return conf;
                    }
                }
            }

            return null;
        }

        public void AddConference(CConference conference)
        {
            m_rwLockLstConference.EnterWriteLock();
            try
            {
                //check if the conference is exist
                CConference confExist = null;
                foreach (CConference conf in m_lstConference)
                {
                    if (conf.FocusUri.Equals(conference.FocusUri, StringComparison.OrdinalIgnoreCase))
                    {
                        confExist = conference;
                        break;
                    }
                }

                //if have not exist, added it
                if (confExist == null)
                {
                    m_lstConference.Add(conference);
                }
            }
            finally
            {
                m_rwLockLstConference.ExitWriteLock();
            }
        }

        public void RemoveConference(CConference conference)
        {
            m_rwLockLstConference.EnterWriteLock();
            try
            {
                m_lstConference.Remove(conference);
            }
            finally
            {
                m_rwLockLstConference.ExitWriteLock();
            }
        }

        public CConference GetConferenceByCreateCallID(string strCallID)
        {
            m_rwLockLstConference.EnterReadLock();
            try
            {
                foreach (CConference conference in m_lstConference)
                {
                    if (conference.CreateCallID.Equals(strCallID, StringComparison.OrdinalIgnoreCase))
                    {
                        return conference;
                    }
                }

                return null;
            }
            finally
            {
                m_rwLockLstConference.ExitReadLock();
            }
        }

        public CConference GetConferenceByFocusUri(string strFocusUri)
        {
            m_rwLockLstConference.EnterReadLock();
            try
            {
                //find in the list
                foreach (CConference conference in m_lstConference)
                {
                    if (conference.FocusUri.Equals(strFocusUri, StringComparison.OrdinalIgnoreCase))
                    {
                        return conference;
                    }
                }

                //not found
                return null;
            }
            finally
            {
                m_rwLockLstConference.ExitReadLock();
            }
        }

        public CConference LoadConferenceFromDB(string strFocusUri)
        {
            CConference conf = new CConference();
            bool bResult = conf.LoadFromDB(strFocusUri);
            if (bResult && conf.IsValid() )
            {
                AddConference(conf);
                return conf;
            }
            return null;
        }

        public CConference GetConferenceByID(string strID)
        {
            m_rwLockLstConference.EnterReadLock();
            try
            {
                //find in the list
                foreach (CConference conference in m_lstConference)
                {
                    if (conference.MeetingID.Equals(strID, StringComparison.OrdinalIgnoreCase))
                    {
                        return conference;
                    }
                }

                return null;
            }
            finally
            {
                m_rwLockLstConference.ExitReadLock();
            }
        }

        public void ClearDataCache()
        {
            m_rwLockLstConference.EnterWriteLock();
            try
            {
                m_lstConference.Clear();
            }
            finally
            {
                m_rwLockLstConference.ExitWriteLock();
            }
        }

#if false
        private void GetConferenceFromDB()
        {
            List<SFBObjectInfo> lstSfbObj = SFBObjectInfo.GetAllObjsFormPersistentInfo(SFBCommon.Common.EMSFB_INFOTYPE.emInfoSFBMeeting);
            List<SFBObjectInfo> lstNlObj = SFBObjectInfo.GetAllObjsFormPersistentInfo(SFBCommon.Common.EMSFB_INFOTYPE.emInfoNLMeeting);

            foreach (SFBObjectInfo sfbObj in lstSfbObj)
            {
                SFBMeetingInfo sfbMeetingInfo = sfbObj as SFBMeetingInfo;
                if (sfbMeetingInfo != null)
                {
                    string strMeetingUri = sfbMeetingInfo.GetItemValue(SFBMeetingInfo.kstrUriFieldName);
                    if(null== GetConferenceByFocusUri(strMeetingUri))
                    {
                        //find NLObj
                        NLMeetingInfo nlMeetingInfo = null;
                        foreach(SFBObjectInfo nlObj in lstNlObj)
                        {
                            if (nlObj.GetItemValue(NLMeetingInfo.kstrUriFieldName).Equals(strMeetingUri, StringComparison.OrdinalIgnoreCase))
                            {
                                nlMeetingInfo = nlObj as NLMeetingInfo;
                                break;
                            }
                        }

                        if(nlMeetingInfo!=null)
                        {
                            CConference conf = new CConference(sfbMeetingInfo, nlMeetingInfo);
                            CConference.theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "GetConferenceFromDB, uri=" + conf.FocusUri);
                            AddConference(conf);
                        }

                    }
                }

            }
        }
#endif
    }
}
