using Nextlabs.SFBServerEnforcer.PolicyHelper;
using NLAssistantWebService.Policies;
using SFBCommon.NLLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace NLAssistantWebService.Models
{
    public class PolicyResultXmlBuilder
    {
        #region XML Node & Attributes Name Definitions

        public static string POLICYRESULTS_NODENAME = "PolicyResults";
        public static string JOINRESULTS_NODENAME = "JoinResult";
        public static string RESULTCODE_NODENAME = "ResultCode";
        public static string QueryIdentify_NODENAME = "QueryIdentify";
        public static string RESULT_NODENAME = "Result";

        public static string ENFORCEMENT_ATTRNAME = "Enforcement";
        public static string PARTICIPANT_ATTRNAME = "Participant";

        #endregion

        private static CLog logger = CLog.GetLogger(typeof(PolicyResultXmlBuilder));

        #region Public Functions

        public static string BuildJoinResultXML(string resultCode, string strQueryIdentify, string[] participantArray, PolicyResult[] policyResultArray)
        {
            string strXMLResult = "";
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement policyResultsElement = XmlVariable.CreateElement(xmlDoc, POLICYRESULTS_NODENAME, null, null);
            XmlElement resultCodeElement = XmlVariable.CreateElement(xmlDoc, RESULTCODE_NODENAME, null, resultCode);
            XmlElement queryIdentifyElement = XmlVariable.CreateElement(xmlDoc, QueryIdentify_NODENAME, null, strQueryIdentify);           
            XmlElement joinResultsElement = XmlVariable.CreateElement(xmlDoc, JOINRESULTS_NODENAME, null, null);


            if(participantArray != null && policyResultArray != null && participantArray.Length == policyResultArray.Length)
            {
                for(int i = 0; i < participantArray.Length; ++i)
                {
                    try
                    {
                        XmlElement resultElement = XmlVariable.CreateElement(xmlDoc, RESULT_NODENAME, null, null);
                        resultElement.SetAttribute(ENFORCEMENT_ATTRNAME, policyResultArray[i].Enforcement.ToString());
                        resultElement.SetAttribute(PARTICIPANT_ATTRNAME, participantArray[i]);
                        joinResultsElement.AppendChild(resultElement);
                    }
                    catch (ArgumentException e)
                    {
                        logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "BuildJoinResultXML method failed, ArgumentException Error:{0}, \nStackTrace: {1}", e.Message, e.StackTrace);
                    }
                    catch(XmlException e)
                    {
                        logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "BuildJoinResultXML method failed, XmlException Error:{0}, \nStackTrace: {1}", e.Message, e.StackTrace);
                    }
                }
            }
            else
            {
                if ((null == participantArray || 0 == participantArray.Length) || (null == policyResultArray || 0 == policyResultArray.Length))
                {
                    // The participant and policy result both null. This is not an error, the builder no need this info.
                }
                else
                {
                    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "The participants and policy results lengths are not equal");
                }
            }

            policyResultsElement.AppendChild(resultCodeElement);
            policyResultsElement.AppendChild(queryIdentifyElement);
            policyResultsElement.AppendChild(joinResultsElement);
            xmlDoc.AppendChild(policyResultsElement);

            strXMLResult = XmlVariable.ConvertXmlDoc2String(xmlDoc);

            return strXMLResult;
        }

        public static string BuildJoinResultXML(string resultCode, string strQueryIdentify, ICollection<string> participantArray, PolicyResult[] policyResultArray)
        {
            string strXMLResult = "";
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement policyResultsElement = XmlVariable.CreateElement(xmlDoc, POLICYRESULTS_NODENAME, null, null);
            XmlElement resultCodeElement = XmlVariable.CreateElement(xmlDoc, RESULTCODE_NODENAME, null, resultCode);
            XmlElement queryIdentifyElement = XmlVariable.CreateElement(xmlDoc, QueryIdentify_NODENAME, null, strQueryIdentify);
            XmlElement joinResultsElement = XmlVariable.CreateElement(xmlDoc, JOINRESULTS_NODENAME, null, null);

            if (participantArray != null && policyResultArray != null && participantArray.Count == policyResultArray.Length)
            {
                int i = 0;
                foreach (string strParticipantItem in participantArray)
                {
                    try
                    {
                        XmlElement resultElement = XmlVariable.CreateElement(xmlDoc, RESULT_NODENAME, null, null);
                        resultElement.SetAttribute(ENFORCEMENT_ATTRNAME, policyResultArray[i].Enforcement.ToString());
                        resultElement.SetAttribute(PARTICIPANT_ATTRNAME, strParticipantItem);
                        joinResultsElement.AppendChild(resultElement);
                    }
                    catch (ArgumentException e)
                    {
                        logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "BuildJoinResultXML method failed, ArgumentException Error:{0}, \nStackTrace: {1}", e.Message, e.StackTrace);
                    }
                    catch (XmlException e)
                    {
                        logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "BuildJoinResultXML method failed, XmlException Error:{0}, \nStackTrace: {1}", e.Message, e.StackTrace);
                    }
                    ++i;
                }
            }
            else
            {
                if ((null == participantArray || 0 == participantArray.Count) || (null == policyResultArray || 0 == policyResultArray.Length))
                {
                    // The participant and policy result both null. This is not an error, the builder no need this info.
                }
                else
                {
                    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "The participants and policy results lengths are not equal");
                }
            }

            policyResultsElement.AppendChild(resultCodeElement);
            policyResultsElement.AppendChild(queryIdentifyElement);
            policyResultsElement.AppendChild(joinResultsElement);
            xmlDoc.AppendChild(policyResultsElement);

            strXMLResult = XmlVariable.ConvertXmlDoc2String(xmlDoc);

            return strXMLResult;
        }

        #endregion
    }
}