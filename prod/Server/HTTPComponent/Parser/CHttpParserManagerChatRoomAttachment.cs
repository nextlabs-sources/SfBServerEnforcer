using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Globalization;

using SFBCommon.NLLog;
using SFBCommon.SFBObjectInfo;
using SFBCommon.ClassifyHelper;
using SFBCommon.Common;
using TagHelper;
using Nextlabs.SFBServerEnforcer.PolicyHelper;

using Nextlabs.SFBServerEnforcer.HTTPComponent.RequestFilters;
using Nextlabs.SFBServerEnforcer.HTTPComponent.Data;
using Nextlabs.SFBServerEnforcer.HTTPComponent.Policy;

namespace Nextlabs.SFBServerEnforcer.HTTPComponent.Parser
{
    class CHttpParserManagerChatRoomAttachment : CHttpParserBase
    {
        #region Const/Read only values
        public const string kstrTimeFormat = "yyyy-MM-dd HH:mm:ss:fff";
        
        static private readonly TimeSpan ktimSpanAccuracy = new TimeSpan(0, 0, 0, 0, 10);
        #endregion

        #region Members
        private RequestFilterChatRoomAttachment m_obRequestFilterChatRoomAttachment = null;
        #endregion

        #region Constructors
        public CHttpParserManagerChatRoomAttachment(HttpRequest request) : base(request, REQUEST_TYPE.REQUEST_MANAGER_CHATROOM_ATTACHMENT)
        {
        }
        #endregion

        #region Override: CHttpParserBase public
        override public void EndRequest(Object source, EventArgs e)
        {
            HttpApplication application = source as HttpApplication;
            if (null != m_obRequestFilterChatRoomAttachment)
            {
                EnforceResult emReturnEnforce = EnforceResult.Enforce_Allow;

                // Get token, channel id, unique file path ==> get full file unc path
                EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE emAttachmentRequestType = m_obRequestFilterChatRoomAttachment.AttachmentRequestType;
                if (EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE.emAppendChunk == emAttachmentRequestType)
                {
                    // Upload action
                    emReturnEnforce = ProcessAttachmentUploadAction(m_obRequestFilterChatRoomAttachment.Token, m_obRequestFilterChatRoomAttachment.ChannelId, m_obRequestFilterChatRoomAttachment.FileName);
                }
                else if (EMSFB_CHATROOM_ATTACHMENT_REQUEST_TYPE.emDownloadChunk == emAttachmentRequestType)
                {
                    // Download action
                    emReturnEnforce = ProcessAttachmentDownloadAction(m_obRequestFilterChatRoomAttachment.Token, m_obRequestFilterChatRoomAttachment.ChannelId, m_obRequestFilterChatRoomAttachment.FileName);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In end request with attachment request type:[{0}], ignore\n", emAttachmentRequestType);
                }
                if (EnforceResult.Enforce_Deny == emReturnEnforce)
                {
                    // Error code, access deny
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Reset response with 403 because policy is deny");
                    application.Context.Response.StatusDescription = "Internal server error";
                    application.Context.Response.StatusCode = 500;  // Cannot return 4XX error, otherwise SFB Client will be not continue transfer files after received 4XX error.
                }
            }

            Reset(application);
        }
        #endregion

        #region Override: CHttpParserBase protected
        override protected RequestFilter CreateRequestFilter(HttpRequest request)
        {
            m_obRequestFilterChatRoomAttachment = null;
            if (null != request)
            {
                m_obRequestFilterChatRoomAttachment = new RequestFilterChatRoomAttachment(request);
            }
            return m_obRequestFilterChatRoomAttachment;
        }
        #endregion

        #region Inner logic processer
        private EnforceResult ProcessAttachmentUploadAction(string strToken, string strChannelId, string strFileName)
        {
            EnforceResult emReturnEnforce = CommonCfgMgr.GetInstance().DefaultPolicyResult ? EnforceResult.Enforce_Allow : EnforceResult.Enforce_Deny;
            if (string.IsNullOrEmpty(strToken) || string.IsNullOrEmpty(strChannelId) || string.IsNullOrEmpty(strFileName))
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "One of basic info is empty, token:[{0}], channelId:[{1}], file name:[{2}]\n", strToken, strChannelId, strFileName);
                return emReturnEnforce;
            }

            SFBChatRoomAttachmentEntryVariableInfo obSFBChatRoomAttachmentEntryVariableInfo = new SFBChatRoomAttachmentEntryVariableInfo();
            bool bEstablished = obSFBChatRoomAttachmentEntryVariableInfo.EstablishObjFormPersistentInfo(SFBChatRoomAttachmentEntryVariableInfo.kstrTokenIDFieldName, strToken);
            if (bEstablished)
            {
                string strChatRoomUri = obSFBChatRoomAttachmentEntryVariableInfo.GetItemValue(SFBChatRoomAttachmentEntryVariableInfo.kstrChatRoomUriFieldName);
                HttpChatRoomInfo obHttpChatRoomInfo = new HttpChatRoomInfo();
                bEstablished = obHttpChatRoomInfo.EstablishChatRoomInfo(strChatRoomUri);
                if (bEstablished && (obHttpChatRoomInfo.NeedEnforce()))
                {
                    string strAttachmentUniqueFlag = GetAttachmentUniqueFlag(strChannelId, strFileName);
                    string strAttachmentFullPath = GetAttachmentFullFilePath(strChannelId, strFileName, obHttpChatRoomInfo.sfbChatRoomInfo);
                    if ((!string.IsNullOrEmpty(strAttachmentFullPath)) && (File.Exists(strAttachmentFullPath)))
                    {
                        string strFileOwner = obSFBChatRoomAttachmentEntryVariableInfo.GetItemValue(SFBChatRoomAttachmentEntryVariableInfo.kstrUserFieldName);
                        string strFileOrgName = obSFBChatRoomAttachmentEntryVariableInfo.GetItemValue(SFBChatRoomAttachmentEntryVariableInfo.kstrFileOrgNameFieldName);
                        NLChatRoomAttachmentInfo obNLChatRoomAttachmentInfo = GreateChatRoomAttachmentInfo(strAttachmentUniqueFlag, strAttachmentFullPath, strFileOwner, strChatRoomUri, strFileOrgName);
                        if (null != obNLChatRoomAttachmentInfo)
                        {
                            obNLChatRoomAttachmentInfo.PersistantSave();

                            // Query policy
                            PolicyResult obPolicyResult = new PolicyResult(emReturnEnforce);
                            HttpPolicy obPolicyInstance = HttpPolicy.GetInstance();
                            obPolicyInstance.QueryPolicyForChatRoomAttachmentAction(EMSFB_ACTION.emChatRoomUpload, strFileOwner, obHttpChatRoomInfo.Creator, obHttpChatRoomInfo, obNLChatRoomAttachmentInfo, out obPolicyResult);
                            emReturnEnforce = obPolicyResult.Enforcement;
                            if (obPolicyResult.IsDeny())
                            {
                                // Deny, remove file
                                File.Delete(strAttachmentFullPath);
                            }
                            else
                            {
                                // Allow or don't care
                            }

                            // Process obligations
                            ObligationHelper.ProcessChatRoomAttachmentCommonObligations(EMSFB_ACTION.emChatRoomUpload, obPolicyResult, "", strFileOwner, obHttpChatRoomInfo, obNLChatRoomAttachmentInfo);
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Init attachment info failed, FileUniqueFlag:[{0}], FileFullPath:[{1}]\n", strAttachmentUniqueFlag, strAttachmentFullPath);
                        }
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current chat room:[{0}] no need enforce or establish chat room info failed, ignroe upload attachment case\n", strChatRoomUri);
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Establish chat room attachment entry variable info with token id:[{0}] failed in attachement upload action\n", strToken);
            }
            return emReturnEnforce;
        }
        private EnforceResult ProcessAttachmentDownloadAction(string strToken, string strChannelId, string strFileName)
        {
            EnforceResult emReturnEnforce = CommonCfgMgr.GetInstance().DefaultPolicyResult ? EnforceResult.Enforce_Allow : EnforceResult.Enforce_Deny;
            if (string.IsNullOrEmpty(strToken) || string.IsNullOrEmpty(strChannelId) || string.IsNullOrEmpty(strFileName))
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "One of basic info is empty, token:[{0}], channelId:[{1}], file name:[{2}]\n", strToken, strChannelId, strFileName);
                return emReturnEnforce;
            }

            SFBChatRoomAttachmentEntryVariableInfo obSFBChatRoomAttachmentEntryVariableInfo = new SFBChatRoomAttachmentEntryVariableInfo();
            bool bEstablished = obSFBChatRoomAttachmentEntryVariableInfo.EstablishObjFormPersistentInfo(SFBChatRoomAttachmentEntryVariableInfo.kstrTokenIDFieldName, strToken);
            if (bEstablished)
            {
                string strChatRoomUri = obSFBChatRoomAttachmentEntryVariableInfo.GetItemValue(SFBChatRoomAttachmentEntryVariableInfo.kstrChatRoomUriFieldName);
                HttpChatRoomInfo obHttpChatRoomInfo = new HttpChatRoomInfo();
                bEstablished = obHttpChatRoomInfo.EstablishChatRoomInfo(strChatRoomUri);
                if (bEstablished && (obHttpChatRoomInfo.NeedEnforce()))
                {
                    string strAttachmentUniqueFlag = GetAttachmentUniqueFlag(strChannelId, strFileName);
                    NLChatRoomAttachmentInfo obNLChatRoomAttachmentInfo = GetChatRoomAttachmentInfo(strAttachmentUniqueFlag, true);
                    bEstablished = (null != obNLChatRoomAttachmentInfo);
                    if (!bEstablished)
                    {
                        string strAttachmentFullPath = GetAttachmentFullFilePath(strChannelId, strFileName, obHttpChatRoomInfo.sfbChatRoomInfo);
                        if (!string.IsNullOrEmpty(strAttachmentFullPath))
                        {
                            string strFileOrgName = obSFBChatRoomAttachmentEntryVariableInfo.GetItemValue(SFBChatRoomAttachmentEntryVariableInfo.kstrFileOrgNameFieldName);
                            obNLChatRoomAttachmentInfo = GreateChatRoomAttachmentInfo(strAttachmentUniqueFlag, strAttachmentFullPath, "", strChatRoomUri, strFileOrgName);
                            if (null != obNLChatRoomAttachmentInfo)
                            {
                                obNLChatRoomAttachmentInfo.PersistantSave();
                                bEstablished = true;    // For current case, no need case if it save success or not. We get all informations here and current case is success.
                            }
                        }
                    }

                    if (bEstablished)
                    {
                        string strFromUser = obSFBChatRoomAttachmentEntryVariableInfo.GetItemValue(SFBChatRoomAttachmentEntryVariableInfo.kstrUserFieldName);
                        string strToUser = obNLChatRoomAttachmentInfo.GetItemValue(NLChatRoomAttachmentInfo.kstrFileOwnerFieldName);
                        if (string.IsNullOrEmpty(strToUser))
                        {
                            strToUser = strFromUser;
                        }

                        // Query policy
                        PolicyResult obPolicyResult = new PolicyResult();
                        HttpPolicy obPolicyInstance = HttpPolicy.GetInstance();
                        obPolicyInstance.QueryPolicyForChatRoomAttachmentAction(EMSFB_ACTION.emChatRoomDownload, strFromUser, strToUser, obHttpChatRoomInfo, obNLChatRoomAttachmentInfo, out obPolicyResult);
                        emReturnEnforce = obPolicyResult.Enforcement;

                        // Process obligations
                        ObligationHelper.ProcessChatRoomAttachmentCommonObligations(EMSFB_ACTION.emChatRoomDownload, obPolicyResult, strToUser, strFromUser, obHttpChatRoomInfo, obNLChatRoomAttachmentInfo);
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current chat room:[{0}] no need enforce, ignroe donwload attachment case\n", strChatRoomUri);
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Establish chat room attachment entry variable info with token id:[{0}] failed in attachment download action\n", strToken);
            }
            return emReturnEnforce;
        }
        #endregion

        #region Inner tools
        private NLChatRoomAttachmentInfo GetChatRoomAttachmentInfo(string strAttachmentUniqueFlag, bool bCheckCacheEffective)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "AttachmentUniqueFlag:[{0}], CheckCacheEffective:[{1}]\n", strAttachmentUniqueFlag, bCheckCacheEffective);
            NLChatRoomAttachmentInfo obNLChatRoomAttachmentInfo = new NLChatRoomAttachmentInfo();
            bool bEstablished = obNLChatRoomAttachmentInfo.EstablishObjFormPersistentInfo(NLChatRoomAttachmentInfo.kstrAttachmentUniqueFlagFieldName, strAttachmentUniqueFlag);
            if (!bEstablished)
            {
                obNLChatRoomAttachmentInfo = null;
            }
            else
            {
                // Get and check file path
                string strFilePath = obNLChatRoomAttachmentInfo.GetItemValue(NLChatRoomAttachmentInfo.kstrFilePathFieldName);
                if (string.IsNullOrEmpty(strFilePath) || (!File.Exists(strFilePath)))
                {
                    obNLChatRoomAttachmentInfo = null;
                }
                else
                {
                    if (bCheckCacheEffective)
                    {
                        try
                        {
                            // Check if the file is changed or not, if changed, update the cache
                            string strFileLastModifyTime = obNLChatRoomAttachmentInfo.GetItemValue(NLChatRoomAttachmentInfo.kstrLastModifyTimeFieldName);
                            DateTime dtCacheFileLastModifyTimeUtc = GetDateTimeFromString(strFileLastModifyTime, kstrTimeFormat);
                            DateTime dtCurFileLastModifyTimeUtc = File.GetLastWriteTimeUtc(strFilePath);
                            if (!IsSameTime(dtCurFileLastModifyTimeUtc, dtCurFileLastModifyTimeUtc, ktimSpanAccuracy))
                            {
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "The attachment file is changed, update attachment cache info, cacheUtc:[{0}], fileUtc:[{1}]\n", dtCacheFileLastModifyTimeUtc.Ticks, dtCurFileLastModifyTimeUtc.Ticks);
                                string strFileOwner = obNLChatRoomAttachmentInfo.GetItemValue(NLChatRoomAttachmentInfo.kstrFileOwnerFieldName);
                                string strChatRoomUri = obNLChatRoomAttachmentInfo.GetItemValue(NLChatRoomAttachmentInfo.kstrFileOwnerFieldName);
                                string strFileOrgName = obNLChatRoomAttachmentInfo.GetItemValue(NLChatRoomAttachmentInfo.kstrFileOrgNameFieldName);
                                obNLChatRoomAttachmentInfo = GreateChatRoomAttachmentInfo(strAttachmentUniqueFlag, strFilePath, strFileOwner, strChatRoomUri, strFileOrgName);
                                if (null != obNLChatRoomAttachmentInfo)
                                {
                                    obNLChatRoomAttachmentInfo.PersistantSave();
                                }
                            }
                            else
                            {
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Attachment do not changed, using the cache information, Path:[{0}]\n", strFilePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            obNLChatRoomAttachmentInfo = null;
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in GetChatRoomAttachmentInfo, {0}\n", ex.Message);
                        }
                    }
                }
            }
            return obNLChatRoomAttachmentInfo;
        }
        private NLChatRoomAttachmentInfo GreateChatRoomAttachmentInfo(string strAttachmentUniqueFlag, string strFilePath, string strFileOwner, string strChatRoomUri, string strFileOrgName)
        {
            NLChatRoomAttachmentInfo obNLChatRoomAttachmentInfo = null;
            try
            {
                TagMain obTagMain = new TagMain();
                Dictionary<string, string> dicTags = obTagMain.ReadTagEx(strFilePath);
                ClassifyTagsHelper obClassifyTagsHelper = new ClassifyTagsHelper(dicTags);
                string strFileTags = obClassifyTagsHelper.GetClassifyXml();
                DateTime timeFileLastModifyTimeUtc = File.GetLastWriteTimeUtc(strFilePath);
                string strFileLastModifyTime = timeFileLastModifyTimeUtc.ToString(kstrTimeFormat);

                obNLChatRoomAttachmentInfo = new NLChatRoomAttachmentInfo(
                    NLChatRoomAttachmentInfo.kstrAttachmentUniqueFlagFieldName, strAttachmentUniqueFlag,
                    NLChatRoomAttachmentInfo.kstrChatRoomUriFieldName, strChatRoomUri,
                    NLChatRoomAttachmentInfo.kstrFileOrgNameFieldName, strFileOrgName,
                    NLChatRoomAttachmentInfo.kstrFileOwnerFieldName, strFileOwner,
                    NLChatRoomAttachmentInfo.kstrFilePathFieldName, strFilePath,
                    NLChatRoomAttachmentInfo.kstrFileTagsFieldName, strFileTags,
                    NLChatRoomAttachmentInfo.kstrLastModifyTimeFieldName, strFileLastModifyTime
                    );
            }
            catch (Exception ex)
            {
                obNLChatRoomAttachmentInfo = null;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in GetChatRoomAttachmentInfo, [{0}]\n", ex.Message);
            }
            return obNLChatRoomAttachmentInfo;
        }
        private string GetAttachmentFullFilePath(string strChannelId, string strUniqueFileName, SFBChatRoomInfo obSFBChatRoomInfo)
        {
            const string kstrPersistentChatFileStore = "PersistentChat";

            string strAttachmentFullFilePath = "";
            string strChatCategoryUri = obSFBChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrCategoryUriFieldName);
            if (!string.IsNullOrEmpty(strChatCategoryUri))
            {
                SFBChatCategoryInfo obSFBChatCategoryInfo = new SFBChatCategoryInfo();
                bool bEstablished = obSFBChatCategoryInfo.EstablishObjFormPersistentInfo(SFBChatCategoryInfo.kstrUriFieldName, strChatCategoryUri);
                if (bEstablished)
                {
                    string strChatPoolFqdn = obSFBChatCategoryInfo.GetItemValue(SFBChatCategoryInfo.kstrPersistentChatPoolFieldName);
                    if (!string.IsNullOrEmpty(strChatPoolFqdn))
                    {
                        SFBPersistentChatServerInfo obSFBPersistentChatServerInfo = new SFBPersistentChatServerInfo();
                        bEstablished = obSFBPersistentChatServerInfo.EstablishObjFormPersistentInfo(SFBPersistentChatServerInfo.kstrPoolFqdnFieldName, strChatPoolFqdn);
                        if (bEstablished)
                        {
                            string strShareUncPath = obSFBPersistentChatServerInfo.GetItemValue(SFBPersistentChatServerInfo.kstrFileStoreUncPathFieldName);
                            string strServiceID = obSFBPersistentChatServerInfo.GetItemValue(SFBPersistentChatServerInfo.kstrServiceIdFieldName);
                            if ((!string.IsNullOrEmpty(strShareUncPath)) && (!string.IsNullOrEmpty(strServiceID)))
                            {
                                strAttachmentFullFilePath = string.Format("{0}\\{1}\\{2}\\{3}\\{4}", strShareUncPath, strServiceID, kstrPersistentChatFileStore, strChannelId, strUniqueFileName);
                            }
                            else
                            {
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Get share unc path or service id failed, ShareUncPath:[{0}], ServiceID:[{1}]\n", strShareUncPath, strServiceID);
                            }
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Establish file store service info failed with FQDN:[{0}]\n", strChatPoolFqdn);
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Get chat pool FQDN from chat category info failed, chat category uri:[{0}]\n", strChatCategoryUri);
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Establish chat category info with URI:[{0}] failed\n", strChatCategoryUri);
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Get chat category URI from SFBChatRoomInfo failed, chat room id:[{0}]\n", obSFBChatRoomInfo.GetItemValue(SFBChatRoomInfo.kstrUriFieldName));
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Attachment file path is:[{0}]\n", strAttachmentFullFilePath);
            return strAttachmentFullFilePath;
        }
        private string GetAttachmentUniqueFlag(string strChannelId, string strFileName)
        {
            return strChannelId + "/" + strFileName;
        }
        static public bool IsSameTime(DateTime dtFirst, DateTime dtSecond, TimeSpan timeSpanAccuracy)
        {
            DateTime dtBigger = dtFirst + timeSpanAccuracy;
            DateTime dtSmaller = dtFirst - timeSpanAccuracy;
            return ((dtBigger >= dtSecond) && (dtSmaller <= dtSecond));
        }
        static public DateTime GetDateTimeFromString(string strDateTime, string strDatetTimePattern)
        {
            DateTime timeRet = new DateTime(0);
            try
            {
                timeRet = DateTime.ParseExact(strDateTime, strDatetTimePattern, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in GetDateTimeFromString, string time:[{0}], pattern:[{1}], Message:[{2}]\n", strDateTime, strDatetTimePattern, ex.Message);
            }
            return timeRet;
        }
        #endregion
    }
}
