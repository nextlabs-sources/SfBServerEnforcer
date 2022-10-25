﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.IO;

// Power shell, reference Ststem.Management.Automation.dll
using System.Management.Automation;
using System.Management.Automation.Runspaces;

// Debug
using SFBCommon.NLLog;

namespace SFBCommon.Common
{
    public class PowerShellHelper : Logger, IDisposable
    {
        #region Const/read only values
        private const string kstrRTCSqlConnectionException = "Microsoft.Rtc.Common.Data.SqlConnectionException";
        #endregion

        //RunPowershell(@".\x.ps1", ""); If failed, this function will set last error code
        static public Collection<PSObject> RunPowershell(string strPSModule, string strPSCommandName, params string[] szParameters)
        {
            Collection<PSObject> psObjects = null;
            try
            {
                // Get the operating environment to run commands.
                InitialSessionState obInitSession = InitialSessionState.CreateDefault();
                obInitSession.ImportPSModule(new string[] { strPSModule });
                using (Runspace runspace = RunspaceFactory.CreateRunspace(obInitSession))
                {
                    runspace.Open();
                    using (Pipeline pipeline = runspace.CreatePipeline())
                    {
                        Command scriptCommand = new Command(strPSCommandName);
                        Collection<CommandParameter> commandParameters = new Collection<CommandParameter>();

                        if ((null != szParameters) && ((0 == szParameters.Length%2)))
                        {
                            for (int i = 1; i < szParameters.Length; i += 2)
                            {
                                CommandParameter commandParm = new CommandParameter(szParameters[i-1], szParameters[i]);
                                commandParameters.Add(commandParm);
                                scriptCommand.Parameters.Add(commandParm);
                            }
                        }
                        pipeline.Commands.Add(scriptCommand);

                        // Invoke
                        psObjects = pipeline.Invoke();
                        if (pipeline.Error.Count > 0)
                        {
                            psObjects = null; // Powershell command failed
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Run power shell command:[{0} {1}] failed.\nError info:\n\t{2}\n", strPSCommandName, string.Join("=", szParameters), pipeline.Error.ToString());
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Run power shell command:[{0} {1}] success.\nObjectCount:[{2}]\n", strPSCommandName, string.Join("=", szParameters), ((null==psObjects) ? -1 : psObjects.Count));
                        }

                        if (null != runspace)
                        {
                            runspace.Close();
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_REFERENCE_NOT_FOUND);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in RunPowershell, reference cannot found. {0} \n", ex.Message);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in RunPowershell. {0}\n", ex.Message);
                Exception exInnerException = ex.InnerException;
                if (null != exInnerException)
                {
                    Type tyInnerException = exInnerException.GetType();
                    if (tyInnerException.FullName.Equals(kstrRTCSqlConnectionException, StringComparison.OrdinalIgnoreCase))
                    {
                        LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_ACCESS_DENY);    // No permission to read data from Lync inner database
                    }
                    else
                    {
                        LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_UNKNOWN);
                    }
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in RunPowershell, inner exception. {0}\n", exInnerException.Message);
                }
                else
                {
                    LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_UNKNOWN);
                }
            }
            return psObjects;
        }

        #region Members
        private string m_strPSModule = "";
        private Runspace m_runspace = null;
        #endregion

        #region Constructors
        public PowerShellHelper(string strPSModule)
        {
            m_strPSModule = strPSModule;

            InitialSessionState obInitSession = InitialSessionState.CreateDefault();
            if (!string.IsNullOrEmpty(strPSModule))
            {
                obInitSession.ImportPSModule(new string[] { strPSModule });
            }
            m_runspace = RunspaceFactory.CreateRunspace(obInitSession);
            m_runspace.Open();
        }
        #endregion

        #region Desctructor
        ~PowerShellHelper()
        {
            Dispose();
        }
        #endregion

        #region Implement interface: IDisposable
        public void Dispose()
        {
            if (null != m_runspace)
            {
                m_runspace.Close();
                m_runspace.Dispose();
                m_runspace = null;
            }
        }
        #endregion

        #region Run PowerShell commands
        public Collection<PSObject> Run(string strPSCommandName, params string[] szParameters)
        {
            Collection<PSObject> psObjects = null;
            try
            {
                if (null == m_runspace)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Run space is null, please check\n");
                    return null;
                }

                using (Pipeline pipeline = m_runspace.CreatePipeline())
                {
                    Command scriptCommand = new Command(strPSCommandName);
                    Collection<CommandParameter> commandParameters = new Collection<CommandParameter>();

                    if ((null != szParameters) && ((0 == szParameters.Length % 2)))
                    {
                        for (int i = 1; i < szParameters.Length; i += 2)
                        {
                            CommandParameter commandParm = new CommandParameter(szParameters[i - 1], szParameters[i]);
                            commandParameters.Add(commandParm);
                            scriptCommand.Parameters.Add(commandParm);
                        }
                    }
                    pipeline.Commands.Add(scriptCommand);

                    // Invoke
                    psObjects = pipeline.Invoke();
                    if (pipeline.Error.Count > 0)
                    {
                        psObjects = null; // Powershell command failed
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Run power shell command:[{0} {1}] failed.\nError info:\n\t{2}\n", strPSCommandName, string.Join("=", szParameters), pipeline.Error.ToString());
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Run power shell command:[{0} {1}] success.\nObjectCount:[{2}]\n", strPSCommandName, string.Join("=", szParameters), ((null == psObjects) ? -1 : psObjects.Count));
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_REFERENCE_NOT_FOUND);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in RunPowershell, reference cannot found. {0} \n", ex.Message);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in RunPowershell. {0}\n", ex.Message);
                Exception exInnerException = ex.InnerException;
                if (null != exInnerException)
                {
                    Type tyInnerException = exInnerException.GetType();
                    if (tyInnerException.FullName.Equals(kstrRTCSqlConnectionException, StringComparison.OrdinalIgnoreCase))
                    {
                        LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_ACCESS_DENY);    // No permission to read data from Lync inner database
                    }
                    else
                    {
                        LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_UNKNOWN);
                    }
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in RunPowershell, inner exception. {0}\n", exInnerException.Message);
                }
                else
                {
                    LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_UNKNOWN);
                }
            }
            return psObjects;
        }
        #endregion
    }
}