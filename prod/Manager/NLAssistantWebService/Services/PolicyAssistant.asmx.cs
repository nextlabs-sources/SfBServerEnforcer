using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Nextlabs.SFBServerEnforcer.PolicyHelper;
using NLAssistantWebService.Models;
using NLAssistantWebService.Policies;
using NLAssistantWebService.Common;
using SFBCommon.SFBObjectInfo;
using SFBCommon.NLLog;
using SFBCommon.Common;
using System.Threading.Tasks;
using NLAssistantWebService.Tokens;
using System.Data;
using System.Diagnostics;

namespace NLAssistantWebService.Services
{
    // For WMQueryPolicyForMeeting web method
    class STUWMQueryPolicyForMeetingRequestParameters
    {
        #region Logger
        private static readonly CLog logger = CLog.GetLogger(typeof(STUWMQueryPolicyForMeetingRequestParameters));
        #endregion

        public readonly string strQueryRequestIdentify = QueryRequestIdentifyManager.GetInstance().GetNewQueryRequestIdentify();
        public EMSFB_ACTION emAction = EMSFB_ACTION.emUnknwon;
        public string strMeetingIdentify = "";
        public string strSender = "";
        public HashSet<string> setRecipients = null;
        public bool bNeedDoObligations = false;

        public STUWMQueryPolicyForMeetingRequestParameters(EMSFB_ACTION emActionIn, string strMeetingIdentifyIn, string strSenderIn, HashSet<string> setRecipientsIn, bool bNeedDoObligationsIn)
        {
            Init(emActionIn, strMeetingIdentifyIn, strSenderIn, setRecipientsIn, bNeedDoObligationsIn);
        }
        public STUWMQueryPolicyForMeetingRequestParameters(EMSFB_ACTION emActionIn, string strMeetingIdentifyIn, string strSenderIn, string strRecipientsIn, string strSepRecipientIn, bool bIgnoreCaseIn, bool bNeedDoObligationsIn)
        {
            HashSet<string> setRecipientsTemp = null;
            string[] szRecipients = CommonHelper.ConvertStringToArray(strRecipientsIn, strSepRecipientIn, bIgnoreCaseIn, StringSplitOptions.RemoveEmptyEntries);
            if ((null != szRecipients) && (0 < szRecipients.Length))
            {
                if (bIgnoreCaseIn)
                {
                    setRecipientsTemp = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    setRecipientsTemp = new HashSet<string>();
                }
                CommonHelper.ConvertArrayToCollection<HashSet<string>, string>(szRecipients, ref setRecipientsTemp);
            }
            Init(emActionIn, strMeetingIdentifyIn, strSenderIn, setRecipientsTemp, bNeedDoObligationsIn);
        }
        public bool IsValidParameters()
        {
            bool bValid = true;
            if ((!CommonTools.IsSFBMeetingAction(emAction)) || String.IsNullOrEmpty(strSender) || String.IsNullOrEmpty(strMeetingIdentify))
            {
                // This web method only support meeting action
                // Meeting identify(entry url or sip uri) and sender user must not be empty
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "The action{0} must be meeting actions. The meeting identify:[{1}] and sender:[{2}] must be not empty\n", emAction, strMeetingIdentify, strSender);
                bValid = false;
            }
            else
            {
                // In meeting invite and join action, the recipients must be not empty
                if ((EMSFB_ACTION.emMeetingInvite == emAction) || (EMSFB_ACTION.emMeetingJoin == emAction))
                {
                    if ((null == setRecipients) || (0 >= setRecipients.Count))
                    {
                        logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "In action{0}(Invite or join) the recepients:[{1}] must no be empty\n", emAction, ((null == setRecipients) ? 0 : setRecipients.Count));
                        bValid = false;
                    }
                }
            }
            return bValid;
        }
        public int GetRecipientsCount()
        {
            if (null != setRecipients)
            {
                return setRecipients.Count;
            }
            return 0;
        }
        public string GetRecipientsString(char chSepRecipient)
        {
            string strRecipientRet = "";
            if (null != setRecipients)
            {
                strRecipientRet = string.Join(chSepRecipient.ToString(), setRecipients);
            }
            return "";
        }

        private void Init(EMSFB_ACTION emActionIn, string strMeetingIdentifyIn, string strSenderIn, HashSet<string> setRecipientsIn, bool bNeedDoObligationsIn)
        {
            emAction = emActionIn;
            strMeetingIdentify = strMeetingIdentifyIn;
            strSender = strSenderIn;
            setRecipients = setRecipientsIn;
            bNeedDoObligations = bNeedDoObligationsIn;
        }
    }

    /// <summary>
    /// Summary description for PolicyAssistant
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class PolicyAssistant : System.Web.Services.WebService
    {
        #region Logger
        private static readonly CLog logger = CLog.GetLogger(typeof(PolicyAssistant));
        #endregion     

        [WebMethod]
        public string WMQueryPolicyForMeetingInviteEx(string strMeetingIdentify, string strInviter, string strInvitees, string strSepInvitee, string strNeedDoObligations)
        {
            logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Query policy for meeting invite parmeters ex, MeetingIdentify:[{0}], Inviter:[{1}], Invitees:[{2}], SepInvitee:[{3}], NeedDoObligation:[{4}]\n",
                strMeetingIdentify, strInviter, strInvitees, strSepInvitee, strNeedDoObligations);

            bool bNeedDoObligations = CommonHelper.ConvertStringToBoolean(strNeedDoObligations, false);
            STUWMQueryPolicyForMeetingRequestParameters stuWMQueryPolicyForMeetingRequestParameters = new STUWMQueryPolicyForMeetingRequestParameters(EMSFB_ACTION.emMeetingInvite, strMeetingIdentify, strInviter, strInvitees, strSepInvitee, true, bNeedDoObligations);
            return InnerWMQueryPolicyForMeetingInvite(stuWMQueryPolicyForMeetingRequestParameters);
        }
        [WebMethod]
        public string WMQueryPolicyForMeetingInvite(string strMeetingIdentify, string strInviter, HashSet<string> setInvitees, bool bNeedDoObligations)
        {
            logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Query policy for meeting invite parmeters, MeetingIdentify:[{0}], Inviter:[{1}], Invitees:[{2}], NeedDoObligation:[{3}]\n",
              strMeetingIdentify, strInviter, setInvitees, bNeedDoObligations);

            STUWMQueryPolicyForMeetingRequestParameters stuWMQueryPolicyForMeetingRequestParameters = new STUWMQueryPolicyForMeetingRequestParameters(EMSFB_ACTION.emMeetingInvite, strMeetingIdentify, strInviter, setInvitees, bNeedDoObligations);
            return InnerWMQueryPolicyForMeetingInvite(stuWMQueryPolicyForMeetingRequestParameters);
        }
        [WebMethod]
        public string WMGetQueryPolicyForMeetingResult(string strQueryIdentify)
        {
            QueryRequestIdentifyManager obQueryRequestIdentifyManagerIn = QueryRequestIdentifyManager.GetInstance();
            return obQueryRequestIdentifyManagerIn.GetQueryResponseInfoByRequestIdentify(strQueryIdentify, true);
        }

        [WebMethod]
        // For web method convert. https://www.cnblogs.com/zifeiniu/p/12069224.html       
        public DataTable AddTable()
        {
            return null;
        }

        #region Tasks
        private string InnerWMQueryPolicyForMeetingInvite(STUWMQueryPolicyForMeetingRequestParameters stuWMQueryPolicyForMeetingRequestParameters)
        {
            string strResponseInfo = "";
            if (null == stuWMQueryPolicyForMeetingRequestParameters)
            {
                // Parameters error
                strResponseInfo = PolicyResultXmlBuilder.BuildJoinResultXML(XmlVariable.OPERATION_FAILED, null, null, null);

                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "PID:[{0}], The query policy for meeting invit task failed. the pass in parameters structure is null, inner code error, please check.\n", Process.GetCurrentProcess().Id.ToString());
            }
            else if (!(stuWMQueryPolicyForMeetingRequestParameters.IsValidParameters()))
            {
                // Parameters error
                strResponseInfo = PolicyResultXmlBuilder.BuildJoinResultXML(XmlVariable.OPERATION_FAILED, null, null, null);

                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "PID:[{0}], The query policy for meeting invit task failed. the pass in parameters is invalid, please check. MeetingIdentify:[{1}], Sender:[{2}], RecipientsCount:[{3}], NeedDoObligation:{4}, RequestIdentity:[{5}]\n",
                    Process.GetCurrentProcess().Id.ToString(), stuWMQueryPolicyForMeetingRequestParameters.strMeetingIdentify, stuWMQueryPolicyForMeetingRequestParameters.strSender,
                    stuWMQueryPolicyForMeetingRequestParameters.GetRecipientsCount(), stuWMQueryPolicyForMeetingRequestParameters.bNeedDoObligations, stuWMQueryPolicyForMeetingRequestParameters.strQueryRequestIdentify);
            }
            else
            {
                // Set processing status
                QueryRequestIdentifyManager obQueryRequestIdentifyManagerIns = QueryRequestIdentifyManager.GetInstance();
                strResponseInfo = PolicyResultXmlBuilder.BuildJoinResultXML(XmlVariable.OPERATION_PROCESSING, stuWMQueryPolicyForMeetingRequestParameters.strQueryRequestIdentify, null, null);
                obQueryRequestIdentifyManagerIns.SaveQueryResponseInfoByRequestIdentify(stuWMQueryPolicyForMeetingRequestParameters.strQueryRequestIdentify, false, strResponseInfo);

                // Asynchronous invoke, maybe the query task need a lot of time with multiple recipients
                Task taskQueryPolicy = new Task(TaskForQueryPolicyForMeeting, stuWMQueryPolicyForMeetingRequestParameters);
                taskQueryPolicy.Start();

                // Wait 500 ms and then check if task success finished
                taskQueryPolicy.Wait(500);

                strResponseInfo = obQueryRequestIdentifyManagerIns.GetQueryResponseInfoByRequestIdentify(stuWMQueryPolicyForMeetingRequestParameters.strQueryRequestIdentify, true);

                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "PID:[{0}], The query policy for meeting invite task complete:[{1}] with response:[{2}].\n\tDetails: MeetingIdentify:[{3}], Sender:[{4}], RecipientsCount:[{5}], NeedDoObligation:{6}, RequestIdentity:[{7}]\n",
                        Process.GetCurrentProcess().Id.ToString(), taskQueryPolicy.IsCompleted, strResponseInfo, stuWMQueryPolicyForMeetingRequestParameters.strMeetingIdentify, stuWMQueryPolicyForMeetingRequestParameters.strSender,
                        stuWMQueryPolicyForMeetingRequestParameters.GetRecipientsCount(), stuWMQueryPolicyForMeetingRequestParameters.bNeedDoObligations, stuWMQueryPolicyForMeetingRequestParameters.strQueryRequestIdentify);
            }
            return strResponseInfo;
        }
        private void TaskForQueryPolicyForMeeting(object obWMQueryPolicyForMeetingRequestParametersIn)
        {
            string strResponseInfo = "";
            QueryRequestIdentifyManager obQueryRequestIdentifyManagerIns = QueryRequestIdentifyManager.GetInstance();
            STUWMQueryPolicyForMeetingRequestParameters stuWMQueryPolicyForMeetingRequestParameters = obWMQueryPolicyForMeetingRequestParametersIn as STUWMQueryPolicyForMeetingRequestParameters;
            if (null == stuWMQueryPolicyForMeetingRequestParameters)
            {
                // Failed
                strResponseInfo = PolicyResultXmlBuilder.BuildJoinResultXML(XmlVariable.OPERATION_FAILED, stuWMQueryPolicyForMeetingRequestParameters.strQueryRequestIdentify, null, null);
                obQueryRequestIdentifyManagerIns.SaveQueryResponseInfoByRequestIdentify(stuWMQueryPolicyForMeetingRequestParameters.strQueryRequestIdentify, true, strResponseInfo);

                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "The query policy for meeting request parameters is error, code error, please check\n");
            }
            else
            {
                // Process => End
                strResponseInfo = InnerQueryPolicyForMeeting(stuWMQueryPolicyForMeetingRequestParameters.emAction, stuWMQueryPolicyForMeetingRequestParameters.strMeetingIdentify, stuWMQueryPolicyForMeetingRequestParameters.strSender, stuWMQueryPolicyForMeetingRequestParameters.setRecipients, stuWMQueryPolicyForMeetingRequestParameters.bNeedDoObligations);               
                obQueryRequestIdentifyManagerIns.SaveQueryResponseInfoByRequestIdentify(stuWMQueryPolicyForMeetingRequestParameters.strQueryRequestIdentify, true, strResponseInfo);
            }
        }
        private string InnerQueryPolicyForMeeting(EMSFB_ACTION emAction, string strMeetingIdentify, string strSender, HashSet<string> setRecipients, bool bNeedDoObligations)
        {
            bool bSuccess = true;
            // Check parameters
            if ((!CommonTools.IsSFBMeetingAction(emAction)) || String.IsNullOrEmpty(strSender) || String.IsNullOrEmpty(strMeetingIdentify))
            {
                // This web method only support meeting action
                // Meeting identify(entry url or sip uri) and sender user must not be empty
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "The action{0} must be meeting actions. The meeting identify:[{1}] and sender:[{2}] must be not empty\n", emAction, strMeetingIdentify, strSender);
                bSuccess = false;
            }
            else
            {
                // In meeting invite and join action, the recipients must be not empty
                if ((EMSFB_ACTION.emMeetingInvite == emAction) || (EMSFB_ACTION.emMeetingJoin == emAction))
                {
                    if ((null == setRecipients) || (0 >= setRecipients.Count))
                    {
                        logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "In action{0}(Invite or join) the recepients:[{1}] must no be empty\n", emAction, ((null == setRecipients) ? 0 : setRecipients.Count));
                        bSuccess = false;
                    }
                }
            }

            string strResultCode = XmlVariable.OPERATION_FAILED;
            PolicyResult[] szPolicyResultArray = null;
            SFBMeetingInfo obSFBMeetingInfo = null;
            SFBMeetingVariableInfo obSFBMeetingVariableInfo = null;
            NLMeetingInfo obNLMeetingInfo = null;
            if (bSuccess)
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_SUCCESS);
                bSuccess = EstablishMeetingObjectsByMeetingIdentify(strMeetingIdentify, out obSFBMeetingInfo, out obSFBMeetingVariableInfo, out obNLMeetingInfo);
                if (bSuccess)
                {
                    // Check manual classify flag
                    if (EMSFB_ACTION.emMeetingCreate != emAction)
                    {
                        // Check force classify flag                       
                        if (obSFBMeetingVariableInfo.IsManualClassifyDone())
                        {
                            // Done, check policy
                        }
                        else
                        {
                            // Manual classify do not complete, check the force munual classify flag						
                            bool bNeedForceManualClassify = String.Equals(obNLMeetingInfo.GetItemValue(NLMeetingInfo.kstrForceManulClassifyFieldName), PolicyMain.KStrObAttributeForceClassifyYes, StringComparison.OrdinalIgnoreCase);
                            if (bNeedForceManualClassify)
                            {
                                // Need force to do manual classify but it is do not done, deny
                                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "The meeting:[{0}] need force manual classify but it is not complete, deny action:[{1}]\n", strMeetingIdentify, emAction.ToString());
                                bSuccess = false;
                            }
                            else
                            {
                                // No need force to do manual classify, check policy
                            }
                        }
                    }
                    if (bSuccess)
                    {
                        // Already done classify or no force classify
                        MeetingPolicy obMeetingPolicyIns = MeetingPolicy.GetInstance();
                        EMSFB_QueryResult emResult = obMeetingPolicyIns.QueryMeetingMultiPolices(emAction, true, strSender, setRecipients, obSFBMeetingInfo, obSFBMeetingVariableInfo, obNLMeetingInfo, out szPolicyResultArray);
                        if (EMSFB_QueryResult.Success == emResult)
                        {
                            strResultCode = XmlVariable.OPERATION_SUCCEED;
                        }
                        else
                        {
                            bSuccess = false;
                        }
                    }
                    else
                    {
                        // Exist force classify but do not complete
                        strResultCode = XmlVariable.OPERATION_NoManualClassify;
                    }                    
                }
                else
                {
                    int nLastError = LastErrorRecorder.GetLastError();
                    if ((LastErrorRecorder.ERROR_DBCONNECTION_FAILED == nLastError) || 
                        (LastErrorRecorder.ERROR_DATA_READ_FAILED == nLastError)
                       )
                    {
                        // Data base error, exception
                        logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Establish meeting object by meeting identify:[{0}] failed, SFBE database connect or read failed with last error:[{1}], please check\n", strMeetingIdentify, nLastError);
                    }
                    else
                    {
                        strResultCode = XmlVariable.OPERATION_MeetingNotExist;
                        logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Establish meeting object by meeting identify:[{0}] failed, the meeting not exist with last error:[{1}], please check invoker\n", strMeetingIdentify, nLastError);
                    }
                }
            }
            else
            {
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Parameters error, failed to query policy for meeting, action:[{0}], meeting identify:[{1}], sender:[{2}]\n",
                    emAction, strMeetingIdentify, strSender);
            }
            
            string strQueryIdentify = Guid.NewGuid().ToString();
            string strResponseXml = PolicyResultXmlBuilder.BuildJoinResultXML(strResultCode, strQueryIdentify, setRecipients, szPolicyResultArray);
            logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "WMQueryPolicyForMeeting, action:[{0}], sender:[{1}], meetingIdentify:[{2}], response:[{3}]\n",
                emAction, strSender, strMeetingIdentify, strResponseXml);
            return strResponseXml;
        }
        #endregion

        #region Inner tools
        bool EstablishMeetingObjectsByMeetingIdentify(string strMeetingIdentify, out SFBMeetingInfo obSFBMeetingInfo, out SFBMeetingVariableInfo obSFBMeetingVariableInfo, out NLMeetingInfo obNLMeetingInfo)
        {
            // Init out parameters
            bool bRet = false;
            obSFBMeetingInfo = null;
            obSFBMeetingVariableInfo = null;
            obNLMeetingInfo = null;

            // Establish SFBMeetingInfo
            if (strMeetingIdentify.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // Meeting entry info
                obSFBMeetingInfo = CommonTools.GetSFBMeetingObjectByEntryInfo(strMeetingIdentify);
            }
            else if (strMeetingIdentify.StartsWith("sip:", StringComparison.OrdinalIgnoreCase))
            {
                // Meeting sip URI
                List<SFBObjectInfo> lsResultSFBObject = SFBObjectInfo.GetObjsFrommPersistentInfo(EMSFB_INFOTYPE.emInfoSFBMeeting, SFBMeetingInfo.kstrUriFieldName, strMeetingIdentify);
                if (1 == lsResultSFBObject.Count)
                {
                    obSFBMeetingInfo = lsResultSFBObject[0] as SFBMeetingInfo;
                }
                else
                {
                    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "\n GetObjsFrommPersistentInfo with meeting uri:[{0}] failed , result.Count != 1 \n", strMeetingIdentify);
                }
            }

            // Try to Establish SFBMeetingVariableInfo and NlMeetingInfo
            if (null == obSFBMeetingInfo)
            {
                logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Failed to get SFBMeetingInfo, meeting identify:[{0}]\n", strMeetingIdentify);
            }
            else
            {
                string strMeetingSipUri = obSFBMeetingInfo.GetItemValue(SFBMeetingInfo.kstrUriFieldName);
                if (String.IsNullOrEmpty(strMeetingSipUri))
                {
                    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "The meeting sip uri:[{0}] is empty, meeting identify:[{0}]\n", strMeetingSipUri, strMeetingIdentify);
                }
                else
                {
                    obSFBMeetingVariableInfo = new SFBMeetingVariableInfo();
                    bool bEstablishRet = obSFBMeetingVariableInfo.EstablishObjFormPersistentInfo(SFBMeetingInfo.kstrUriFieldName, strMeetingSipUri);
                    if (bEstablishRet)
                    {
                        obNLMeetingInfo = new NLMeetingInfo();
                        bEstablishRet = obNLMeetingInfo.EstablishObjFormPersistentInfo(SFBMeetingInfo.kstrUriFieldName, strMeetingSipUri);
                        if (bEstablishRet)
                        {
                            // All success
                            bRet = true;        
                        }
                        else
                        {
                            logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "The NLMeetingInfo is null, meeting uri:{0}, meeting identify:[{1}]\n", strMeetingSipUri, strMeetingIdentify);
                            obNLMeetingInfo = null;
                        }
                    }
                    else
                    {
                        logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "The SFBMeetingVariableInfo is null, meeting uri:{0}, meeting identify:[{1}]\n", strMeetingSipUri, strMeetingIdentify);
                        obSFBMeetingVariableInfo = null;
                    }
                }               
            }
            return bRet;
        }
        #endregion
    }
}
