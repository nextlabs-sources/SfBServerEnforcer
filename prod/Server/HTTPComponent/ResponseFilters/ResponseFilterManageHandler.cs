using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Diagnostics;
using System.Xml;

using SFBCommon.SFBObjectInfo;
using SFBCommon.Common;
using SFBCommon.NLLog;
using SFBCommon.ClassifyHelper;

using Nextlabs.SFBServerEnforcer.HTTPComponent.Common;
using Nextlabs.SFBServerEnforcer.HTTPComponent.Data;
using Nextlabs.SFBServerEnforcer.HTTPComponent.RequestFilters;

namespace Nextlabs.SFBServerEnforcer.HTTPComponent.ResponseFilters
{
    class LandResultRoom
    {
        public string ChatRoomGuid;
        public string Name;
    }

    class ChatRoomOperatorResult
    {
        public bool bSuccess;
        public string strUser;
    }

    class ResponseFilterManageHandler : ResponseFilter
    {

        private ChatRoomOperatorResult m_roomOperatorResult;

        private List<LandResultRoom> m_lstLandingResult;
        private RequestFilterManageHandler m_requestFilter;
        public ResponseFilterManageHandler(HttpResponse httpResponse, RequestFilterManageHandler requestFilter) : base(httpResponse)
        {
            m_requestFilter = requestFilter;
            m_roomOperatorResult = new ChatRoomOperatorResult()
            {
                bSuccess = false,
                strUser = ""
            };
        }

        public ChatRoomOperatorResult GetRoomOperatorResult()
        {
            return m_roomOperatorResult;
        }

        public List<LandResultRoom> GetLandingResult()
        {
            return m_lstLandingResult;
        }

        public override void Close()
        {
            //get string content
            m_streamContentBuf.Position = 0;
            StreamReader contentReader = new StreamReader(m_streamContentBuf);
            String strContent = contentReader.ReadToEnd();

            //parse xml
           if((strContent!=null) && (strContent.Length>0))
           {
               const string strDefaultXmlNS = "mydefaultXmlNS";

               try
               {
                   //load xml
                   XmlDocument xmldoc = new XmlDocument();
                   xmldoc.LoadXml(strContent);

                   XmlNamespaceManager xmlnsm = new XmlNamespaceManager(xmldoc.NameTable);
                   xmlnsm.AddNamespace(strDefaultXmlNS, "http://schemas.microsoft.com/2006/09/rtc/cwa");
                   xmlnsm.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                   xmlnsm.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");

                   string strCmdName = xmldoc.DocumentElement.FirstChild.Name;
                   if (strCmdName.Equals("NewRoomInfoResult", StringComparison.OrdinalIgnoreCase))
                   {//get new room info command,act when user click "Create A new Room" 
                       ParseNewRoomInfoCommand(xmldoc, xmlnsm, strDefaultXmlNS);
                       strContent = SFBCommon.Common.CommonHelper.XmlDocmentToString(xmldoc);
                   }
                   else if (strCmdName.Equals("CreateResult", StringComparison.OrdinalIgnoreCase))
                   {//get new room info command
                       ParseCreateNewOrModifyRoomResult(xmldoc, xmlnsm, strDefaultXmlNS, true);
                       strContent = SFBCommon.Common.CommonHelper.XmlDocmentToString(xmldoc);
                   }
                   else if (strCmdName.Equals("EditResult", StringComparison.OrdinalIgnoreCase))
                   {//get new room info command
                       ParseModifyRoomResult(xmldoc, xmlnsm, strDefaultXmlNS);
                       strContent = SFBCommon.Common.CommonHelper.XmlDocmentToString(xmldoc);
                   }
                   else if (strCmdName.Equals("LandingResult", StringComparison.OrdinalIgnoreCase))
                   {
                       ParseLandingResult(xmldoc, xmlnsm, strDefaultXmlNS);
                   }
                   else if (strCmdName.Equals("RoomAccessResult", StringComparison.OrdinalIgnoreCase))
                   {//get the current room info, act when use edit a chat room
                       ParseRoomAccessRightResult(xmldoc, xmlnsm, strDefaultXmlNS);
                       strContent = SFBCommon.Common.CommonHelper.XmlDocmentToString(xmldoc);
                   }
               }
               catch(Exception ex)
               {
                   theBaseLog.OutputLog(SFBCommon.NLLog.EMSFB_LOGLEVEL.emLogLevelError, "exception:{0}", ex.ToString());
               }
           }
          
            //write back
            WriteToOldStream(strContent);
            m_oldFilter.Close();

            //log
            theBaseLog.OutputLog(SFBCommon.NLLog.EMSFB_LOGLEVEL.emLogLevelDebug, "Response:{0}", strContent);
        }

        private void ParseCreateNewOrModifyRoomResult(XmlDocument xmlDoc, XmlNamespaceManager xmlnsm, string strDefaulXmlns, bool bIsCreateNew)
        {
            m_roomOperatorResult = new ChatRoomOperatorResult();
            m_roomOperatorResult.bSuccess = false;

            //success
            XmlNode nodeSuccess = xmlDoc.DocumentElement.SelectSingleNode("//" + strDefaulXmlns + ":Success", xmlnsm);
            if (nodeSuccess != null)
            {
                m_roomOperatorResult.bSuccess = nodeSuccess.InnerText.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            //UserName
            XmlNode nodeUser = xmlDoc.DocumentElement.SelectSingleNode("//" + strDefaulXmlns + ":UserName", xmlnsm);
            if (nodeUser != null)
            {
                m_roomOperatorResult.strUser = nodeUser.InnerText;
            }

            //added our enforce result
            XmlNode nodeResult = xmlDoc.DocumentElement.SelectSingleNode("//" + strDefaulXmlns + (bIsCreateNew ? ":CreateResult" : ":EditResult"), xmlnsm);
            if(nodeResult!=null)
            {
                XmlNode nodeNxlResult = xmlDoc.CreateNode(XmlNodeType.Element, "NxlResult", xmlnsm.LookupNamespace(strDefaulXmlns));
                if(nodeNxlResult!=null)
                {
                    XmlNode allowResult = xmlDoc.CreateNode(XmlNodeType.Element, "AllowMember", xmlnsm.LookupNamespace(strDefaulXmlns));
                    allowResult.InnerText = m_requestFilter.GetAllowUserAndGroupMemberAsString(false);
                    nodeNxlResult.AppendChild(allowResult);

                    XmlNode denyResult = xmlDoc.CreateNode(XmlNodeType.Element, "DenyMember", xmlnsm.LookupNamespace(strDefaulXmlns));
                    denyResult.InnerText = m_requestFilter.GetDenyMemberAsString();
                    nodeNxlResult.AppendChild(denyResult);

                    XmlNode allowManagerResult = xmlDoc.CreateNode(XmlNodeType.Element, "AllowManager", xmlnsm.LookupNamespace(strDefaulXmlns));
                    allowManagerResult.InnerText = m_requestFilter.GetAllowUserAndGroupManagerAsString(false);
                    nodeNxlResult.AppendChild(allowManagerResult);

                    XmlNode denyManagerResult = xmlDoc.CreateNode(XmlNodeType.Element, "DenyManager", xmlnsm.LookupNamespace(strDefaulXmlns));
                    denyManagerResult.InnerText = m_requestFilter.GetDenyManagerAsString();
                    nodeNxlResult.AppendChild(denyManagerResult);

                    nodeResult.AppendChild(nodeNxlResult);
                }
            }
        }

        private void ParseModifyRoomResult(XmlDocument xmlDoc, XmlNamespaceManager xmlnsm, string strDefaulXmlns)
        {
            ParseCreateNewOrModifyRoomResult(xmlDoc, xmlnsm, strDefaulXmlns, false);
        }

        private void ParseNewRoomInfoCommand(XmlDocument xmlDoc, XmlNamespaceManager xmlnsm, string strDefaulXmlns)
        {
            xmlnsm.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            xmlnsm.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            XmlNodeList lstNodeCategories = xmlDoc.DocumentElement.SelectNodes("//" + strDefaulXmlns + ":CategoryTag", xmlnsm);
            if(lstNodeCategories != null)
            {
#if false
                //query data to check if this category need enforce
                List<SFBObjectInfo> lstNLCategoryInfo = SFBObjectInfo.GetAllObjsFormPersistentInfo(SFBCommon.Common.EMSFB_INFOTYPE.emInfoNLChatCategory);
               
                //check each category
                foreach (XmlNode cateNode in lstNodeCategories)
                {
                    string strURI = cateNode.ChildNodes[1].InnerText;
                    SFBObjectInfo nlCateInfo = FindObjectInfoByUri(lstNLCategoryInfo, strURI);

                    AddNxlEnforcerNodeForNewRoomInfo(cateNode, nlCateInfo, xmlDoc, lstNLCategoryInfo, xmlnsm, strDefaulXmlns);
                }
#else
                foreach (XmlNode cateNode in lstNodeCategories)
                {
                    string strURI = cateNode.ChildNodes[1].InnerText;
                    NLChatCategoryInfo obNLChatCategoryInfo = new NLChatCategoryInfo();
                    bool bResult = obNLChatCategoryInfo.EstablishObjFormPersistentInfo(NLChatCategoryInfo.kstrUriFieldName, strURI);
                    if((!bResult) || (obNLChatCategoryInfo.GetInfo().Count<=0) )
                    {
                        obNLChatCategoryInfo.SetItem(NLChatCategoryInfo.kstrUriFieldName, strURI);
                        obNLChatCategoryInfo.SetItem(NLChatCategoryInfo.kstrEnforcerFieldName, CommonCfgMgr.GetInstance().DefaultChatRoomCategoryEnforcer.ToString() );
                        obNLChatCategoryInfo.SetItem(NLChatCategoryInfo.kstrForceEnforcerFieldName, CommonCfgMgr.GetInstance().DefaultForceInheritChatCategoryEnforcer.ToString() );

                        theBaseLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "obNLChatCategoryInfo.EstablishObjFormPersistentInfo failed, use default value, uri={0}", strURI);
                    }

                    AddNxlEnforcerNodeForNewRoomInfo(cateNode, obNLChatCategoryInfo, xmlDoc, xmlnsm, strDefaulXmlns);
                }
#endif
            }
        }

        //landing result contains all chat rooms
        private void ParseLandingResult(XmlDocument xmlDoc, XmlNamespaceManager xmlnsm, string strDefaulXmlns)
        {
            if (m_lstLandingResult == null)
            {
                m_lstLandingResult = new List<LandResultRoom>();
            }
            m_lstLandingResult.Clear();

            XmlNodeList lstNodeRoom = xmlDoc.DocumentElement.SelectNodes("//" + strDefaulXmlns + ":ChatRoomTag", xmlnsm);
            if (lstNodeRoom != null)
            {
                foreach (XmlNode nodeRoom in lstNodeRoom)
                {
                    string roomID = nodeRoom.ChildNodes[0].InnerText;
                    string roomName = nodeRoom.ChildNodes[2].InnerText;
                    roomName = HttpUtility.HtmlDecode(roomName);

                    LandResultRoom landRoom = new LandResultRoom()
                    {
                        Name = roomName,
                        ChatRoomGuid = roomID
                    };

                    m_lstLandingResult.Add(landRoom);
                }
            }
        }

        private void ParseRoomAccessRightResult(XmlDocument xmlDoc, XmlNamespaceManager xmlnsm, string strDefaulXmlns)
        {
            //get room ID
            string strRoomID = "";
             XmlNode nodeRoomID = xmlDoc.DocumentElement.SelectSingleNode("//" + strDefaulXmlns + ":RequestedRoomId", xmlnsm);
             if (nodeRoomID != null)
             {
                 strRoomID = nodeRoomID.InnerText;
             }

            //get category uri,
             string strCategoryUri = "";
             XmlNode nodeCategoryUri = xmlDoc.DocumentElement.SelectSingleNode("//" + strDefaulXmlns + ":CategoryUri", xmlnsm);
            if(nodeCategoryUri!=null)
            {
                strCategoryUri = nodeCategoryUri.InnerText;
            }

            //insert node
             XmlNode nodeRoomInfo = xmlDoc.DocumentElement.SelectSingleNode("//" + strDefaulXmlns + ":RoomInfo", xmlnsm);
             if (nodeRoomInfo != null)
             {
                NLChatCategoryInfo obNLChatCategoryInfo = new NLChatCategoryInfo();
                bool bResult = obNLChatCategoryInfo.EstablishObjFormPersistentInfo(NLChatCategoryInfo.kstrUriFieldName, strCategoryUri);
                if ((!bResult) || (obNLChatCategoryInfo.GetInfo().Count <= 0))
                {
                    obNLChatCategoryInfo.SetItem(NLChatCategoryInfo.kstrUriFieldName, strCategoryUri);
                    obNLChatCategoryInfo.SetItem(NLChatCategoryInfo.kstrEnforcerFieldName, CommonCfgMgr.GetInstance().DefaultChatRoomCategoryEnforcer.ToString());
                    obNLChatCategoryInfo.SetItem(NLChatCategoryInfo.kstrForceEnforcerFieldName, CommonCfgMgr.GetInstance().DefaultForceInheritChatCategoryEnforcer.ToString());

                    theBaseLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "obNLChatCategoryInfo.EstablishObjFormPersistentInfo failed, use default value, uri={0}", strCategoryUri);
                }

                AddNxlEnforcerNodeForNewRoomInfo(nodeRoomInfo, obNLChatCategoryInfo, xmlDoc, xmlnsm, strDefaulXmlns);

                //added NLRoomINfo node
                SFBChatRoomVariableInfo roomVar = new SFBChatRoomVariableInfo();
                roomVar.EstablishObjFormPersistentInfo(SFBChatRoomVariableInfo.kstrUriFieldName, strRoomID);
                AddNxlRoomInfo(nodeRoomInfo, roomVar, xmlDoc, xmlnsm, strDefaulXmlns);
             }

        }


        private SFBObjectInfo FindObjectInfoByUri(List<SFBObjectInfo> lstObj, string strURI)
        {
            foreach (SFBObjectInfo sfbObj in lstObj)
            {
                string strAttributeUri = sfbObj.GetItemValue(NLChatCategoryInfo.kstrUriFieldName);
                if ((strAttributeUri != null) && strAttributeUri.Equals(strURI, StringComparison.OrdinalIgnoreCase))
                {
                    return sfbObj;
                }
            }

            return null;
        }

        private string GetChatRoomEnforceStatus(string strRoomID, string strCategoryUri)
        {
            //get from room
            NLChatRoomInfo nlChatRoom = new NLChatRoomInfo();
            bool bResult = nlChatRoom.EstablishObjFormPersistentInfo(NLChatRoomInfo.kstrUriFieldName, strRoomID);
            if(bResult)
            {
                theBaseLog.OutputLog(SFBCommon.NLLog.EMSFB_LOGLEVEL.emLogLevelInfo, "GetChatRoomEnforceStatus, get from chat room, result=" + nlChatRoom.GetItemValue(NLChatRoomInfo.kstrEnforcerFieldName));
                return nlChatRoom.GetItemValue(NLChatRoomInfo.kstrEnforcerFieldName);
            }
            else
            {//get from category
                NLChatCategoryInfo nlCategory = new NLChatCategoryInfo();
                bResult = nlCategory.EstablishObjFormPersistentInfo(NLChatCategoryInfo.kstrUriFieldName, strCategoryUri);
                if(bResult)
                {
                    theBaseLog.OutputLog(SFBCommon.NLLog.EMSFB_LOGLEVEL.emLogLevelInfo, "GetChatRoomEnforceStatus, get from category, result=" + nlCategory.GetItemValue(NLChatCategoryInfo.kstrEnforcerFieldName));
                    return nlCategory.GetItemValue(NLChatCategoryInfo.kstrEnforcerFieldName);
                }
            }

            theBaseLog.OutputLog(SFBCommon.NLLog.EMSFB_LOGLEVEL.emLogLevelInfo, "GetChatRoomEnforceStatus, get from default value result=" + CHttpTools.kstrDefaultEnforceForChatRoom);

            return CHttpTools.kstrDefaultEnforceForChatRoom;   
        }

        private void AddNxlRoomInfo(XmlNode nodeRoomInfo, SFBChatRoomVariableInfo roomVar, XmlDocument xmlDoc, XmlNamespaceManager xmlnsm, string strDefaultXmlns)
        {
            try
            {
                //create new node
                XmlNode nodeNLRoomInfo = xmlDoc.CreateNode(XmlNodeType.Element, "NLRoomInfo", xmlnsm.LookupNamespace(strDefaultXmlns));
                if (nodeNLRoomInfo != null)
                {
                    nodeRoomInfo.AppendChild(nodeNLRoomInfo);

                    //added Tags
                    XmlNode nodeTags = xmlDoc.CreateNode(XmlNodeType.Element, "Tags", xmlnsm.LookupNamespace(strDefaultXmlns));
                    if(null != nodeTags)
                    {
                        nodeNLRoomInfo.AppendChild(nodeTags);

                        string strTagsXml = roomVar.GetItemValue(SFBChatRoomVariableInfo.kstrClassifyTagsFieldName);
                        if(!string.IsNullOrWhiteSpace(strTagsXml))
                        {
                            ClassifyTagsHelper classifyHelper = new ClassifyTagsHelper(strTagsXml);
                            Dictionary<string, string> dicTags = classifyHelper.ClassifyTags;
                            if((null!=dicTags) && dicTags.Count>0)
                            {
                                foreach(KeyValuePair<string,string> pairTag in dicTags)
                                {
                                    if((!string.IsNullOrWhiteSpace(pairTag.Key)) && (!string.IsNullOrWhiteSpace(pairTag.Value)))
                                    {
                                        XmlNode nodeTag = xmlDoc.CreateNode(XmlNodeType.Element, "Tag", xmlnsm.LookupNamespace(strDefaultXmlns));
                                        nodeTags.AppendChild(nodeTag);

                                        nodeTag.Attributes.Append(xmlDoc.CreateAttribute("name"));
                                        nodeTag.Attributes["name"].Value = pairTag.Key;

                                        nodeTag.Attributes.Append(xmlDoc.CreateAttribute("value"));
                                        nodeTag.Attributes["value"].Value = pairTag.Value;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                theBaseLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception on AddNxlRoomInfo:{0}", ex.ToString());
            }
        }

        private void AddNxlEnforcerNodeForNewRoomInfo(XmlNode nodeCate, NLChatCategoryInfo nlCateInfo, XmlDocument xmlDoc, XmlNamespaceManager xmlnsm, string strDefaulXmlns)
        {
            try
            {
                //query data
                 string  strNeedEnforce = nlCateInfo.GetItemValue(NLChatCategoryInfo.kstrEnforcerFieldName);
                 string  strForceEnforce = nlCateInfo.GetItemValue(NLChatCategoryInfo.kstrForceEnforcerFieldName);


                //create new node
                XmlNode enforcerNode = xmlDoc.CreateNode(XmlNodeType.Element, "NLEnforce", xmlnsm.LookupNamespace(strDefaulXmlns));
                if (enforcerNode != null)
                {
                    nodeCate.AppendChild(enforcerNode);

                    //added attribute
                    enforcerNode.Attributes.Append(xmlDoc.CreateAttribute("NeedEnforce"));
                    enforcerNode.Attributes["NeedEnforce"].Value = strNeedEnforce;

                    enforcerNode.Attributes.Append(xmlDoc.CreateAttribute("ForceEnforce"));
                    enforcerNode.Attributes["ForceEnforce"].Value = strForceEnforce;

                    //added sub node
                    XmlNode nodeTextEnableEnforce = xmlDoc.CreateNode(XmlNodeType.Element, "TextEnableEnforce", xmlnsm.LookupNamespace(strDefaulXmlns));
                    nodeTextEnableEnforce.InnerText = CConfig.strTextEnableEnforce;
                    enforcerNode.AppendChild(nodeTextEnableEnforce);

                    XmlNode nodeTextEnforceDescYes = xmlDoc.CreateNode(XmlNodeType.Element, "TextEnforceStatusYes", xmlnsm.LookupNamespace(strDefaulXmlns));
                    nodeTextEnforceDescYes.InnerText = CConfig.strTextEnforceDescYes;
                    enforcerNode.AppendChild(nodeTextEnforceDescYes);

                    XmlNode nodeTextEnforceDescNo = xmlDoc.CreateNode(XmlNodeType.Element, "TextEnforceStatusNo", xmlnsm.LookupNamespace(strDefaulXmlns));
                    nodeTextEnforceDescNo.InnerText = CConfig.strTextEnforceDescNo;
                    enforcerNode.AppendChild(nodeTextEnforceDescNo);

                    //added manual classify info(xml)
                    XmlNode nodeClassification = xmlDoc.CreateNode(XmlNodeType.Element, "Classification", xmlnsm.LookupNamespace(strDefaulXmlns));
                    enforcerNode.AppendChild(nodeClassification);

                    string  strClassifyInfo = nlCateInfo.GetItemValue(NLChatCategoryInfo.kstrClassificationFieldName);
                    XmlCDataSection  cDataClassification = xmlDoc.CreateCDataSection(strClassifyInfo);
                    nodeClassification.AppendChild(cDataClassification);

                }
            }
            catch(Exception ex)
            {
               theBaseLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, ex.ToString());
            }

        }
    }
}
