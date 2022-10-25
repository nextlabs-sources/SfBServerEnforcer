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
    public class SFBFileStoreServiceInfo : SFBObjectInfo
    {
        #region Static values: field names
        public static readonly string kstrPoolFqdnFieldName = "filestore_poolfqdn";
        public static readonly string kstrShareNameFieldName = "filestore_sharename";
        public static readonly string kstrUNCPathFieldName = "filestore_uncpath";
        public static readonly string kstrIsDfsShareFieldName = "filestore_isdfsshare";
        public static readonly string kstrDependentServiceListFieldName = "filestore_dependentservicelist";
        public static readonly string kstrServiceIDFieldName = "filestore_serviceid";
        public static readonly string kstrSiteIDFieldName = "filestore_siteid";
        public static readonly string kstrVersionFieldName = "filestore_version";
        public static readonly string kstrRoleFieldName = "filestore_role";
        #endregion

        #region Static values: power shell command
        static private readonly string kstrPSCmdGetService = "Get-CsService";
        static private readonly string kstrPSCmdParamFileStore = "-FileStore";
        #endregion

        #region Constructors
        public SFBFileStoreServiceInfo(Microsoft.Rtc.Management.Xds.DisplayFileStore obXdsDisplayFileStore)
        {
            InitFileStoreServiceInfoByPSFileStoreService(obXdsDisplayFileStore);
        }
        public SFBFileStoreServiceInfo(params string[] szStrKeyAndValus) : base(szStrKeyAndValus)
        {
        }
        public SFBFileStoreServiceInfo(params KeyValuePair<string, string>[] szPairKeyAndValus) : base(szPairKeyAndValus)
        {
        }
        public SFBFileStoreServiceInfo(Dictionary<string, string> dirInfos) : base(dirInfos)
        {
        }
        #endregion

        #region Implement Abstract functions: SFBObjectInfo
        public override EMSFB_INFOTYPE GetSFBInfoType()
        {
            return EMSFB_INFOTYPE.emInfoSFBFileStoreService;
        }
        #endregion

        #region Tools
        private void InitFileStoreServiceInfoByPSFileStoreService(Microsoft.Rtc.Management.Xds.DisplayFileStore obXdsDisplayFileStore)
        {
            // Warning: in Microsoft.Rtc.Management.Xds.DisplayFileStore there is no file store identity info, this is an very important string to speicify a file store
            InnerSetItem(kstrPoolFqdnFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayFileStore.PoolFqdn));
            InnerSetItem(kstrShareNameFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayFileStore.ShareName));
            InnerSetItem(kstrUNCPathFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayFileStore.UncPath));
            InnerSetItem(kstrIsDfsShareFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayFileStore.IsDfsShare.ToString()));
            InnerSetItem(kstrDependentServiceListFieldName, CommonHelper.ConvertListToString(obXdsDisplayFileStore.DependentServiceList, SFBObjectInfo.kstrMemberSeparator, false));
            InnerSetItem(kstrServiceIDFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayFileStore.ServiceId));
            InnerSetItem(kstrSiteIDFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayFileStore.SiteId));
            InnerSetItem(kstrVersionFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayFileStore.Version.ToString()));
            InnerSetItem(kstrRoleFieldName, CommonHelper.GetObjectStringValue(obXdsDisplayFileStore.Role));
        }
        #endregion

        #region Powershell functions
        // Cannot get file store identity from power shell output object
        // The identity info is very important but cannot get, so current function is no used
        static private List<SFBFileStoreServiceInfo> GetAllCurFileStoreServiceInfo()
        {
            List<SFBFileStoreServiceInfo> lsSFBFileStoreServiceInfo = null;
            Collection<PSObject> collectionPSChatCategoryInfos = PowerShellHelper.RunPowershell(kstrLyncPSModuleName, kstrPSCmdGetService, kstrPSCmdParamFileStore, null);
            if (null != collectionPSChatCategoryInfos)
            {
                lsSFBFileStoreServiceInfo = new List<SFBFileStoreServiceInfo>();
                foreach (PSObject obPSObject in collectionPSChatCategoryInfos)
                {
                    Microsoft.Rtc.Management.Xds.DisplayFileStore obXdsDisplayFileStore = obPSObject.BaseObject as Microsoft.Rtc.Management.Xds.DisplayFileStore;
                    if (null != obXdsDisplayFileStore)
                    {
                        lsSFBFileStoreServiceInfo.Add(new SFBFileStoreServiceInfo(obXdsDisplayFileStore));
                    }
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "failed to invoke:[{0}] with parameter:[{1}]\n", kstrPSCmdGetService, kstrPSCmdParamFileStore);
            }
            return lsSFBFileStoreServiceInfo;
        }
        #endregion
    }
}
