using ClassificationTool.Models;
using ClassificationTool.Services;
using SFBCommon.NLLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassificationTool
{
    public partial class ClassificationForm : Form
    {
        private TagService m_tagService = null;
        private ServiceProvider m_serviceProvider = null;
        private static readonly CLog logger = CLog.GetLogger(typeof(ClassificationForm));

        public ClassificationForm()
        {
            InitializeComponent();
            InitMembers();
            InitBrowser();
            InvokeJsInit();
        }

        private void InitMembers()
        {
            m_tagService = TagService.GetInstance();
            m_serviceProvider = new ServiceProvider();
        }

        private void InitBrowser()
        {
            string strPageDirPath = GetPagesDirectoryPath();
            string classficationPath = Path.Combine(strPageDirPath, "classification.html");
            string schemaPath = Path.Combine(strPageDirPath, "schema.html");
            this.classificationBrowser.Url = new Uri(classficationPath);
            this.schemaBrowser.Url = new Uri(schemaPath);

            this.classificationBrowser.ObjectForScripting = m_tagService;
            this.schemaBrowser.ObjectForScripting = m_serviceProvider;
        }

        private string GetPagesDirectoryPath()
        {
            string strPath = "";
            string strPagesDirName = "Pages";

            string strAppDir = Application.StartupPath;
            //string strPathSeparator = @"\";
            //string strRootDirName = "ClassificationTool";
            //int nRootIndex = strAppDir.IndexOf(strRootDirName);
            //if(nRootIndex > -1)
            //{
            //    string strRootDir = strAppDir.Substring(0, nRootIndex+strRootDirName.Length);
            //    string[] pathArray = strRootDir.Split(new string[] { strPathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            //    string rootPath = string.Join(@"\", pathArray);
            //    strPath = Path.Combine(rootPath, strPagesDirName);
            //}
            //else
            //{
            //    logger.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "GetPagesDirectoryPath failed, {0} is not in path", strRootDirName);
            //}
            strPath = Path.Combine(strAppDir, strPagesDirName);

            return strPath;
        }

        //call js init function after initializing data
        private async void InvokeJsInit()
        {
            await InitDataAsync();
            this.classificationBrowser.Document.InvokeScript("init", new object[]{});
            this.schemaBrowser.Document.InvokeScript("init", new object[]{});
        }

        //init data in background
        private async Task InitDataAsync()
        {
            await Task.Run(() => 
            {
                m_tagService.InitTagInfoFromPersistentInfo();
                List<Schema> schemaList = m_serviceProvider.SchemaList;
            });
        }
    }
}
