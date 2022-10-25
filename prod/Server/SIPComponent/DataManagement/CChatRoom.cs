using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.SFBObjectInfo;
using System.Diagnostics;
using System.Threading;

using Nextlabs.SFBServerEnforcer.SIPComponent.Common;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.DataManagement
{
    class CChatRoom
    {
        public string Name { get { return m_sfbChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrNameFieldName); } }
        public string CreatorUri{get {return m_sfbChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrPresentersFieldName);}}
        public string ID { get { return m_sfbChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrUriFieldName);} }
        public string Description { get { return m_sfbChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrDescriptionFieldName); } }
        public string Members { get { return m_sfbChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrMembersFieldName); } }
        public string Managers { get { return m_sfbChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrManagersFieldName); } }

        public SFBChatRoomInfo SFBChatRoom { get { return m_sfbChatRoomInfo; } }
        public NLChatRoomInfo NLChatRoom { get { return m_nlChatRoomInfo; } }

        private SFBChatRoomInfo m_sfbChatRoomInfo;
        private NLChatRoomInfo m_nlChatRoomInfo;

        public CChatRoom(SFBChatRoomInfo sfbRoomInfo, NLChatRoomInfo nlRoomInfo)
        {
            m_sfbChatRoomInfo = sfbRoomInfo;
            m_nlChatRoomInfo = nlRoomInfo;
        }

        public bool LoadFromDB(string roomID)
        {
            //load sfb info
            m_sfbChatRoomInfo = new SFBChatRoomInfo();
            bool bResult = m_sfbChatRoomInfo.EstablishObjFormPersistentInfo(SFBChatRoomInfo.kstrUriFieldName, roomID);

            if (bResult)
            {
                //load nl info
                m_nlChatRoomInfo = new NLChatRoomInfo();
                bResult = m_nlChatRoomInfo.EstablishObjFormPersistentInfo(NLChatRoomInfo.kstrUriFieldName, roomID);
            }

            return bResult;
        }

        public bool IsValid()
        {
            if (m_sfbChatRoomInfo == null)
            {
                return false;
            }

            string strUri = m_sfbChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrUriFieldName);
            bool bValidUri = !string.IsNullOrEmpty(strUri);

            return bValidUri;
        }


        public bool IsCreator(string strName)
        {
            return CreatorUri.Equals(strName, StringComparison.OrdinalIgnoreCase);
        }

        public bool NeedEnforce()
        {
            return m_nlChatRoomInfo.GetItemValue(NLChatRoomInfo.kstrEnforcerFieldName).Equals("true", StringComparison.OrdinalIgnoreCase);
        }

    }

    class CChatRoomManager
    {
        static protected CChatRoomManager m_chatRoomManager;

        protected List<CChatRoom> m_lstChatRoom;
        protected ReaderWriterLockSlim m_rwLockLstChatRoom = new ReaderWriterLockSlim();//no-recursion lock

        static CChatRoomManager()
        {
            m_chatRoomManager = new CChatRoomManager();
        }

        static public CChatRoomManager GetInstance()
        {
            return m_chatRoomManager;
        }

        protected CChatRoomManager()
        {
            m_lstChatRoom = new List<CChatRoom>();
        }

        public void AddChatRoom(CChatRoom room)
        {
            m_rwLockLstChatRoom.EnterWriteLock();
            try
            {
                //check room exist
                CChatRoom roomExist = null;
                foreach (CChatRoom roomIndex in m_lstChatRoom)
                {
                    if (room.ID.Equals(roomIndex.ID))
                    {
                        roomExist = room;
                        break;
                    }
                }

                //if not exist, added it
                if(roomExist==null)
                {
                    m_lstChatRoom.Add(room);
                }
            }
            finally
            {
                m_rwLockLstChatRoom.ExitWriteLock();
            }
        }

        public CChatRoom GetChatRoomByUri(string strRoomUri)
        {
            string strRoomID = CSIPTools.GetChatRoomIDFromUri(strRoomUri);
            return GetChatRoomByID(strRoomID);
        }

        public CChatRoom GetChatRoomByID(string strRoomID)
        {
            m_rwLockLstChatRoom.EnterReadLock();
            CChatRoom chatRoomResult=null;
            try
            {
                foreach(CChatRoom room in m_lstChatRoom)
                {
                    if(room.ID.Equals(strRoomID))
                    {
                        chatRoomResult = room;
                        break;
                    }
                }
            }
            finally
            {
                m_rwLockLstChatRoom.ExitReadLock();
            }

            return chatRoomResult;
        }

        public CChatRoom GetChatRoomByIDEx(string strRoomID, bool bAutoUpdateFromDB)
        {
            CChatRoom obChatRoom = GetChatRoomByID(strRoomID);
            if ((null == obChatRoom) && (bAutoUpdateFromDB))
            {
                obChatRoom = GetChatRoomFromDB(strRoomID);
            }
            return obChatRoom;
        }

        public CChatRoom GetChatRoomFromDB(string strRoomID)
        {
            CChatRoom chatRoom = new CChatRoom(null, null);
            bool bResult = chatRoom.LoadFromDB(strRoomID);
            if (bResult && chatRoom.IsValid() )
            {
                AddChatRoom(chatRoom);
                return chatRoom;
            }
            return null;
        }

        public void ClearDataCache()
        {
            m_rwLockLstChatRoom.EnterWriteLock();
            try
            {
                m_lstChatRoom.Clear();
            }
            finally
            {
                m_rwLockLstChatRoom.ExitWriteLock();
            }
        }


#if false
        protected void GetChatRoomFromDB()
        {
            List<SFBObjectInfo> lstSfbObj = SFBObjectInfo.GetAllObjsFormPersistentInfo(SFBCommon.Common.EMSFB_INFOTYPE.emInfoSFBChatRoom);
            List<SFBObjectInfo> lstNlObj = SFBObjectInfo.GetAllObjsFormPersistentInfo(SFBCommon.Common.EMSFB_INFOTYPE.emInfoNLChatRoom);

            foreach (SFBObjectInfo sfbObj in lstSfbObj)
            {
                SFBChatRoomInfo sfbChatRoom = sfbObj as SFBChatRoomInfo;
                if(sfbChatRoom != null)
                {
                    string strRoomID = sfbChatRoom.GetItemValue(SFBChatRoomInfo.kstrUriFieldName);
                    CChatRoom existChatRoom = GetChatRoomByID(strRoomID);
                    if(existChatRoom!=null)
                    {//chat room exist, update data

                    }
                    else
                    {//find NLChatRoomInfo
                        NLChatRoomInfo nlChatRoom = null;
                        foreach (SFBObjectInfo nlObj in lstNlObj)
                        {
                            if(nlObj.GetItemValue(NLChatRoomInfo.kstrUriFieldName).Equals(strRoomID, StringComparison.OrdinalIgnoreCase))
                            {
                                nlChatRoom = nlObj as NLChatRoomInfo;
                                break;
                            }
                        }  

                        //
                        if(nlChatRoom!=null)
                        {
                            CChatRoom chatRoom = new CChatRoom(sfbChatRoom, nlChatRoom);
                            Trace.WriteLine("GetChatRoomFromDB, name=" + chatRoom.Name + "  ID:" + chatRoom.ID);
                            AddChatRoom(chatRoom);
                        }
                    }      
                }
            }
        }
#endif
    }
}
