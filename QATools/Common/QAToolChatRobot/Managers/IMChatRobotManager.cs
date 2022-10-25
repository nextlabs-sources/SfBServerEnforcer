using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using QAToolSFBCommon.NLLog;
using QAToolSFBCommon.Common;

namespace QAToolNLChatRobot.Managers
{
    public class IMChatRobotManager
    {
        #region Logger
        static private CLog theLog = CLog.GetLogger(typeof(IMChatRobotManager));
        #endregion

        #region Members
        private ReaderWriterLockSlim m_rwLockLstChatRobots = new ReaderWriterLockSlim();            //no-recursion lock
        private Dictionary<string, ChatRobot> m_dicChatRobots = new Dictionary<string,ChatRobot>(); // <Remote uri, IM Chat Robot>
        #endregion

        #region Constructors
        public IMChatRobotManager()
        {

        }
        #endregion

        #region Public functions
        public bool SaveChatRobot(string strRemoteUri, ChatRobot obChatRobot, bool bForceSave)
        {
            try
            {
                m_rwLockLstChatRobots.EnterWriteLock();
                if ((!string.IsNullOrEmpty(strRemoteUri)) && (null != obChatRobot))
                {
                    if (bForceSave || (!m_dicChatRobots.Keys.Contains(strRemoteUri)))   // Force or do not exist ==> save
                    {
                        CommonHelper.AddKeyValuesToDir(m_dicChatRobots, strRemoteUri, obChatRobot);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in SaveChatRobot, [{0}]\n", ex.Message);
            }
            finally
            {
                m_rwLockLstChatRobots.ExitWriteLock();
            }
            return false;
        }
        public ChatRobot GetChatRobot(string strRemoteUri)
        {
            try
            {
                m_rwLockLstChatRobots.EnterReadLock();
                return CommonHelper.GetValueByKeyFromDir(m_dicChatRobots, strRemoteUri, null);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in GetChatRobot, [{0}]\n", ex.Message);
            }
            finally
            {
                m_rwLockLstChatRobots.ExitReadLock();
            }
            return null;
        }
        public void DeleteChatRobot(string strRemoteUri)
        {
            try
            {
                m_rwLockLstChatRobots.EnterWriteLock();
                CommonHelper.RemoveKeyValuesFromDir(m_dicChatRobots, strRemoteUri);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in DeleteChatRobot, [{0}]\n", ex.Message);
            }
            finally
            {
                m_rwLockLstChatRobots.ExitWriteLock();
            }
        }
        #endregion

        #region Inner tools
        #endregion
    }
}
