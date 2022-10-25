using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SDKWrapperLib;

namespace Nextlabs.SFBServerEnforcer.PolicyHelper
{
    public class PolicyObligation
    {
        private string m_strObligationName;
        private string m_strPolicyName;
        protected List<KeyValuePair<string, string>> m_lstAttribute;

        public PolicyObligation(string strName, Obligation ob)
        {
            Name = strName;
            ob.get_policyname(out m_strPolicyName);
            m_lstAttribute = new List<KeyValuePair<string, string>>();
            CEAttres ceAttres = new CEAttres();
            ob.get_attres(out ceAttres);
            SetAttributes(ceAttres);
        }

        public string Name { get { return m_strObligationName; } set { m_strObligationName = value;} }
        public string PolicyName { get { return m_strPolicyName; }  }

        public string GetAttribute(string strName)
        {
            foreach(KeyValuePair<string,string> attr in m_lstAttribute)
            {
                if(attr.Key.Equals(strName, StringComparison.OrdinalIgnoreCase))
                {
                    return attr.Value;
                }
            }
            return null;
        }

        private void SetAttributes(CEAttres ceAtts)
        {
            int AttrCount = 0;
            ceAtts.get_count(out AttrCount);
            for(int nIndex = 0; nIndex<AttrCount; nIndex++)
            {
                string strName, strValue;
                ceAtts.get_attre(nIndex, out strName, out strValue);
                m_lstAttribute.Add(new KeyValuePair<string, string>(strName, strValue));
            }
        }

        public static Dictionary<string, string> GetDictionaryTagsFromAutoClassifyObligation(List<PolicyObligation> lstAutoTagObligations)
        {
            Dictionary<string, string> dicTags = new Dictionary<string, string>(lstAutoTagObligations.Count);
            for (int nNewTagIndex = 0; nNewTagIndex < lstAutoTagObligations.Count; nNewTagIndex++)
            {
                string strTagName = lstAutoTagObligations[nNewTagIndex].GetAttribute(PolicyMain.kStrObAttributeAutoClassifyTagName);
                string strTagValue = lstAutoTagObligations[nNewTagIndex].GetAttribute(PolicyMain.KstrObAttributeAutoClassifyTagValue);
                if ((!string.IsNullOrWhiteSpace(strTagName)) && (!string.IsNullOrWhiteSpace(strTagValue)))
                {
                    SFBCommon.Common.CommonHelper.AddKeyValuesToDir(dicTags, strTagName, strTagValue);
                }
            }

            return dicTags;
        }

    }
}
