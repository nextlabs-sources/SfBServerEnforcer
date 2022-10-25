using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SFBCommon.ClassifyHelper;
using SFBCommon.CommandHelper;
using SFBCommon.Common;
using SFBCommon.NLLog;
using SFBCommon.SFBObjectInfo;

namespace NLChatRobot.ClassifyRobot
{
    public enum EMSFB_CLASSIFYROBOTSTATE
    {
        emState_Unknown,

        emState_Idle,
        // emState_SendClassifyInfo,
        emState_WaitUserClassifySelect
    }

    public class ClassifyFullInfo
    {
        public STUSFB_CLASSIFYCMDINFO m_stuClassifyCmdInfo = null;
        public ManulClassifyObligationHelper m_obManulClassifyObligationHelper = null;
        public ClassifyTagsHelper m_obClassifyTagsInfo = null;

        public ClassifyFullInfo(STUSFB_CLASSIFYCMDINFO stuClassifyCmdInfo, ManulClassifyObligationHelper obManulClassifyObligationHelper, ClassifyTagsHelper obClassifyTagsInfo)
        {
            m_stuClassifyCmdInfo = stuClassifyCmdInfo;
            m_obManulClassifyObligationHelper = obManulClassifyObligationHelper;
            m_obClassifyTagsInfo = obClassifyTagsInfo;
        }
    }

    public class ClassifyChatRobot : ChatRobot
    {
        #region Const/Readonly values
        private const int knCheckInterval = 100;    // ms
        #endregion

        #region Members
        private AutoResetEvent m_eventRobotTalker = new AutoResetEvent(false);
        // <sfb obj sip uri, classify infos>
        private Dictionary<string, ClassifyFullInfo> m_dicClassifyFullInfo = new Dictionary<string,ClassifyFullInfo>();
        
        private bool m_bStart = false;

        private ClassifyRobotLanguage obClassifyRobotLanguage = new ClassifyRobotLanguage();
        private EMSFB_CLASSIFYROBOTSTATE m_emClassifyRobotState = EMSFB_CLASSIFYROBOTSTATE.emState_Unknown;

        private object m_obLockMainClassifyThreadObj = new object();

        private STUSFB_CLASSIFYCMDINFO m_stuActiveClassifyCmdInfo = null;
        private ClassifyFullInfo m_obActiveClassifyFullInfo = null;
        private List<StuManulClassifyOb> m_lsActiveManulClassifyOb = null;
        private StuManulClassifyOb m_obActiveManulClassifyOb = null;
        private ClassifyTagsHelper m_obOriginalClassifyTagHelper = null;
        private Dictionary<string, string> m_dicActiveTags = null;
        private int m_nActiveTree = -1;
        private int m_nAactiveIndex = -1;

        private Thread m_obMainClassifyThread = null;
        #endregion

        #region Constructor
        public ClassifyChatRobot(IChatSpeaker pIChatSpeaker) : base(pIChatSpeaker)
        {
            SetStartFlag(false);
        }
        #endregion

        #region Overwrite function
        override public bool SayHi(string strHiMessage)
        {
            return ChatSpeaker.SendMessage(strHiMessage, false);
        }
        override public bool AutoReply(string strReceivedMessage)
        {
            string strRobotAutoRepyInfo = "";
            bool bUserStopClassify = false;
            bool bFinishedClassify = false;
            EMSFB_CLASSIFYROBOTSTATE emClassifyRobotState = GetRobotState();
            ClassifyIMCmdInfo obClassifyIMCmdInfo = obClassifyRobotLanguage.AnalysisReceivedInfo(strReceivedMessage);
            switch (obClassifyIMCmdInfo.m_emClassifyCmdType)
            {
            case EMSFB_CLASSIFYCMDTYPE.emCmdType_NLTagMeeting:
            {
                if ((EMSFB_CLASSIFYROBOTSTATE.emState_Idle == emClassifyRobotState) || (EMSFB_CLASSIFYROBOTSTATE.emState_Unknown == emClassifyRobotState))
                {
                    bool bSuccess = false;
                    strRobotAutoRepyInfo = GetBeginClassifyRobotReply(obClassifyIMCmdInfo, out bSuccess);
                    if (!bSuccess)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Error, NLTagMeeting Failed\n");
                        strRobotAutoRepyInfo = "";   // If NLTagMeeting prepared failed, no need send failed info to user
                    }
                    emClassifyRobotState = EMSFB_CLASSIFYROBOTSTATE.emState_WaitUserClassifySelect;
                }
                else if (EMSFB_CLASSIFYROBOTSTATE.emState_WaitUserClassifySelect == emClassifyRobotState)
                {
                    if (!obClassifyIMCmdInfo.m_strClassifyIMInput.Equals(GetActiveSFBObjUri(), StringComparison.OrdinalIgnoreCase))
                    {
                        ThreadHelper.AsynchronousTheadPoolInvokeHelper(true, SaveClassifyInfoThread, obClassifyIMCmdInfo);
                    }
                }
                break;
            }
            case EMSFB_CLASSIFYCMDTYPE.emCmdType_UserTagMeeting:
            {
                if ((EMSFB_CLASSIFYROBOTSTATE.emState_Idle == emClassifyRobotState) || (EMSFB_CLASSIFYROBOTSTATE.emState_Unknown == emClassifyRobotState))
                {
                    bool bSuccess = false;
                    strRobotAutoRepyInfo = GetBeginClassifyRobotReply(obClassifyIMCmdInfo, out bSuccess);
                    emClassifyRobotState = EMSFB_CLASSIFYROBOTSTATE.emState_WaitUserClassifySelect;
                }
                else if (EMSFB_CLASSIFYROBOTSTATE.emState_WaitUserClassifySelect == emClassifyRobotState)
                {
                    // Ignore, in one time only support to classify one SFB Object
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "ignore uer input info:[{0}], because the robot is wait user select for another meeting:[{1}]\n", obClassifyIMCmdInfo.m_strClassifyIMInput, GetActiveSFBObjUri());
                }
                break;
            }
            case EMSFB_CLASSIFYCMDTYPE.emCmdType_UserClassifySelect:
            {
                if ((EMSFB_CLASSIFYROBOTSTATE.emState_Idle == emClassifyRobotState) || (EMSFB_CLASSIFYROBOTSTATE.emState_Unknown == emClassifyRobotState))
                {
                    // Send out command prompt message
                    strRobotAutoRepyInfo = ClassifyRobotLanguage.GetUserClassfiyIMCmdPrompt();
                }
                else if (EMSFB_CLASSIFYROBOTSTATE.emState_WaitUserClassifySelect == emClassifyRobotState)
                {
                    strRobotAutoRepyInfo = GetNextClassifyTagsReplyWithUserSelectTags(obClassifyIMCmdInfo, out bUserStopClassify, out bFinishedClassify);
                    emClassifyRobotState = EMSFB_CLASSIFYROBOTSTATE.emState_WaitUserClassifySelect;
                    if (bUserStopClassify || bFinishedClassify)
                    {
                        emClassifyRobotState = EMSFB_CLASSIFYROBOTSTATE.emState_Idle;
                    }
                }
                break;
            }
            case EMSFB_CLASSIFYCMDTYPE.emCmdType_Unknown:
            {
                // Ignore
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "ignore uer input info:[{0}], current active object uri:[{1}]\n", obClassifyIMCmdInfo.m_strClassifyIMInput, GetActiveSFBObjUri());
                break;
            }
            default:
            {
                // Ignore;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "ignore uer input info:[{0}], current active object uri:[{1}]\n", obClassifyIMCmdInfo.m_strClassifyIMInput, GetActiveSFBObjUri());
                break;
            }
            }
            bool bRet = true;
            if (!string.IsNullOrEmpty(strRobotAutoRepyInfo))
            {
                bRet = ChatSpeaker.SendMessage(strRobotAutoRepyInfo, false);
                if (bFinishedClassify)
                {
                    bUserStopClassify = true;
                    SaveCurrentActiveTags();
                }
                if (bUserStopClassify)
                {
                    ResetActiveSFBObjInfo(true);
                }
                if (bRet)
                {
                    if (bUserStopClassify)
                    {
                        // Check if there is another SFB object need classify
                        STUSFB_CLASSIFYCMDINFO obNextClassifyCmdInfo = GetNextClassifyCmdInfo();
                        if (null != obNextClassifyCmdInfo)
                        {
                            if (EMSFB_COMMAND.emCommandClassifyMeeting == obNextClassifyCmdInfo.m_emCommandType)
                            {
                                strRobotAutoRepyInfo = string.Format("You have another meeting need classify, meeting uri:[{0}]\n", obNextClassifyCmdInfo.m_strSFBObjUri);
                            }
                            else /*if (EMSFB_COMMAND.emCommandClassifyChatRoom == obNextClassifyCmdInfo.m_emCommandType)*/
                            {
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "The command type:[{0}] do not support in classify chat robot\n", obNextClassifyCmdInfo.m_emCommandType);
                            }
                            bRet = ChatSpeaker.SendMessage(strRobotAutoRepyInfo, false);
                            string strNLTagMeetingCmdInfo = ClassifyRobotLanguage.GetNLDoMeetingManulClassifyIMCmd(obNextClassifyCmdInfo.m_strSFBObjUri);
                            ChatSpeaker.SetReceivedMessage(strNLTagMeetingCmdInfo);
                        }
                    }
                    SetRobotState(emClassifyRobotState);
                }
                else
                {
                    SetRobotState(EMSFB_CLASSIFYROBOTSTATE.emState_Unknown);
                }
            }
            else
            {
                SetRobotState(EMSFB_CLASSIFYROBOTSTATE.emState_Unknown);
            }
            return bRet;
        }
        override public bool SayEnd(string strEndMessage)
        {
            return ChatSpeaker.SendMessage(strEndMessage, false);
        }
        override public void Start()
        {
            if (!GetStartFlag())
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Start classify chat robot for [{0}]\n", ChatSpeaker.GetRemoteChatSpeakerUri());
                ThreadHelper.AsynchronousTheadPoolInvokeHelper(true, StartWorkThread, null);
                SetStartFlag(true);
            }
        }
        override public void Exit()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exist classify chat robot for [{0}]\n", ChatSpeaker.GetRemoteChatSpeakerUri());
            m_eventRobotTalker.Set();
        }
        #endregion

        #region Public functions
        public void SetClassifyInfo(STUSFB_CLASSIFYCMDINFO stuClassifyCmdInfo, ManulClassifyObligationHelper obManulClassifyObligationHelper, ClassifyTagsHelper obClassifyTagsInfo)
        {
            lock(m_dicClassifyFullInfo)
            {
                CommonHelper.AddKeyValuesToDir(m_dicClassifyFullInfo, stuClassifyCmdInfo.m_strSFBObjUri, new ClassifyFullInfo(stuClassifyCmdInfo, obManulClassifyObligationHelper, obClassifyTagsInfo));
            }
        }
        #endregion

        #region Thread functions
        private void StartWorkThread(object obStartInfo)
        {
            try
            {
                // Note:
                // 1. If invote ChatSpeaker.GetRemoteChatSpeakerUri() to early, it the function maybe return an empty string
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "An classify chat robot start. [{0}]\n", ChatSpeaker.GetRemoteChatSpeakerUri());
                SaveMainClassifyThreadObject();
                SetRobotState(EMSFB_CLASSIFYROBOTSTATE.emState_Idle);
                while (true)
                {
                    try
                    {
                        bool bReceivedSignal = m_eventRobotTalker.WaitOne(knCheckInterval);
                        if (bReceivedSignal)
                        {
                            // Received event, exit loop to end thread
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Received robot exit event, exit classify chat robot for [{0}]\n", ChatSpeaker.GetRemoteChatSpeakerUri());
                            break;
                        }

                        // Do manul classify
                        string strReceivedMessage = ChatSpeaker.GetReveivedMessage();
                        if (!string.IsNullOrEmpty(strReceivedMessage))
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Received user reply info:[{0}]\n", strReceivedMessage);
                            AutoReply(strReceivedMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in chat robot StartWorkThread loop\n {0}\n", ex.Message);
                    }
                }
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "End classify chat robot for [{0}]\n", ChatSpeaker.GetRemoteChatSpeakerUri());
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in chat robot StartWorkThread\n {0}\n", ex.Message);
            }
        }
        private void SaveClassifyInfoThread(object obParamClassifyIMCmdInfo)
        {
            try
            {
                ClassifyIMCmdInfo obClassifyIMCmdInfo = obParamClassifyIMCmdInfo as ClassifyIMCmdInfo;
                if (null != obClassifyIMCmdInfo)
                {
                    STUSFB_CLASSIFYCMDINFO stuClassifyCmdInfo = PrepareClassifyCmdInfo(obClassifyIMCmdInfo);
                    if (ClassifyCommandHelper.IsClassifyManager(stuClassifyCmdInfo))
                    {
                        ManulClassifyObligationHelper obManulClassifyObligationHelper = ClassifyCommandHelper.GetClassifyObligationInfo(stuClassifyCmdInfo);
                        ClassifyTagsHelper obClassifyTagsInfo = ClassifyCommandHelper.GetClassifyTagsInfo(stuClassifyCmdInfo);
                        SetClassifyInfo(stuClassifyCmdInfo, obManulClassifyObligationHelper, obClassifyTagsInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!!Exception in save classify info thread, [{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Auto replay logic functions
        private string GetBeginClassifyRobotReply(ClassifyIMCmdInfo obClassifyIMCmdInfo, out bool bSuccess)
        {
            bSuccess = false;
            string strRobotAutoRepyInfo = "";
            string strClassifyReplyInfo = "";
            bool bIsManager = false;
            STUSFB_CLASSIFYCMDINFO stuClassifyCmdInfo = PrepareAndSaveClassifyCmdInfo(obClassifyIMCmdInfo, out bIsManager);
            if (bIsManager)
            {
                if (null != stuClassifyCmdInfo)
                {
                    if (string.IsNullOrEmpty(stuClassifyCmdInfo.m_strSFBObjUri))
                    {
                        strRobotAutoRepyInfo = ClassifyRobotLanguage.GetUserClassfiyIMCmdPrompt();
                    }
                    else
                    {
                        bool bInitActiveInfo = InitActiveSFBObjInfo(stuClassifyCmdInfo);
                        StuManulClassifyItem obManulClassifyItem = null;
                        if ((null != m_lsActiveManulClassifyOb) && (0 < m_lsActiveManulClassifyOb.Count))
                        {
                            m_nActiveTree = 0;
                            for (int nTree = m_nActiveTree; nTree < m_lsActiveManulClassifyOb.Count; ++nTree)
                            {
                                m_nActiveTree = nTree;
                                m_nAactiveIndex = 0;
                                m_obActiveManulClassifyOb = m_lsActiveManulClassifyOb[m_nActiveTree];
                                if (null == m_obActiveManulClassifyOb)
                                {
                                    // Find in next tree
                                }
                                else
                                {
                                    obManulClassifyItem = m_obActiveManulClassifyOb.ManulClassifyItem;
                                    if (IsEffectManulClassifyObDefine(obManulClassifyItem))
                                    {
                                        strClassifyReplyInfo = GetRobotClassifyReplyInfo(obManulClassifyItem);
                                        break;
                                    }
                                    else
                                    {
                                        // Find in next tree
                                    }
                                }
                            }
                        }
                    }
                }
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current active tree:[{0}], classifyReplyInfo:[{1}]\n", m_nActiveTree, strClassifyReplyInfo);
                if (string.IsNullOrEmpty(strClassifyReplyInfo))
                {
                    strRobotAutoRepyInfo = string.Format("Do find classify info according your input\n");
                }
                else
                {
                    strRobotAutoRepyInfo = string.Format("Now let's classify the meeting:[{0}]\n{1}", stuClassifyCmdInfo.m_strSFBObjUri, strClassifyReplyInfo); // Meeting header prompt message
                    bSuccess = true;
                }
            }
            else
            {
                strRobotAutoRepyInfo = string.Format("You are not the meeting manager");
            }
            return strRobotAutoRepyInfo;
        }
        private string GetNextClassifyTagsReplyWithUserSelectTags(ClassifyIMCmdInfo obClassifyIMCmdInfo, out bool bUserStopClassify, out bool bFinishedClassify)
        {
            string strRobotAutoRepyInfo = "";
            bUserStopClassify = false;
            bFinishedClassify = false;
            bool bSelectRight = false;
            try
            {
                // Analysis current select info
                if ((null == m_obActiveManulClassifyOb) || (null == m_obActiveManulClassifyOb.ManulClassifyItem))
                {
                    // Unknown error, end classify
                    strRobotAutoRepyInfo = string.Format("Unknown error happened\n");
                    bUserStopClassify = true;
                }
                else
                {
                    string strUserSelect = obClassifyIMCmdInfo.m_strClassifyIMInput;
                    int nUserSelect = int.Parse(strUserSelect);
                    StuManulClassifyItem obManulClassifyItem = m_obActiveManulClassifyOb.ManulClassifyItem;
                    if ((0 <= nUserSelect) && (obManulClassifyItem.Values.Count >= nUserSelect))
                    {
                        if (0 == nUserSelect)
                        {
                            // Stop
                            bUserStopClassify = true;
                            strRobotAutoRepyInfo = string.Format("Stop to exit\n");
                        }
                        else
                        {
                            if (null == m_dicActiveTags)
                            {
                                m_dicActiveTags = new Dictionary<string,string>();
                            }
                            CommonHelper.AddKeyValuesToDir(m_dicActiveTags, obManulClassifyItem.Name, obManulClassifyItem.Values[nUserSelect - 1]);
                            bSelectRight = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception, maybe user do not input right select index, [{0}]\n", ex.Message);
            }
            if (bSelectRight)
            {
                // Get next effect item
                strRobotAutoRepyInfo = GetNextEffiveRobotReply(out bFinishedClassify, out m_obActiveManulClassifyOb);
            }
            else
            {
                // Repeat
                strRobotAutoRepyInfo = GetRobotClassifyReplyInfo(m_obActiveManulClassifyOb.ManulClassifyItem);
            }
            return strRobotAutoRepyInfo;
        }
        private string GetNextEffiveRobotReply(out bool bFinishedClassify, out StuManulClassifyOb obNextEffectiveManulClassifyOb)
        {
            bFinishedClassify = true;
            string strRobotAutoRepyInfo = string.Format("Classify finished\n");
            while (true)
            {
                obNextEffectiveManulClassifyOb = GetNextEffectiveOb();
                if ((null == obNextEffectiveManulClassifyOb) || (null == obNextEffectiveManulClassifyOb.ManulClassifyItem))
                {
                    // End
                    bFinishedClassify = true;
                    break;
                }
                else
                {
                    StuManulClassifyItem obManulClassifyItem = obNextEffectiveManulClassifyOb.ManulClassifyItem;
                    if (IsEffectManulClassifyObDefine(obManulClassifyItem))
                    {
                        strRobotAutoRepyInfo = GetRobotClassifyReplyInfo(obManulClassifyItem);
                        bFinishedClassify = false;
                        break;
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Continue find, the manul classify obligation defined maybe have some error\n");
                    }
                }
            }
            return strRobotAutoRepyInfo;
        }
        #endregion

        #region Inner tools
        private StuManulClassifyOb GetNextEffectiveOb()
        {
            StuManulClassifyOb obNextEffectiveManulClassifyOb = m_lsActiveManulClassifyOb[m_nActiveTree].GetNextEffectiveNodeByIndex(m_nAactiveIndex, m_dicActiveTags);
            if ((null == obNextEffectiveManulClassifyOb) || (null == obNextEffectiveManulClassifyOb.ManulClassifyItem))
            {
                m_nAactiveIndex = 0;
                obNextEffectiveManulClassifyOb = GetNextActiveTree();
            }
            else
            {
                if ((null == obNextEffectiveManulClassifyOb) || (null == obNextEffectiveManulClassifyOb.ManulClassifyItem))
                {
                    return null;
                }
                else
                {
                    m_nAactiveIndex = obNextEffectiveManulClassifyOb.ManulClassifyItem.Index;
                }
            }
            return obNextEffectiveManulClassifyOb;
        }
        private StuManulClassifyOb GetNextActiveTree()
        {
            ++m_nActiveTree;
            if ((0 <= m_nActiveTree) && (m_lsActiveManulClassifyOb.Count > m_nActiveTree))
            {
                return m_lsActiveManulClassifyOb[m_nActiveTree];
            }
            return null;
        }
        private bool IsEffectManulClassifyObDefine(StuManulClassifyItem obManulClassifyItem)
        {
            bool bIsEffective = false;
            if ((null != obManulClassifyItem) && (!string.IsNullOrEmpty(obManulClassifyItem.Name)))
            {
                if ((0 >= obManulClassifyItem.Values.Count) && (!obManulClassifyItem.Editable))
                {
                    bIsEffective = false;
                }
                else
                {
                    bIsEffective = true;
                }
            }
            return bIsEffective;
        }
        private string GetRobotClassifyReplyInfo(StuManulClassifyItem obManulClassifyItem)
        {
            string strRobotAutoRepyInfo = "";
            if ((null != obManulClassifyItem) && (!string.IsNullOrEmpty(obManulClassifyItem.Name)))
            {
                if ((0 < obManulClassifyItem.Values.Count) || (obManulClassifyItem.Editable))
                {
                    strRobotAutoRepyInfo += string.Format("Please reply the value index to select the value for tag:[{0}]\n", obManulClassifyItem.Name);
                    int iValue = 0;
                    for (iValue = 0; iValue < obManulClassifyItem.Values.Count; ++iValue)
                    {
                        strRobotAutoRepyInfo += string.Format("{0}:[{1}]\n", iValue + 1, obManulClassifyItem.Values[iValue]);
                    }
                    if (obManulClassifyItem.Editable)
                    {
                        strRobotAutoRepyInfo += string.Format("{0}:[{1}]\n", iValue + 1, "[select this item to custom your tag value]");
                    }
                    strRobotAutoRepyInfo += string.Format("{0}:[{1}]\n", 0, "stop and exit classify");
                }
            }
            return strRobotAutoRepyInfo;  // Return next reply
        }

        private STUSFB_CLASSIFYCMDINFO PrepareClassifyCmdInfo(ClassifyIMCmdInfo obClassifyIMCmdInfo)
        {
            STUSFB_CLASSIFYCMDINFO obClassifyCmdInfo = new STUSFB_CLASSIFYCMDINFO(EMSFB_COMMAND.emCommandUnknown, ChatSpeaker.GetRemoteChatSpeakerUri(), "");
            if (EMSFB_CLASSIFYCMDTYPE.emCmdType_NLTagMeeting == obClassifyIMCmdInfo.m_emClassifyCmdType)
            {
                obClassifyCmdInfo.m_emCommandType = EMSFB_COMMAND.emCommandClassifyMeeting;
                obClassifyCmdInfo.m_strSFBObjUri = obClassifyIMCmdInfo.m_strClassifyIMInput;
            }
            else if (EMSFB_CLASSIFYCMDTYPE.emCmdType_UserTagMeeting == obClassifyIMCmdInfo.m_emClassifyCmdType)
            {
                obClassifyCmdInfo.m_emCommandType = EMSFB_COMMAND.emCommandClassifyMeeting;
                MeetingEntryInfo obMeetingEntryInfo = new MeetingEntryInfo(obClassifyIMCmdInfo.m_strClassifyIMInput);
                string strMeetingLikeUri = obMeetingEntryInfo.GetMeetingLikeUri();

                List<SFBObjectInfo> lsSFBMeetingInfo = SFBObjectInfo.GetObjsFromPersistentInfoByLikeValue(EMSFB_INFOTYPE.emInfoNLMeeting, SFBMeetingInfo.kstrUriFieldName, strMeetingLikeUri);
                if (0 < lsSFBMeetingInfo.Count)
                {
                    obClassifyCmdInfo.m_strSFBObjUri = lsSFBMeetingInfo[0].GetItemValue(SFBMeetingInfo.kstrUriFieldName);

                    if (1 < lsSFBMeetingInfo.Count)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!!An special case happened which the meeting ID is same or user input a wrong URI but cannot checked\n");
                    }
                }
            }
            return obClassifyCmdInfo;
        }
        private STUSFB_CLASSIFYCMDINFO PrepareAndSaveClassifyCmdInfo(ClassifyIMCmdInfo obClassifyIMCmdInfo, out bool bIsManager)
        {
            bIsManager = true;
            STUSFB_CLASSIFYCMDINFO stuClassifyCmdInfo = PrepareClassifyCmdInfo(obClassifyIMCmdInfo);
            if ((null != stuClassifyCmdInfo) && (EMSFB_COMMAND.emCommandUnknown != stuClassifyCmdInfo.m_emCommandType) && (!string.IsNullOrEmpty(stuClassifyCmdInfo.m_strSFBObjUri)))
            {
                ClassifyFullInfo obClassifyFullInfo = GetClassifyInfo(stuClassifyCmdInfo.m_strSFBObjUri);
                if (null == obClassifyFullInfo)
                {
                    bIsManager = ClassifyCommandHelper.IsClassifyManager(stuClassifyCmdInfo);
                    if (bIsManager)
                    {
                        ManulClassifyObligationHelper obManulClassifyObligationHelper = ClassifyCommandHelper.GetClassifyObligationInfo(stuClassifyCmdInfo);
                        ClassifyTagsHelper obClassifyTagsInfo = ClassifyCommandHelper.GetClassifyTagsInfo(stuClassifyCmdInfo);
                        SetClassifyInfo(stuClassifyCmdInfo, obManulClassifyObligationHelper, obClassifyTagsInfo);
                    }
                }
            }
            return stuClassifyCmdInfo;
        }
        private EMSFB_CLASSIFYROBOTSTATE GetRobotState() { return m_emClassifyRobotState; }
        private void SetRobotState(EMSFB_CLASSIFYROBOTSTATE emClassifyRobotState) { m_emClassifyRobotState = emClassifyRobotState; }
        private bool GetStartFlag() { return m_bStart; }
        private void SetStartFlag(bool bStart) { m_bStart = bStart; }
        private ClassifyFullInfo GetClassifyInfo(string strSFBObjUri)
        {
            lock (m_dicClassifyFullInfo)
            {
                return CommonHelper.GetValueByKeyFromDir(m_dicClassifyFullInfo, strSFBObjUri, null);
            }
        }
        private void DeleteClassifyInfo(string strSFBObjUri)
        {
            lock (m_dicClassifyFullInfo)
            {
                CommonHelper.RemoveKeyValuesFromDir(m_dicClassifyFullInfo, strSFBObjUri);
            }
        }
        private bool InitActiveSFBObjInfo(STUSFB_CLASSIFYCMDINFO stuActiveClassifyCmdInfo)
        {
            try
            {
                if ((null != stuActiveClassifyCmdInfo) && (!string.IsNullOrEmpty(stuActiveClassifyCmdInfo.m_strSFBObjUri)) && IsMainClassifyThread())    // Only can set active object info in main classify thread
                {
                    m_obActiveClassifyFullInfo = GetClassifyInfo(stuActiveClassifyCmdInfo.m_strSFBObjUri);
                    if (null != m_obActiveClassifyFullInfo)
                    {
                        if (null != m_obActiveClassifyFullInfo.m_obManulClassifyObligationHelper)
                        {
                            m_lsActiveManulClassifyOb = m_obActiveClassifyFullInfo.m_obManulClassifyObligationHelper.GetStuManulClassifyObs();
                        }
                        if (null != m_obActiveClassifyFullInfo.m_obClassifyTagsInfo)
                        {
                            m_obOriginalClassifyTagHelper = m_obActiveClassifyFullInfo.m_obClassifyTagsInfo;
                        }
                        m_nActiveTree = 0;
                        m_nAactiveIndex = 0;
                        m_stuActiveClassifyCmdInfo = stuActiveClassifyCmdInfo;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ResetActiveSFBObjInfo(false);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in SetCurActionSFBObjUri, [{0}]\n", ex.Message);
            }
            return false;
        }
        private bool SaveCurrentActiveTags()
        {
            if ((null != m_dicActiveTags) && (null != m_stuActiveClassifyCmdInfo))
            {
                if (EMSFB_COMMAND.emCommandClassifyMeeting == m_stuActiveClassifyCmdInfo.m_emCommandType)
                {
                    SFBMeetingVariableInfo obSFBMeetingVariableInfo = new SFBMeetingVariableInfo(SFBMeetingVariableInfo.kstrUriFieldName, m_stuActiveClassifyCmdInfo.m_strSFBObjUri, SFBMeetingVariableInfo.kstrDoneManulClassifyFieldName, SFBMeetingVariableInfo.kstrDoneManulClassifyYes);
                    obSFBMeetingVariableInfo.SetDictionaryTags(m_dicActiveTags);
                    return obSFBMeetingVariableInfo.PersistantSave();
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current command type:[{0}] do not support in classify chat robot now\n", m_stuActiveClassifyCmdInfo.m_emCommandType);
                }
            }
            return true;
        }
        private void ResetActiveSFBObjInfo(bool bRemoveSavedClassifyInfo)
        {
            if (bRemoveSavedClassifyInfo && (null != m_stuActiveClassifyCmdInfo))
            {
                DeleteClassifyInfo(m_stuActiveClassifyCmdInfo.m_strSFBObjUri);
            }
            m_stuActiveClassifyCmdInfo = null;
            m_obActiveClassifyFullInfo = null;
            m_lsActiveManulClassifyOb = null;
            m_obActiveManulClassifyOb = null;
            m_obOriginalClassifyTagHelper = null;
            m_dicActiveTags = null;
            m_nActiveTree = -1;
            m_nAactiveIndex = -1;
        }
        private STUSFB_CLASSIFYCMDINFO GetNextClassifyCmdInfo()
        {
            if ((null != m_dicClassifyFullInfo) && (0 < m_dicClassifyFullInfo.Keys.Count))
            {
                foreach (KeyValuePair<string, ClassifyFullInfo> pairClassifyFullInfo in m_dicClassifyFullInfo)
                {
                    return pairClassifyFullInfo.Value.m_stuClassifyCmdInfo;
                }
            }
            return null;
        }
        private string GetActiveSFBObjUri()
        {
            return m_stuActiveClassifyCmdInfo.m_strSFBObjUri;
        }
        private void SaveMainClassifyThreadObject()
        {
            lock (m_obLockMainClassifyThreadObj)
            {
                m_obMainClassifyThread = Thread.CurrentThread;
            }
        }
        private bool IsMainClassifyThread()
        {
            lock (m_obLockMainClassifyThreadObj)
            {
                if ((null != m_obMainClassifyThread) && (m_obMainClassifyThread.IsAlive))
                {
                    return (m_obMainClassifyThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId);
                }
            }
            return false;
        }
        #endregion
    }
}
