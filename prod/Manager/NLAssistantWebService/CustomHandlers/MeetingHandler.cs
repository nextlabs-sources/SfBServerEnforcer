using NLAssistantWebService.Models;
using System;
using System.IO;
using System.Web;
using System.Xml;
using SFBCommon.SFBObjectInfo;
using SFBCommon.CommandHelper;
using System.Collections.Generic;
using NLAssistantWebService.Tokens;
using NLAssistantWebService.Common;
using SFBCommon.ClassifyHelper;
using SFBCommon.Common;
using SFBCommon.NLLog;

namespace NLAssistantWebService.CustomHandlers
{
    public class MeetingHandler : IHttpHandler
    {
        #region IHttpHandler Members
        private SFBMeetingVariableInfo sfbVarMeetingInfo;
        private SFBMeetingInfo sfbMeetingInfo;
        private NLMeetingInfo nlMeetingInfo ;
        private static TokenManager tokenMgr = new TokenManager();

        private string m_strSipUri = "";
        private string m_strTokenId = "";
        private string m_strOpType = "";
        #endregion

        #region Constructors
        public MeetingHandler()
        {
            sfbMeetingInfo = new SFBMeetingInfo();
            sfbVarMeetingInfo = new SFBMeetingVariableInfo();
            nlMeetingInfo = new NLMeetingInfo();
        }
        #endregion

        #region Request Process
        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
            ParseRequestBody(request.InputStream);
        }

        private void ParseRequestBody(Stream stream)
        {
            string request = "";
            string responseXmlStr = "";

            using(StreamReader sr = new StreamReader(stream))
            {
                request = sr.ReadToEnd();
            }

            if(!string.IsNullOrEmpty(request))
            {
                TransModel model ;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(request);

                XmlNode cmdNode = xmlDoc.SelectSingleNode(XmlVariable.meetingCommandNodeName) ;
                model = MeetingXmlParser.ParseRequestXml(cmdNode) ;

                InitMeetingCmdAttributes(model);

                if (tokenMgr.CheckUserToken(m_strSipUri, m_strTokenId, false))
                {
                    switch (m_strOpType)
                    {
                        case XmlVariable.SEARCH:
                            {
                                SearchHandler(model);
                                break;
                            }
                        case XmlVariable.SAVE:
                            {
                                SaveHandler(model);
                                break;
                            }
                        case XmlVariable.SHOW:
                            {
                                ShowHandler(model);
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }

                    responseXmlStr = MeetingXmlBuilder.BuildResponseXml(model);

                }
                else
                {
                    XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n CheckUserToken() failed , token invalid \n");
                    model.ResultCode = XmlVariable.OPERATION_TOKENINVALID;
                    ClearModel(model);
                    responseXmlStr = MeetingXmlBuilder.BuildResponseXml(model);
                }

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ContentType = "text/xml";
                HttpContext.Current.Response.Write(responseXmlStr);
            }
            else
            {
                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, " \n parse request error : request null \n");
            }
        }

        #region Requet Handlers

        private void SearchHandler(TransModel model)
        {
            if(model != null)
            {
                if (model.Meetings.Count == 1)
                {
                    string meetingUri = model.Meetings[0].MeetingInfoAttributes[XmlVariable.uriFieldName];
                    string entryInfo = model.Meetings[0].MeetingInfoAttributes[XmlVariable.entryInfoFieldName];

                    bool bValid = false;
                    XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n SearchHandler(), meetingUri:[0], entryInfo:[{1}]\n", meetingUri, entryInfo); // For classify, both this two item are empty
                    if (string.IsNullOrEmpty(meetingUri))
                    {
                        SFBMeetingInfo obSFBMeetingInfo = CommonTools.GetSFBMeetingObjectByEntryInfo(entryInfo);
                        if (null != obSFBMeetingInfo)
                        {
                            meetingUri = obSFBMeetingInfo.GetItemValue(SFBMeetingInfo.kstrUriFieldName);
                            bValid = (!String.IsNullOrEmpty(meetingUri));
                        }
                    }
                    else
                    {
                        List<SFBObjectInfo> lsSFBObjects = SFBObjectInfo.GetObjsFrommPersistentInfo(EMSFB_INFOTYPE.emInfoSFBMeeting, SFBMeetingInfo.kstrUriFieldName, meetingUri);
                        bValid = (1 == lsSFBObjects.Count);
                    }

                    model.Meetings[0].MeetingInfoAttributes[XmlVariable.uriFieldName] = meetingUri;

                    if (bValid)
                    {
                        STUSFB_CLASSIFYCMDINFO classifyCmdInfo = new STUSFB_CLASSIFYCMDINFO(EMSFB_COMMAND.emCommandClassifyMeeting, m_strSipUri, meetingUri);
                        bool isClassifyManager = ClassifyCommandHelper.IsClassifyManager(classifyCmdInfo);

                        FillMeetingInfoAttributes(model.Meetings[0]);

                        if (isClassifyManager)
                        {
                            FillMeetingInfoTags(model.Meetings[0]);
                            model.ResultCode = XmlVariable.OPERATION_SUCCEED;
                            model.Meetings[0].Classification = GetSFBObXml(model.Meetings[0]);
                        }
                        else
                        {
                            XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n not authorized to classify tags \n");
                            model.ResultCode = XmlVariable.OPERATION_UNAUTHORIZED;
                            ClearModel(model);
                        }
                    }
                    else
                    {
                        XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n meeting not exists \n");
                        model.ResultCode = XmlVariable.OPERATION_FAILED;
                        ClearModel(model);
                    }
                }
                else
                {
                    XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n SearchHandler() model.Meetings.Count != 1 \n ");
                    model.ResultCode = XmlVariable.OPERATION_FAILED;
                    ClearModel(model);
                }

                if (tokenMgr.RefreshUserToken(m_strSipUri, m_strTokenId))
                {
                    XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n refresh token succeed \n");
                }
                else
                {
                    XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n refresh token failed \n");
                }

            }
            else
            {
                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "\n SearchHandler() failed , model is null \n");
            }
        }

        private void SaveHandler(TransModel model)
        {
            if(model != null)
            {
                if (model.Meetings.Count == 1)
                {
                    string meetingUri = model.Meetings[0].MeetingInfoAttributes[XmlVariable.uriFieldName];
                    string entryInfo = model.Meetings[0].MeetingInfoAttributes[XmlVariable.entryInfoFieldName];

                    if (string.IsNullOrEmpty(meetingUri))
                    {
                        meetingUri = CommonTools.GetMeetingSipUriStrByEntryInfo(entryInfo);
                    }

                    model.Meetings[0].MeetingInfoAttributes[XmlVariable.uriFieldName] = meetingUri;

                    STUSFB_CLASSIFYCMDINFO classifyCmdInfo = new STUSFB_CLASSIFYCMDINFO(EMSFB_COMMAND.emCommandClassifyMeeting, m_strSipUri, meetingUri);
                    bool isClassifyManager = ClassifyCommandHelper.IsClassifyManager(classifyCmdInfo);

                    if(isClassifyManager)
                    {
                        SaveTags(model.Meetings[0]);

                        if (sfbVarMeetingInfo.PersistantSave())
                        {
                            model.ResultCode = XmlVariable.OPERATION_SUCCEED;
                            ClearModel(model);//clear classification value & tags value 
                        }
                        else
                        {
                            XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "\n sfbVarMeetingInfo PersistantSave() failed \n");
                            model.ResultCode = XmlVariable.OPERATION_FAILED;
                            ClearModel(model);//clear classification value & tags value 
                        }
                    }
                    else
                    {
                        XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n not authorized to save tags \n");
                        model.ResultCode = XmlVariable.OPERATION_UNAUTHORIZED;
                        ClearModel(model);
                    }
                }
                else
                {
                    XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n SaveHandler() model.Meetings.Count != 1 \n ");
                    model.ResultCode = XmlVariable.OPERATION_FAILED;
                    ClearModel(model);
                }

                if (tokenMgr.RefreshUserToken(m_strSipUri, m_strTokenId))
                {
                    XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n refresh token succeed \n");
                }
                else
                {
                    XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n refresh token failed \n");
                }
            }
            else
            {
                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "\n SaveHandler() failed , model is null \n");
            }
        }

        private void ShowHandler(TransModel model)
        {
            if (model != null)
            {
                if (model.Meetings.Count == 1)
                {
                    string meetingUri = model.Meetings[0].MeetingInfoAttributes[XmlVariable.uriFieldName];
                    string entryInfo = model.Meetings[0].MeetingInfoAttributes[XmlVariable.entryInfoFieldName];
                    string creator = CommonTools.GetCreatorFromSipUri(m_strSipUri);
                    string timeFilter = model.FilterAttributes[XmlVariable.intervalFilterFieldName];
                    string hideFilter = model.FilterAttributes[XmlVariable.showMeetingsFieldName];

                    if (string.IsNullOrEmpty(meetingUri))
                    {
                        meetingUri = CommonTools.GetMeetingSipUriStrByEntryInfo(entryInfo);
                    }

                    model.Meetings[0].MeetingInfoAttributes[XmlVariable.uriFieldName] = meetingUri;

                    List<SFBObjectInfo> filteredMeetings = new List<SFBObjectInfo>();

                    DateTime utcNow = DateTime.UtcNow;
                    utcNow = utcNow.Add(XmlVariable.GetFilterTimeSpan(timeFilter));

                    model.Meetings.Clear();

                    List<SFBObjectInfo> myMeetings = SFBObjectInfo.GetObjsFrommPersistentInfo(SFBCommon.Common.EMSFB_INFOTYPE.emInfoSFBMeeting, SFBMeetingInfo.kstrCreatorFieldName, creator);

                    foreach (var meeting in myMeetings)
                    {
                        string createTime = meeting.GetItemValue(SFBMeetingInfo.kstrCreateTimeFieldName);
                        if (!XmlVariable.IsExpired(createTime, utcNow))
                        {
                            if (hideFilter.ToLower() == XmlVariable.allValueName)
                            {
                                filteredMeetings.Add(meeting);
                            }
                            else if (hideFilter.ToLower() == XmlVariable.undoneValueName)
                            {
                                if (sfbVarMeetingInfo.EstablishObjFormPersistentInfo(SFBMeetingVariableInfo.kstrUriFieldName, meeting.GetItemValue(SFBMeetingInfo.kstrUriFieldName)))
                                {
                                    if (string.IsNullOrEmpty(sfbVarMeetingInfo.GetItemValue(SFBMeetingVariableInfo.kstrClassifyTagsFieldName)))
                                    {
                                        filteredMeetings.Add(meeting);
                                    }
                                }
                            }
                            else if (hideFilter.ToLower() == XmlVariable.doneValueName)
                            {
                                if (sfbVarMeetingInfo.EstablishObjFormPersistentInfo(SFBMeetingVariableInfo.kstrUriFieldName, meeting.GetItemValue(SFBMeetingInfo.kstrUriFieldName)))
                                {
                                    if (!string.IsNullOrEmpty(sfbVarMeetingInfo.GetItemValue(SFBMeetingVariableInfo.kstrClassifyTagsFieldName)))
                                    {
                                        filteredMeetings.Add(meeting);
                                    }
                                }
                            }
                            else
                            {
                                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n hide filter un-matched , hideFilter : {0} \n", hideFilter);
                            }
                        }
                    }

                    foreach (SFBObjectInfo meeting in filteredMeetings)
                    {
                        MeetingModel m = new MeetingModel();
                        string uri = meeting.GetItemValue(SFBMeetingInfo.kstrUriFieldName);
                        m.MeetingInfoAttributes[XmlVariable.uriFieldName] = uri;
                        m.MeetingInfoAttributes[XmlVariable.creatorFieldName] = meeting.GetItemValue(SFBMeetingInfo.kstrCreatorFieldName);
                        m.MeetingInfoAttributes[XmlVariable.createTimeFieldName] = meeting.GetItemValue(SFBMeetingInfo.kstrCreateTimeFieldName);
                        m.Classification = GetSFBObXml(m);

                        if (sfbVarMeetingInfo.EstablishObjFormPersistentInfo(SFBMeetingVariableInfo.kstrUriFieldName, uri))
                        {
                            m.Tags = sfbVarMeetingInfo.GetDictionaryTags();
                        }
                        else
                        {
                            XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "\n ShowHandler()  sfbVarMeetingInfo.EstablishObjFormPersistentInfo() failed , uri : {0} \n",uri);
                        }
                        model.Meetings.Add(m);
                    }

                    model.ResultCode = XmlVariable.OPERATION_SUCCEED;
                }
                else
                {
                    XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n ShowHandler() model.Meetings.Count != 1 \n ");
                    model.ResultCode = XmlVariable.OPERATION_FAILED;
                    ClearModel(model);
                }
            }
            else
            {
                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "\n ShowHandler() failed , model is null \n");
            }
        }
        #endregion

        #region Inner tools
        private void ClearModel(TransModel model)
        {
            if (model != null)
            {
                foreach (var meeting in model.Meetings)
                {
                    meeting.Classification = "";
                }

                foreach (MeetingModel m in model.Meetings)
                {
                    m.Tags.Clear();
                }
            }
            else
            {
                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n ClearModel() failed , model is null \n");
            }
        }

        private void FillMeetingInfoAttributes(MeetingModel model)
        {
            if (model != null)
            {
                if (sfbMeetingInfo.EstablishObjFormPersistentInfo(SFBMeetingInfo.kstrUriFieldName, model.MeetingInfoAttributes[XmlVariable.uriFieldName]))
                {
                    model.MeetingInfoAttributes[XmlVariable.creatorFieldName] = sfbMeetingInfo.GetItemValue(SFBMeetingInfo.kstrCreatorFieldName);
                    model.MeetingInfoAttributes[XmlVariable.createTimeFieldName] = sfbMeetingInfo.GetItemValue(SFBMeetingInfo.kstrCreateTimeFieldName);
                    model.MeetingInfoAttributes[XmlVariable.entryInfoFieldName] = sfbMeetingInfo.GetItemValue(SFBMeetingInfo.kstrEntryInformationFieldName);
                }
                else
                {
                    XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "\n sfbMeetingInfo EstablishObjFormPersistentInfo() failed \n");
                }
            }
            else
            {
                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n FillMeetingInfoAttributes() failed , model is null \n");
            }
        }

        private void FillMeetingInfoTags(MeetingModel model)
        {
            if (model != null)
            {
                if (sfbVarMeetingInfo.EstablishObjFormPersistentInfo(SFBMeetingVariableInfo.kstrUriFieldName, model.MeetingInfoAttributes[XmlVariable.uriFieldName]))
                {
                    model.Tags = sfbVarMeetingInfo.GetDictionaryTags();
                }
                else
                {
                    XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "\n sfbVarMeetingInfo EstablishObjFormPersistentInfo() failed \n");
                }
            }
            else
            {
                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n FillMeetingInfoTags() failed , model is null");
            }
        }

        private string GetSFBObXml(MeetingModel model)
        {
            string obXmlStr = "";

            if (model != null)
            {
                if (nlMeetingInfo.EstablishObjFormPersistentInfo(NLMeetingInfo.kstrUriFieldName, model.MeetingInfoAttributes[XmlVariable.uriFieldName]))
                {
                    ManulClassifyObligationHelper manulObHelper = nlMeetingInfo.GetManulClassifyObligation();
                    if (manulObHelper != null)
                    {
                        obXmlStr = manulObHelper.GetClassifyXml();
                    }
                    else
                    {
                        XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n get obligation xml failed , ManulClassifyObligationHelper is null \n");
                    }
                }
                else
                {
                    XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "\n nlMeetingInfo EstablishObjFormPersistentInfo() failed \n");
                }
            }
            else
            {
                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n GetSFBObXml() failed , model is null \n");
            }
            
            return obXmlStr ;
        }

        private void InitMeetingCmdAttributes(TransModel model)
        {
            if (model != null)
            {
                m_strOpType = model.CommandAttriutes[XmlVariable.operationFieldName];
                m_strSipUri = model.CommandAttriutes[XmlVariable.sipUriFieldName];
                m_strTokenId = model.CommandAttriutes[XmlVariable.idFieldName];
            }
            else
            {
                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\n FillMeetingCmdAttributes() failed , model is null \n");
            }
        }

        private void SaveTags(MeetingModel model)
        {
            Dictionary<string, string> tags = (Dictionary<string, string>)model.Tags;

            if (sfbVarMeetingInfo.EstablishObjFormPersistentInfo(SFBMeetingVariableInfo.kstrUriFieldName, model.MeetingInfoAttributes[XmlVariable.uriFieldName]))
            {
                Dictionary<string, string> oldTagDict;
                List<string> obTagNameList;
                string obXml = GetSFBObXml(model);
                oldTagDict = sfbVarMeetingInfo.GetDictionaryTags();//get total tags of the meeting
                ManulClassifyObligationHelper manualObHelper = new ManulClassifyObligationHelper(obXml, false);
                obTagNameList = manualObHelper.GetTagNameListFromSFBObligationXml();

                foreach (string obTagName in obTagNameList)
                {
                    if (oldTagDict.ContainsKey(obTagName))
                    {
                        CommonHelper.RemoveKeyValuesFromDir(oldTagDict, obTagName);
                    }
                }

                foreach (KeyValuePair<string, string> newTag in tags)
                {
                    CommonHelper.AddKeyValuesToDir(oldTagDict, newTag.Key, newTag.Value);
                }

                sfbVarMeetingInfo.SetItem(SFBMeetingVariableInfo.kstrClassifyTagsFieldName, "");//clear all tags of the meeting

                sfbVarMeetingInfo.AddedNewTags(oldTagDict);

                sfbVarMeetingInfo.SetItem(SFBMeetingVariableInfo.kstrDoneManulClassifyFieldName, SFBMeetingVariableInfo.kstrDoneManulClassifyYes);
            }
            else
            {
                XmlVariable.HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "\n SaveTags() EstablishObjFormPersistentInfo() failed \n");
                sfbVarMeetingInfo.SetItem(SFBMeetingVariableInfo.kstrDoneManulClassifyFieldName, SFBMeetingVariableInfo.kstrDoneManulClassifyNo);
            }
        }

        #endregion

        #endregion
    }
}
