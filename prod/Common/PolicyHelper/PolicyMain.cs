using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDKWrapperLib;
using SFBCommon.Common;
using SFBCommon.SFBObjectInfo;
using SFBCommon.NLLog;

namespace Nextlabs.SFBServerEnforcer.PolicyHelper
{
    public enum EMSFB_ACTION
    {
        emUnknwon,

        emMeetingCreate,
        emMeetingInvite,
        emMeetingJoin,

        emMeetingShare,
        emMeetingShareJoin,

        emChatRoomCreate,
        emChatRoomManagerInvite,
        emChatRoomInvite,
        emChatRoomJoin,

        emChatRoomUpload,
        emChatRoomDownload,
        
        emNotifyUsers,
        emUserConditionQuery,
    }

    public class PolicyMain
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(PolicyMain));
        #endregion

        #region Common values
        private const int g_knMultilpleQueryLimitedMaxCount = 2;

        private const int CE_RESULT_SUCCESS = 0;
        private const int CE_RESULT_FAILED = 1;
        #endregion

        #region obligation name and attribute name const define
        public const string kStrObNameAutoEnforcement = "SFB_AutoSet_Enforcement";
        // public const string kStrObAttributeEnableEnforcer = "Enforcement";

        public const string kStrObNameNotification = "SFB_Notify";
        public const string kStrObAttributeNotifyMessage = "Message";

        public const string KStrObNameAutoClassify = "SFB_Auto_Classify";
        public const string kStrObAttributeAutoClassifyTagName = "Key";
        public const string KstrObAttributeAutoClassifyTagValue = "Value";

        public const string KStrObNameManualClassify = "SFB_Manual_Classify";
        public const string KStrObAttributeManualClassifyData = "Data";
        public const string KStrObAttributeForceClassify = "Force_Classify";
        public const string KStrObAttributeForceClassifyYes = "Yes";
        public const string KStrObAttributeForceClassifyNo = "No";

        public const string KStrObNameEnableClassifyManager = "SFB_AutoSet_Classify_Manager";

        public const string KStrObNameUserConditoin = "SFB_User_Condition";
        public const string KStrObAttributeUserConditionSubject = "Subject";
        public const string KStrObAttributeUserConditionSubjectValueParticipate = "Participants";
        public const string KStrObAttributeUserConditionValue = "Matches_Policy_With_Condition";
        #endregion 

        #region policy action name define
        private const string kStrActionNameMeetingCreate = "SFB_MEETING_CREATE";
        private const string kStrActionNameMeetingInvite = "SFB_MEETING_INVITE";
        private const string kStrActionNameMeetingJoin = "SFB_MEETING_JOIN";

        private const string kStrActionNameMeetingShareCreate = "SFB_MEETING_SHARE";
        private const string kStrActionNameMeetingShareJoin = "SFB_MEETING_SHARE_JOIN";

        private const string KStrActionChatRoomCreate = "SFB_CHATROOM_CREATE";
        private const string kStrActionChatRoomManagerInvite = "SFB_CHATROOMMANAGER_INVITE";
        private const string kStrActionChatRoomInvite = "SFB_CHATROOM_INVITE";
        private const string kStrActionChatRoomJoin = "SFB_CHATROOM_JOIN";

        private const string kStrActionChatRoomUpload = "SFB_CHATROOM_UPLOAD";
        private const string kStrActionChatRoomDownload = "SFB_CHATROOM_DOWNLOAD";

        private const string kStrActionNotifyUsers = "SFB_USER_CONDITION_QUERY";
        private const string KStrActionUserConditionQuery = "SFB_USER_CONDITION_QUERY";

        static private readonly Dictionary<EMSFB_ACTION, string> s_kdicSFBActionTypeAndName = new Dictionary<EMSFB_ACTION,string>()
        {
            {EMSFB_ACTION.emUnknwon, ""},

            {EMSFB_ACTION.emMeetingCreate, kStrActionNameMeetingCreate},
            {EMSFB_ACTION.emMeetingInvite, kStrActionNameMeetingInvite},
            {EMSFB_ACTION.emMeetingJoin, kStrActionNameMeetingJoin},

            {EMSFB_ACTION.emMeetingShare, kStrActionNameMeetingShareCreate},
            {EMSFB_ACTION.emMeetingShareJoin, kStrActionNameMeetingShareJoin},

            {EMSFB_ACTION.emChatRoomCreate, KStrActionChatRoomCreate},
            {EMSFB_ACTION.emChatRoomManagerInvite, kStrActionChatRoomManagerInvite},
            {EMSFB_ACTION.emChatRoomInvite, kStrActionChatRoomInvite},
            {EMSFB_ACTION.emChatRoomJoin, kStrActionChatRoomJoin},

            {EMSFB_ACTION.emChatRoomUpload, kStrActionChatRoomUpload},
            {EMSFB_ACTION.emChatRoomDownload, kStrActionChatRoomDownload},

            {EMSFB_ACTION.emNotifyUsers, kStrActionNotifyUsers},
            {EMSFB_ACTION.emUserConditionQuery, KStrActionUserConditionQuery}
        };
        #endregion

        #region policy model define
        private const string kStrPolicyModelMeeting = "sfb_meeting";
        private const string KStrPolicyModelChatroom = "sfb_chatroom";
        private const string kStrPolicyModelUser = "sfb_user";

        static private readonly Dictionary<EMSFB_ACTION, string> s_kdicSFBPolicyModel = new Dictionary<EMSFB_ACTION, string>()
        {
            {EMSFB_ACTION.emUnknwon, ""},

            {EMSFB_ACTION.emMeetingCreate, kStrPolicyModelMeeting},
            {EMSFB_ACTION.emMeetingInvite, kStrPolicyModelMeeting},
            {EMSFB_ACTION.emMeetingJoin, kStrPolicyModelMeeting},
            {EMSFB_ACTION.emMeetingShare, kStrPolicyModelMeeting},
            {EMSFB_ACTION.emMeetingShareJoin, kStrPolicyModelMeeting},

            {EMSFB_ACTION.emChatRoomCreate, KStrPolicyModelChatroom},
            {EMSFB_ACTION.emChatRoomManagerInvite, KStrPolicyModelChatroom},
            {EMSFB_ACTION.emChatRoomInvite, KStrPolicyModelChatroom},
            {EMSFB_ACTION.emChatRoomJoin, KStrPolicyModelChatroom},
            {EMSFB_ACTION.emChatRoomUpload, KStrPolicyModelChatroom},
            {EMSFB_ACTION.emChatRoomDownload, KStrPolicyModelChatroom},

            {EMSFB_ACTION.emNotifyUsers, kStrPolicyModelUser},
            {EMSFB_ACTION.emUserConditionQuery, kStrPolicyModelUser}
        };
        #endregion

        static public readonly string s_kstrAppPath = SFBCommon.Common.CommonHelper.GetApplicationFile();

        #region SFB type and policy type define
        static private readonly Dictionary<SFBCommon.Common.EMSFB_INFOTYPE, string> s_kdicSFBTypeToPolicyType = new Dictionary<SFBCommon.Common.EMSFB_INFOTYPE, string>()
        {
            {SFBCommon.Common.EMSFB_INFOTYPE.emInfoUnknown, "unknown"},
            {SFBCommon.Common.EMSFB_INFOTYPE.emInfoSFBMeeting, "meeting"},
            {SFBCommon.Common.EMSFB_INFOTYPE.emInfoSFBChatRoom, "chatroom"}
        };
        #endregion

        #region Static public tools
        static public string GetActionStringNameByType(EMSFB_ACTION emAction)
        {
            return CommonHelper.GetValueByKeyFromDir(s_kdicSFBActionTypeAndName, emAction, "");
        }

        static public string GetPolicyModelByAction(EMSFB_ACTION emAction)
        {
            return CommonHelper.GetValueByKeyFromDir(s_kdicSFBPolicyModel, emAction, "");
        }
        #endregion

        #region Static private tools
        static private string GetPolicyTypeBySFBType(SFBCommon.Common.EMSFB_INFOTYPE emSfbType)
        {
            return CommonHelper.GetValueByKeyFromDir(s_kdicSFBTypeToPolicyType, emSfbType, "");
        }
        #endregion

        #region Public functions
        // Return 0 failed, 1 success
        public int QueryPolicy(EMSFB_ACTION emAction, string strUserSender, string strReceiver, SFBCommon.SFBObjectInfo.SFBObjectInfo[] objs, List<string> lstParticipants, PolicyResult policyResult)
        {
            //
            QueryPC thePc = new QueryPC();
            int lCookie = 0;
            thePc.get_cookie(out lCookie);

            //construct request
            PolicyRequestInfo requestInfo = new PolicyRequestInfo(emAction, strUserSender, strReceiver, objs, null, lstParticipants);
            Request pReq = requestInfo.ConstructRequest();
            thePc.set_request(pReq, lCookie);

            //query
            int lResult = 0;
            try
            {
                thePc.check_resource(lCookie, 1000 * 60, 1, out lResult);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Exception on thePc.check_resource:{0}", ex.ToString());
                return 0;
            }

            //get policy result
            int iObNum = 0;
            thePc.get_result(lCookie, 0, out lResult, out iObNum);

            policyResult.SetEnforceResult(lResult);

            if (iObNum > 0)
            {
                //get obligation
                GetObligationsFromResult(thePc, lCookie, 0, iObNum, policyResult);
            }

            return 1;
        }
        public int QueryPolicyMulti(PolicyRequestInfo[] szRequestInfo, out PolicyResult[] szPolicyResult)
        {
            int nQueryResult = 0;
            szPolicyResult = null;
            if ((null == szRequestInfo) || (0 == szRequestInfo.Length))
            {
                // No request info, return query success
                nQueryResult = 1;
            }
            else
            {
                szPolicyResult = new PolicyResult[szRequestInfo.Length];
                for (int i = 0; i < szPolicyResult.Length; i += g_knMultilpleQueryLimitedMaxCount)
                {
                    nQueryResult = InnerQueryPolicyMulti(szRequestInfo, i, g_knMultilpleQueryLimitedMaxCount, ref szPolicyResult);
                    if (1 != nQueryResult)
                    {
                        break;
                    }
                }
            }
            return nQueryResult;
        }
        #endregion

        #region Private inner tools
        // szRequestInfo and szPolicyResult both cannot be null and the length must be the same: szRequestInfo.length == szPolicyResult.length
        // [nStartIndex, nStartIndex + nMaxLenght)
        private int InnerQueryPolicyMulti(PolicyRequestInfo[] szRequestInfo, int nStartIndex, int nMaxLenght, ref PolicyResult[] szPolicyResult)
        {
            QueryPC thePc = new QueryPC();
            int lCookie = 0;
            thePc.get_cookie(out lCookie);

            //construct all request
            int nRequestCount = 0;
            int nMaxEndIndex = nStartIndex + nMaxLenght;    // Not include
            for (int i = nStartIndex; (i < szRequestInfo.Length) && (i < nMaxEndIndex); ++i, ++nRequestCount)
            {
                Request pRequest = szRequestInfo[i].ConstructRequest();
                thePc.set_request(pRequest, lCookie);
            }

            //query policy
            int lCEQueryResult = -1;
            try
            {
                if (0 < nRequestCount)
                {
                    thePc.check_resourceex(lCookie, "", 0, 1000 * 60, 1, ref lCEQueryResult);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "thePc.check_resourceex, lQueryResult={0}", lCEQueryResult);

                    //get policy result
                    if (lCEQueryResult == CE_RESULT_SUCCESS)
                    {
                        for (int nResIndex = 0; nResIndex < nRequestCount; ++nResIndex)
                        {
                            int iObNum = 0;
                            int lEnforcementResult = -1;
                            thePc.get_result(lCookie, nResIndex, out lEnforcementResult, out iObNum);

                            szPolicyResult[nResIndex + nStartIndex] = new PolicyResult();
                            PolicyResult policyResult = szPolicyResult[nResIndex + nStartIndex];
                            policyResult.SetEnforceResult(lEnforcementResult);

                            //get obligation
                            if (iObNum > 0)
                            {
                                GetObligationsFromResult(thePc, lCookie, nResIndex, iObNum, policyResult);
                            }
                        }
                    }
                }
                else
                {
                    // No effective request, return CE_RESULT_SUCCESS
                    lCEQueryResult = CE_RESULT_SUCCESS;
                }

            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Exception on thePc.check_resourceex:{0}", ex.ToString());
                lCEQueryResult = CE_RESULT_FAILED;
            }

            return (lCEQueryResult == CE_RESULT_SUCCESS) ? 1 : 0;
        }
        private void GetObligationsFromResult(QueryPC thePC, int iCookie, int nResultIndex, int iObligationCount, PolicyResult policyResult)
        {
            //get obligation
            string[] arryStrObName = { kStrObNameAutoEnforcement, kStrObNameNotification, KStrObNameAutoClassify, KStrObNameManualClassify,
                                       KStrObNameEnableClassifyManager, KStrObNameUserConditoin};
            foreach (string strObName in arryStrObName)
            {
                for (int i = 0; i < iObligationCount; i++)
                {
                    try
                    {
                        Obligation ob = new Obligation();
                        thePC.get_obligation(iCookie, strObName, nResultIndex, i, out ob);
                        if (ob != null)
                        {
                            PolicyObligation policyOb = new PolicyObligation(strObName, ob);
                            policyResult.AddObligation(policyOb);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception on GetObligationsFromResult:" + ex.ToString());
                    }
                }
            }
        }

        private int InnerQueryPolicyMulti_OrgBackup(PolicyRequestInfo[] arrayRequestInfo, out PolicyResult[] arrayPolicyResult)
        {
            arrayPolicyResult = null;

            QueryPC thePc = new QueryPC();
            int lCookie = 0;
            thePc.get_cookie(out lCookie);

            //construct all request
            foreach (PolicyRequestInfo requestInfo in arrayRequestInfo)
            {
                Request pRequest = requestInfo.ConstructRequest();
                thePc.set_request(pRequest, lCookie);
            }

            //query policy
            int lQueryResult = -1;
            try
            {
                thePc.check_resourceex(lCookie, "", 0, 1000 * 60, 1, ref lQueryResult);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "thePc.check_resourceex, lQueryResult={0}", lQueryResult);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "Exception on thePc.check_resourceex:{0}", ex.ToString());
                return 0;
            }

            //get policy result
            if (lQueryResult == 0/*CE_RESULT_SUCCESS*/)
            {
                arrayPolicyResult = new PolicyResult[arrayRequestInfo.Length];
                for (int nResIndex = 0; nResIndex < arrayPolicyResult.Length; nResIndex++)
                {
                    int iObNum = 0;
                    int lEnforcementResult = -1;
                    thePc.get_result(lCookie, nResIndex, out lEnforcementResult, out iObNum);

                    arrayPolicyResult[nResIndex] = new PolicyResult();
                    PolicyResult policyResult = arrayPolicyResult[nResIndex];
                    policyResult.SetEnforceResult(lEnforcementResult);

                    //get obligation
                    if (iObNum > 0)
                    {
                        GetObligationsFromResult(thePc, lCookie, nResIndex, iObNum, policyResult);
                    }
                }
            }

            return (lQueryResult == 0/*CE_RESULT_SUCCESS*/ ? 1 : 0);
        }
        #endregion
    }

}
