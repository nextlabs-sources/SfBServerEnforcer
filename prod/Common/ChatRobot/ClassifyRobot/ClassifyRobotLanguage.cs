using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon.NLLog;
using SFBCommon.Common;

namespace NLChatRobot.ClassifyRobot
{
    public enum EMSFB_CLASSIFYCMDTYPE
    {
        emCmdType_Unknown,

        emCmdType_UserTagMeeting,
        emCmdType_NLTagMeeting,
        emCmdType_UserClassifySelect
    }

    public class ClassifyIMCmdInfo
    {
        #region Members
        public EMSFB_CLASSIFYCMDTYPE m_emClassifyCmdType = EMSFB_CLASSIFYCMDTYPE.emCmdType_Unknown;
        public string m_strClassifyIMInput = "";
        #endregion

        #region Constructor
        public ClassifyIMCmdInfo(EMSFB_CLASSIFYCMDTYPE emClassifyCmdType, string strClassifyIMInput)
        {
            m_emClassifyCmdType = emClassifyCmdType;
            m_strClassifyIMInput = strClassifyIMInput;
        }
        #endregion
    }

    public class ClassifyRobotLanguage
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(ClassifyRobotLanguage));
        #endregion

        #region Const/Readonly values
        private const string kstrMeetingFixFlag = ";gruu;opaque=app:conf:focus:id:";

        private const string kstrSepCmdkeyAndValue = " ";
        private const string kstrCmdUserTagMeetingKey = "TagMeeting";
        private const string kstrCmdNLTagMeetingKey = "NLTagMeeting";
        #endregion

        #region Static functions
        static public string GetNLDoMeetingManulClassifyIMCmd(string strMeetingSipUri)
        {
            // TagMeeting sip:john.tyler@lync.nextlabs.solutions;gruu;opaque=app:conf:focus:id:FHWMF12B
            // TagMeeting FHWMF12B
            return kstrCmdNLTagMeetingKey + kstrSepCmdkeyAndValue + strMeetingSipUri;
        }
        static public string GetUserClassfiyIMCmdPrompt()
        {
            return string.Format("You can type in \"{0}{1}{2}\" to classify a meeting", kstrCmdUserTagMeetingKey, kstrSepCmdkeyAndValue, "Meeting entry info");
        }
        #endregion

        #region Members
        public ClassifyRobotLanguage() { }
        #endregion

        #region Constructor
        
        #endregion

        #region Public functions
        public ClassifyIMCmdInfo AnalysisReceivedInfo(string strReceivedMessage)
        {
            try
            {
                strReceivedMessage = TrimReceivedInfo(strReceivedMessage);
                string[] szStrReceiveInfo = strReceivedMessage.Split(new string[] { kstrSepCmdkeyAndValue }, StringSplitOptions.RemoveEmptyEntries);
                {
                    for (int i=0; i<szStrReceiveInfo.Length; ++i)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Received:[{0}:[{1}]]\n", i, szStrReceiveInfo[i]);
                    }
                }
                
                int knLength = szStrReceiveInfo.Length;
                if ((null != szStrReceiveInfo) && (0 < knLength))
                {
                    EMSFB_CLASSIFYCMDTYPE emClassifyCmdType = GetClassifyCmdType(szStrReceiveInfo[0]);
                    string strClassifyCmdInfo = "";
                    if ((EMSFB_CLASSIFYCMDTYPE.emCmdType_NLTagMeeting == emClassifyCmdType) || (EMSFB_CLASSIFYCMDTYPE.emCmdType_UserTagMeeting == emClassifyCmdType))
                    {
                        strClassifyCmdInfo = CommonHelper.GetArrayValueByIndex(szStrReceiveInfo, 1, "");
                    }
                    else if (EMSFB_CLASSIFYCMDTYPE.emCmdType_UserClassifySelect == emClassifyCmdType)
                    {
                        strClassifyCmdInfo = szStrReceiveInfo[0];
                    }
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "AnalysisReceivedInfo, Original:[{0}], Result:[type:[{1}]], [info:[{2}]]\n", strReceivedMessage, emClassifyCmdType, strClassifyCmdInfo);
                    return new ClassifyIMCmdInfo(emClassifyCmdType, strClassifyCmdInfo);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in ClassifyRobotLanguage, maybe received illegality reply info, [{0}]\n", ex.Message);
            }
            return new ClassifyIMCmdInfo(EMSFB_CLASSIFYCMDTYPE.emCmdType_Unknown, "");
        }
        #endregion

        #region Inner tools
        private string TrimReceivedInfo(string strReceivedInfo)
        {
            return strReceivedInfo.Trim();
        }
        private EMSFB_CLASSIFYCMDTYPE GetClassifyCmdType(string strCmdString)
        {
            if (!string.IsNullOrEmpty(strCmdString))
            {
                if (kstrCmdNLTagMeetingKey.Equals(strCmdString, StringComparison.OrdinalIgnoreCase))
                {
                    return EMSFB_CLASSIFYCMDTYPE.emCmdType_NLTagMeeting;
                }
                else if (kstrCmdUserTagMeetingKey.Equals(strCmdString, StringComparison.OrdinalIgnoreCase))
                {
                    return EMSFB_CLASSIFYCMDTYPE.emCmdType_UserTagMeeting;
                }
                else
                {
                    return EMSFB_CLASSIFYCMDTYPE.emCmdType_UserClassifySelect;
                }
            }
            return EMSFB_CLASSIFYCMDTYPE.emCmdType_UserClassifySelect;
        }
        #endregion
    }
}
