using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.Common;
using SFBCommon.SFBObjectInfo;
using SFBCommon.NLLog;

namespace TestProject.SFBObjectTest
{
    class SFBObjTester
    {
        static public void Test()
        {
            NLTagLabelInfo obNLTagLabelInfo = new NLTagLabelInfo();
            bool bEstablished = obNLTagLabelInfo.EstablishObjFormPersistentInfo(NLTagLabelInfo.kstrTagNameFieldName, "KimTest");
            if (bEstablished)
            {
                obNLTagLabelInfo.OutputObjInfo();
            }
        }
        static public void Test_GetDatasFromPersistentInfoWithFullSearchConditions()
        {
            // select sfb.sfbmeetingvariabletable.uri, sfb.sfbmeetingtable.creator, isstaticmeeting from sfb.sfbmeetingtable, sfb.sfbmeetingvariabletable where (sfb.sfbmeetingtable.uri = sfb.sfbmeetingvariabletable.uri and sfb.sfbmeetingtable.expirytime > '2017-02-00T17:33:06Z' and sfb.sfbmeetingvariabletable.donemanulclassify = 'Yes') or (sfb.sfbmeetingtable.uri = sfb.sfbmeetingvariabletable.uri and sfb.sfbmeetingtable.isstaticmeeting = 'True')
            // sfb.sfbmeetingvariabletable.uri, sfb.sfbmeetingtable.creator, isstaticmeeting
            List<STUSFB_INFOFIELD> lsSpecifyOutFields = new List<STUSFB_INFOFIELD>()
            {
                {new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeetingVariable, SFBMeetingVariableInfo.kstrUriFieldName)},
                {new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeeting, SFBMeetingInfo.kstrCreatorFieldName)},
                {new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoUnknown, SFBMeetingInfo.kstrMeetingTypeFieldName)}
            };
            // sfb.sfbmeetingtable, sfb.sfbmeetingvariabletable
            List<EMSFB_INFOTYPE> lsSearchScopes = new List<EMSFB_INFOTYPE>()
            {
                {EMSFB_INFOTYPE.emInfoSFBMeeting},
                {EMSFB_INFOTYPE.emInfoSFBMeetingVariable}
            };
            /*
             * (sfb.sfbmeetingtable.uri = sfb.sfbmeetingvariabletable.uri and sfb.sfbmeetingtable.expirytime > '2017-02-00T17:33:06Z' and sfb.sfbmeetingvariabletable.donemanulclassify = 'Yes')
             * or 
             * (sfb.sfbmeetingtable.uri = sfb.sfbmeetingvariabletable.uri and sfb.sfbmeetingtable.isstaticmeeting = 'True')
            */
            List<STUSFB_INFOITEM> lsComditonItemsA = new List<STUSFB_INFOITEM>()
            {
                {new STUSFB_INFOITEM(new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeeting, SFBMeetingInfo.kstrUriFieldName), new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeetingVariable, SFBMeetingVariableInfo.kstrUriFieldName), EMSFB_INFOCOMPAREOP.emSearchOp_Equal)},
                {new STUSFB_INFOITEM(new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeeting, SFBMeetingInfo.kstrExpiryTimeFieldName), new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoUnknown, "2017-02-00T17:33:06Z'"), EMSFB_INFOCOMPAREOP.emSearchOp_Above)},
                {new STUSFB_INFOITEM(new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeetingVariable, SFBMeetingVariableInfo.kstrDoneManulClassifyFieldName), new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoUnknown, SFBMeetingVariableInfo.kstrDoneManulClassifyYes), EMSFB_INFOCOMPAREOP.emSearchOp_Equal)}
            
            };
            STUSFB_CONDITIONGROUP stuConditionsGroupA = new STUSFB_CONDITIONGROUP(lsComditonItemsA, EMSFB_INFOLOGICOP.emSearchLogicAnd);
            List<STUSFB_INFOITEM> lsComditonItemsB = new List<STUSFB_INFOITEM>()
            {
                {new STUSFB_INFOITEM(new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeeting, SFBMeetingInfo.kstrUriFieldName), new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeetingVariable, SFBMeetingVariableInfo.kstrUriFieldName), EMSFB_INFOCOMPAREOP.emSearchOp_Equal)},
                {new STUSFB_INFOITEM(new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeeting, SFBMeetingInfo.kstrMeetingTypeFieldName), new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoUnknown, "Tru\\e"), EMSFB_INFOCOMPAREOP.emSearchOp_Equal)}
            };
            STUSFB_CONDITIONGROUP stuConditionsGroupB = new STUSFB_CONDITIONGROUP(lsComditonItemsB, EMSFB_INFOLOGICOP.emSearchLogicAnd);
            List<KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>> lsSearchConditions = new List<KeyValuePair<EMSFB_INFOLOGICOP,STUSFB_CONDITIONGROUP>>()
            {
                {new KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>(EMSFB_INFOLOGICOP.emSearchLogicOr, stuConditionsGroupA)},
                {new KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>(EMSFB_INFOLOGICOP.emSearchLogicOr, stuConditionsGroupB)}
            };
            {
                Dictionary<EMSFB_INFOTYPE, Dictionary<string, string>>[] szAllObjInfo = SFBObjectInfo.GetDatasFromPersistentInfoWithFullSearchConditions(lsSpecifyOutFields, lsSearchScopes, lsSearchConditions);
                if (null != szAllObjInfo)
                {
                    for (int i = 0; i < szAllObjInfo.Length; ++i)
                    {
                        TestProject.CommonTools.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Index:{0}:\n", i);
                        foreach (KeyValuePair<EMSFB_INFOTYPE, Dictionary<string, string>> pairObjInfo in szAllObjInfo[i])
                        {
                            TestProject.CommonTools.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\tType:{0}:\n", pairObjInfo.Key);
                            foreach (KeyValuePair<string, string> pairKeyValues in pairObjInfo.Value)
                            {
                                TestProject.CommonTools.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\t\tKey:[{0}],Value:[{1}]\n", pairKeyValues.Key, pairKeyValues.Value);
                            }
                        }
                    }
                }
            }
        }
    }
}
