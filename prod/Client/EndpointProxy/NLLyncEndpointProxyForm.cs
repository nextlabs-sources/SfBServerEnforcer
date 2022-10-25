using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Configuration;

// NEXTLABS
using NLLyncEndpointProxy.Common;
using NLLyncEndpointProxy.Listener;

// Other project
using SFBCommon.CommandHelper;
using SFBCommon.NLLog;
using SFBCommon.Common;
using SFBCommon.ClassifyHelper;

namespace NLLyncEndpointProxy
{
    public partial class NLLyncEndpointProxyForm : Form, IMessageDisplayer
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(NLLyncEndpointProxyForm));
        #endregion

        #region Const/Readonly values
        #endregion

        public NLLyncEndpointProxyForm(string strTitle)
        {
            InitializeComponent();
            NLLyncEndpointProxy.NLMessagingFlow.NLIMFlow.SetIMessageDisplayer(this);

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Title:{0}\n", strTitle);
            if (!string.IsNullOrEmpty(strTitle))
            {
                this.Text += " - " + strTitle;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            NLLyncEndpointProxyMain.s_obNLLyncEndpointProxyObj.CurLyncEndpoint.SendNotifyMessage(true, comboxUsers.Text, textSendMessage.Text, "");
        }

        #region Implement Interface: ISaveMessage
        public void SaveMessage(string strMessage)
        {
            textReceivedMessage.Text += strMessage;
        }
        #endregion

        private void buttonManulClassifyCmd_Click(object sender, EventArgs e)
        {
            STUSFB_CLASSIFYCMDINFO stuClassifyCmdInfo = new STUSFB_CLASSIFYCMDINFO(EMSFB_COMMAND.emCommandClassifyMeeting, comboxUsers.Text, comboBoxSFBObjUri.Text);
            NLLyncEndpointProxyMain.s_obNLLyncEndpointProxyObj.CurLyncEndpoint.DoManulClassify(stuClassifyCmdInfo);
        }

        #region Private backup code
        private void buttonManulClassifyCmd_Click_WithClassifyManagerCheck(object sender, EventArgs e)
        {
#if false
            STUSFB_CLASSIFYCMDINFO stuClassifyCmdInfo = new STUSFB_CLASSIFYCMDINFO(EMSFB_COMMAND.emCommandClassifyMeeting, comboxUsers.Text, comboBoxSFBObjUri.Text);
            ManulClassifyObligationHelper obManulClassifyObligationHelper = new ManulClassifyObligationHelper(textSendMessage.Text/*kstrXmlManulClassifyObligations*/, false);
            ClassifyTagsHelper obClassifyTagsInfo = new ClassifyTagsHelper(kstrXmlClassifyTags);

            NLLyncEndpointProxyMain.s_obNLLyncEndpointProxyObj.CurLyncEndpoint.DoManulClassify(stuClassifyCmdInfo, obManulClassifyObligationHelper, obClassifyTagsInfo);
#endif
        }
        #endregion
    }
}
