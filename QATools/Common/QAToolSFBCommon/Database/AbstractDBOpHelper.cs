using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;

// Current project
using QAToolSFBCommon.Common;
using QAToolSFBCommon.NLLog;

namespace QAToolSFBCommon.Database
{
    enum EMSFB_DBPARAMTYPE
    {
        emDBParam_Unknown,

        emDBParam_TableName,
        emDBParam_FieldType,
        emDBParam_FieldName,
        emDBParam_FieldValue
    }

    enum EMSFB_DBEXCUTEEXCEPTION
    {
        emDBEx_Unknow,
        emDBEx_SyntaxError,
        emDBEx_DuplicateColumn,
        emDBEx_DuplicateEntity,
        emDBEx_DuplicateTable,
        emDBEx_DuplicateKey,
        emDBEx_ServerNotFound,
        emDBEx_LoginFailed
    }

    /// <summary>
    /// Note: Now params T[] support: params string[], params KeyValuePair<string, string>[]
    /// For example, if you want to create table, there are two right invoke way
    ///     1. CreateTable("TestTableName", "name", "varchar(255)", "value", "varchar(255)");
    //      2. CreateTable("TestTableName", "name", "varchar(255)", "value", "varchar(255)");
    /// </summary>
    abstract class AbstractDBOpHelper : IDisposable
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger("DBOpHelper");
        #endregion

        #region Const and Static read only values for database operator define
        private static readonly Dictionary<EMSFB_INFOCOMPAREOP, string> kdicCompareOp = new Dictionary<EMSFB_INFOCOMPAREOP,string>()
        {
            {EMSFB_INFOCOMPAREOP.emSearchOp_Equal, "="},
            {EMSFB_INFOCOMPAREOP.emSearchOp_NotEqual, "!="},
            {EMSFB_INFOCOMPAREOP.emSearchOp_AboveEqual, ">="},
            {EMSFB_INFOCOMPAREOP.emSearchOp_LessEqual, "<="},
            {EMSFB_INFOCOMPAREOP.emSearchOp_Above, ">"},
            {EMSFB_INFOCOMPAREOP.emSearchOp_Less, "<"},
            {EMSFB_INFOCOMPAREOP.emSearchOp_Like, "LIKE"},
        };
        private static readonly Dictionary<EMSFB_INFOLOGICOP, string> kdicLogicOp = new Dictionary<EMSFB_INFOLOGICOP, string>()
        {
            {EMSFB_INFOLOGICOP.emSearchLogicAnd, "AND"},
            {EMSFB_INFOLOGICOP.emSearchLogicOr, "OR"}
        };
        #endregion

        #region Static members and functions for database connections
        static private int s_nCheckInterval = 5 * 60 * 1000;    // five minutes
        static private AutoResetEvent s_eventDbConnectionMgr = new AutoResetEvent(false);
        static private Dictionary<int, KeyValuePair<Thread, DbConnection>> s_dirDbConnection = new Dictionary<int, KeyValuePair<Thread, DbConnection>>();
        static private ReaderWriterLockSlim s_rwLockDirDbConnection = new ReaderWriterLockSlim();//no-recursion lock
        static private void ManagerDbConnections()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Start manager DB connections\n");
            while(true)
            {
                try
                {
                    bool bReceivedSignal = s_eventDbConnectionMgr.WaitOne(s_nCheckInterval);
                    if (bReceivedSignal)
                    {
                        // Received event, exit loop to end thread
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Received db connection mgr event, exit DB connection manager thread\n");
                        break;
                    }
                    try
                    {
                        s_rwLockDirDbConnection.EnterWriteLock();

                        if ((null != s_dirDbConnection) && (0 < s_dirDbConnection.Count))
                        {
                            List<int> lsNeedRemovedItems = new List<int>();
                            foreach (KeyValuePair<int, KeyValuePair<Thread, DbConnection>> pairDbConnections in s_dirDbConnection)
                            {
                                // Check if the thread is alive or not
                                Thread obThreadInfo = pairDbConnections.Value.Key;
                                DbConnection obDbConnection = pairDbConnections.Value.Value;
                                if ((null == obThreadInfo) || (!obThreadInfo.IsAlive))
                                {
                                    if (null != obDbConnection)
                                    {
                                        obDbConnection.Dispose();
                                        obDbConnection = null;
                                    }
                                    lsNeedRemovedItems.Add(pairDbConnections.Key); // record the key which need removed
                                }
                            }
                            foreach (int nNeedRemovedItemId in lsNeedRemovedItems)
                            {
                                s_dirDbConnection.Remove(nNeedRemovedItemId);
                            }
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "current DB connections is empty\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in ManagerDbConnections, during management DB connections\n {0}\n", ex.Message);
                    }
                    finally
                    {
                        s_rwLockDirDbConnection.ExitWriteLock();
                    }
                }
                catch (Exception ex)
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in ManagerDbConnections\n {0}\n", ex.Message);
                }
            }
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "End manager DB connections\n");
        }
        static internal void StartDbConnectionMgrThread()
        {
            Common.ThreadHelper.AsynchronousInvokeHelper(true, ManagerDbConnections);
        }
        static internal void EndDbConnectionMgrThread()
        {
            s_eventDbConnectionMgr.Set();
        }
        #endregion

        #region Fields
        protected DbConnection CurDbConnection
        {
            get { return GetDbConnectionForCurrentThread(); }
        }
        #endregion

        #region Members
        protected string m_strDBServerAddr = "";
        protected uint m_unPortNumber = 0;
        protected string m_strCatalogName = ""; // MSSQL: Catalog, MYSQL: Database
        protected string m_strUserName = "";
        protected string m_strPassword = "";
        private bool m_bEstablishConnectSuccess = false;
        #endregion

        #region Interface: IDisponse 
        public abstract void Dispose();
        #endregion

        #region Execute SQL Command
        public bool ExecuteSQLNoQueryCommand(string strSQLCommand)
        {
            int nInfluenceLines = 0;
            return ExecuteSQLNoQueryCommand(strSQLCommand, out nInfluenceLines);
        }
        public bool ExecuteSQLNoQueryCommand(string strSQLCommand, out int nInfluenceLines) //ExecuteNonQuery, ADD, DELETE, UPDATE
        {
            bool bRet = false;
            nInfluenceLines = 0;
            try
            {
                CurDbConnection.Open();
                using (DbCommand obSqlCmd = CreateDbCommand(strSQLCommand, CurDbConnection))
                {
                    nInfluenceLines = obSqlCmd.ExecuteNonQuery();
                }
                bRet = true;
            }
            catch (DbException ex)
            {
				//we don't want use recursive call at dbexception handle, because it will make CurDbConnection complex
                EMSFB_DBEXCUTEEXCEPTION dbExcuteException=TranfromDBException(ex);
                if (WhetherReTryWhenException(dbExcuteException))
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "DbException in ExecuteSQLNoQueryCommand, {0}\n", ex.Message);
                    try
                    {
                        if (CurDbConnection.State.Equals(ConnectionState.Closed))
                        {
                            CurDbConnection.Open();
                        }
                        using (DbCommand obSqlCmd = CreateDbCommand(strSQLCommand, CurDbConnection))
                        {
                            nInfluenceLines = obSqlCmd.ExecuteNonQuery();
                        }
                        bRet = true;
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "ReExecuteSQLNoQueryCommand Success");
                    }
                    catch (Exception exRetry)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "DbException in Re ExecuteSQLCommand, {0}\n", exRetry.Message);
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "DbException in ExecuteSQLNoQueryCommand, {0}\n", ex.Message);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in ExecuteSQLCommand, {0}\n", ex.Message);
            }
            finally
            {
                CurDbConnection.Close();
            }
            return bRet;
        }
        public object ExecuteSQLQueryCommand(string strSQLCommand) //ExecuteScalar, return first column of the SELECT result
        {
            object obScalarResult = null;
            try
            {
                CurDbConnection.Open();
                using (DbCommand obSqlCmd = CreateDbCommand(strSQLCommand, CurDbConnection))
                {
                    obSqlCmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in ExecuteSQLCommand, {0}\n", ex.Message);
            }
            finally
            {
                CurDbConnection.Close();
            }
            return obScalarResult;
        }
        #endregion

        #region Database table operator
        public bool CreateTable<T>(bool bCheckTableExists, bool bKeyNotNull, string strTableName, string strKeyFieldName, string strKeyFieldType, params T[] szInFieldNameAndTeypes)
        {
            // Check and make standard SQL command parameters
            bool bCorrectParameters = false;
            T[] szFieldNameAndTeypes = null;
            if (CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_TableName, ref strTableName) && 
                CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_FieldName, ref strKeyFieldName) &&
                CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_FieldType, ref strKeyFieldType)
                )
            {
                bCorrectParameters = true;
                if (null != szInFieldNameAndTeypes)
                {
                    szFieldNameAndTeypes = CheckAndMakeStandardSqlKeyAndValues(EMSFB_DBPARAMTYPE.emDBParam_FieldName, EMSFB_DBPARAMTYPE.emDBParam_FieldType, szInFieldNameAndTeypes);
                    bCorrectParameters = (null != szFieldNameAndTeypes);
                }
            }
            if (bCorrectParameters)
            {
                string strSqlCommand = GetCreateTableSqlCommand(bCheckTableExists, bKeyNotNull, strTableName, strKeyFieldName, strKeyFieldType, szFieldNameAndTeypes);
                if (!String.IsNullOrEmpty(strSqlCommand))
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "CreateTable command: [{0}]", strSqlCommand);
                    return ExecuteSQLNoQueryCommand(strSqlCommand);
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Parameter error in AbstractDBOpHelper::CreateTable");
            }
            return false;
        }
        private bool DeleteTable(bool bCheckTableExists, string strTableName)    // Back, no need delete table
        {
            // Check and make standard SQL command parameters
            if (CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_TableName, ref strTableName))
            {
                string strSqlCommand = GetDropTableSqlCommand(bCheckTableExists, strTableName);
                if (!String.IsNullOrEmpty(strSqlCommand))
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "DropTable command: [{0}]", strSqlCommand);
                    return ExecuteSQLNoQueryCommand(strSqlCommand);
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Parameter error in AbstractDBOpHelper::DeleteTable");
            }
            return false;
        }
        public bool AddColumn<T>(string strTableName, params T[] szInFieldNameAndTeypes)
        {
            // Check and make standard SQL command parameters
            bool bCorrectParameters = false;
            T[] szFieldNameAndTeypes = null;
            if (CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_TableName, ref strTableName))
            {
                if (null != szInFieldNameAndTeypes) // For Add Column, szInFieldNameAndTeypes must not be null.
                {
                    szFieldNameAndTeypes = CheckAndMakeStandardSqlKeyAndValues(EMSFB_DBPARAMTYPE.emDBParam_FieldName, EMSFB_DBPARAMTYPE.emDBParam_FieldType, szInFieldNameAndTeypes);
                    bCorrectParameters = (null != szFieldNameAndTeypes);
                }
            }
            if (bCorrectParameters)
            {
                string strSqlCommand = GetAddColumnsSqlCommand(strTableName, szFieldNameAndTeypes);
                if (!String.IsNullOrEmpty(strSqlCommand))
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "AddColumn command: [{0}]", strSqlCommand);
                    return ExecuteSQLNoQueryCommand(strSqlCommand);
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Parameter error in AbstractDBOpHelper::AddColumn");
            }
            return false;
        }
        public bool DropColumn(string strTableName, params string[] szFieldNames)
        {
            // Check and make standard SQL command parameters
            bool bCorrectParameters = false;
            if (CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_TableName, ref strTableName))
            {
                if ((null != szFieldNames) && (0 < szFieldNames.Length)) // For Drop Column, szFieldNames must not be null or empty
                {
                    bCorrectParameters = true;
                    for (int i=0; i<szFieldNames.Length; ++i)
                    {
                        if (!CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_FieldName, ref szFieldNames[i]))
                        {
                            bCorrectParameters = false;
                            break;
                        }
                    }
                }
            }
            if (bCorrectParameters)
            {
                string strSqlCommand = GetDropColumnsSqlCommand(strTableName, szFieldNames);
                if (!String.IsNullOrEmpty(strSqlCommand))
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "DropColumn command: [{0}]", strSqlCommand);
                    return ExecuteSQLNoQueryCommand(strSqlCommand);
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Parameter error in AbstractDBOpHelper::DropColumn");
            }
            return false;
        }
        public bool AddIndex(string strTableName, string strFieldName)
        {
            // Check and make standard SQL command parameters
            if (CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_TableName, ref strTableName) &&
                CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_FieldName, ref strFieldName)
                )
            {
                string strSqlCommand = GetAddIndexSqlCommand(strTableName, strFieldName);
                if (!String.IsNullOrEmpty(strSqlCommand))
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "AddIndex command: [{0}]", strSqlCommand);
                    return ExecuteSQLNoQueryCommand(strSqlCommand);
                }
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Parameter error in AbstractDBOpHelper::AddIndex");
            }
            return false;
        }
        #endregion

        #region Database field operators
        public bool AddItem<T>(string strTableName, string strKeyFieldName, string strKeyFieldValue, params T[] szInFieldNameAndValues)
        {
            try
            {
                // Check and make standard SQL command parameters
                bool bCorrectParameters = false;
                T[] szFieldNameAndValues = null;
                if (CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_TableName, ref strTableName) &&
                    CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_FieldName, ref strKeyFieldName) &&
                    CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_FieldValue, ref strKeyFieldValue)
                    )
                {
                    bCorrectParameters = true;
                    if (null != szInFieldNameAndValues)
                    {
                        szFieldNameAndValues = CheckAndMakeStandardSqlKeyAndValues(EMSFB_DBPARAMTYPE.emDBParam_FieldName, EMSFB_DBPARAMTYPE.emDBParam_FieldValue, szInFieldNameAndValues);
                        bCorrectParameters = (null != szFieldNameAndValues);
                    }
                }
                if (bCorrectParameters)
                {
                    DataTable obDataTable = SelectItem(strTableName, false, false, strKeyFieldName, strKeyFieldValue);
                    string strSqlCommand = "";
                    if ((null != obDataTable) && (0 < obDataTable.Rows.Count))
                    {
                        strSqlCommand = GetUpdateSqlCommand(strTableName, strKeyFieldName, strKeyFieldValue, szFieldNameAndValues);
                    }
                    else
                    {
                        strSqlCommand = GetInsertSqlCommand(strTableName, strKeyFieldName, strKeyFieldValue, szFieldNameAndValues);
                    }
                    if (!String.IsNullOrEmpty(strSqlCommand))
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "AddItem command: [{0}]", strSqlCommand);
                        return ExecuteSQLNoQueryCommand(strSqlCommand);
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Parameter error in AbstractDBOpHelper::AddItem");
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in AddItem, {0}\n", ex.Message);
            }
            return false;
        }
        public bool DeleteItem(string strTableName, string strKeyFieldName, string strKeyFieldValue)
        {
            try
            {
                // Check and make standard SQL command parameters
                if (CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_TableName, ref strTableName) &&
                    CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_FieldName, ref strKeyFieldName) &&
                    CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_FieldValue, ref strKeyFieldValue)
                    )
                {
                    string strSqlCommand = GetDeleteSqlCommand(strTableName, strKeyFieldName, strKeyFieldValue);
                    if (!String.IsNullOrEmpty(strSqlCommand))
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "DeleteItem command: [{0}]", strSqlCommand);
                        return ExecuteSQLNoQueryCommand(strSqlCommand);
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Parameter error in AbstractDBOpHelper::DeleteItem");
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in AddItem, {0}\n", ex.Message);
            }
            return false;
        }
        public DataTable SelectItem<T>(string strTableName, bool bLogicAnd, bool bUsingLike, params T[] szInFieldNameAndValues)
        {
            try
            {
                // Check and make standard SQL command parameters
                bool bCorrectParameters = false;
                T[] szFieldNameAndValues = null;
                if (CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_TableName, ref strTableName))
                {
                    if (null != szInFieldNameAndValues)     // For Select Column, szInFieldNameAndValues must not be null.
                    {
                        szFieldNameAndValues = CheckAndMakeStandardSqlKeyAndValues(EMSFB_DBPARAMTYPE.emDBParam_FieldName, EMSFB_DBPARAMTYPE.emDBParam_FieldValue, szInFieldNameAndValues);
                        bCorrectParameters = (null != szFieldNameAndValues);
                    }
                }
                if (bCorrectParameters)
                {
                    string strSqlCommand = GetSelectSqlCommand(strTableName, bLogicAnd, bUsingLike, szFieldNameAndValues);
                    if (!String.IsNullOrEmpty(strSqlCommand))
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "SelectItem command: [{0}]", strSqlCommand);
                        using (DbDataAdapter obDbDataAdapter = CreateDbDataAdapter(strSqlCommand))
                        {
                            if (null != obDbDataAdapter)
                            {
                                using (DataSet obDataSet = CreateDataSet(obDbDataAdapter, strTableName))
                                {
                                    if (null != obDataSet)
                                    {
                                        return obDataSet.Tables[strTableName];
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Parameter error in AbstractDBOpHelper::SelectItem");
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in SelectItem, {0}\n", ex.Message);
            }
            LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DATA_READ_FAILED);
            return null;
        }
        public DataTable SelectItemEx(List<STUSFB_INFOFIELD> lsSpecifyOutFields, List<EMSFB_INFOTYPE> lsSearchScopes, List<KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>> lsSearchConditions)
        {
            try
            {
                // Check and make standard SQL command parameters
                if (((null != lsSearchScopes) && (0 < lsSearchScopes.Count)) && ((null != lsSearchConditions) && (0 < lsSearchConditions.Count)))
                {
                    string strOutFiledsPartCommand = GetSearchOutFiledsPartSQLCommand(lsSpecifyOutFields);
                    string strScopesPartSQLCommand = GetSearchScopesPartSQLCommand(lsSearchScopes);
                    string strConditionsPartSQLCommand = GetSearchConditionsPartSQLCommand(lsSearchConditions);

                    if (!string.IsNullOrEmpty(strOutFiledsPartCommand) && !string.IsNullOrEmpty(strScopesPartSQLCommand) && !string.IsNullOrEmpty(strConditionsPartSQLCommand))
                    {
                        string strFullSearchCommand = "select " + strOutFiledsPartCommand + " from " + strScopesPartSQLCommand + " where " + strConditionsPartSQLCommand;
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Full search command:[{0}]\n", strFullSearchCommand);
                        if (!string.IsNullOrEmpty(strFullSearchCommand))
                        {
                            using (DbDataAdapter obDbDataAdapter = CreateDbDataAdapter(strFullSearchCommand))
                            {
                                if (null != obDbDataAdapter)
                                {
                                    const string kstrDefaultTableName = "Table";
                                    using (DataSet obDataSet = CreateDataSet(obDbDataAdapter, ""))
                                    {
                                        if (null != obDataSet)
                                        {
                                            return obDataSet.Tables[kstrDefaultTableName];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in SelectItemEx, {0}\n", ex.Message);
            }
            return null;
        }
        #endregion

        #region Abstract, Ole Database
        abstract protected DbConnection EstablishDBConnection(string strDBServerAddr, uint unPortNumber, string strCatalogName, string strUserName, string strPassword);
        abstract protected DbDataAdapter CreateDbDataAdapter(string strSqlCmd);
        abstract protected DbCommandBuilder CreateDbCommandBuilder(DataAdapter obDataAdapter);
        abstract protected DbCommand CreateDbCommand(string strSQLCommand, DbConnection dbConnection);
        abstract protected EMSFB_DBEXCUTEEXCEPTION TranfromDBException(DbException dbException);
        #endregion

        #region DbConnection Manager
        private DbConnection GetDbConnectionForCurrentThread()
        {
            return GetDbConnectionByThreadId(Thread.CurrentThread.ManagedThreadId);
        }
        private DbConnection GetDbConnectionByThreadId(int nThreadId)
        {
            DbConnection obRetDbConnection = null;
            try
            {
                s_rwLockDirDbConnection.EnterWriteLock();

                if (null == s_dirDbConnection)
                {
                    s_dirDbConnection = new Dictionary<int, KeyValuePair<Thread, DbConnection>>();
                }
                if (s_dirDbConnection.Keys.Contains(nThreadId))
                {
                    Thread obThreadInfo = s_dirDbConnection[nThreadId].Key;
                    obRetDbConnection = s_dirDbConnection[nThreadId].Value;
                    if ((null == obThreadInfo) || (!obThreadInfo.IsAlive))
                    {
                        if (null != obRetDbConnection)
                        {
                            obRetDbConnection.Dispose();
                        }
                        obRetDbConnection = null;
                    }
                }
                if (null == obRetDbConnection)
                {
                    obRetDbConnection = EstablishDBConnection(m_strDBServerAddr, m_unPortNumber, m_strCatalogName, m_strUserName, m_strPassword);
                    KeyValuePair<Thread, DbConnection> pairNewTheadAndDbConnection = new KeyValuePair<Thread, DbConnection>(Thread.CurrentThread, obRetDbConnection);
                    s_dirDbConnection.Add(Thread.CurrentThread.ManagedThreadId, pairNewTheadAndDbConnection);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Exception in GetDbConnectionByThreadId, {0}\n", ex.Message);
            }
            finally
            {
                s_rwLockDirDbConnection.ExitWriteLock();
            }
            return obRetDbConnection;
        }
        #endregion

        #region Common Tools
        public bool GetEstablishConnectionFlag() { return m_bEstablishConnectSuccess; }
        protected void SetEstablishConnectionFlag(bool bEstablishConnectionSuccess) { m_bEstablishConnectSuccess = bEstablishConnectionSuccess; }
        protected void SaveDBConnectionInfo(string strDBServerAddr, uint unPortNumber, string strCatalogName, string strUserName, string strPassword)
        {
            m_strDBServerAddr = strDBServerAddr;
            m_unPortNumber = unPortNumber;
            m_strCatalogName = strCatalogName;
            m_strUserName = strUserName;
            m_strPassword = strPassword;
        }
        protected DataSet CreateDataSet(DbDataAdapter obDbDataAdapter, string strTableName)
        {
            try
            {
                if (null != obDbDataAdapter)
                {
                    DataSet obDataSet = new DataSet();
                    if (string.IsNullOrEmpty(strTableName))
                    {
                        obDbDataAdapter.Fill(obDataSet);
                    }
                    else
                    {
                        obDbDataAdapter.Fill(obDataSet, strTableName);
                    }
                    return obDataSet;
                }
            }
            catch (DbException ex)
            {
                //we don't want use recursive call at dbexception handle, because it will make CurDbConnection complex
                EMSFB_DBEXCUTEEXCEPTION dbExcuteException = TranfromDBException(ex);
                if (WhetherReTryWhenException(dbExcuteException))
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "DbException in CreateDataSet, {0}\n", ex.Message);
                    try
                    {
                        DataSet obDataSet = new DataSet();
                        obDbDataAdapter.Fill(obDataSet, strTableName);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelWarn, "ReCreateDataSet Success");
                        return obDataSet;
                    }
                    catch(Exception exRetry)
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "DbException in Re CreateDataSet, {0}\n", exRetry.Message);
                    }
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "DbException in CreateDataSet, {0}\n", ex.Message);
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in CreateDataSet, {0}\n", ex.Message);
            }
            return null;
        }
        private bool WhetherReTryWhenException(EMSFB_DBEXCUTEEXCEPTION dbExcuteException)
        {
            bool bresult = false;
            if (dbExcuteException != EMSFB_DBEXCUTEEXCEPTION.emDBEx_DuplicateColumn
                    && dbExcuteException != EMSFB_DBEXCUTEEXCEPTION.emDBEx_DuplicateEntity
                    && dbExcuteException != EMSFB_DBEXCUTEEXCEPTION.emDBEx_DuplicateTable
                    && dbExcuteException != EMSFB_DBEXCUTEEXCEPTION.emDBEx_SyntaxError
                    && dbExcuteException != EMSFB_DBEXCUTEEXCEPTION.emDBEx_DuplicateKey
                &&dbExcuteException!=EMSFB_DBEXCUTEEXCEPTION.emDBEx_ServerNotFound)
            {
                bresult = true;

            }
            return bresult;
        }
        #endregion

        #region SQL Command builder tools,  Now params T[] support: params string[], params KeyValuePair<string, string>[]
        #region Abstract/Virtual, SQL Command builder tools
        abstract protected string MakeAsStandardSqlValue(string strValue);

        abstract protected string GetCreateTableSqlCommand<T>(bool bCheckTableExists, bool bKeyNotNull, string strTableName, string strKeyFieldName, string strKeyFieldType, params T[] szFieldNameAndTeypes);
        abstract protected string GetDropTableSqlCommand(bool bCheckTableExists, string strTableName);
        abstract protected string GetAddColumnsSqlCommand<T>(string strTableName, params T[] szFieldNameAndValues);
        abstract protected string GetDropColumnsSqlCommand(string strTableName, params string[] szFieldNames);

        virtual protected string GetSelectSqlCommand<T>(string strTableName, bool bLogicAnd, bool bUsingLike, params T[] szFieldNameAndValues)
        {
            string strBasicSelectSqlString = "SELECT * FROM " + strTableName;
            string strLogicFlag = bLogicAnd ? " AND " : " OR ";
            string strCompareFlag = bUsingLike ? " LIKE " : " = ";
            string strWhereFilterString = GetKeyValueConnectString(strCompareFlag, strLogicFlag, true, false, szFieldNameAndValues);
            if (!string.IsNullOrEmpty(strWhereFilterString))
            {
                return strBasicSelectSqlString + " WHERE " + strWhereFilterString;
            }
            return strBasicSelectSqlString;
        }
        virtual protected string GetUpdateSqlCommand<T>(string strTableName, string strKeyFieldName, string strKeyFieldValue, params T[] szFieldNameAndValues)
        {
            // UPDATE Person SET FirstName = 'Fred' WHERE LastName = 'Wilson'
            string strCommand = "UPDATE " + strTableName + " SET ";
            strCommand += GetKeyValueConnectString("=", ",", true, false, szFieldNameAndValues);
            strCommand += " WHERE " + strKeyFieldName + " = '" + strKeyFieldValue + "' ";
            return strCommand;
        }
        virtual protected string GetInsertSqlCommand<T>(string strTableName, string strKeyFieldName, string strKeyFieldValue, params T[] szParamKeyAndValues)
        {
            // INSERT INTO Persons (LastName, Address) VALUES ('Wilson', 'Champs-Elysees')
            string strCommand = "INSERT INTO " + strTableName + " ";
            string strKeys = strKeyFieldName;
            string strValues = " '" + strKeyFieldValue + "'";
            if (szParamKeyAndValues is string[])
            {
                string[] szFieldNameAndValues = szParamKeyAndValues as string[];
                for (int i = 1; i < szFieldNameAndValues.Length; i += 2)
                {
                    strKeys += ", " + szFieldNameAndValues[i - 1];
                    strValues += ", '" + szFieldNameAndValues[i] + "'";
                }
                strCommand += "( " + strKeys + " ) VALUES ( " + strValues + " )";
            }
            else if (szParamKeyAndValues is KeyValuePair<string, string>[])
            {
                KeyValuePair<string, string>[] szFieldNameAndValues = szParamKeyAndValues as KeyValuePair<string, string>[];
                for (int i = 0; i < szFieldNameAndValues.Length; ++i)
                {
                    strKeys += ", " + szFieldNameAndValues[i].Key;
                    strValues += ", '" + szFieldNameAndValues[i].Value + "'";
                }
                strCommand += "( " + strKeys + " ) VALUES ( " + strValues + " )";
            }
            else
            {
                strCommand = "";
            }
            return strCommand;
        }
        virtual protected string GetDeleteSqlCommand(string strTableName, string strKeyFieldName, string strKeyFieldValue)
        {
            return "DELETE FROM " + strTableName + " WHERE " + strKeyFieldName + " = '" + strKeyFieldValue + "'";
        }
        virtual protected string GetAddIndexSqlCommand(string strTableName, string strFieldName)
        {
            return "CREATE INDEX " + strFieldName + " ON " + strTableName + "( " + strFieldName + " )";
        }
        #endregion

        #region Common SQL Command builder tools
        protected bool CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE emDBParamType, ref string strDBParam)
        {
            // Inner const values
            const char kstrWhiteSpace = ' ';
            const char kstrSingleQuote = '\'';
            const char kstrBackSlash = '\\';

            bool bRet = false;
            switch (emDBParamType)
            {
            case EMSFB_DBPARAMTYPE.emDBParam_TableName:
            {
                // SFBE table name cannot null and contains white space, single quote and backslash
                bRet = (!string.IsNullOrEmpty(strDBParam)) && (!CommonHelper.ContainsOneOfChars(strDBParam, kstrWhiteSpace, kstrSingleQuote, kstrBackSlash));
                break;
            }
            case EMSFB_DBPARAMTYPE.emDBParam_FieldType:
            {
                // SFBE field type cannot null and contains white spaces, single quote and backslash
                bRet = (!string.IsNullOrEmpty(strDBParam)) && (!CommonHelper.ContainsOneOfChars(strDBParam, kstrWhiteSpace, kstrSingleQuote, kstrBackSlash));
                break;
            }
            case EMSFB_DBPARAMTYPE.emDBParam_FieldName:
            {
                // SFBE field name cannot null and contains white spaces, single quote and backslash
                bRet = (!string.IsNullOrEmpty(strDBParam)) && (!CommonHelper.ContainsOneOfChars(strDBParam, kstrWhiteSpace, kstrSingleQuote, kstrBackSlash));
                break;
            }
            case EMSFB_DBPARAMTYPE.emDBParam_FieldValue:
            {
                // SFBE field value cannot null and need check and make sure it is standard SQL command value
                if (null != strDBParam)
                {
                    bRet = true;
                    strDBParam = MakeAsStandardSqlValue(strDBParam);
                }
                break;
            }
            default:
            {
                break;
            }
            }
            return bRet;
        }
        protected T[] CheckAndMakeStandardSqlKeyAndValues<T>(EMSFB_DBPARAMTYPE emDBParamTypeKey, EMSFB_DBPARAMTYPE emDBParamTypeValue, params T[] szParamKeyAndValues)
        {
            if (null != szParamKeyAndValues)
            {
                if (szParamKeyAndValues is string[])
                {
                    string[] szKeyAndValues = szParamKeyAndValues as string[];
                    if (0 == (szKeyAndValues.Length%2))
                    {
                        for (int i = 1; i < szKeyAndValues.Length; i += 2)
                        {
                            if ((!CheckAndMakeStandardSQLCommandParams(emDBParamTypeKey, ref szKeyAndValues[i - 1])) || (!CheckAndMakeStandardSQLCommandParams(emDBParamTypeValue, ref szKeyAndValues[i])))
                            {
                                return null;
                            }
                        }
                        return szKeyAndValues as T[];
                    }
                    return null;
                }
                else if (szParamKeyAndValues is KeyValuePair<string, string>[])
                {
                    KeyValuePair<string, string>[] szOrgKeyAndValues = szParamKeyAndValues as KeyValuePair<string, string>[];
                    KeyValuePair<string, string>[] szKeyAndValues = new KeyValuePair<string, string>[szOrgKeyAndValues.Length];
                    for (int i = 0; i < szOrgKeyAndValues.Length; ++i)
                    {
                        string strCurKey = szOrgKeyAndValues[i].Key;
                        string strCurValue = szOrgKeyAndValues[i].Value;
                        if ((!CheckAndMakeStandardSQLCommandParams(emDBParamTypeKey, ref strCurKey)) || (!CheckAndMakeStandardSQLCommandParams(emDBParamTypeValue, ref strCurValue)))
                        {
                            return null;
                        }
                        szKeyAndValues[i] = new KeyValuePair<string, string>(strCurKey, strCurValue);
                    }
                    return szKeyAndValues as T[];
                }
            }
            return null;
        }
        protected string GetKeyValueConnectString<T>(string strKeyValueSeparator, string strKeysSeparator, bool bAddSingleQuoteForValue, bool bEndWithKeysSeprator, params T[] szParamKeyAndValues)
        {
            if ((null == szParamKeyAndValues) || (0 >= szParamKeyAndValues.Length))
            {
                return "";
            }

            // Make super separators
            string strSuperKeyValueSeparator = strKeyValueSeparator + " '";
            string strSuperKeysSeparator = "' " + strKeysSeparator;
            if (!bAddSingleQuoteForValue)
            {
                strSuperKeyValueSeparator = strKeyValueSeparator + " ";
                strSuperKeysSeparator = " " + strKeysSeparator;
            }

            // Connect key and values
            StringBuilder strKeyValueConnectString = new StringBuilder("");
            if (szParamKeyAndValues is string[])
            {
                string[] szKeyAndValues = szParamKeyAndValues as string[];
                for (int i = 1; i < szKeyAndValues.Length; i += 2)
                {
                    strKeyValueConnectString.Append(szKeyAndValues[i - 1] + strSuperKeyValueSeparator + szKeyAndValues[i] + strSuperKeysSeparator);
                }
            }
            else if (szParamKeyAndValues is KeyValuePair<string, string>[])
            {
                KeyValuePair<string, string>[] szKeyAndValues = szParamKeyAndValues as KeyValuePair<string, string>[];
                for (int i = 0; i < szKeyAndValues.Length; ++i)
                {
                    strKeyValueConnectString.Append(szKeyAndValues[i].Key + strSuperKeyValueSeparator + szKeyAndValues[i].Value + strSuperKeysSeparator);
                }
            }
            if (!bEndWithKeysSeprator)
            {
                CommonHelper.SubStringBuilder(ref strKeyValueConnectString, strKeysSeparator.Length);
            }
            return strKeyValueConnectString.ToString();
        }
        
        protected string GetSearchOutFiledsPartSQLCommand(List<STUSFB_INFOFIELD> lsSpecifyOutFields)
        {
            // eg: select sfb.sfbmeetingvariabletable.uri, sfb.sfbmeetingtable.creator, isstaticmeeting from sfb.sfbmeetingtable, sfb.sfbmeetingvariabletable where (sfb.sfbmeetingtable.uri = sfb.sfbmeetingvariabletable.uri and sfb.sfbmeetingtable.expirytime > '2017-02-00T17:33:06Z' and sfb.sfbmeetingvariabletable.donemanulclassify = 'Yes') or (sfb.sfbmeetingtable.uri = sfb.sfbmeetingvariabletable.uri and sfb.sfbmeetingtable.isstaticmeeting = 'True')
            if ((null == lsSpecifyOutFields) || (0 >= lsSpecifyOutFields.Count))
            {
                return " * "; // out put all matched info
            }
            else
            {
                const string kstrSepConnectOutFields = ", ";
                StringBuilder strOutFieldsPartCommand = new StringBuilder("");
                foreach (STUSFB_INFOFIELD stuConditionField in lsSpecifyOutFields)
                {
                    string strInfoConditionFieldCommand = GetInfoFieldPartSQLCommand(stuConditionField, false, EMSFB_DBPARAMTYPE.emDBParam_FieldName);
                    if (!string.IsNullOrEmpty(strInfoConditionFieldCommand))
                    {
                        strOutFieldsPartCommand.Append(" " + strInfoConditionFieldCommand + kstrSepConnectOutFields);
                    }
                    else
                    {
                        strOutFieldsPartCommand.Length = 0;
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error command in GetSearchOutFiledsPartSQLCommand");
                        break;  // Error command
                    }
                }
                CommonHelper.SubStringBuilder(ref strOutFieldsPartCommand, kstrSepConnectOutFields.Length);
                return strOutFieldsPartCommand.ToString();
            }
        }
        protected string GetSearchScopesPartSQLCommand(List<EMSFB_INFOTYPE> lsSearchScopes)
        {
            // eg: select sfb.sfbmeetingvariabletable.uri, sfb.sfbmeetingtable.creator, isstaticmeeting from sfb.sfbmeetingtable, sfb.sfbmeetingvariabletable where (sfb.sfbmeetingtable.uri = sfb.sfbmeetingvariabletable.uri and sfb.sfbmeetingtable.expirytime > '2017-02-00T17:33:06Z' and sfb.sfbmeetingvariabletable.donemanulclassify = 'Yes') or (sfb.sfbmeetingtable.uri = sfb.sfbmeetingvariabletable.uri and sfb.sfbmeetingtable.isstaticmeeting = 'True')
            StringBuilder strScopesPartSQLCommand = new StringBuilder("");
            if ((null != lsSearchScopes) && (0 < lsSearchScopes.Count))
            {
                const string kstrSepConnectScopesFields = ", ";
                foreach (EMSFB_INFOTYPE emTableInfoType in lsSearchScopes)
                {
                    StuTableInfo stuTableInfo = CommonHelper.GetValueByKeyFromDir(SFBDBMgr.kdirTableNames, emTableInfoType, new StuTableInfo("", "", "", null, false));
                    string strTableName = stuTableInfo.strTableName;
                    if (CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_TableName, ref strTableName))
                    {
                        strScopesPartSQLCommand.Append(strTableName + kstrSepConnectScopesFields);
                    }
                    else
                    {
                        strScopesPartSQLCommand.Length = 0;
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!! TableName:[{0}] error in GetSearchScopesPartSQLCommand", strTableName);
                        break;  // Error command
                    }
                }
                CommonHelper.SubStringBuilder(ref strScopesPartSQLCommand, kstrSepConnectScopesFields.Length);
            }
            return strScopesPartSQLCommand.ToString();
        }
        protected string GetSearchConditionsPartSQLCommand(List<KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>> lsSearchConditions)
        {
            // eg: select sfb.sfbmeetingvariabletable.uri, sfb.sfbmeetingtable.creator, isstaticmeeting from sfb.sfbmeetingtable, sfb.sfbmeetingvariabletable where (sfb.sfbmeetingtable.uri = sfb.sfbmeetingvariabletable.uri and sfb.sfbmeetingtable.expirytime > '2017-02-00T17:33:06Z' and sfb.sfbmeetingvariabletable.donemanulclassify = 'Yes') or (sfb.sfbmeetingtable.uri = sfb.sfbmeetingvariabletable.uri and sfb.sfbmeetingtable.isstaticmeeting = 'True')
            StringBuilder strConditionsPartSQLCommand = new StringBuilder("");
            if ((null != lsSearchConditions) && (0 < lsSearchConditions.Count))
            {
                int i = 0;
                string strSepNextConnectConditionGroups = "";
                foreach (KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP> pairConditionGroupInfo in lsSearchConditions)
                {
                    strSepNextConnectConditionGroups = CommonHelper.GetValueByKeyFromDir(kdicLogicOp, pairConditionGroupInfo.Key, "");
                    if ((!string.IsNullOrEmpty(strSepNextConnectConditionGroups)) || (0 == (lsSearchConditions.Count - i - 1)))   // the last one no need set group logic operator
                    {
                        string strConditionGroupCommand = GetConditionGroupPartSQLCommand(pairConditionGroupInfo.Value);
                        if (!string.IsNullOrEmpty(strConditionGroupCommand))
                        {
                            strConditionsPartSQLCommand.Append(" " + strConditionGroupCommand + strSepNextConnectConditionGroups);
                        }
                        else
                        {
                            strConditionsPartSQLCommand.Length = 0;
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error command in GetSearchConditionsPartSQLCommand 1");
                            break;  // Error command
                        }
                    }
                    else
                    {
                        strConditionsPartSQLCommand.Length = 0;
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error command in GetSearchConditionsPartSQLCommand 2");
                        break;  // Error command
                    }
                    ++i;
                }
                CommonHelper.SubStringBuilder(ref strConditionsPartSQLCommand, strSepNextConnectConditionGroups.Length);
            }
            return strConditionsPartSQLCommand.ToString();
        }
        
        protected string GetConditionGroupPartSQLCommand(STUSFB_CONDITIONGROUP stuConditionGroup)
        {
            StringBuilder strConditionsPartSQLCommand = new StringBuilder("");
            string kstrSepConditionGroupItems = CommonHelper.GetValueByKeyFromDir(kdicLogicOp, stuConditionGroup.emLogicOp, "");
            if ((!string.IsNullOrEmpty(kstrSepConditionGroupItems)) || (1 == stuConditionGroup.lsComditonItems.Count))  // Only one item, no need set item logic operator
            {
                foreach (STUSFB_INFOITEM stuInfoItem in stuConditionGroup.lsComditonItems)
                {
                    string strInfoItemCommand = GetInfoItemPartSQLCommand(stuInfoItem);
                    if (!string.IsNullOrEmpty(strInfoItemCommand))
                    {
                        strConditionsPartSQLCommand.Append(" " + strInfoItemCommand + " " + kstrSepConditionGroupItems);
                    }
                    else
                    {
                        strConditionsPartSQLCommand.Length = 0;
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error command in GetConditionGroupPartSQLCommand");
                        break;  // Error command
                    }
                }
                CommonHelper.SubStringBuilder(ref strConditionsPartSQLCommand, kstrSepConditionGroupItems.Length);
            }
            if (0 < strConditionsPartSQLCommand.Length)
            {
                return "( " + strConditionsPartSQLCommand.ToString() + " )";
            }
            else
            {
                return "";
            }
        }
        protected string GetInfoItemPartSQLCommand(STUSFB_INFOITEM stuInfoItem)
        {
            string strFieldKeyCommand = GetInfoFieldPartSQLCommand(stuInfoItem.stuFiledName, false, EMSFB_DBPARAMTYPE.emDBParam_FieldName);
            string strFieldValueCommand = GetInfoFieldPartSQLCommand(stuInfoItem.stuFiledValue, true, EMSFB_DBPARAMTYPE.emDBParam_FieldValue);
            string strItemOp = CommonHelper.GetValueByKeyFromDir(kdicCompareOp, stuInfoItem.emInfoCompareOp, "");
            if ((!string.IsNullOrEmpty(strFieldKeyCommand)) && (!string.IsNullOrEmpty(strFieldValueCommand)) && (!string.IsNullOrEmpty(strItemOp)))
            {
                return strFieldKeyCommand + " " + strItemOp + " " + strFieldValueCommand;
            }
            return null;
        }
        protected string GetInfoFieldPartSQLCommand(STUSFB_INFOFIELD stuInfoField, bool bAddSingleQuote, EMSFB_DBPARAMTYPE emDBParamType)
        {
            if (!string.IsNullOrEmpty(stuInfoField.strField))
            {
                string strCurField = stuInfoField.strField;
                StuTableInfo stuTableInfo = CommonHelper.GetValueByKeyFromDir(SFBDBMgr.kdirTableNames, stuInfoField.emTableInfoType, new StuTableInfo("", "", "", null, false));
                string strTableName = stuTableInfo.strTableName;
                if (string.IsNullOrEmpty(strTableName))
                {
                    if (CheckAndMakeStandardSQLCommandParams(emDBParamType, ref strCurField))
                    {
                        if (bAddSingleQuote)
                        {
                            return "'" + strCurField + "'";
                        }
                        else
                        {
                            return strCurField;
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!! Parameter error error in GetInfoFieldPartSQLCommand. Field:[{0}], Type:[{1}]\n", strCurField, emDBParamType);
                    }
                }
                else
                {
                    if (CheckAndMakeStandardSQLCommandParams(EMSFB_DBPARAMTYPE.emDBParam_TableName, ref strTableName))
                    {
                        if (EMSFB_DBPARAMTYPE.emDBParam_FieldValue == emDBParamType)
                        {
                            emDBParamType = EMSFB_DBPARAMTYPE.emDBParam_FieldName;
                        }
                        if (CheckAndMakeStandardSQLCommandParams(emDBParamType, ref strCurField))
                        {
                            return strTableName + "." + strCurField;
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!! Parameter error error in GetInfoFieldPartSQLCommand. Field:[{0}], Type:[{1}]\n", strCurField, emDBParamType);
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!! Parameter error error in GetInfoFieldPartSQLCommand. TableName:[{0}]\n", strTableName);
                    }
                }
            }
            return "";
        }
        #endregion

        #endregion
    }
}
