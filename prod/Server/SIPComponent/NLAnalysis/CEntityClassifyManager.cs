using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.SFBObjectInfo;
using SFBCommon.NLLog;
using Nextlabs.SFBServerEnforcer.PolicyHelper;
using SFBCommon.ClassifyHelper;

namespace Nextlabs.SFBServerEnforcer.SIPComponent
{
    class CEntityClassifyManager
    {
        private static CLog theLog = CLog.GetLogger("EntityClassifyManager");

        public static bool ProcessAutoClassifyObligations(List<PolicyObligation> lstAutoTagObligations, SFBMeetingVariableInfo meetingVar)
        {
            //get tags from obligations
            Dictionary<string, string> dicNewTags = PolicyObligation.GetDictionaryTagsFromAutoClassifyObligation(lstAutoTagObligations);

            //added to exist tag
            bool bHaveNewTags = dicNewTags.Count > 0;
            if(bHaveNewTags)
            {
                meetingVar.AddedNewTags(dicNewTags);
            }

            return bHaveNewTags;
        }

        public static bool ProcessAutoClassifyObligations(List<PolicyObligation> lstAutoTagObligations, SFBChatRoomVariableInfo roomVar)
        {
            //get tags from obligations
            Dictionary<string, string> dicNewTags = PolicyObligation.GetDictionaryTagsFromAutoClassifyObligation(lstAutoTagObligations);

            //added to exist tag
            bool bHaveNewTags = dicNewTags.Count > 0;
            if (bHaveNewTags)
            {
                roomVar.AddedNewTags(dicNewTags);
            }

            return bHaveNewTags;
        }

        public static string ProcessManualClassifyObligations(List<PolicyObligation> lstManualTagObligation, ref string strForceManualClassify)
        {
            strForceManualClassify = PolicyMain.KStrObAttributeForceClassifyNo;
            ManulClassifyObligationHelper manualClassifyHelper = null;
            foreach(PolicyObligation manualTag in lstManualTagObligation)
            {
                string strManuTagXml = manualTag.GetAttribute(PolicyMain.KStrObAttributeManualClassifyData);
                string strForce = manualTag.GetAttribute(PolicyMain.KStrObAttributeForceClassify);
                if(!string.IsNullOrWhiteSpace(strManuTagXml))
                {
                    if(null==manualClassifyHelper)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Found manual classify:{0}, force:{1}", strManuTagXml, strForce);
                        manualClassifyHelper = new ManulClassifyObligationHelper(strManuTagXml, false);
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "manual classify appened:{0}", strManuTagXml);
                        manualClassifyHelper.Append(strManuTagXml, false);
                    }
                }

                //if one of the manual classify is force, all is force.
                if((!string.IsNullOrWhiteSpace(strForce)) && strForce.Equals(PolicyMain.KStrObAttributeForceClassifyYes, StringComparison.OrdinalIgnoreCase))
                {
                    strForceManualClassify = PolicyMain.KStrObAttributeForceClassifyYes;
                }
            }

            if(null!=manualClassifyHelper)
            {
                return manualClassifyHelper.GetClassifyXml();
            }

            return string.Empty;
        }
    }
}
