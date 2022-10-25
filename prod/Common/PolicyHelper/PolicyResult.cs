using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nextlabs.SFBServerEnforcer.PolicyHelper
{
    public enum EnforceResult
    {
        Enforce_Deny = 0,
        Enforce_Allow = 1,
        Enforce_DontCare = 2,
        Enforce_Unknow = 100
    }

    public class PolicyResult
    {
        #region member var
        private EnforceResult m_EnforceResult;
        private List<PolicyObligation> m_lstObligation;
        #endregion


        public PolicyResult(EnforceResult emDefaultEnforceResult = EnforceResult.Enforce_Unknow)
        {
            m_EnforceResult = emDefaultEnforceResult;
            m_lstObligation = new List<PolicyObligation>();
        }

        public bool IsDeny() { return m_EnforceResult == EnforceResult.Enforce_Deny; }
        public bool IsAllow() { return m_EnforceResult == EnforceResult.Enforce_Allow; }
        public bool IsDontCare() { return m_EnforceResult == EnforceResult.Enforce_DontCare; }

        public EnforceResult Enforcement { get { return m_EnforceResult; }
            set { m_EnforceResult = value; }
        }

        public void SetEnforceResult(long lResult)
        {
           switch(lResult)
           {
               case 0:
                   m_EnforceResult = EnforceResult.Enforce_Deny;
                   break;

               case 1:
                   m_EnforceResult = EnforceResult.Enforce_Allow;
                   break;

               case 2:
                   m_EnforceResult = EnforceResult.Enforce_DontCare;
                   break;

               default:
                   m_EnforceResult = EnforceResult.Enforce_Unknow;
                   break;
           }
        }

        public int AddObligation(PolicyObligation ob)
        {
            m_lstObligation.Add(ob);
            return m_lstObligation.Count;
        }

        public int ObligationCount()
        {
            return m_lstObligation.Count;
        }

        public PolicyObligation IterateObligation(int nIndex)
        {
            try
            {
                return m_lstObligation.ElementAt(nIndex);
            }
            catch
            {
                return null;
            }
        }

        public PolicyObligation GetFirstObligationByName(string strObName)
        {
            for (int iOb = 0; iOb < ObligationCount(); iOb++)
            {
                PolicyObligation obligation = IterateObligation(iOb);
                if ((obligation != null) && (obligation.Name.Equals(strObName, StringComparison.OrdinalIgnoreCase)))
                {
                    return obligation;
                }
            }
            return null;
        }

        public List<PolicyObligation> GetAllObligationByName(string strObName)
        {
            List<PolicyObligation> lstObs = new List<PolicyObligation>();
            for (int iOb = 0; iOb < ObligationCount(); iOb++)
            {
                PolicyObligation obligation = IterateObligation(iOb);
                if ((obligation != null) && (obligation.Name.Equals(strObName, StringComparison.OrdinalIgnoreCase)))
                {
                    lstObs.Add(obligation);
                }
            }
            return lstObs;
        }

        public List<PolicyObligation> GetAllConditionObligations()
        {
            string[] arryStrConditonObName = {PolicyMain.KStrObNameUserConditoin};

            List<PolicyObligation> lstObligation = new List<PolicyObligation>(m_lstObligation.Count);
            foreach(string strObName in arryStrConditonObName)
            {
                List<PolicyObligation> lstOb = GetAllObligationByName(strObName);
                lstObligation.AddRange(lstOb);
            }
            return lstObligation;
        }

        public void RemoveObligationsByPolicyName(string strPolicyName)
        {
             m_lstObligation.RemoveAll(item => item.PolicyName.Equals(strPolicyName,StringComparison.OrdinalIgnoreCase));
        }

        public void RemoveObligationsByPolicyNameExcept(string strPolicyName)
        {
            m_lstObligation.RemoveAll(item => !item.PolicyName.Equals(strPolicyName, StringComparison.OrdinalIgnoreCase));
        }

        public static Dictionary<string, List<PolicyObligation>> SplitObligationsByPolicyName(List<PolicyObligation> lstOb)
        {
            Dictionary<string, List<PolicyObligation>> dicPolicyObligation = new Dictionary<string, List<PolicyObligation>>();
            foreach(PolicyObligation ob in lstOb)
            {
                if(!dicPolicyObligation.ContainsKey(ob.PolicyName))
                {
                    dicPolicyObligation[ob.PolicyName] = new List<PolicyObligation>();
                }

                dicPolicyObligation[ob.PolicyName].Add(ob);  
            }
            return dicPolicyObligation;
        }


    }
}
