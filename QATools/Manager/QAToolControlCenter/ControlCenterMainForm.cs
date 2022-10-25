using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using QAToolSFBCommon.NLLog;
using QAToolSFBCommon.Common;

using QAToolControlCenter.Common;

namespace QAToolControlCenter
{
    using TypeNodeTags = Dictionary<string, object>;

    public partial class ControlCenterMainForm : Form
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(ControlCenterMainForm));
        #endregion

        #region Const/Read only values
        private const string kstrNodeTagConferenceInfoKey = "conferenceinfo";
        #endregion

        #region Members
        private BasicConfigInfo m_basicConfigInfo = null;
        private BasicUserInfo m_basicUserInfo = null;
        #endregion

        public ControlCenterMainForm()
        {
            InitializeComponent();
        }

        #region UI events: basic info
        private void btnBasicInfoOK_Click(object sender, EventArgs e)
        {
            try
            {
                // Init basic info
                string strPerformanceOrgFolder = textPerformanceOrgLogFolder.Text;
                string strPerformanceResultLogFolder = textPerformanceResultLogFolder.Text;
                int nPort = int.Parse(textControlCenterPort.Text);
                m_basicConfigInfo = new BasicConfigInfo(strPerformanceOrgFolder, strPerformanceResultLogFolder, nPort);
            }
            catch (Exception ex)
            {
                m_basicConfigInfo = null;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in btnBasicInfoOK_Click, [{0}]\n", ex.Message);
                ShowAlterMessage("Save failed\n");
            }
        }
        #endregion

        #region UI events: user info
        private void btnUserChecking_Click(object sender, EventArgs e)
        {
            try
            {
                int nUserIndex = int.Parse(textUserCheckedIndex.Text);

                // Find the user's port and send show UI command
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in btnUserChecking_Click, [{0}]\n", ex.Message);
                ShowAlterMessage("Check failed\n");
            }
        }
        private void btnUserInfoOK_Click(object sender, EventArgs e)
        {
            try
            {
                string strServerFQDN = textServerFQDN.Text;
                string strUserDomain = textUserDomain.Text;
                string strUserBaseFirstName = textUserBaseFirstName.Text;
                string strUserLastName = textUserLastName.Text;
                string strPassword = comboxPassword.Text;
                int nUserMinIndex = int.Parse(textUserMinIndex.Text);
                int nUserMaxIndex = int.Parse(textUserMaxIndex.Text);

                m_basicUserInfo = new BasicUserInfo(strServerFQDN, strUserDomain, strUserBaseFirstName, strUserLastName, strPassword, nUserMinIndex, nUserMaxIndex);
            }
            catch (Exception ex)
            {
                m_basicUserInfo = null;
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in btnUserInfoOK_Click, [{0}]\n", ex.Message);
                ShowAlterMessage("Save failed\n");
            }
        }
        #endregion

        #region UI event: conference info
        private void btnAppendConference_Click(object sender, EventArgs e)
        {
            string strConferenceUri = GetMeetingSipUri(textConferenceUri.Text.Trim());
            if (string.IsNullOrWhiteSpace(strConferenceUri))
            {
                ShowAlterMessage("Append new failed. The conference URI is error.");
            }
            else
            {
                ConferenceInfo obConferenceInfo = CreateConferenceInfoWithUISetting();
                if (null != obConferenceInfo)
                {
                    AddConferenceInfo(obConferenceInfo);
                }
            }
        }
        private void btnDeleteConferenceInfo_Click(object sender, EventArgs e)
        {
            TreeNode ndCurNode = treeConference.SelectedNode;
            if (null == ndCurNode)
            {
                ShowAlterMessage("You click to delete button, but you do not select any node to delete.");
            }
            else if (!IsConferenceInfoSubNode(ndCurNode))
            {
                ShowAlterMessage("You click to delete button, but current select node is not conference node and cannot delete by this button.");
            }
            else
            {
                DeleteConferenceInfo(ndCurNode);
            }
        }
        private void treeConference_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode ndCurNode = e.Node;
            if (null == ndCurNode)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!! Unknown error in tree before expand, current node is null");
            }
            else
            {
                TreeNode ndRootConferenceNode = GetRootConferenceNode();
                if (ndCurNode == ndRootConferenceNode)
                {
                    ShowConferenceInfoPage(null);
                }
                else if (IsConferenceInfoSubNode(ndCurNode))
                {
                    TypeNodeTags dicCurNodeTags = ndCurNode.Tag as TypeNodeTags;
                    ConferenceInfo obConferenceInfo = CommonHelper.GetValueByKeyFromDir(dicCurNodeTags, kstrNodeTagConferenceInfoKey, null) as ConferenceInfo;
                    ShowConferenceInfoPage(obConferenceInfo);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Current node is not conference info node:[{0}]\n", ndCurNode.Text);
                    ShowConferenceInfoPage(null);
                }
            }
        }
        #endregion

        #region Tools
        private void ShowConferenceInfoPage(ConferenceInfo obConferenceInfo)
        {
            try
            {
                bool bIsEmptyConferenceInfo = ((null == obConferenceInfo) || (string.IsNullOrWhiteSpace(obConferenceInfo.m_strConferenceUri))) ;
                if (bIsEmptyConferenceInfo)
                {
                    obConferenceInfo = new ConferenceInfo("");
                }
                btnDeleteConferenceInfo.Visible = !bIsEmptyConferenceInfo;
                textConferenceUri.Text = obConferenceInfo.m_strConferenceUri;
                textConferenceLength.Text = obConferenceInfo.m_nConferenceLength.ToString();
                textJoinTimes.Text = obConferenceInfo.m_nJoinTimes.ToString();
                textJoinInterval.Text = obConferenceInfo.m_nJoinInterval.ToString();
                textIMInterval.Text = obConferenceInfo.m_nIMInterval.ToString();
                textJoinedUserMinIndex.Text = obConferenceInfo.m_nJoinedUserMinIndex.ToString();
                textJoinedUserMaxIndex.Text = obConferenceInfo.m_nJoinedUserMaxIndex.ToString();
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in InitConferenceInfo, [{0}]\n", ex.Message);
            }
        }
        private DialogResult ShowAlterMessage(string strAlterMessage)
        {
            return MessageBox.Show(this.tabMain, strAlterMessage);
        }
        private bool IsConferenceInfoSubNode(TreeNode ndCurNode)
        {
            bool bIsSubNode = false;
            if (null != ndCurNode)
            {
                TreeNode ndRootConferenceNode = GetRootConferenceNode();
                bIsSubNode = (ndRootConferenceNode != ndCurNode);
            }
            return bIsSubNode;
        }
        private string GetMeetingSipUri(string strMeetingAddr)
        {
            MeetingAddrInfo obMeetingAddrInfo = new MeetingAddrInfo(strMeetingAddr);
            if (obMeetingAddrInfo.GetAnalysisFlag())
            {
                return obMeetingAddrInfo.GetMeetingSipUri();
            }
            return "";
        }
        private bool IsConferenceExist(string strConferenceUri)
        {
            bool bExist = false;
            if (!string.IsNullOrWhiteSpace(strConferenceUri))
            {
                strConferenceUri = strConferenceUri.Trim();
                TreeNode ndRootConferenceNode = GetRootConferenceNode();
                foreach (TreeNode ndItem in ndRootConferenceNode.Nodes)
                {
                    if (string.Equals(GetNodeInnerName(ndItem), strConferenceUri, StringComparison.OrdinalIgnoreCase))
                    {
                        bExist = true;
                        break;
                    }
                }
            }
            return bExist;
        }
        private ConferenceInfo CreateConferenceInfoWithUISetting()
        {
            ConferenceInfo obConferenceInfo = null;
            try
            {
                string strConferenceUri = GetMeetingSipUri(textConferenceUri.Text.Trim());
                if (!string.IsNullOrWhiteSpace(strConferenceUri))
                {
                    int nConferenceLength = int.Parse(textConferenceLength.Text);
                    int nJoinTimes = int.Parse(textJoinTimes.Text);
                    int nJoinInterval = int.Parse(textJoinInterval.Text);
                    int nIMInterval = int.Parse(textIMInterval.Text);
                    int nJoinedUserMinIndex = int.Parse(textJoinedUserMinIndex.Text);
                    int nJoinedUserMaxIndex = int.Parse(textJoinedUserMaxIndex.Text);
                    obConferenceInfo = new ConferenceInfo(strConferenceUri, nConferenceLength, nJoinTimes, nJoinInterval, nIMInterval, nJoinedUserMinIndex, nJoinedUserMaxIndex);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in InitConferenceInfo, [{0}]\n", ex.Message);
            }
            return obConferenceInfo;
        }
        private void AddConferenceInfo(ConferenceInfo obNewConferenceInfo)
        {
            if ((null == obNewConferenceInfo) || (string.IsNullOrWhiteSpace(obNewConferenceInfo.m_strConferenceUri)))
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "The conference info or URI is empty");
                return ;
            }
            TreeNode ndCurConferenceNode = null;
            TreeNode ndRootConferenceNode = GetRootConferenceNode();
            foreach (TreeNode ndItem in ndRootConferenceNode.Nodes)
            {
                if (string.Equals(GetNodeInnerName(ndItem), obNewConferenceInfo.m_strConferenceUri, StringComparison.OrdinalIgnoreCase))
                {
                    ndCurConferenceNode = ndItem;
                    break;
                }
            }
            if (null == ndCurConferenceNode)
            {
                string strNodeText = CreateNodeTextByConferenceInfo(obNewConferenceInfo);
                if (string.IsNullOrWhiteSpace(strNodeText))
                {
                    strNodeText = obNewConferenceInfo.m_strConferenceUri.ToLower();
                }
                ndCurConferenceNode = new TreeNode(strNodeText);
                ndCurConferenceNode.Name = obNewConferenceInfo.m_strConferenceUri.ToLower();
                InitConferenceNodeTags(ndCurConferenceNode, obNewConferenceInfo);
                ndRootConferenceNode.Nodes.Add(ndCurConferenceNode);
            }
            else
            {
                TypeNodeTags dicCurNodeTags = ndCurConferenceNode.Tag as TypeNodeTags;
                CommonHelper.AddKeyValuesToDir(dicCurNodeTags, kstrNodeTagConferenceInfoKey, obNewConferenceInfo);
            }
        }
        private TreeNode GetRootConferenceNode()
        {
            return treeConference.Nodes[0];
        }
        private void DeleteConferenceInfo(TreeNode ndDelNode)
        {
            string strDelConferenceUri = GetNodeInnerName(ndDelNode);
            TreeNode ndRootConferenceNode = GetRootConferenceNode();
            ndRootConferenceNode.Nodes.Remove(ndDelNode);
        }
        private void InitConferenceNodeTags(TreeNode ndCurConferenceNode, ConferenceInfo obNewConferenceInfo)
        {
            if (null != ndCurConferenceNode)
            {
                string strUri = GetNodeInnerName(ndCurConferenceNode);
                ndCurConferenceNode.Tag = new TypeNodeTags()
                {
                    {kstrNodeTagConferenceInfoKey, obNewConferenceInfo}
                };
            }
        }
        private string GetNodeInnerName(TreeNode ndNode)
        {
            if (string.IsNullOrWhiteSpace(ndNode.Name))
            {
                return ndNode.Text;
            }
            return ndNode.Name;
        }
        private string CreateNodeTextByConferenceInfo(ConferenceInfo obNewConferenceInfo)
        {
            string strNodeText = "";
            if (null != obNewConferenceInfo)
            {
                strNodeText = obNewConferenceInfo.m_strConferenceUri;
                MeetingAddrInfo obMeetingAddrInfo = new MeetingAddrInfo(obNewConferenceInfo.m_strConferenceUri.ToLower());
                if (obMeetingAddrInfo.GetAnalysisFlag())
                {
                    strNodeText = obMeetingAddrInfo.Creator + ":" + obMeetingAddrInfo.MeetingID;
                }
            }
            return strNodeText;
        }
        #endregion
    }
}
