using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;         // Power shell
using System.Collections.ObjectModel;

using SFBCommon.Common;
using SFBCommon.NLLog;

namespace SFBCommon.SFBObjectInfo
{
    public class SFBPersistentChatServerInfo : SFBObjectInfo
    {
        #region Static values: field names
        public static readonly string kstrPoolFqdnFieldName = "persistentchatserver_poolfqdn";
        public static readonly string kstrRegistrarFieldName = "persistentchatserver_registrar";
        public static readonly string kstrFileStoreFieldName = "persistentchatserver_filestore";
        public static readonly string kstrFileStoreUncPathFieldName = "persistentchatserver_filestore_uncpath";
        public static readonly string kstrMtlsPortFieldName = "persistentchatserver_mtlsport";
        public static readonly string kstrDisplayNameFieldName = "persistentchatserver_displayname";
        public static readonly string kstrActiveServersFieldName = "persistentchatserver_activeservers";
        public static readonly string kstrInactiveServersFieldName = "persistentchatserver_inactiveservers";
        public static readonly string kstrDependentServiceListFieldName = "persistentchatserver_dependentservicelist";
        public static readonly string kstrServiceIdFieldName = "persistentchatserver_serviceid";
        public static readonly string kstrSiteIdFieldName = "persistentchatserver_siteid";
        public static readonly string kstrVersionFieldName = "persistentchatserver_version";
        public static readonly string kstrRoleFieldName = "persistentchatserver_role";

        // backup, the following items are all persistent chat server information, but currently no used
        //private readonly string kstrPersistentChatDatabaseFieldName = "PersistentChatDatabase";
        //private readonly string kstrMirrorPersistentChatDatabaseFieldName = "MirrorPersistentChatDatabase";
        //private readonly string kstrBackupPersistentChatDatabaseFieldName = "BackupPersistentChatDatabase";
        //private readonly string kstrBackupMirrorPersistentChatDatabaseFieldName = "BackupMirrorPersistentChatDatabase";
        //private readonly string kstrPersistentChatComplianceDatabaseFieldName = "PersistentChatComplianceDatabase";
        //private readonly string kstrMirrorPersistentChatComplianceDatabaseFieldName = "MirrorPersistentChatComplianceDatabase";
        //private readonly string kstrBackupPersistentChatComplianceDatabaseFieldName = "BackupPersistentChatComplianceDatabase";
        //private readonly string kstrBackupMirrorPersistentChatComplianceDatabaseFieldName = "BackupMirrorPersistentChatComplianceDatabase";
        #endregion

        #region Static values: power shell command
        static private readonly string kstrPSCmdGetService = "Get-CsService";
        static private readonly string kstrPSCmdParamPersistentChatServer = "-PersistentChatServer";
        static private readonly string kstrPSCmdParamIdentity = "-Identity";
        #endregion

        #region Constructors
        public SFBPersistentChatServerInfo(Microsoft.Rtc.Management.Xds.DisplayPersistentChatServer obXdsDisplayPersistentServer, string strFileStoreUncPath)
        {
            InitPersistentChatServerInfoByPSPersistentChatServer(obXdsDisplayPersistentServer, strFileStoreUncPath);
        }
        public SFBPersistentChatServerInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public SFBPersistentChatServerInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public SFBPersistentChatServerInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoSFBPersistentChatServer;
        }
        #endregion

        #region Tools
        private void InitPersistentChatServerInfoByPSPersistentChatServer(Microsoft.Rtc.Management.Xds.DisplayPersistentChatServer obXdsDisplayPersistentServer, string strFileStoreUncPath)
        {
            InnerSetItem(kstrPoolFqdnFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayPersistentServer.PoolFqdn));
            InnerSetItem(kstrRegistrarFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayPersistentServer.Registrar));
            InnerSetItem(kstrFileStoreFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayPersistentServer.FileStore));
            InnerSetItem(kstrFileStoreUncPathFieldName, CommonHelper.GetObjectStringValue(strFileStoreUncPath));
            InnerSetItem(kstrMtlsPortFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayPersistentServer.MtlsPort.ToString()));
            InnerSetItem(kstrDisplayNameFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayPersistentServer.DisplayName));
            InnerSetItem(kstrActiveServersFieldName, CommonHelper.ConvertListToString(obXdsDisplayPersistentServer.ActiveServers, SFBObjectInfo.kstrMemberSeparator, false));
            InnerSetItem(kstrInactiveServersFieldName, CommonHelper.ConvertListToString(obXdsDisplayPersistentServer.InactiveServers, SFBObjectInfo.kstrMemberSeparator, false));
            InnerSetItem(kstrDependentServiceListFieldName,CommonHelper.ConvertListToString(obXdsDisplayPersistentServer.DependentServiceList, SFBObjectInfo.kstrMemberSeparator, false));
            InnerSetItem(kstrServiceIdFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayPersistentServer.ServiceId));
            InnerSetItem(kstrSiteIdFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayPersistentServer.SiteId));
            InnerSetItem(kstrVersionFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayPersistentServer.Version.ToString()));
            InnerSetItem(kstrRoleFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayPersistentServer.Role));
        }
        private string GetServerFQDNFromIdentity(string strFileStoreIdentity)
        {
            // FileStore:PoolFqdn
            int nIndex = strFileStoreIdentity.IndexOf(':');
            if ((0 < nIndex) && (strFileStoreIdentity.Length > nIndex))
            {
                return strFileStoreIdentity.Substring(nIndex+1);
            }
            return "";
        }
        #endregion

        #region Powershell functions
        static public List<SFBPersistentChatServerInfo> GetAllCurPersistentChatServerInfo()
        {
            List<SFBPersistentChatServerInfo> lsSFBFileStoreServiceInfo = null;
            try
            {
                PowerShellHelper obPowerShellHelper = new PowerShellHelper(kstrLyncPSModuleName);
                Collection<PSObject> collectionPSChatCategoryInfos = obPowerShellHelper.Run(kstrPSCmdGetService, kstrPSCmdParamPersistentChatServer, null);
                if (null != collectionPSChatCategoryInfos)
                {
                    lsSFBFileStoreServiceInfo = new List<SFBPersistentChatServerInfo>();
                    foreach (PSObject obPSObject in collectionPSChatCategoryInfos)
                    {
                        Microsoft.Rtc.Management.Xds.DisplayPersistentChatServer obXdsDisplayPersistentServer = obPSObject.BaseObject as Microsoft.Rtc.Management.Xds.DisplayPersistentChatServer;
                        if (null != obXdsDisplayPersistentServer)
                        {
                            string strFileStoreIdentity = GetFileStoreUncPathByFileStoreIdentity(obXdsDisplayPersistentServer.FileStore, obPowerShellHelper);
                            lsSFBFileStoreServiceInfo.Add(new SFBPersistentChatServerInfo(obXdsDisplayPersistentServer, strFileStoreIdentity));
                        }
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "failed to invoke:[{0}] with parameter:[{1}]\n", kstrPSCmdGetService, kstrPSCmdParamPersistentChatServer);
                }
                obPowerShellHelper.Dispose();
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Exception in GetAllCurPersistentChatServerInfo, [{0}]\n", ex.Message);
            }
            return lsSFBFileStoreServiceInfo;
        }
        #endregion

        #region Inner static tools
        static private string GetFileStoreUncPathByFileStoreIdentity(string strFileStoreIdentity, PowerShellHelper obPowerShellHelper)
        {
            string strFileStoreUncPath = "";
            if (null == obPowerShellHelper)
            {
                obPowerShellHelper = new PowerShellHelper(kstrLyncPSModuleName);
            }

            Collection<PSObject> collectionPSChatCategoryInfos = obPowerShellHelper.Run(kstrPSCmdGetService, kstrPSCmdParamIdentity, strFileStoreIdentity);
            if (null != collectionPSChatCategoryInfos)
            {
                if (1 == collectionPSChatCategoryInfos.Count)
                {
                    Microsoft.Rtc.Management.Xds.DisplayFileStore obXdsDisplayFileStore = collectionPSChatCategoryInfos[0].BaseObject as Microsoft.Rtc.Management.Xds.DisplayFileStore;
                    if (null != obXdsDisplayFileStore)
                    {
                        strFileStoreUncPath = CommonHelper.GetSolidString(obXdsDisplayFileStore.UncPath);
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "!!!Error, Get our more than one file store by file store identity:[{0}]\n", strFileStoreIdentity);
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "failed to invoke:[{0}] with parameter:[{1}]\n", kstrPSCmdGetService, kstrPSCmdParamIdentity);
            }
            return strFileStoreUncPath;
        }
        #endregion
    }
}
