using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Diagnostics;
using System.Xml;

using Nextlabs.SFBServerEnforcer.PolicyHelper;
using SFBCommon.SFBObjectInfo;
using SFBCommon.NLLog;
using SFBCommon.ClassifyHelper;

using Nextlabs.SFBServerEnforcer.HTTPComponent.Common;
using Nextlabs.SFBServerEnforcer.HTTPComponent.Data;
using Nextlabs.SFBServerEnforcer.HTTPComponent.Policy;

namespace Nextlabs.SFBServerEnforcer.HTTPComponent.RequestFilters
{
    public enum MANAGEHANDLE_TYPE
    {
        MANAGEHANDLE_UNKNOW = 0,
        MANAGEHANDLE_CREATEROOM,
        MANAGEHANDLE_GETALLROOM,
        MANAGEHANDLE_MODIFYROOM
    }

    class RequestFilterManageHandler : RequestFilter
    {
        #region Static members
        static List<HttpChatRoomInfo> m_lstDelayProcessChatRoomInfo = new List<HttpChatRoomInfo>();
        #endregion

        #region Static functions
        static public HttpChatRoomInfo GetDelayProcessChatRoomInfoByName(string strName)
        {
            lock (m_lstDelayProcessChatRoomInfo)
            {
                foreach (HttpChatRoomInfo chatRoom in m_lstDelayProcessChatRoomInfo)
                {
                    if (chatRoom.Name.Equals(strName, StringComparison.OrdinalIgnoreCase))
                    {
                        return chatRoom;
                    }
                }
            }
            return null;
        }
        static public void AddDelyProcessChatRoomInfo(HttpChatRoomInfo httpChatRoom)
        {
            lock (m_lstDelayProcessChatRoomInfo)
            {
                m_lstDelayProcessChatRoomInfo.Add(httpChatRoom);
            }
        }
        static public void RemoveFromDelyProcessChatRoomInfo(HttpChatRoomInfo httpChatRoom)
        {
            lock (m_lstDelayProcessChatRoomInfo)
            {
                m_lstDelayProcessChatRoomInfo.Remove(httpChatRoom);
            }
        }
        #endregion

        #region Members
        MANAGEHANDLE_TYPE m_emManageHandleType = MANAGEHANDLE_TYPE.MANAGEHANDLE_UNKNOW;
        HttpChatRoomInfo m_currentChatRoomInfo;
        HttpApplication m_httpApplication;
        string m_strCurrentUser = "";//m_httpApplication.Context.User.Identity.Name; 

        //when create a new chat room, we can't get the GUID for that chat room.
        //we can get it in the next request, so we buffered the chatroom info here wait for another request result. 
        object m_lockLstChatRoomInfo = new object();

        //store data for chat room members
        List<KeyValuePair<string, string>> m_lstAllUserMembers = new List<KeyValuePair<string, string>>();
        List<KeyValuePair<string, string>> m_lstNxlAllowUserMembers = new List<KeyValuePair<string,string>>();
        List<KeyValuePair<string, string>> m_lstNxlDenyUserMembers = new List<KeyValuePair<string,string>>();
        List<string> m_lstGroupMembers = new List<string>();

        //store data for chat room manager
        List<KeyValuePair<string, string>> m_lstAllUserManager = new List<KeyValuePair<string, string>>();
        List<KeyValuePair<string, string>> m_lstNxlAllowUserManager= new List<KeyValuePair<string, string>>();
        List<KeyValuePair<string, string>> m_lstNxlDenyUserManager= new List<KeyValuePair<string, string>>();
        List<string> m_lstGroupManager = new List<string>();
        #endregion

        #region Constructors
        public RequestFilterManageHandler(HttpRequest httpRequest) : base(httpRequest)
        {
            m_emManageHandleType = MANAGEHANDLE_TYPE.MANAGEHANDLE_UNKNOW;
        }
        #endregion

        #region Public functions
        public MANAGEHANDLE_TYPE GetManageHandleType()
        {
            return m_emManageHandleType;
        }
        public void SetHttpApplication(HttpApplication app)
        {
            m_httpApplication = app;
        }
        public HttpChatRoomInfo GetCurrentChatRoomInfo()
        {
            return m_currentChatRoomInfo;
        }
        public string GetAllowUserAndGroupMemberAsString(bool bFullName)
        {
            return GetUserAndGroupAsString(m_lstNxlAllowUserMembers, m_lstGroupMembers, bFullName);
        }
        public string GetAllowUserAndGroupManagerAsString(bool bFullName)
        {
            return GetUserAndGroupAsString(m_lstNxlAllowUserManager, m_lstGroupManager, bFullName);
        }
        public string GetDenyMemberAsString()
        {
            string strDenyMembers = "";
            foreach (KeyValuePair<string, string> pairDenyMember in m_lstNxlDenyUserMembers)
            {
                strDenyMembers += pairDenyMember.Key + SFBObjectInfo.kstrMemberSeparator;
            }
            return strDenyMembers;
        }
        public string GetDenyManagerAsString()
        {
            string strDenyMgrs = "";
            foreach (KeyValuePair<string, string> pairDenyMgr in m_lstNxlDenyUserManager)
            {
                strDenyMgrs += pairDenyMgr.Key + SFBObjectInfo.kstrMemberSeparator;
            }
            return strDenyMgrs;
        }
        #endregion

        #region Override: RequestFilterManageHandler protected functions
        override protected void DoRequestFilter()
        {
            m_strCurrentUser = m_httpApplication.Context.User.Identity.Name;
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "DoRequestFilter: RequestFilterManageHandler, current user is:{0}", m_strCurrentUser);

            //get request content
            m_oldFilter.Position = 0;
            StreamReader reader = new StreamReader(m_oldFilter);
            string strContent = reader.ReadToEnd();

            //parse request
            ParseRequestBody(strContent);

            //do enforcement on member and manager
            if ((m_emManageHandleType == MANAGEHANDLE_TYPE.MANAGEHANDLE_CREATEROOM) || (m_emManageHandleType == MANAGEHANDLE_TYPE.MANAGEHANDLE_MODIFYROOM))
            {
                if ((m_currentChatRoomInfo != null) && m_currentChatRoomInfo.NeedEnforce())
                {
                    ReplaceDisplayNameWithFullName();

                    //query policy for meeting Create
                    DoEnforcementForMeetingCreate(m_currentChatRoomInfo);

                    //do enforcement for managers
                    strContent = EnforceChatRoomManagersWithinRequest(strContent);

                    //do enforcement for members
                    strContent = EnforceChatRoomMembersWithinRequest(strContent);


                }
            }

            //write m_streamContentBuf
            try
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "strContent={0}", strContent);
                byte[] byteContent = System.Text.Encoding.UTF8.GetBytes(strContent);
                m_streamContentBuf.Position = 0;
                m_streamContentBuf.Write(byteContent, 0, byteContent.Length);
                m_streamContentBuf.Flush();
                m_streamContentBuf.Position = 0;
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, ex.ToString());
            }
        }
        #endregion

        #region Inner tools
        private bool IsCreateChatRoom()
        {
            return m_emManageHandleType == MANAGEHANDLE_TYPE.MANAGEHANDLE_CREATEROOM;
        }
        private bool IsModifyChatRoom()
        {
            return m_emManageHandleType == MANAGEHANDLE_TYPE.MANAGEHANDLE_MODIFYROOM;
        }
        private bool IsCurrentUser(string strUser)
        {
            return m_strCurrentUser.Equals(strUser, StringComparison.OrdinalIgnoreCase);
        }
        private string GetUserAndGroupAsString(List<KeyValuePair<string, string>> lstUser, List<string> lstGroup, bool bFullName)
        {
            string strAllowMembers = "";
            foreach (KeyValuePair<string, string> pairAllowMember in lstUser)
            {
                strAllowMembers += (bFullName ? pairAllowMember.Value : pairAllowMember.Key) + SFBObjectInfo.kstrMemberSeparator;
            }

            foreach (string strGroup in lstGroup)
            {
                strAllowMembers += strGroup + SFBObjectInfo.kstrMemberSeparator;
            }

            return strAllowMembers;
        }
        private void ParseRequestBody(string strContent)
        {
            if(strContent.Length<=0)
            {
                return;
            }

            //
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(strContent);

                XmlNode commandNode = xmlDoc.DocumentElement.FirstChild;
                XmlNamespaceManager xnsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
                xnsMgr.AddNamespace("ns", "http://schemas.microsoft.com/2006/09/rtc/cwa");

                if(commandNode!=null)
                {
                    //get command type
                    string strCmdType = CHttpTools.GetXmlAttributeValueByName(commandNode.Attributes, "Type");
                    if(strCmdType.Equals("CreateRoom", StringComparison.OrdinalIgnoreCase))
                    {
                        m_emManageHandleType = MANAGEHANDLE_TYPE.MANAGEHANDLE_CREATEROOM;
                        GetChatRoomInfoFromCreateRequest(commandNode,xnsMgr);
                    }
                    else if (strCmdType.Equals("Landing", StringComparison.OrdinalIgnoreCase))
                    {
                        m_emManageHandleType = MANAGEHANDLE_TYPE.MANAGEHANDLE_GETALLROOM;
                    }
                    else if (strCmdType.Equals("ModifyRoom", StringComparison.OrdinalIgnoreCase))
                    {
                        m_emManageHandleType = MANAGEHANDLE_TYPE.MANAGEHANDLE_MODIFYROOM;
                        GetChatRoomInfoFromModifyRequest(commandNode,xnsMgr);
                    }
                
                }

            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception on ParseRequestBody:{0}", ex.ToString());
            } 
        }
        private void GetChatRoomInfoFromModifyRequest(XmlNode commandNode,XmlNamespaceManager xnsMgr)
        {
            m_currentChatRoomInfo = new HttpChatRoomInfo();
            SFBChatRoomInfo sfbChatRoomInfo = m_currentChatRoomInfo.sfbChatRoomInfo;
            NLChatRoomInfo nlChatRoomInfo = m_currentChatRoomInfo.nlChatRoomInfo;
            SFBChatRoomVariableInfo roomVar = m_currentChatRoomInfo.sfbRoomVar;

            //get old data
            XmlAttribute xmlAttr = commandNode.Attributes["RoomId"];
            if(xmlAttr!=null && !string.IsNullOrWhiteSpace(xmlAttr.Value))
            {
                string strRoomID = xmlAttr.Value;
                sfbChatRoomInfo.EstablishObjFormPersistentInfo(SFBChatRoomInfo.kstrUriFieldName, strRoomID);

                roomVar.EstablishObjFormPersistentInfo(SFBChatRoomVariableInfo.kstrUriFieldName, strRoomID);
            }

            //if have no crate time
            if(string.IsNullOrWhiteSpace(sfbChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrCreateTimeFieldName)))
            {
                string strCreateTime = DateTime.UtcNow.ToString(SFBObjectInfo.kstrTimeFormatUTCSecond);
                sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrCreateTimeFieldName, strCreateTime);
            }

            //creator
            GetCurrentChatRoomInfo().Creator = m_httpApplication.Context.User.Identity.Name;
      
            //parse request attribute
            GetChatRoomInfoFromXmlAttribute(commandNode.Attributes, sfbChatRoomInfo, nlChatRoomInfo, roomVar);

            GetNLNeedEnforceFromXmlNode(commandNode, xnsMgr);
        }
        private void GetChatRoomInfoFromCreateRequest(XmlNode commandNode,XmlNamespaceManager xnsMgr)
        {
            m_currentChatRoomInfo = new HttpChatRoomInfo();

            //
            string strCreateTime = DateTime.UtcNow.ToString(SFBObjectInfo.kstrTimeFormatUTCSecond);
            m_currentChatRoomInfo.sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrCreateTimeFieldName, strCreateTime);

            //creator
            GetCurrentChatRoomInfo().Creator = m_httpApplication.Context.User.Identity.Name;   

            //Parse
            GetChatRoomInfoFromXmlAttribute(commandNode.Attributes, m_currentChatRoomInfo.sfbChatRoomInfo, m_currentChatRoomInfo.nlChatRoomInfo, m_currentChatRoomInfo.sfbRoomVar);

            GetNLNeedEnforceFromXmlNode(commandNode, xnsMgr);
        }
        // RoomName="testRoom01" RoomDesc="testRoom01 desc" Privacy="Closed" AddIn="c000f25a-abc9-45fb-b572-f872a6dac75c"
        // CategoryUri="ma-cat://lync.nextlabs.solutions/396ac1bf-a36a-4633-a0c7-dbf74991ec7a" Managers="john tyler; " 
        //Members="john tyler; abraham lincoln; " Notification="inherit" NLNeedEnforce="true"
        private void GetChatRoomInfoFromXmlAttribute(XmlAttributeCollection xmlAttributes, SFBChatRoomInfo sfbChatRoomInfo, NLChatRoomInfo nlChatRoomInfo, SFBChatRoomVariableInfo roomVar)
        {
            foreach (XmlAttribute attribute in xmlAttributes)
            {
                string strAttributeName = attribute.Name;
                string strAttributeValue = attribute.Value;

                if (strAttributeName.Equals("RoomName", StringComparison.OrdinalIgnoreCase))
                {
                    sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrNameFieldName, strAttributeValue);
                }
                else if (strAttributeName.Equals("RoomDesc", StringComparison.OrdinalIgnoreCase))
                {
                    sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrDescriptionFieldName, strAttributeValue);
                }
                else if (strAttributeName.Equals("Privacy", StringComparison.OrdinalIgnoreCase))
                {
                    sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrPrivacyFieldName, strAttributeValue);
                }
                else if (strAttributeName.Equals("AddIn", StringComparison.OrdinalIgnoreCase))
                {
                    sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrAddInFieldName, strAttributeValue);
                }
                else if (strAttributeName.Equals("CategoryUri", StringComparison.OrdinalIgnoreCase))
                {
                    sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrCategoryUriFieldName, strAttributeValue);
                }
                else if (strAttributeName.Equals("Managers", StringComparison.OrdinalIgnoreCase))
                {
                    sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrManagersFieldName, strAttributeValue);
                }
                else if (strAttributeName.Equals("Members", StringComparison.OrdinalIgnoreCase))
                {
                    string strMembers = strAttributeValue; //ParseMembersToSipAddress(strAttributeValue);
                    sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrMembersFieldName, strMembers);
                }
                else if (strAttributeName.Equals("Notification", StringComparison.OrdinalIgnoreCase))
                {
                    sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrEnableInvitationsFieldName, strAttributeValue);
                }
                else if (strAttributeName.Equals("RoomId", StringComparison.OrdinalIgnoreCase))//only for modify chat room
                {
                    sfbChatRoomInfo.SetItem(SFBChatRoomInfo.kstrUriFieldName, strAttributeValue);
                    nlChatRoomInfo.SetItem(NLChatRoomInfo.kstrUriFieldName, strAttributeValue);
                    roomVar.SetItem(SFBChatRoomVariableInfo.kstrUriFieldName, strAttributeValue);
                }
            }
        }
        private void GetNLNeedEnforceFromXmlNode(XmlNode commandNode, XmlNamespaceManager xnsMgr)
        {
            //parse NeedEnforce attribute from node 
            XmlAttribute needEnforce = commandNode.SelectSingleNode("ns:NLEnforce", xnsMgr).Attributes["NLNeedEnforce"];
            string classificationObStr = "" ;
            string categoryUri = "";

            List<string> obligationTagNameList = null ;
            Dictionary<string , string> newTagDict = new Dictionary<string, string>();
            Dictionary<string, string> oldTagDict = null ;

            try
            {
                if (commandNode != null && commandNode.Name == "RMCommand")
                {
                    XmlNodeList tagNodes = commandNode.SelectNodes("//ns:tag", xnsMgr);
                    categoryUri = commandNode.Attributes["CategoryUri"].Value;

                    oldTagDict = m_currentChatRoomInfo.sfbRoomVar.GetDictionaryTags();

                    if(!string.IsNullOrEmpty(categoryUri))
                    {
                        classificationObStr = GetObXmlStr(categoryUri);
                        ManulClassifyObligationHelper manualObHelper = new ManulClassifyObligationHelper(classificationObStr, false);
                        obligationTagNameList = manualObHelper.GetTagNameListFromSFBObligationXml();
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, " GetNLNeedEnforceFromXmlNode() failed , categoryUri is null or empty ");
                    }

                    //get new tag dict
                    foreach (XmlNode tag in tagNodes)
                    {
                        if (tag.Name.ToLower() == "tag")
                        {
                            string tagName = HttpUtility.HtmlDecode(tag.Attributes["name"].Value.ToString().Trim().ToLower());
                            string tagValue = HttpUtility.HtmlDecode(tag.Attributes["value"].Value.ToString().Trim().ToLower());
                            newTagDict[tagName] = tagValue;
                        }
                    }

                    //remove keys exist in sfb obligation tags from old tags
                    foreach (string tagName in obligationTagNameList)
                    {
                        if (oldTagDict.ContainsKey(tagName.ToLower()))
                        {
                            oldTagDict.Remove(tagName.ToLower());
                        }
                    }

                    foreach (KeyValuePair<string,string> newTag in newTagDict)
                    {
                        oldTagDict[newTag.Key] = newTag.Value;
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, " GetNLNeedEnforceFromXmlNode() failed , commandNode is null or invalid tag RMCommand ");
                }
            }
            catch (Exception e)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, " GetNLNeedEnforceFromXmlNode() failed , error message : {0} ", e.Message);
            }

        
            //save needEnfor & tags info
            if (m_currentChatRoomInfo != null)
            {
                m_currentChatRoomInfo.nlChatRoomInfo.SetItem(NLChatRoomInfo.kstrEnforcerFieldName, needEnforce.Value);

                m_currentChatRoomInfo.sfbRoomVar.SetItem(SFBChatRoomVariableInfo.kstrClassifyTagsFieldName, "");
                m_currentChatRoomInfo.sfbRoomVar.AddedNewTags(oldTagDict);
                m_currentChatRoomInfo.sfbRoomVar.SetItem(SFBChatRoomVariableInfo.kstrDoneManulClassifyFieldName, SFBMeetingVariableInfo.kstrDoneManulClassifyYes);//SFBChatRoomVariableInfo doesn't have kstrDoneManulClassifyYes field ;
            }
        }
        private void GetUserSIPAddressFromDisplayName(string strDisplayNames, List<KeyValuePair<string, string>> lstUsers, List<string> lstGroup)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "GetUserSIPAddressFromDisplayName, dispNames={0}", strDisplayNames);

            //separate display name
            string[] arrayDisplayName = strDisplayNames.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int nIndex = 0; nIndex < arrayDisplayName.Length; nIndex++)
            {
                arrayDisplayName[nIndex] = arrayDisplayName[nIndex].Trim();
            }

            if (null != arrayDisplayName)
            {
                //get display from database
                List<SFBObjectInfo> lstSfbObj = SFBObjectInfo.GetObjsFrommPersistentInfo(SFBCommon.Common.EMSFB_INFOTYPE.emInfoSFBUserInfo,
                                                                                         SFBUserInfo.kstrDisplayName, arrayDisplayName);
                List<SFBObjectInfo> lstSfbObjBySamAccount = SFBObjectInfo.GetObjsFrommPersistentInfo(SFBCommon.Common.EMSFB_INFOTYPE.emInfoSFBUserInfo,
                                                                                     SFBUserInfo.kstrSamAccountName, arrayDisplayName);
                string strProcessedDispName = "";
                foreach (string strDispName in arrayDisplayName)
                {
                    if (string.IsNullOrWhiteSpace(strDispName))
                    {
                        continue;
                    }

                    //check if the dispName is already processed, for user may input userA;UserA;
                    string strFindDisp = strDispName + ";";
                    if(strProcessedDispName.IndexOf(strFindDisp, StringComparison.OrdinalIgnoreCase)>=0)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "{0} already processed, ignore this one", strDispName);
                        continue;
                    }
                    else
                    {
                        strProcessedDispName += strFindDisp;
                    }                  

                    //find userinfo by display name;
                    string strFullName = null;
                    foreach (SFBObjectInfo sfbObj in lstSfbObj)
                    {
                        SFBUserInfo sfbUserInfo = sfbObj as SFBUserInfo;
                        if (null != sfbUserInfo)
                        {
                            string strUserDispNmae = sfbUserInfo.GetItemValue(SFBUserInfo.kstrDisplayName);
                            string strUserFullName = sfbUserInfo.GetItemValue(SFBUserInfo.kstrFullName);
                            if ((!string.IsNullOrWhiteSpace(strUserDispNmae)) && strUserDispNmae.Equals(strDispName, StringComparison.OrdinalIgnoreCase) &&
                                (!string.IsNullOrWhiteSpace(strUserFullName)))
                            {
                                strFullName = strUserFullName;
                                break;
                            }
                        }
                    }

                    //find userinfo by SamAccountName
                    if(null==strFullName)
                    {
                        foreach (SFBObjectInfo sfbObj in lstSfbObjBySamAccount)
                        {
                            SFBUserInfo sfbUserInfo = sfbObj as SFBUserInfo;
                            if (null != sfbUserInfo)
                            {
                                string strUserSamAccountName = sfbUserInfo.GetItemValue(SFBUserInfo.kstrSamAccountName);
                                string strUserFullName = sfbUserInfo.GetItemValue(SFBUserInfo.kstrFullName);
                                if ((!string.IsNullOrWhiteSpace(strUserSamAccountName)) && strUserSamAccountName.Equals(strDispName, StringComparison.OrdinalIgnoreCase) &&
                                    (!string.IsNullOrWhiteSpace(strUserFullName)))
                                {
                                    strFullName = strUserFullName;
                                    break;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(strFullName))
                    {
                        lstUsers.Add(new KeyValuePair<string, string>(strDispName, strFullName));
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "GetUserSIPAddressFromDisplayName,success to get full name, displayName={0}, fullName={1}", strDispName, strFullName);
                    }
                    else
                    {
                        lstGroup.Add(strDispName);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "GetUserSIPAddressFromDisplayName,failed to get full name, displayName={0}, we treat it as a Group", strDispName);
                    }
                }
            }
        }
        private string ResetAllowMembers(string strContent)
        {
            //get allow members as string
            string strAllowMembers = GetAllowUserAndGroupMemberAsString(false);
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "ResetAllowMembers:" + strAllowMembers);

            //replace
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strContent);

            XmlNode commandNode = xmlDoc.DocumentElement.FirstChild;
            if (commandNode != null)
            {
                foreach (XmlAttribute attribute in commandNode.Attributes)
                {
                    if(attribute.Name.Equals("Members", StringComparison.OrdinalIgnoreCase))
                    {
                        attribute.Value = strAllowMembers;
                    }
                }
            }

            //return new xml
            return SFBCommon.Common.CommonHelper.XmlDocmentToString(xmlDoc);
        }
        private string ResetAllowManager(string strOldRequestXml)
        {
            //get allow members as string
            string strAllowManagers = GetAllowUserAndGroupManagerAsString(false);
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "ResetAllowManagers:" + strAllowManagers);

            //replace
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strOldRequestXml);

            XmlNode commandNode = xmlDoc.DocumentElement.FirstChild;
            if (commandNode != null)
            {
                foreach (XmlAttribute attribute in commandNode.Attributes)
                {
                    if (attribute.Name.Equals("Managers", StringComparison.OrdinalIgnoreCase))
                    {
                        attribute.Value = strAllowManagers;
                    }
                }
            }

            //return new xml
            return SFBCommon.Common.CommonHelper.XmlDocmentToString(xmlDoc);
        }
        private string EnforceChatRoomMembersWithinRequest(string strOldRequestXml)
        {
            //query policy for user members
            if (m_lstAllUserMembers.Count > 0)
            {
                PolicyResult[] arrayPolicyResult = null;
                int nRes = HttpPolicy.GetInstance().QueyPolicyForChatRoomInviteMulti(m_currentChatRoomInfo.Creator, m_lstAllUserMembers, m_currentChatRoomInfo, out arrayPolicyResult);

                if (null != arrayPolicyResult)
                {
                    for (int nResultIndex = 0; nResultIndex < arrayPolicyResult.Length; nResultIndex++)
                    {
                        if (!arrayPolicyResult[nResultIndex].IsDeny())
                        {
                            m_lstNxlAllowUserMembers.Add(m_lstAllUserMembers[nResultIndex]);
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Added to Allow member:" + m_lstAllUserMembers[nResultIndex].Key);
                        }
                        else
                        {
                            m_lstNxlDenyUserMembers.Add(m_lstAllUserMembers[nResultIndex]);
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Added to Deny member:" + m_lstAllUserMembers[nResultIndex].Key);
                        }
                    }
                }

                //when have deny member, Replace members with nxlAllowMembers
                if(m_lstNxlDenyUserMembers.Count>0)
                {
                    m_currentChatRoomInfo.Members = GetAllowUserAndGroupMemberAsString(true);

                    //reset in the request
                    string strNewContent = ResetAllowMembers(strOldRequestXml);
                    return strNewContent;
                }
            }

            return strOldRequestXml;
        }
        private string EnforceChatRoomManagersWithinRequest(string strOldRequestXml)
        {
            bool bUpdateManager = false;
            bool bCreateChatRoom = IsCreateChatRoom();

            //query policy for user manager
            if (m_lstAllUserManager.Count > 0)
            {
                PolicyResult[] arrayPolicyResult = null;
                int nRes = HttpPolicy.GetInstance().QueyPolicyForChatRoomManagerInviteMulti(m_currentChatRoomInfo.Creator, m_lstAllUserManager, m_currentChatRoomInfo, out arrayPolicyResult);

                if (null != arrayPolicyResult)
                {
                    for (int nResultIndex = 0; nResultIndex < arrayPolicyResult.Length; nResultIndex++)
                    {
                        if (!arrayPolicyResult[nResultIndex].IsDeny())
                        {
                            m_lstNxlAllowUserManager.Add(m_lstAllUserManager[nResultIndex]);
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Added to Allow manager:" + m_lstAllUserManager[nResultIndex].Key);
                        }
                        else
                        {
                            if (bCreateChatRoom && IsCurrentUser(m_lstAllUserManager[nResultIndex].Value))
                            {//creator can't be deny from manager list when create chat room
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "Policy deny creator as chat room manager, ignore it." );
                            }
                            else
                            {
                                bUpdateManager = true;
                                m_lstNxlDenyUserManager.Add(m_lstAllUserManager[nResultIndex]);
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Added to Deny manager:" + m_lstAllUserManager[nResultIndex].Key);
                            }

                        }
                    }
                }
            }

            if (IsModifyChatRoom())
            {
                //for modify a chat room, if the manager allow list is empty, we added the current operator as the manager
                if (m_lstNxlAllowUserManager.Count == 0)
                {
                    bUpdateManager = true;
                    AddedCurrentUserAsManager();
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "the manager list is empty after enforcement when modify chat room, so added the operator as the manager.");
                }
            }
            else if (bCreateChatRoom)
            {
                //for create chat room, if the current user is not in manager list, we must added it and save to data base, because Lync Server itself will added it automatically
                //event if the origin manager list didn't contains the current user, we must added it too.
                if (FindUserInListByFullName(m_lstNxlAllowUserManager, m_strCurrentUser).Equals(default(KeyValuePair<string, string>)))
                {
                    bUpdateManager = true;
                    AddedCurrentUserAsManager();
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "the manager list didn't contains the creator, we added it{0}.", m_strCurrentUser);
                }
            }

            //update managers with all allow managers
            if (bUpdateManager)
            {
                m_currentChatRoomInfo.Managers = GetAllowUserAndGroupManagerAsString(true);

                string strNewContent = ResetAllowManager(strOldRequestXml);
                return strNewContent;
            }

            return strOldRequestXml;
        }
        private void AddedCurrentUserAsManager()
        {
            //added
            m_lstNxlAllowUserManager.Add(new KeyValuePair<string,string>(m_strCurrentUser, m_strCurrentUser));

            //delete the current user from deny manager list
            foreach(KeyValuePair<string,string> pairDenyManager in m_lstNxlDenyUserManager)
            {
                if(pairDenyManager.Value.Equals(m_strCurrentUser, StringComparison.OrdinalIgnoreCase))
                {
                    m_lstNxlDenyUserManager.Remove(pairDenyManager);
                    break;
                }
            }

        }
        private void ReplaceDisplayNameWithFullName()
        {
            //get user SIP address for members
            string strMembers = m_currentChatRoomInfo.Members;
            GetUserSIPAddressFromDisplayName(strMembers, m_lstAllUserMembers, m_lstGroupMembers);
            m_currentChatRoomInfo.Members = GetUserAndGroupAsString(m_lstAllUserMembers, m_lstGroupMembers, true);

            //get user SIP address for managers
            string strManagers = m_currentChatRoomInfo.Managers;
            GetUserSIPAddressFromDisplayName(strManagers, m_lstAllUserManager, m_lstGroupManager);
            m_currentChatRoomInfo.Managers = GetUserAndGroupAsString(m_lstAllUserManager, m_lstGroupManager, true);
        }
        private void DoEnforcementForMeetingCreate(HttpChatRoomInfo chatRoomInfo)
        {
            PolicyResult policyResult = new PolicyResult();
            HttpPolicy.GetInstance().QueyPolicyForChatRoomCreate(chatRoomInfo.Creator, chatRoomInfo, policyResult);

            //auto classification
            ObligationHelper.ProcessChatRoomAttachmentCommonObligations(EMSFB_ACTION.emChatRoomCreate, policyResult, "", chatRoomInfo.Creator, chatRoomInfo, null);
        }
        private KeyValuePair<string,string> FindUserInListByFullName(List<KeyValuePair<string,string>> lstUser, string strUserFullName)
        {
            foreach(KeyValuePair<string,string> pairUser in lstUser)
            {
                if(pairUser.Value.Equals(strUserFullName, StringComparison.OrdinalIgnoreCase))
                {
                    return pairUser;
                }
            }
            return default(KeyValuePair<string,string>);
        }
        private string GetObXmlStr(string categoryUri)
        {
            string xmlStr = "";
            NLChatCategoryInfo m_nlCategory = new NLChatCategoryInfo();
            if (m_nlCategory.EstablishObjFormPersistentInfo(NLChatCategoryInfo.kstrUriFieldName, categoryUri))
            {
                xmlStr = m_nlCategory.GetItemValue(NLChatCategoryInfo.kstrClassificationFieldName);
                if (string.IsNullOrEmpty(xmlStr))
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, " GetObXmlStr() failed , xmlStr is null or empty ");
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, " m_nlCategory.EstablishObjFormPersistentInfo() failed ");
            }

            return xmlStr;
        }
        #endregion
    }
}
