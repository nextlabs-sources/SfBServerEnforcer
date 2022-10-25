using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDKWrapperLib;
using SFBCommon.Common;
using SFBCommon.SFBObjectInfo;
using SFBCommon.ClassifyHelper;

namespace Nextlabs.SFBServerEnforcer.PolicyHelper
{
    public class PolicyRequestInfo
    {
        #region Const/read only values
        private string kstrAttachmentClassificationPrefix = "file_";
        #endregion

        #region Members
        private EMSFB_ACTION m_emAction;
        private string m_strAction;
        private string m_strUserSender;
        private List<KeyValuePair<string, string>> m_lstUserAttribute;
        private string m_strReceiver;
        private SFBCommon.SFBObjectInfo.SFBObjectInfo[] m_objs;
        private List<string> m_lstParticipants;
        #endregion

        public PolicyRequestInfo(EMSFB_ACTION emAction, string strUser, string strReceiver, SFBObjectInfo[] objs, List<KeyValuePair<string,string>> lstUserAttribute=null, List<string> lstParticipants=null)
        {
            this.m_emAction = emAction;
            this.m_strAction = PolicyMain.GetActionStringNameByType(emAction);
            this.m_strUserSender = strUser;
            this.m_strReceiver = strReceiver;
            this.m_lstUserAttribute = lstUserAttribute;
            this.m_objs = objs;
            this.m_lstParticipants = lstParticipants;
        }

        public SDKWrapperLib.Request ConstructRequest()
        {
            Request pReq = new Request();
            pReq.set_action(m_strAction);

            pReq.set_app(System.IO.Path.GetFileName(PolicyMain.s_kstrAppPath), PolicyMain.s_kstrAppPath, "", null);
            pReq.set_noiseLevel(2);

            CEAttres theUserAttres = new CEAttres();
            if(null != m_lstUserAttribute)
            {
                foreach(KeyValuePair<string,string> pairAttr in m_lstUserAttribute)
                {
                    theUserAttres.add_attre(pairAttr.Key, pairAttr.Value);
                }
            }
            /*
                If we want to support both the newest UAP and the old UAP, we need do the follow steps:
                    1. CC Side:
                        1. When we enroll, we need map “User.string.unixId” as “userPrincipalName”  in configure file ad.qapf1.def
                    2. PC Side: 
                        1. If we used the newest UAP, we need set “DOMAIN_1_user_key_attributes = ci:mail” in configure file ServerUserAttributeProvider.properties
                    3. PEP Side:
                        1. When we query policy by using the CESDK, we need set the userID as email address, old we set this number as empty string.
            */
            pReq.set_user(m_strUserSender.ToLower(), m_strUserSender.ToLower(), theUserAttres);
            pReq.set_performObligation(1);

            CEAttres theSrcAttres = new CEAttres();
            theSrcAttres.add_attre("ce::nocache", "yes");
            theSrcAttres.add_attre("ce::filesystemcheck", "yes");

            if(null!=m_objs)
            {
                string strPrefix = "";
                foreach (SFBCommon.SFBObjectInfo.SFBObjectInfo obj in m_objs)
                {
                    if (null != obj)
                    {
                        Dictionary<string, string> objInfoDic = obj.GetInfo();
                        foreach (var vInfo in objInfoDic)
                        {
                            if ((!string.IsNullOrWhiteSpace(vInfo.Key)) && (!string.IsNullOrWhiteSpace(vInfo.Value)))
                            {
                                if (PolicyTools.IgnoreAttribute(obj, vInfo.Key))
                                {
                                    continue;
                                }
                                else if (PolicyTools.NeedSeprateAttribute(obj, vInfo.Key))
                                {
                                    PolicyTools.SeprateAttribute(vInfo.Value, SFBObjectInfo.kstrMemberSeparator, vInfo.Key, theSrcAttres);
                                }
                                else if (IsClassificationInfo(obj, vInfo.Key, out strPrefix))
                                {
                                    AddedClassificationTags(theSrcAttres, vInfo.Value, strPrefix);
                                }
                                else
                                {
                                    theSrcAttres.add_attre(vInfo.Key, vInfo.Value);
                                }
                            }
                        }
                    } // if (null != obj)
                } // foreach (SFBCommon.SFBObjectInfo.SFBObjectInfo obj in objs)
            }// if(null!=objs)

            //added participants
            if(null!=m_lstParticipants)
            {
                foreach(string strParticipant in m_lstParticipants)
                {
                    theSrcAttres.add_attre(SFBCommon.SFBObjectInfo.SFBMeetingVariableInfo.kstrParticipatesFieldName, strParticipant);
                }
            }


            //we get the first object's type as the policy object type
           // theSrcAttres.add_attre("entity_type", PolicyMain.GetPolicyTypeBySFBType(objs[0].GetSFBInfoType()));
            //get policy model name
            string strPolicyModel = PolicyMain.GetPolicyModelByAction(m_emAction);
            if(string.IsNullOrWhiteSpace(strPolicyModel))
            {
                strPolicyModel = "fso";
            }

            string strResourceName = string.Format("SFB:/{0}" , strPolicyModel) ;
            pReq.set_param(strResourceName, strPolicyModel, theSrcAttres, 0);

            //set name attribute
             CEAttres nameAttribute = new CEAttres();
             nameAttribute.add_attre("dont-care-acceptable", "yes");
             pReq.set_param("environment", "", nameAttribute, 2);

            pReq.set_recipient(m_strReceiver.ToLower());

            return pReq;
        }

        private bool IsClassificationInfo(SFBObjectInfo obj, string strAttrName, out string strPrefix)
        {
            strPrefix = "";
            bool bIsClassifyInfo = false;
            switch (obj.GetSFBInfoType())
            {
            case EMSFB_INFOTYPE.emInfoSFBChatRoomVariable:
            {
                bIsClassifyInfo = strAttrName.Equals(SFBChatRoomVariableInfo.kstrClassifyTagsFieldName);
                break;
            }
            case EMSFB_INFOTYPE.emInfoSFBMeetingVariable:
            {
                bIsClassifyInfo = strAttrName.Equals(SFBMeetingVariableInfo.kstrClassifyTagsFieldName);
                break;
            }
            case EMSFB_INFOTYPE.emInfoNLChatRoomAttachment:
            {
                bIsClassifyInfo = strAttrName.Equals(NLChatRoomAttachmentInfo.kstrFileTagsFieldName);
                strPrefix = kstrAttachmentClassificationPrefix;
                break;
            }
            default:
                bIsClassifyInfo = false;
                break;
            }

            return bIsClassifyInfo;
        }

        private void AddedClassificationTags(CEAttres ceAttres, string strTagsXml, string strPrefix)
        {
            if(!string.IsNullOrWhiteSpace(strTagsXml))
            {
                ClassifyTagsHelper classifyHelper = new ClassifyTagsHelper(strTagsXml);
                Dictionary<string, string> dicTags = classifyHelper.ClassifyTags;
                if(dicTags!=null)
                {
                    strPrefix = CommonHelper.GetSolidString(strPrefix);
                    foreach(KeyValuePair<string,string> pairTag in dicTags)
                    {
                        ceAttres.add_attre(strPrefix+pairTag.Key, pairTag.Value);
                    }
                }   
            }
        }
    }
}
