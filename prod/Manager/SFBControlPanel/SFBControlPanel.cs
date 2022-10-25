using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

using SFBCommon.Common;
using SFBCommon.SFBObjectInfo;
using SFBCommon.NLLog;
using SFBCommon.ClassifyHelper;
using System.IO;

namespace SFBControlPanel
{
    using TypeNodeTags = Dictionary<string, object>;

    enum EMSFB_TREETYPE
    {
        emTreeTypeUnknown,

        emTreeTypeChatCategories
    }
    enum EMSFB_TABPAGETYPE
    {
        emTabMessagePage,
        emTabChatCategorySettingPage
    }
    enum EMSFB_RUNTIMESTATUS
    {
        emStartLoading,
        emLoadComplete,
        emClassificationSchemaWarning,

        emSuccess,  // Default, above is success status, below is error status

        emUnknownError,
        emDBConnectionError,
        emReadDefaultValueError,
        emSaveDataError,
        emNoPermission
    }
    enum EMSFB_FORMCLOSEREASON
    {
        emUnknown,

        emCloseByOkBtn,
        emCloseByTopRightCloseBtn
    }
    enum EMSFB_RECODESTATUS
    {
        emSuccess,
        emUnknow,
        emCannotGetCurrectNode,
        emCurrectNodeError,
        emCannotGetChatCategorySetting,
        emCannotGetSFBInfo,
        emCannotGetUri
    }
    public partial class SFBControlPanel : Form
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(SFBControlPanel));
        #endregion

        #region Const/Read only values
        static private readonly EMSFB_TREETYPE[] kszTreeTypes = new EMSFB_TREETYPE[]
        {
            EMSFB_TREETYPE.emTreeTypeChatCategories
        };

        // Flag for tree node extra info
        private const string kstrExpandedTagKey = "Expanded";
        private const string kstrSFBChatCategoryUri = "SFBChatCategoryUri";
        private const string kstrUserSettingTagKey = "UserSetting";
        private const string kstrSFBChatCategoryInfo = "SFBChatCategoryInfo";
        #endregion

        #region Prompt message
        static private STUSFB_PROMPTMSG s_stuPormptMsg = GetPromptMsg();
        static private STUSFB_PROMPTMSG GetPromptMsg()
        {
            ConfigureFileManager obCfgFileMgr = new ConfigureFileManager(EMSFB_MODULE.emSFBModule_SFBControlPanel);
            STUSFB_PROMPTMSG stuPormptMsg = obCfgFileMgr.GetPromptMsg(EMSFB_CFGINFOTYPE.emCfgInfoSFBCtlPanel);
            return (null == stuPormptMsg) ? (new STUSFB_PROMPTMSG()) : stuPormptMsg;
        }
        private const string kstrTrue = "true";
        private const string kstrUnknownErrorMessage = "Unknown error";

        static public readonly Dictionary<long, string> s_dirDisplayMsgInfo = new Dictionary<long,string>()
        {
            // Runtime success info
            {(long)EMSFB_RUNTIMESTATUS.emStartLoading, CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirRuntimeInfo, ConfigureFileManager.kstrXMLStartLoadingFlag, "Loading chat room categories information.")},
            {(long)EMSFB_RUNTIMESTATUS.emLoadComplete, CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirRuntimeInfo, ConfigureFileManager.kstrXMLEndLoadingFlag, "Chat room category information is loaded.")},
            {(long)EMSFB_RUNTIMESTATUS.emClassificationSchemaWarning, CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirRuntimeInfo, ConfigureFileManager.kstrXMLClassficationWarningInfoFlag, "Old classification schema name is \\ORGCLASSIFICATIONSCHEMANAME;, but this is not exist or change in classification schema define table. You can ask for your system admin for help.")},
            // End runtime info

            // Runtime error
            {(long)EMSFB_RUNTIMESTATUS.emSuccess, "Success"},
            {(long)EMSFB_RUNTIMESTATUS.emUnknownError, CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirErrorMsg, ConfigureFileManager.kstrXMLUnknownErrorFlag, new STUSFB_ERRORMSG("Unknown error. Please try again. If the problem persists, please contact your system administrator.", 0)).m_strErrorMsg},
            {(long)EMSFB_RUNTIMESTATUS.emReadDefaultValueError, CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirErrorMsg, ConfigureFileManager.kstrXMLReadPersistentValueErrorFlag, new STUSFB_ERRORMSG("Failed to read chat room category's default value. Please try again. If the problem persists, please contact your system administrator.", 0)).m_strErrorMsg},
            {(long)EMSFB_RUNTIMESTATUS.emSaveDataError, CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirErrorMsg, ConfigureFileManager.kstrXMLPersistentSaveErrorFlag, new STUSFB_ERRORMSG("Failed to save your chat room category's configuration. Please try again. If the problem persists, please contact your system administrator.", 0)).m_strErrorMsg},
            {(long)EMSFB_RUNTIMESTATUS.emNoPermission, CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirErrorMsg, ConfigureFileManager.kstrXMLNoPermissionFlag, new STUSFB_ERRORMSG("You have no RTC Universal Server Admins permission. Please contact your system administrator.", 0)).m_strErrorMsg},
            {(long)EMSFB_RUNTIMESTATUS.emDBConnectionError, CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirErrorMsg, ConfigureFileManager.kstrXMLDBConectionErrorFlag, new STUSFB_ERRORMSG("Connection to database failed", 0)).m_strErrorMsg},
            // End runtime error
        };
        #endregion

        #region Windows message
        private const int WM_USER = 0x0500;
        private const int WM_LOADEDSFBINFO  = WM_USER + 1;
        private const int WM_LOADINGSFBINFO = WM_USER + 2;
        #endregion

        #region Members
        private int m_nDefaultClassificationSchemaIndex = 0;
        private Dictionary<string, NLClassificationSchemaInfo> m_dicClassificationSchemas = new Dictionary<string,NLClassificationSchemaInfo>();
        private Dictionary<EMSFB_TABPAGETYPE, TabPage> m_dicTabPages = new Dictionary<EMSFB_TABPAGETYPE,TabPage>(); // Table pages
        private List<SFBChatCategoryInfo> m_lsSFBChatCategoryInfo = new List<SFBChatCategoryInfo>();
        private readonly bool m_bSupportForceEnforcerOption = CommonHelper.ConverStringToBoolFlag(CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirRuntimeInfo, ConfigureFileManager.kstrXMLSupportForceEnforcerOptionFlag, "false"), false);
        private readonly string m_strFormTitle = CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirRuntimeInfo, ConfigureFileManager.kstrXMLFormTitleFlag, "NextLabs Entitlement Management Control Panel for Skype for Business");
        private EMSFB_FORMCLOSEREASON m_emFormCloseReason = EMSFB_FORMCLOSEREASON.emUnknown;
        #endregion

        #region Constructors
        public SFBControlPanel()
        {
            InitializeComponent();
            InitTablePages();
            InitComboxForClassificationSchemas();

            this.Text = m_strFormTitle; // Set form title
            ThreadHelper.AsynchronousInvokeHelper(true, InitSFBInfo, this.Handle);
        }
        #endregion

        #region Init
        private void InitComboxForClassificationSchemas()
        {
            comboxClassificationSchemas.Items.Clear();
            comboxClassificationSchemas.Items.Add(" ");  // Add an empty item, if user do not want to add classification for current chat category he can select this empty item.
            SetDefaultClassificationSelectedIndex(0);

            List<SFBObjectInfo> lsClassificationSchemas = SFBObjectInfo.GetAllObjsFrommPersistentInfo(EMSFB_INFOTYPE.emInfoNLClassificationSchema);   // Init all schema info
            if (null != lsClassificationSchemas)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "A Classification schemas lenght:[{0}]\n", lsClassificationSchemas.Count);
                string strClassificationSchemaName = "";
                foreach (SFBObjectInfo obClassificationSchema in lsClassificationSchemas)
                {
                    strClassificationSchemaName = obClassificationSchema.GetItemValue(NLClassificationSchemaInfo.kstrSchemaNameFieldName).ToLower();
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Classification schema name:[{0}]\n", strClassificationSchemaName);
                    if (!string.IsNullOrWhiteSpace(strClassificationSchemaName))
                    {
                        CommonHelper.AddKeyValuesToDir(m_dicClassificationSchemas, strClassificationSchemaName, obClassificationSchema as NLClassificationSchemaInfo);
                        comboxClassificationSchemas.Items.Add(strClassificationSchemaName);
                    }
                }
            }
            comboxClassificationSchemas.SelectedIndex = GetDefaultClassificationSelectedIndex();
        }
        private void InitTablePages()
        {
            txtEnforcerExplain.Text = CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirRuntimeInfo, ConfigureFileManager.kstrXMLEnforcerExplainFlag, txtEnforcerExplain.Text);
            txtForceEnforcerExplain.Text = CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirRuntimeInfo, ConfigureFileManager.kstrXMLForceEnforcerExplainFlag, txtForceEnforcerExplain.Text);
            groupClassficationArea.Text = CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirRuntimeInfo, ConfigureFileManager.KstrXMLClassficationAreaTitleFlag, groupClassficationArea.Text);
            if (!m_bSupportForceEnforcerOption)
            {
                groupClassficationArea.Location = groupForceEnforcerArea.Location;
            }
            if (null == m_dicTabPages)
            {
                m_dicTabPages = new Dictionary<EMSFB_TABPAGETYPE,TabPage>();
            }
            m_dicTabPages.Add(EMSFB_TABPAGETYPE.emTabMessagePage, pgMessagePage);
            m_dicTabPages.Add(EMSFB_TABPAGETYPE.emTabChatCategorySettingPage, pgCategorySetting);
        }
        private void InitSFBInfo(object obHandle)
        {
            IntPtr hCurHandle = (IntPtr)obHandle;
            EMSFB_RUNTIMESTATUS emRunTimeError = EMSFB_RUNTIMESTATUS.emSuccess;
            try
            {
                ExternApi.SendMessage(hCurHandle, WM_LOADINGSFBINFO, new IntPtr(0), new IntPtr((long)EMSFB_RUNTIMESTATUS.emSuccess));
                List<SFBChatCategoryInfo> lsSFBChatCategoryInfo = null;
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_SUCCESS);
                lsSFBChatCategoryInfo = SFBChatCategoryInfo.GetAllCurChatCategoryInfo();   // Using power shell to get all chat Category info, need take a few seconds
                if ((null == lsSFBChatCategoryInfo) || (0 == lsSFBChatCategoryInfo.Count))
                {
                    int nLastError = LastErrorRecorder.GetLastError();
                    if (LastErrorRecorder.ERROR_SUCCESS != nLastError)
                    {
                        if (LastErrorRecorder.ERROR_ACCESS_DENY == nLastError)
                        {
                            emRunTimeError = EMSFB_RUNTIMESTATUS.emNoPermission;
                        }
                        else
                        {
                            emRunTimeError = EMSFB_RUNTIMESTATUS.emUnknownError;
                        }
                    }
                }
                if (EMSFB_RUNTIMESTATUS.emSuccess == emRunTimeError)
                {
                    InitSFBPersistentChatServerInfo();
                }
                SetSFBChatCategorysInfo(lsSFBChatCategoryInfo);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! Get SFB Object info exception,{0}\n", ex.Message);
            }
            finally
            {
                ExternApi.SendMessage(hCurHandle, WM_LOADEDSFBINFO, new IntPtr((long)emRunTimeError), new IntPtr(0));
            }
        }
        #endregion

        #region UI events: tree view events
        private void trSFBESettings_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode ndCurNode = e.Node;
            if (null == ndCurNode)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! Unknown error in tree after select, current node is null");
            }
            else
            {
                EMSFB_TREETYPE emTreeType = GetNodeTreeType(ndCurNode);
                switch (emTreeType)
                {
                case EMSFB_TREETYPE.emTreeTypeChatCategories:
                {
                    ChatCategoryTree_AfterSelect(ndCurNode);
                    break;
                }
                default:
                {
                    break;
                }
                }
            }
        }
        private void trSFBESettings_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode ndCurNode = e.Node;
            if (null == ndCurNode)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! Unknown error in tree before expand, current node is null");
            }
            else
            {
                EMSFB_TREETYPE emTreeType = GetNodeTreeType(ndCurNode);
                switch (emTreeType)
                {
                case EMSFB_TREETYPE.emTreeTypeChatCategories:
                {
                    ChatCategoryTree_BeforeExpand(ndCurNode);
                    break;
                }
                default:
                {
                    break;
                }
                }
            }
        }
        #endregion

        #region UI events: Chat Category Setting page events
        private void btnApply_Click(object sender, EventArgs e)
        {
            EMSFB_RECODESTATUS emRecodeStatus = RecordCurrentSelectInfo(true);
            if (emRecodeStatus != EMSFB_RECODESTATUS.emSuccess)
            {
                PopupErrorInfoByRecordStatus(emRecodeStatus);
            }
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            EMSFB_RECODESTATUS emRecodeStatus=RecordCurrentSelectInfo(true);
            if (emRecodeStatus == EMSFB_RECODESTATUS.emSuccess)
            {
                bool bSubmitSuccess = SubmitAllSelectInfo();
                if (bSubmitSuccess)
                {
                    CloseWindows(EMSFB_FORMCLOSEREASON.emCloseByOkBtn);
                }
                else
                {
                    ShowTablePages(EMSFB_TABPAGETYPE.emTabMessagePage, CommonHelper.GetValueByKeyFromDir(s_dirDisplayMsgInfo, (long)EMSFB_RUNTIMESTATUS.emSaveDataError, null));
                }
            }
            else
            {
                PopupErrorInfoByRecordStatus(emRecodeStatus);
            }
        }
        private void checkBoxEnforcer_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cbEnforcer = sender as CheckBox;
            if (null != cbEnforcer)
            {
                ShowCurrentNodeClassificationArea(cbEnforcer.Checked);
            }
        }
        #endregion

        #region UI events: Form Events
        private void SFBControlPanel_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (EMSFB_FORMCLOSEREASON.emCloseByOkBtn != GetFormCloseReason())
            {
                SubmitAllSelectInfo();
            }
        }
        #endregion

        #region UI events: Classification seting pages events
        #endregion

        #region Window Message
        protected override void DefWndProc(ref Message obMsg)
        {
            switch (obMsg.Msg)
            {
            case WM_LOADINGSFBINFO:
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Start Loading SFB Info");
                ProcessLoadingSFBInfo();
                break;
            }
            case WM_LOADEDSFBINFO:
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Load SFB Info complete with status:{0}\n", obMsg.WParam);

                EMSFB_RUNTIMESTATUS emRuntimeError = CommonHelper.ConvertStringToEnum(obMsg.WParam.ToString(), true, EMSFB_RUNTIMESTATUS.emSuccess);
                ProcessLoadedSFBInfo(emRuntimeError);
                break;
            }
            default:
            {
                break;
            }
            }

            base.DefWndProc(ref obMsg);
        }
        private void ProcessLoadingSFBInfo()
        {
            trSFBESettings.Enabled = false;
            ShowTablePages(EMSFB_TABPAGETYPE.emTabMessagePage, CommonHelper.GetValueByKeyFromDir(s_dirDisplayMsgInfo, (long)EMSFB_RUNTIMESTATUS.emStartLoading, "Start loading"));
        }
        private void ProcessLoadedSFBInfo(EMSFB_RUNTIMESTATUS emRuntimeError)
        {
            if (EMSFB_RUNTIMESTATUS.emSuccess == emRuntimeError)
            {
                ShowTablePages(EMSFB_TABPAGETYPE.emTabMessagePage, CommonHelper.GetValueByKeyFromDir(s_dirDisplayMsgInfo, (long)EMSFB_RUNTIMESTATUS.emLoadComplete, "Load complete"));
                List<SFBChatCategoryInfo> lsChatCategoryInfo = GetSFBChatCategorysInfo();
                if (null != lsChatCategoryInfo)
                {
                    trSFBESettings.Enabled = true;
                    TreeNode ndChatCategoryRootNode = GetTreeRootNodeByType(EMSFB_TREETYPE.emTreeTypeChatCategories);
                    if (null != ndChatCategoryRootNode)
                    {
                        ndChatCategoryRootNode.Expand();
                    }
                }
                else
                {
                    ShowTablePages(EMSFB_TABPAGETYPE.emTabMessagePage, CommonHelper.GetValueByKeyFromDir(s_dirDisplayMsgInfo, (long)EMSFB_RUNTIMESTATUS.emUnknownError, null));
                }
            }
            else
            {
                ShowTablePages(EMSFB_TABPAGETYPE.emTabMessagePage, CommonHelper.GetValueByKeyFromDir(s_dirDisplayMsgInfo, (long)emRuntimeError, null));
            }
        }
        #endregion

        #region tools
        private void SetDefaultClassificationSelectedIndex(int nDefaultIndex)
        {
            m_nDefaultClassificationSchemaIndex = nDefaultIndex;
        }
        private int GetDefaultClassificationSelectedIndex()
        {
            return m_nDefaultClassificationSchemaIndex;
        }
        private void ShowCurrentNodeClassificationArea(bool bEnforcerChecked)
        {
            if (bEnforcerChecked)
            {
                groupClassficationArea.Visible = true;
                TreeNode ndCurNode = trSFBESettings.SelectedNode;
                if (null != ndCurNode)
                {
                    if (ndCurNode.Parent == GetTreeRootNodeByType(EMSFB_TREETYPE.emTreeTypeChatCategories))
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "current node is:[{0}]\n", ndCurNode.Text);
                        TypeNodeTags dirNodeTags = ndCurNode.Tag as TypeNodeTags;
                        ChatCategorySetting obChatCategorySetting = CommonHelper.GetValueByKeyFromDir(dirNodeTags, kstrUserSettingTagKey, null) as ChatCategorySetting;
                        ShowClassificationArea(obChatCategorySetting);
                    }
                }
            }
            else
            {
                groupClassficationArea.Visible = false;
            }
        }
        private void ShowClassificationArea(ChatCategorySetting obChatCategorySetting)
        {
            if (null != obChatCategorySetting)
            {
                string strClassificationSchemaName = obChatCategorySetting.GetClassficationDisplayName();
                int nIndex = GetIndexFormClassificationSchemasList(strClassificationSchemaName);
                if (-1 == nIndex)
                {
                    nIndex = GetDefaultClassificationSelectedIndex();
                }
                comboxClassificationSchemas.SelectedIndex = nIndex;
                string strClassificationWarningInfo = obChatCategorySetting.GetClassificationWarningInfo();

                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "current display name:[{0}], classification warning info:[{1}]\n", strClassificationSchemaName, strClassificationWarningInfo);

                if (string.IsNullOrWhiteSpace(strClassificationWarningInfo))
                {
                    txtClassificationWarning.Text = "";
                    txtClassificationWarning.Visible = false;
                }
                else
                {
                    txtClassificationWarning.Text = strClassificationWarningInfo;
                    txtClassificationWarning.Visible = true;
                }
            }
        }
        private int GetIndexFormClassificationSchemasList(string strClassificationSchemaName)
        {
            int nIndex = -1;
            if (!string.IsNullOrWhiteSpace(strClassificationSchemaName))
            {
                foreach (string strItem in comboxClassificationSchemas.Items)
                {
                    ++nIndex;
                    if (string.Equals(strClassificationSchemaName, strItem, StringComparison.OrdinalIgnoreCase))
                    {
                        return nIndex;
                    }
                }
            }
            return -1;
        }
        private ChatCategorySetting GetAndUpdateNodeChatCategorySetting(TreeNode ndNode)
        {
            ChatCategorySetting obChatCategorySetting = null;
            if (null != ndNode)
            {
                TypeNodeTags dicNodeTags = ndNode.Tag as TypeNodeTags;
                obChatCategorySetting = CommonHelper.GetValueByKeyFromDir(dicNodeTags, kstrUserSettingTagKey, null) as ChatCategorySetting;
                if (null == obChatCategorySetting)
                {
                    string strChatCategoryUri = CommonHelper.GetValueByKeyFromDir(dicNodeTags, kstrSFBChatCategoryUri, null) as string;
                    if (!string.IsNullOrWhiteSpace(strChatCategoryUri))
                    {
                        // Init chat category setting
                        obChatCategorySetting = new ChatCategorySetting(strChatCategoryUri, false);
                        EMSFB_RUNTIMESTATUS emRuntimeStatus = obChatCategorySetting.GetRuntimeStatus();
                        if (EMSFB_RUNTIMESTATUS.emSuccess == emRuntimeStatus)
                        {
                            CommonHelper.AddKeyValuesToDir(dicNodeTags, kstrUserSettingTagKey, obChatCategorySetting);
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! Unknown node, current node is:[{0}] without find uri\n", ndNode.Text);
                    }
                }
            }
            return obChatCategorySetting;
        }
        private EMSFB_TREETYPE GetNodeTreeType(TreeNode ndCurNode)
        {
            EMSFB_TREETYPE emTreeType = EMSFB_TREETYPE.emTreeTypeUnknown;
            if (null != ndCurNode)
            {
                TreeNode ndCurRootNode = GetRootNodeForCurNode(ndCurNode);
                for (int i = 0; (i < kszTreeTypes.Length) && (i < trSFBESettings.Nodes.Count); ++i)
                {
                    if (trSFBESettings.Nodes[i] == ndCurRootNode)
                    {
                        emTreeType = kszTreeTypes[i];
                        break;
                    }
                }
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current node:[{0}] type is [{1}]\n", ndCurNode.Text, emTreeType);
            }
            return emTreeType;
        }
        private TreeNode GetRootNodeForCurNode(TreeNode ndNode)
        {
            if (null == ndNode)
            {
                return null;
            }
            else
            {
                if (0 == ndNode.Level)
                {
                    return ndNode;
                }
                else
                {
                    return GetRootNodeForCurNode(ndNode.Parent);
                }
            }
        }
        private EMSFB_FORMCLOSEREASON GetFormCloseReason() { return m_emFormCloseReason; }
        private void SetFormCloseReason(EMSFB_FORMCLOSEREASON emFormCloseReason) { m_emFormCloseReason = emFormCloseReason; }

        private TreeNode GetTreeRootNodeByType(EMSFB_TREETYPE emTreeType)
        {
            TreeNode ndRootNode = null;
            for (int i=0; (i<kszTreeTypes.Length) && (i<trSFBESettings.Nodes.Count); ++i)
            {
                if (emTreeType == kszTreeTypes[i])
                {
                    ndRootNode = trSFBESettings.Nodes[i];
                }
            }
            return ndRootNode;
        }
        private TypeNodeTags CreateNewChatCategoryNodeTags(SFBChatCategoryInfo obSFBChatCategoryInfo)
        {
            string strUri = "";
            if (null != obSFBChatCategoryInfo)
            {
                strUri = obSFBChatCategoryInfo.GetItemValue(SFBChatCategoryInfo.kstrUriFieldName);
            }
            return new TypeNodeTags()
            {
                {kstrExpandedTagKey, false},
                {kstrSFBChatCategoryUri, strUri},
                {kstrUserSettingTagKey, null},
                {kstrSFBChatCategoryInfo, obSFBChatCategoryInfo}
            };
        }
        private void ShowTablePages(EMSFB_TABPAGETYPE emTabPage, string strMessage)
        {
            tabControl.TabPages.Clear();
            foreach (KeyValuePair<EMSFB_TABPAGETYPE, TabPage> pairTabPage in m_dicTabPages)
            {
                if (pairTabPage.Key == emTabPage)
                {
                    if (EMSFB_TABPAGETYPE.emTabChatCategorySettingPage == emTabPage)
                    {
                        groupForceEnforcerArea.Visible = m_bSupportForceEnforcerOption;
                    }
                    tabControl.TabPages.Add(pairTabPage.Value);
                }
                else
                {
                    tabControl.TabPages.Remove(pairTabPage.Value);
                }
            }
            if (EMSFB_TABPAGETYPE.emTabMessagePage == emTabPage)
            {
                if (string.IsNullOrEmpty(strMessage))
                {
                    txtMessage.Text = kstrUnknownErrorMessage;
                }
                else
                {
                    txtMessage.Text = strMessage;
                }
            }
        }
        private void InitSFBPersistentChatServerInfo()
        {
            List<SFBPersistentChatServerInfo> lsSFBPersistentChatServerInfo = SFBPersistentChatServerInfo.GetAllCurPersistentChatServerInfo();  // Using power shell to get all file store service info, need take a few seconds
            if (null != lsSFBPersistentChatServerInfo)
            {
                foreach (SFBPersistentChatServerInfo obSFBPersistentChatServerInfo in lsSFBPersistentChatServerInfo)
                {
                    obSFBPersistentChatServerInfo.PersistantSave();
                }
            }
        }
        private void SetSFBChatCategorysInfo(List<SFBChatCategoryInfo> lsChatCategoryInfo)
        {
            lock (m_lsSFBChatCategoryInfo)
            {
                if (null != lsChatCategoryInfo)
                {
                    m_lsSFBChatCategoryInfo = lsChatCategoryInfo;

                    // Save all SFBChatCategory info into database
                    foreach (SFBChatCategoryInfo obSFBChatCategoryInfo in m_lsSFBChatCategoryInfo)
                    {
                        obSFBChatCategoryInfo.PersistantSave();
                    }
                }
            }
        }
        private List<SFBChatCategoryInfo> GetSFBChatCategorysInfo()
        {
            lock (m_lsSFBChatCategoryInfo)
            {
                return m_lsSFBChatCategoryInfo;
            }
        }
        private void PopupErrorInfoByRecordStatus(EMSFB_RECODESTATUS emRecodeStatus)
        {
            switch (emRecodeStatus)
            {
            case EMSFB_RECODESTATUS.emSuccess:
            {
                break;
            }
            default:
            {
                STUSFB_ERRORMSG errorMsg = CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirErrorMsg, ConfigureFileManager.kstrXMLPersistentSaveErrorFlag, new STUSFB_ERRORMSG(">Failed to save your chat room category's configuration. Please try again. If the problem persists, please contact your system administrator.", 0));
                MessageBox.Show(errorMsg.m_strErrorMsg, groupClassficationArea.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                break;
            }
            }
        }
        private EMSFB_RECODESTATUS RecordCurrentSelectInfo(bool bApplied)
        {
            EMSFB_RECODESTATUS emResult = EMSFB_RECODESTATUS.emUnknow;
            TreeNode ndCurNode = trSFBESettings.SelectedNode;
            if (null != ndCurNode)
            {
                if (ndCurNode.Parent == GetTreeRootNodeByType(EMSFB_TREETYPE.emTreeTypeChatCategories))
                {
                    ChatCategorySetting obChatCategorySetting = GetAndUpdateNodeChatCategorySetting(ndCurNode);
                    if ((null != obChatCategorySetting) && (EMSFB_RUNTIMESTATUS.emSuccess == obChatCategorySetting.GetRuntimeStatus()))
                    {
                        string strClassificationSchemaName = checkBoxEnforcer.Checked ? comboxClassificationSchemas.Text : "";
                        obChatCategorySetting.UpdateSettingInfo(checkBoxEnforcer.Checked, checkBoxForceEnforcer.Checked, strClassificationSchemaName);
                        obChatCategorySetting.SetAppliedFlag(bApplied);
                        emResult = EMSFB_RECODESTATUS.emSuccess;
                    }
                }
                else
                {
                    emResult = EMSFB_RECODESTATUS.emCurrectNodeError;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! Current select node is not chat category node\n");
                }
            }
            else
            {
                emResult = EMSFB_RECODESTATUS.emCannotGetCurrectNode;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! Cannot get current node\n");
            }
            return emResult;
        }
        private bool SubmitAllSelectInfo()
        {
            bool bRet = true;
            TreeNode ndChatCategoryRootNode = GetTreeRootNodeByType(EMSFB_TREETYPE.emTreeTypeChatCategories);
            if (null != ndChatCategoryRootNode)
            {
                for (int i = 0; i<ndChatCategoryRootNode.Nodes.Count; ++i)  // foreach return a clone, for return the reference
                {
                    TreeNode ndCurItem = ndChatCategoryRootNode.Nodes[i];
                    ChatCategorySetting obChatCategorySetting = GetAndUpdateNodeChatCategorySetting(ndCurItem);
                    if ((null != obChatCategorySetting) && (EMSFB_RUNTIMESTATUS.emSuccess == obChatCategorySetting.GetRuntimeStatus()))
                    {
                        if (obChatCategorySetting.GetAppliedFlag())
                        {
                            string[] szItemInfo = new string[]
                            {
                                NLChatCategoryInfo.kstrUriFieldName, obChatCategorySetting.GetUri(),
                                NLChatCategoryInfo.kstrEnforcerFieldName, obChatCategorySetting.IsNeedEnforcer().ToString(),
                                NLChatCategoryInfo.kstrForceEnforcerFieldName, obChatCategorySetting.IsNeedForceEnforcer().ToString(),
                                NLChatCategoryInfo.kstrClassificationSchemaNameFieldName, CommonHelper.GetSolidString(obChatCategorySetting.GetClassficationDisplayName()),
                                NLChatCategoryInfo.kstrClassificationFieldName, CommonHelper.GetSolidString(obChatCategorySetting.GetClassificationInfo())
                            };
                            NLChatCategoryInfo obNLChatCategory = new NLChatCategoryInfo(szItemInfo);
                            bool bSuccessPersistantSaved = obNLChatCategory.PersistantSave();
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "NodeName:{0},Persistent save:[{1}]\n", ndCurItem.Text, bSuccessPersistantSaved.ToString());
                            obChatCategorySetting.SetAppliedFlag(!bSuccessPersistantSaved);
                            bRet &= bSuccessPersistantSaved;
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "Node:[{0}] does not applied\n", ndCurItem.Text);
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Node :[{0}] do not update\n", ndCurItem.Text);
                    }
                }
            }
            return bRet;
        }
        private void CloseWindows(EMSFB_FORMCLOSEREASON emCloseReason)
        {
            SetFormCloseReason(emCloseReason);
            this.Close();
        }
        #endregion

        #region UI tools
        private void ChatCategoryTree_AfterSelect(TreeNode ndCurNode)
        {
            if ((null == ndCurNode) || (0 == ndCurNode.Level) || (null == ndCurNode.Tag))
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current node null or root node or no tags, return.");
                return;
            }

            EMSFB_RUNTIMESTATUS emRuntimeStatus = EMSFB_RUNTIMESTATUS.emUnknownError;
            ChatCategorySetting obChatCategorySetting = GetAndUpdateNodeChatCategorySetting(ndCurNode);
            if (null != obChatCategorySetting)
            {
                emRuntimeStatus = obChatCategorySetting.GetRuntimeStatus();
            }

            if ((null != obChatCategorySetting) && (EMSFB_RUNTIMESTATUS.emSuccess == emRuntimeStatus))
            {
                checkBoxEnforcer.Checked = obChatCategorySetting.IsNeedEnforcer();
                checkBoxForceEnforcer.Checked = obChatCategorySetting.IsNeedForceEnforcer();
                ShowCurrentNodeClassificationArea(checkBoxEnforcer.Checked);

                ShowTablePages(EMSFB_TABPAGETYPE.emTabChatCategorySettingPage, "");
            }
            else
            {
                ShowTablePages(EMSFB_TABPAGETYPE.emTabMessagePage, CommonHelper.GetValueByKeyFromDir(s_dirDisplayMsgInfo, (long)emRuntimeStatus, null));
            }
        }
        private void ChatCategoryTree_BeforeExpand(TreeNode ndCurNode)
        {
            if (null == ndCurNode)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! Unknown error, current node is null"); ;
                return;
            }

            if (0 == ndCurNode.Level)
            {
                bool bNeedAddNodes = true;
                if (null == ndCurNode.Tag)
                {
                    ndCurNode.Tag = CreateNewChatCategoryNodeTags(null);
                }
                TypeNodeTags dirNodeTags = ndCurNode.Tag as TypeNodeTags;
                string strExpanded = CommonHelper.GetValueByKeyFromDir(dirNodeTags, kstrExpandedTagKey, null) as string;
                if (!string.IsNullOrEmpty(strExpanded))
                {
                    bNeedAddNodes = !kstrTrue.Equals(strExpanded, StringComparison.OrdinalIgnoreCase);
                }

                if (bNeedAddNodes)
                {
                    List<SFBChatCategoryInfo> lsChatCategoryInfo = GetSFBChatCategorysInfo();
                    if (null != lsChatCategoryInfo)
                    {
                        foreach (SFBObjectInfo obSFBObjInfo in lsChatCategoryInfo)
                        {
                            string strName = obSFBObjInfo.GetItemValue(SFBChatCategoryInfo.kstrNameFieldName);
                            if (!string.IsNullOrEmpty(strName))
                            {
                                TreeNode ndChild = new TreeNode(strName);
                                ndChild.Tag = CreateNewChatCategoryNodeTags(obSFBObjInfo as SFBChatCategoryInfo);
                                ndCurNode.Nodes.Add(ndChild);
                            }
                            else
                            {
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!SFBChatCategoryInfo error\n");
                            }
                        }
                        CommonHelper.AddKeyValuesToDir(dirNodeTags, kstrExpandedTagKey, kstrTrue);

                        if (ndCurNode.Nodes.Count != (lsChatCategoryInfo.Count + 1))
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Some error happened and lose some category info when we add chat category info to tree nodes\n");
                        }
                    }

                    if (1 >= ndCurNode.Nodes.Count)
                    {
                        ndCurNode.Nodes.Clear();
                        ShowTablePages(EMSFB_TABPAGETYPE.emTabMessagePage, CommonHelper.GetValueByKeyFromDir(s_dirDisplayMsgInfo, (long)EMSFB_RUNTIMESTATUS.emUnknownError, null));
                    }
                    else
                    {
                        ndCurNode.Nodes.Remove(ndCurNode.Nodes[0]);
                    }
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelInfo, "The node{0} has been expanded, node level:[{1}]\n", ndCurNode.Text, ndCurNode.Level);
            }
        }
        #endregion
    }
}
