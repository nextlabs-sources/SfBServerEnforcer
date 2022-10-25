using SFBCommon.NLLog;
using System;
using System.Collections.Generic;
using System.Web;
using SFBCommon.SFBObjectInfo;
using Nextlabs.SFBServerEnforcer.PolicyHelper;
using NLAssistantWebService.Policies;
using NLAssistantWebService.Models;
using System.Threading;
using System.Xml;
using System.IO;
using SFBCommon.ClassifyHelper;
using SFBCommon.Common;

namespace NLAssistantWebService.CustomHandlers
{
    public class MeetingParticipantRecheckHandler : IHttpHandler
    {

        #region Members
        private static readonly CLog logger = CLog.GetLogger(typeof(MeetingParticipantRecheckHandler));

        private SFBMeetingInfo meetingObj = null;
        private SFBMeetingVariableInfo meetingVariableObj = null;
        private NLMeetingInfo nlMeetingObj = null;

        public bool IsReusable
        {
            get { return false; }
        }

        #endregion

        #region Implementation

        public void ProcessRequest(HttpContext context)
        {
            HttpRequest request = null;
            HttpResponse response = null;
            TransModel requestModel = null;
            MeetingPolicy policy = null;

            string strRequestXml = "";
            string strResponseXml = "";
            string strObligationXml = "";
            string strCreator = "";
            string[] szStrParticipants = null;

            Dictionary<string, string> oldTagDict = null;
            List<string> obligationTagNameList = null;
            PolicyResult[] policyResultArray = null;

            EMSFB_QueryResult emResult = EMSFB_QueryResult.Unknown;

            if(context == null)
            {
                return;
            }

            if(context.Request != null && context.Response != null)
            {
                request = context.Request;
                response = context.Response;

                strRequestXml = GetRequestXmlString(request);
                requestModel = ParseRequestXml(strRequestXml);

                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "request xml: {0}", strRequestXml);

                if (requestModel != null)
                {
                    FillMeetingModel(requestModel);
                    strObligationXml = GetObligationXmlString(requestModel);
                    obligationTagNameList = GetObligationTagNameList(strObligationXml);
                    oldTagDict = GetOldTagDictionary(requestModel);
                    strCreator = GetMeetingCreator();
                    szStrParticipants = GetParticaipantsArray();

                    if(oldTagDict != null)
                    {
                        UpdateTagsInMemory(oldTagDict, obligationTagNameList, requestModel);

                        policy = MeetingPolicy.GetInstance();

                        emResult = policy.QueryMeetingMultiForParticipantsRecheck(EMSFB_ACTION.emMeetingJoin, szStrParticipants, strCreator, meetingObj, meetingVariableObj, nlMeetingObj, out policyResultArray);
                        strResponseXml = BuildResponseXML(emResult, szStrParticipants, policyResultArray);
                    }
                    else
                    {
                        logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "oldTagDict is null");
                    }
                }
                else
                {
                    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "request model is null");
                }


                try
                {
                    response.Clear();
                    response.ContentType = "text/xml";
                    response.Write(strResponseXml);
                }
                catch(HttpException e)
                {
                    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Response Content-Type Error: {0}, \nStackTrace: \n{1}", e.Message, e.StackTrace);
                }
            }
            else
            {
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "ProcessRequest method failed, request or response is null");
            }

        }

        #endregion

        #region Inner Tools

        private string BuildResponseXML(EMSFB_QueryResult emResult, string[] participantArray, PolicyResult[] policyResultArray)
        {
            string strXml = "";
            string strOperationResultCode = XmlVariable.OPERATION_FAILED;

            if (emResult == EMSFB_QueryResult.Success)
            {
                strOperationResultCode = XmlVariable.OPERATION_SUCCEED;
            }

            strXml = PolicyResultXmlBuilder.BuildJoinResultXML(strOperationResultCode, null, participantArray, policyResultArray);

            return strXml;
        }

        private string GetRequestXmlString(HttpRequest request)
        {
            string strRequestXml = "";

            using (Stream requestStream = request.InputStream)
            {
                if (requestStream != null)
                {
                    using (StreamReader sr = new StreamReader(requestStream))
                    {
                        strRequestXml = sr.ReadToEnd();
                    }
                }
            }

            return strRequestXml;
        }

        private TransModel ParseRequestXml(string strXml)
        {
            XmlDocument xmlDoc = null;
            XmlNode rootNode = null;
            TransModel model = null;

            if (!string.IsNullOrEmpty(strXml))
            {
                xmlDoc = new XmlDocument();

                try
                {
                    xmlDoc.LoadXml(strXml);
                    rootNode = xmlDoc.SelectSingleNode(XmlVariable.meetingCommandNodeName);
                }
                catch (XmlException e)
                {
                    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "ParseRequestXml failed, Xml: {0}, \nError: {1}, \nStackTrace: {2}", strXml, e.Message, e.StackTrace);
                }
                catch (System.Xml.XPath.XPathException e)
                {
                    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "ParseRequestXml failed, \nError: {1}, \nStackTrace: {2}", e.Message, e.StackTrace);
                }

                model = MeetingXmlParser.ParseRequestXml(rootNode);
            }
            else
            {
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "ParseRequestXml failed, request xml: {0}", strXml);
            }

            return model;
        }

        private void FillMeetingModel(TransModel model)
        {
            meetingObj = new SFBMeetingInfo();
            meetingVariableObj = new SFBMeetingVariableInfo();
            nlMeetingObj = new NLMeetingInfo();

            string strUri = model.Meetings[0].MeetingInfoAttributes[XmlVariable.uriFieldName];

            if (!meetingObj.EstablishObjFormPersistentInfo(SFBMeetingInfo.kstrUriFieldName, strUri))
            {
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "SFBMeetingInfo EstablishObjFormPersistentInfo failed");
            }

            if (!meetingVariableObj.EstablishObjFormPersistentInfo(SFBMeetingVariableInfo.kstrUriFieldName, strUri))
            {
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "SFBMeetingVariableInfo EstablishObjFormPersistentInfo failed");
            }

            if(!nlMeetingObj.EstablishObjFormPersistentInfo(NLMeetingInfo.kstrUriFieldName, strUri))
            {
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "NLMeetingInfo EstablishObjFormPersistentInfo failed");
            }
        }

        private string GetObligationXmlString(TransModel model)
        {
            string strXml = "";
            string strUri = model.Meetings[0].MeetingInfoAttributes[XmlVariable.uriFieldName];
            
            if(nlMeetingObj != null)
            {
                strXml = nlMeetingObj.GetItemValue(NLMeetingInfo.kstrManulClassifyObsFieldName);
            }

            return strXml;
        }

        private List<string> GetObligationTagNameList(string strObligationXml)
        {
            List<string> tagNameList = new List<string>();
            ManulClassifyObligationHelper manualObHelper = new ManulClassifyObligationHelper(strObligationXml, false);

            tagNameList = manualObHelper.GetTagNameListFromSFBObligationXml();

            return tagNameList;
        }

        private Dictionary<string, string> GetOldTagDictionary(TransModel model)
        {
            Dictionary<string, string> tagDict = null;

            if(meetingVariableObj != null)
            {
                tagDict = meetingVariableObj.GetDictionaryTags();
            }

            return tagDict;
        }

        private void UpdateTagsInMemory(Dictionary<string, string> oldTagDict, List<string> tagNameList, TransModel model)
        {
            string strTagXml = "";
            ClassifyTagsHelper tagHelper = null;
            Dictionary<string, string> newTagDict = model.Meetings[0].Tags as Dictionary<string, string>;

            foreach (KeyValuePair<string, string> tag in oldTagDict)
            {
                if (!tagNameList.Contains(tag.Key))
                {
                    CommonHelper.AddKeyValuesToDir(newTagDict, tag.Key, tag.Value);
                }
            }

            meetingVariableObj.SetItem(SFBMeetingVariableInfo.kstrClassifyTagsFieldName, "");
            tagHelper = new ClassifyTagsHelper(newTagDict);
            strTagXml = tagHelper.GetClassifyXml();
            meetingVariableObj.SetItem(SFBMeetingVariableInfo.kstrClassifyTagsFieldName, strTagXml);
        }

        private string GetMeetingCreator()
        {
            string strCreator = "";

            strCreator = meetingObj.GetItemValue(SFBMeetingInfo.kstrCreatorFieldName);

            return strCreator;
        }

        private string[] GetParticaipantsArray()
        {
            string[] szStrParticipants = null;
            List<string> lsStrParticipants = SFBParticipantManager.GetDistinctParticipantsAsList(meetingVariableObj);
            if (null != lsStrParticipants)
            {
                szStrParticipants = lsStrParticipants.ToArray();
            }
            return szStrParticipants;
        }

        #endregion
    }
}
