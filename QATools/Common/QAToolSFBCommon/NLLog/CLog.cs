using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using log4net;
using System.Diagnostics.Tracing;
using System.IO;

// Current project
using QAToolSFBCommon.Common;

namespace QAToolSFBCommon.NLLog
{
    public enum EMSFB_LOGLEVEL
    {
        emLogLevelDebug,
        emLogLevelInfo,
        emLogLevelWarn,
        emLogLevelError,
        emLogLevelFatal
    }

    public class CLog
    {
        #region Static initialize
        static internal void Init(EMSFB_MODULE emModule)
        {
            try
            {
                string strCfgFile = ConfigureFileManager.GetCfgFilePath(EMSFB_CFGTYPE.emCfgLog, emModule);
                if (File.Exists(strCfgFile))
                {
                    InnerInit(strCfgFile);
                }
                else
                {
                    OutputTraceLog("Notify: cannot find config files. SFB install path is:[{0}]\n", CommonHelper.kstrSFBInstallPath);
                }
            }
            catch (Exception ex)
            {
                OutputTraceLog("Exception happened in CLog::Init,[{0}]\n", ex.Message);
            }
        }
        #endregion

        #region Const Values
        private const string kstrCfgFileExtension = ".xml";
        #endregion

        #region Members
        static private  List<KeyValuePair<log4net.ILog,CLog>> m_lstLogger = new List<KeyValuePair<ILog,CLog>>();
        static private  Object logLocker = new Object();

        private log4net.ILog m_log = null;
        #endregion

        #region Static function
        static public void OutputTraceLog(string strFormat, params object[] szArgs)
        {
            Trace.TraceInformation(strFormat, szArgs);
        }
        static private void InnerInit(string strCfgFileName)  // when we want to use the CLog, we need initialize it by log format configure file first.
        {
            try
            {
                OutputTraceLog("Load Log config file from:" + strCfgFileName);
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(strCfgFileName));   // Init log4net by configure file
            }
            catch(Exception ex)
            {
                OutputTraceLog("Exception happened in CLog::InnerInit,[{0}]\n", ex.Message);
            }
        }
        static public CLog GetLogger(string strName)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(strName);
            return GetWrapperLog(log);
        }
        static public CLog GetLogger(Type typeName)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(typeName);
            return GetWrapperLog(log);
        }

        static private CLog GetWrapperLog(log4net.ILog log)
        {
            CLog existWrapperLog = FindExistWrapperLog(log);
            if (null == existWrapperLog)
            {
                return CreateWrapperLog(log);
            }
            else
            {
                return existWrapperLog;
            }
        }
        static private CLog FindExistWrapperLog(log4net.ILog log)
        {
            CLog wrapperLog = null;
            lock (logLocker)
            {
                foreach (KeyValuePair<log4net.ILog, CLog> wrapperLogInfo in m_lstLogger)
                {
                    if (wrapperLogInfo.Key.Equals(log))
                    {
                        wrapperLog = wrapperLogInfo.Value;
                        break;
                    }
                }
            }
            return wrapperLog;
        }
        static private CLog CreateWrapperLog(log4net.ILog log)
        {
            CLog wrapperLog = new CLog(log);
            lock (logLocker)
            {
                m_lstLogger.Add(new KeyValuePair<ILog, CLog>(log, wrapperLog));
            }
            return wrapperLog;
        }
        #endregion

        #region Constructors: all constructors is private. User cannot create CLog object directory and must be get the instance by GetLogger.
        private CLog(log4net.ILog log)
        {
            m_log = log;
        }
        #endregion

        #region Public functions
        /* Test if a level is enabled for logging */
        public bool IsDebugEnabled { get { return m_log.IsDebugEnabled; } }
        public bool IsInfoEnabled { get { return m_log.IsInfoEnabled; } }
        public bool IsWarnEnabled { get { return m_log.IsWarnEnabled; } }
        public bool IsErrorEnabled { get { return m_log.IsErrorEnabled; } }
        public bool IsFatalEnabled { get { return m_log.IsFatalEnabled; } }

        /* Log a message object */
        public void OutputLog(EMSFB_LOGLEVEL emLogLevel, string strFormat, params object[] szArgs)
        {
            try
            {
                switch (emLogLevel)
                {
                case EMSFB_LOGLEVEL.emLogLevelDebug:
                {
                    m_log.DebugFormat(strFormat, szArgs);
                    break;
                }
                case EMSFB_LOGLEVEL.emLogLevelInfo:
                {
                    m_log.InfoFormat(strFormat, szArgs);
                    break;
                }
                case EMSFB_LOGLEVEL.emLogLevelWarn:
                {
                    m_log.WarnFormat(strFormat, szArgs);
                    break;
                }
                case EMSFB_LOGLEVEL.emLogLevelError:
                {
                    m_log.ErrorFormat(strFormat, szArgs);
                    break;
                }
                case EMSFB_LOGLEVEL.emLogLevelFatal:
                {
                    m_log.FatalFormat(strFormat, szArgs);
                    break;
                }
                default:
                {
                    break;
                }
                }
            }
            catch (Exception ex)
            {
                OutputTraceLog("Exception happened in CLog::OutputLog,[{0}]\n", ex.Message);
            }
        }
        #endregion
    }
}
