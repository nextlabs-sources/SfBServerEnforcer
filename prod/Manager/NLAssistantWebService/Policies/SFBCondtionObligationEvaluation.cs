using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nextlabs.SFBServerEnforcer.PolicyHelper;
using SFBCommon.SFBObjectInfo;
using SFBCommon.NLLog;

namespace NLAssistantWebService.Policies
{
    enum ConditionEvaluationResult
    {
        Evaluation_Allow = 0,
        Evaluation_DontCare_Deny = 1, // the action on dontcare is deny
        Evaluation_DontCare_Allow = 2, //the action on dontcare is allow
        Evaluation_DontCare_DontCare = 3, //the action on dontcare is dontcare
        Evaluation_Unknow = 1000
    }

    class SFBCondtionObligationEvaluation
    {
        public static CLog theLog = CLog.GetLogger("CondtionObligationEvaluation");
        
        //Do Evaluation for condition within obligation.
        //here we will separate obligation according to the policy name.
        //if one of the obligation within a policy cause the result to deny, we will stop the evaluation and remove all other obligations.
        public static void DoEvalutionForConditionWithinObligation(PolicyResult policyResult, List<string> lstParticipants)
        {
            List<PolicyObligation> lstCondOb = policyResult.GetAllConditionObligations();

            if (lstCondOb.Count>0)
            {
                //evaluation
                Dictionary<string, List<PolicyObligation>> dicPolicyOb = PolicyResult.SplitObligationsByPolicyName(lstCondOb);
                List<ConditionEvaluationResult> lstEvaResult = new List<ConditionEvaluationResult>(dicPolicyOb.Count);
                foreach(KeyValuePair<string,List<PolicyObligation>> policyOb in dicPolicyOb)
                {
                    ConditionEvaluationResult evaluationRes = InnerDoEvalutionForUserConditonObligation(policyOb.Value, lstParticipants);
                    lstEvaResult.Add(evaluationRes);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "DoEvalutionForUserConditonObligationForMeeting result is:{0}", evaluationRes);

                    if(IsDontCareEvaluationResult(evaluationRes))
                    {//if the result is dontcare, means this policy is not match, we delete all the obligation within this policy
                        policyResult.RemoveObligationsByPolicyName(policyOb.Key);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Delete obligation within policy:{0}", policyOb.Key);
                    }
                    else if(evaluationRes==ConditionEvaluationResult.Evaluation_Allow)
                    {//when the result id Allow, means this policy is matched.
                      
                      if(policyResult.IsDeny())
                      {//if policyresult is deny, then the final result is Deny, we need to remove other obligation and return.
                         policyResult.RemoveObligationsByPolicyNameExcept(policyOb.Key);
                         theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "the policy result is deny. So delete all obligation except within policy:{0}", policyOb.Key);
                         break;
                      }
                      else
                      {//no action
                          
                      }

                    }
                    //else have no other result
                }

                //combine the condition result with the policy result
                if (HaveAllowEvalution(lstEvaResult))
                {//not to change the policy result.
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "DoEvalutionForConditionWithinObligation all condition is allow, the final enforcement is not change:{0}", policyResult.Enforcement);
                }
                else if(HaveDontCareDenyEvalution(lstEvaResult))
                {
                    policyResult.Enforcement = EnforceResult.Enforce_Deny;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "DoEvalutionForConditionWithinObligation have dontcare_deny condition, the final enforcement is change to:{0}", policyResult.Enforcement);
                }
                else
                {
                    policyResult.Enforcement = EnforceResult.Enforce_DontCare;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "DoEvalutionForConditionWithinObligation have dontcare_allow/dontcard condition, the final enforcement is change to:{0}", policyResult.Enforcement);
                }
            }
        }

        private static ConditionEvaluationResult InnerDoEvalutionForUserConditonObligation(List<PolicyObligation> lstOb, List<string> lstParticipants)
        {
            List<PolicyObligation> lstParticipateConditionObs = GetParticipatesConditionObligations(lstOb);
            if(lstParticipateConditionObs.Count>0)
            {
                if (lstParticipants.Count>0)
                {
                    return DoEvalutionForParticipateCondtions(lstOb, lstParticipants);
                }
                else
                {
                    return ConditionEvaluationResult.Evaluation_DontCare_DontCare;
                }
            }
            else
            {
                return ConditionEvaluationResult.Evaluation_DontCare_DontCare;
            }
        }


        private static ConditionEvaluationResult DoEvalutionForParticipateCondtions(List<PolicyObligation> lstOb, List<string> lstParticipate)
        {
            //parse participate
            if((null==lstParticipate) || lstParticipate.Count<=0)
            {
                return ConditionEvaluationResult.Evaluation_Allow;
            }

            //get all conditions
            List<KeyValuePair<string, string>> lstConditions = GetAllUserConditionsFromObligation(lstOb);
            if ((null == lstConditions) || lstConditions.Count <= 0)
            {
                return ConditionEvaluationResult.Evaluation_Allow;
            }

            //Do Evaluation
            PolicyResult[] arrayPolicyResult = null;
            int nPolicyQueryResult = MeetingPolicy.GetInstance().QueryPolicyForUserConditionObligation(lstParticipate, lstConditions, out arrayPolicyResult);
            if(nPolicyQueryResult!=0)
            {
                foreach(PolicyResult policyResult in arrayPolicyResult)
                {
                    if(policyResult.IsAllow())//if one of the participate matches the policy. the whole condition matches.
                    {
                        return ConditionEvaluationResult.Evaluation_Allow;
                    }
                }

            }

            return ConditionEvaluationResult.Evaluation_DontCare_DontCare;
        }


        private static List<PolicyObligation> GetParticipatesConditionObligations(List<PolicyObligation> lstOb)
        {
            List<PolicyObligation> lstParticipateConditionObs = new List<PolicyObligation>(lstOb.Count);
            foreach(PolicyObligation ob in lstOb)
            {
                string strSubject = ob.GetAttribute(PolicyMain.KStrObAttributeUserConditionSubject);
                if((!string.IsNullOrWhiteSpace(strSubject)) && strSubject.Equals(PolicyMain.KStrObAttributeUserConditionSubjectValueParticipate) )
                {
                    lstParticipateConditionObs.Add(ob);
                }
            }
            return lstParticipateConditionObs;
        }

        private static List<KeyValuePair<string,string>> GetAllUserConditionsFromObligation(List<PolicyObligation> lstObligation)
        {
            List<KeyValuePair<string, string>> lstUserConditions = new List<KeyValuePair<string, string>>(lstObligation.Count);
            foreach(PolicyObligation ob in lstObligation)
            {
                string strCondition = ob.GetAttribute(PolicyMain.KStrObAttributeUserConditionValue);
                if(!string.IsNullOrWhiteSpace(strCondition))
                {
                    string strConKey, strConValue;
                    ParseUserCondition(strCondition, out strConKey, out strConValue);
                    if ((!string.IsNullOrWhiteSpace(strConKey)) && (!string.IsNullOrWhiteSpace(strConValue)))
                    {
                        lstUserConditions.Add(new KeyValuePair<string, string>(strConKey, strConValue));
                    }
                }
            }
            return lstUserConditions;
        }

        private static void ParseUserCondition(string strCondition, out string strConKey, out string strConValue)
        {
            strConKey = null;
            strConValue = null;

            int nPos = strCondition.IndexOf('=');
            if((nPos>0) && (nPos<strCondition.Length-1))
            {
                strConKey = strCondition.Substring(0, nPos);
                strConValue = strCondition.Substring(nPos + 1);
            }
        }

        private static bool IsDontCareEvaluationResult(ConditionEvaluationResult evaResult)
        {
            return evaResult == ConditionEvaluationResult.Evaluation_DontCare_Allow ||
                evaResult == ConditionEvaluationResult.Evaluation_DontCare_Deny ||
                evaResult == ConditionEvaluationResult.Evaluation_DontCare_DontCare;
        }

        private static bool HaveAllowEvalution(List<ConditionEvaluationResult> lstEvaResult)
        {
            foreach(ConditionEvaluationResult evaRes in lstEvaResult)
            {
                if(evaRes==ConditionEvaluationResult.Evaluation_Allow)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HaveDontCareDenyEvalution(List<ConditionEvaluationResult> lstEvaResult)
        {
            foreach (ConditionEvaluationResult evaRes in lstEvaResult)
            {
                if (evaRes == ConditionEvaluationResult.Evaluation_DontCare_Deny)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
