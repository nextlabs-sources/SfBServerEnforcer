using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Rtc.Sip;
using System.Collections;

using SFBCommon.NLLog;
using SFBCommon.SFBObjectInfo;
using Nextlabs.SFBServerEnforcer.PolicyHelper;

using Nextlabs.SFBServerEnforcer.SIPComponent.SIPContentHelper;
using Nextlabs.SFBServerEnforcer.SIPComponent.DataManagement;
using Nextlabs.SFBServerEnforcer.SIPComponent.Common;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.NLAnalysis
{
    class NLChatRoomAttachmentAnalyser
    {
        #region Logger
        private CLog theLog = CLog.GetLogger(typeof(NLChatRoomAttachmentAnalyser));
        #endregion

        #region Const/read only values
        private const string kstrSepEventIDAndSequenceID = "--";
        #endregion

        #region members
        private DicCacheTemplate<string, KeyValuePair<EMSFB_ACTION, XCCosContent>> m_dicAttachmentGetCmdContentCache = new DicCacheTemplate<string, KeyValuePair<EMSFB_ACTION, XCCosContent>>();
        #endregion

        #region Sigleton
        static public NLChatRoomAttachmentAnalyser GetInstance() { return s_obInstance; }
        static private NLChatRoomAttachmentAnalyser s_obInstance = new NLChatRoomAttachmentAnalyser();
        private NLChatRoomAttachmentAnalyser() { }
        #endregion

        #region Chat room attachment upload/download request analysis
        /* 
         * In get file upload/download token request, we can get chat room URI and current user URI
         * In reply get file token request, we can get token ID and current user URI
         * We can use command sequence id, event id and user URI to connect the CMD and REPLY request
         * In the reply request we will save token ID, chat room URI, current user and action to table: SFBChatRoomAttachmentEntryVariableTable
        */
        public void ParseChatRoomGetFileUploadTokenRequest(Request sipRequest, XCCosContentCmdGetFileUploadToken obXCCosContentCmdGetFileUploadToken)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In ParseChatRoomGetFileUploadTokenRequest, request content:[{0}]\n", sipRequest.Content);

            if (null != obXCCosContentCmdGetFileUploadToken)
            {
                string strKey = GetAttachmentGetCmdContentCacheKey(obXCCosContentCmdGetFileUploadToken);
                m_dicAttachmentGetCmdContentCache.SetValue(strKey, new KeyValuePair<EMSFB_ACTION, XCCosContent>(EMSFB_ACTION.emChatRoomUpload, obXCCosContentCmdGetFileUploadToken));
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "obXCCosContentCmdGetFileUploadToken is null in ParseChatRoomGetFileUploadTokenRequest, this maybe an error\n");
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Out ParseChatRoomGetFileUploadTokenRequest\n");
        }
        public void ParseChatRoomGetFileDownloadTokenRequest(Request sipRequest, XCCosContentCmdGetFileDownloadToken obXCCosContentCmdGetFileDownloadToken)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In ParseChatRoomGetFileDownloadTokenRequest, request content:[{0}]\n", sipRequest.Content);
            if (null != obXCCosContentCmdGetFileDownloadToken)
            {
                string strKey = GetAttachmentGetCmdContentCacheKey(obXCCosContentCmdGetFileDownloadToken);
                m_dicAttachmentGetCmdContentCache.SetValue(strKey, new KeyValuePair<EMSFB_ACTION, XCCosContent>(EMSFB_ACTION.emChatRoomDownload, obXCCosContentCmdGetFileDownloadToken));
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "obXCCosContentCmdGetFileDownloadToken is null in ParseChatRoomGetFileDownloadTokenRequest, this maybe an error\n");
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Out ParseChatRoomGetFileDownloadTokenRequest\n");
        }
        public void ParseChatRoomReplyGetFileTokenRequest(Request sipRequest, XCCosContentReplyGetFileToken obXCCosContentReplyGetFileToken)
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "In ParseChatRoomReplyGetFileTokenRequest, request content:[{0}]\n", sipRequest.Content);
            if (null != obXCCosContentReplyGetFileToken)
            {
                bool bSuccess = false;
                string strChatRoomID = "";
                string strFileName = "";
                string strCurUserUri = CSIPTools.GetUserUriFromRequestHeader(sipRequest.AllHeaders.FindFirst(CSIPTools.SIP_HDR_TO));
                string strKey = GetAttachmentGetCmdContentCacheKey(obXCCosContentReplyGetFileToken);
                KeyValuePair<EMSFB_ACTION, XCCosContent> pairGetCmdContentInfo = m_dicAttachmentGetCmdContentCache.GetValue(strKey, new KeyValuePair<EMSFB_ACTION, XCCosContent>(EMSFB_ACTION.emUnknwon, null));
                if (EMSFB_ACTION.emChatRoomUpload == pairGetCmdContentInfo.Key)
                {
                    XCCosContentCmdGetFileUploadToken obXCCosContentCmdGetFileUploadToken = pairGetCmdContentInfo.Value as XCCosContentCmdGetFileUploadToken;
                    if (null != obXCCosContentCmdGetFileUploadToken)
                    {
                        strChatRoomID = obXCCosContentCmdGetFileUploadToken.ChatRoomID;
                        strFileName = obXCCosContentCmdGetFileUploadToken.FileName;
                        bSuccess = true;
                    }
                }
                else if (EMSFB_ACTION.emChatRoomDownload == pairGetCmdContentInfo.Key)
                {
                    XCCosContentCmdGetFileDownloadToken obXCCosContentCmdGetFileDownloadToken = pairGetCmdContentInfo.Value as XCCosContentCmdGetFileDownloadToken;
                    if (null != obXCCosContentCmdGetFileDownloadToken)
                    {
                        strChatRoomID = obXCCosContentCmdGetFileDownloadToken.ChatRoomID;
                        strFileName = obXCCosContentCmdGetFileDownloadToken.FileName;
                        bSuccess = true;
                    }
                }
                if (bSuccess)
                {
                    CChatRoom obChatRoom = CChatRoomManager.GetInstance().GetChatRoomByIDEx(strChatRoomID, true);
                    if ((null != obChatRoom) && (obChatRoom.NeedEnforce()))
                    {
                        SFBChatRoomAttachmentEntryVariableInfo obSFBChatRoomAttachmentEntryVariableInfo = new SFBChatRoomAttachmentEntryVariableInfo(
                                            SFBChatRoomAttachmentEntryVariableInfo.kstrTokenIDFieldName, obXCCosContentReplyGetFileToken.TokenID,
                                            SFBChatRoomAttachmentEntryVariableInfo.kstrChatRoomUriFieldName, strChatRoomID,
                                            SFBChatRoomAttachmentEntryVariableInfo.kstrUserFieldName, strCurUserUri,
                                            SFBChatRoomAttachmentEntryVariableInfo.kstrActionFieldName, pairGetCmdContentInfo.Key.ToString(),
                                            SFBChatRoomAttachmentEntryVariableInfo.kstrFileOrgNameFieldName, strFileName
                                            );
                        obSFBChatRoomAttachmentEntryVariableInfo.PersistantSave();
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current chat room no need enforce or cannot load chat room info, ChatRoomID:[{0}]\n", strChatRoomID);
                    }
                }
                m_dicAttachmentGetCmdContentCache.Delete(strKey);
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "obXCCosContentReplyGetFileToken is null in ParseChatRoomReplyGetFileTokenRequest, this maybe an error\n");
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Out ParseChatRoomReplyGetFileTokenRequest\n");
        }
        #endregion

        #region Inner tools
        private string GetAttachmentGetCmdContentCacheKey(XCCosContentCmdGetFileUploadToken obXCCosContentCmdGetFileUploadToken)
        {
            return obXCCosContentCmdGetFileUploadToken.EventID + kstrSepEventIDAndSequenceID + obXCCosContentCmdGetFileUploadToken.SequenceID;
        }
        private string GetAttachmentGetCmdContentCacheKey(XCCosContentCmdGetFileDownloadToken obXCCosContentCmdGetFileDownloadToken)
        {
            return obXCCosContentCmdGetFileDownloadToken.EventID + kstrSepEventIDAndSequenceID + obXCCosContentCmdGetFileDownloadToken.SequenceID;
        }
        private string GetAttachmentGetCmdContentCacheKey(XCCosContentReplyGetFileToken obXCCosContentReplyGetFileToken)
        {
            return obXCCosContentReplyGetFileToken.EventID + kstrSepEventIDAndSequenceID + obXCCosContentReplyGetFileToken.SequenceID;
        }
        #endregion

    }
}
