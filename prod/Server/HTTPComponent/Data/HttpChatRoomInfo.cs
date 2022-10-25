using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.SFBObjectInfo;
using SFBCommon.Common;

namespace Nextlabs.SFBServerEnforcer.HTTPComponent.Data
{
    class HttpChatRoomInfo
    {
        #region Fields
        public SFBChatRoomInfo sfbChatRoomInfo { get { return m_sfbChatRoomInfo; } }
        public NLChatRoomInfo nlChatRoomInfo { get { return m_nlChatRoomInfo; } }
        public SFBChatRoomVariableInfo sfbRoomVar { get { return m_sfbChatRoomVar; } }

        public string Name { get { return m_sfbChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrNameFieldName); } }
        public string ChatRoomID { get { return m_sfbChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrUriFieldName); } }
        public string Creator
        {
            set { m_sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrPresentersFieldName, value); }
            get { return m_sfbChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrPresentersFieldName); }
        }
        public string Members
        {
            get { return m_sfbChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrMembersFieldName); }
            set { m_sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrMembersFieldName, value); }
        }
        public string Managers
        {
            get { return m_sfbChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrManagersFieldName); }
            set { m_sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrManagersFieldName, value); }
        }
        #endregion

        #region Members
        private SFBChatRoomInfo m_sfbChatRoomInfo;
        private NLChatRoomInfo m_nlChatRoomInfo;
        private SFBChatRoomVariableInfo m_sfbChatRoomVar;
        #endregion

        #region Constructor
        public HttpChatRoomInfo()
        {
            m_sfbChatRoomInfo = new SFBChatRoomInfo();
            m_nlChatRoomInfo = new NLChatRoomInfo();
            m_sfbChatRoomVar = new SFBChatRoomVariableInfo();
        }
        #endregion

        #region Public functions
        public bool EstablishChatRoomInfo(string strRoomID)
        {
            bool bEstablished = m_sfbChatRoomInfo.EstablishObjFormPersistentInfo(SFBChatRoomInfo.kstrUriFieldName, strRoomID);
            if (bEstablished)
            {
                bEstablished = m_nlChatRoomInfo.EstablishObjFormPersistentInfo(NLChatRoomInfo.kstrUriFieldName, strRoomID);
                if (bEstablished)
                {
                    bEstablished = m_sfbChatRoomVar.EstablishObjFormPersistentInfo(SFBChatRoomVariableInfo.kstrUriFieldName, strRoomID);
                }
            }
            return bEstablished;
        }
        public void SetRoomID(string strID)
        {
            m_sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrUriFieldName, strID);
            m_nlChatRoomInfo.SetItem(NLChatRoomInfo.kstrUriFieldName, strID);
            m_sfbChatRoomVar.SetItem(SFBChatRoomVariableInfo.kstrUriFieldName, strID);
        }
        public void SaveToDataBase()
        {
            m_sfbChatRoomInfo.PersistantSave();
            m_nlChatRoomInfo.PersistantSave();
            m_sfbChatRoomVar.PersistantSave();
        }
        public bool NeedEnforce()
        {
            string strEnforcerValue = m_nlChatRoomInfo.GetItemValue(NLChatRoomInfo.kstrEnforcerFieldName);
            return CommonHelper.ConverStringToBoolFlag(strEnforcerValue, false);
        }
        public List<string> GetParticipants()
        {
            return SFBCommon.SFBObjectInfo.SFBParticipantManager.GetDistinctParticipantsAsList(m_sfbChatRoomVar);
        }
        #endregion
    }
}
