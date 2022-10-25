using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;

// Current project
using QAToolSFBCommon.NLLog;

namespace QAToolSFBCommon.Database
{
    class DBOpMYSQLHelper : AbstractDBOpHelper
    {
        #region Constructors
        public DBOpMYSQLHelper(string strDBServerAddr, uint unPortNumber, string strCatalogName, string strUserName, string strPassword)
        {
            try
            {
                SaveDBConnectionInfo(strDBServerAddr, unPortNumber, strCatalogName, strUserName, strPassword);
                SetEstablishConnectionFlag(null != CurDbConnection);
            }
            catch(Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in DBOpHelper constructor, {0}\n", ex.Message);
            }
        }
        #endregion

        public override void Dispose()
        {
            // Note, no need dispose CurDbConnection, we have a manager thread to do this
            // here we can release some resource for current object
        }

        #region Override, Ole Database
        override protected DbConnection EstablishDBConnection(string strDBServerAddr, uint unPortNumber, string strCatalogName, string strUserName, string strPassword)
        {
            // Create SQL connection string
            MySqlConnectionStringBuilder obSqlConnectionString = new MySqlConnectionStringBuilder();
            obSqlConnectionString.Server = strDBServerAddr;
            obSqlConnectionString.Port = unPortNumber;
            obSqlConnectionString.Database = strCatalogName;
            obSqlConnectionString.UserID = strUserName;
            obSqlConnectionString.Password = strPassword;
            return new MySqlConnection(obSqlConnectionString.ConnectionString);
        }
        override protected DbDataAdapter CreateDbDataAdapter(string strSQLCommand)
        {
            try
            {
                MySqlConnection theMySqlConnection = CurDbConnection as MySqlConnection;
                return new MySqlDataAdapter(strSQLCommand, theMySqlConnection);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in CreateDataAdapter, {0}\n", ex.Message);
            }
            return null;
        }
        override protected DbCommandBuilder CreateDbCommandBuilder(DataAdapter obDataAdapter)
        {
            return new MySqlCommandBuilder(obDataAdapter as MySqlDataAdapter);
        }
        override protected DbCommand CreateDbCommand(string strSQLCommand, DbConnection dbConnection)
        {
            try
            {
                MySqlConnection theMySqlConnection = dbConnection as MySqlConnection;
                return new MySqlCommand(strSQLCommand, theMySqlConnection);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in CreateDbCommand, {0}\n", ex.Message);
            }
            return null;
        }
        #endregion

        #region SQL Command builder tools. Now params T[] support: params string[], params KeyValuePair<string, string>[]
        override protected string MakeAsStandardSqlValue(string strValue)
        {
            strValue = strValue.Replace("'", "''");
            return strValue.Replace("\\", "\\\\");
        }
        override protected string GetCreateTableSqlCommand<T>(bool bCheckTableExists, bool bKeyNotNull, string strTableName, string strKeyFieldName, string strKeyFieldType, params T[] szFieldNameAndTeypes)
        {
            string strCreateTable = bCheckTableExists ? "CREATE TABLE IF NOT EXISTS " : "CREATE TABLE ";
            if (bKeyNotNull)
            {
                strKeyFieldType = strKeyFieldType + " NOT NULL ";
            }
            string strSqlCommand = strCreateTable + strTableName + " ( " + strKeyFieldName + " " + strKeyFieldType + ", ";
            strSqlCommand += GetKeyValueConnectString(" ", ",", false, true, szFieldNameAndTeypes);
            strSqlCommand += "PRIMARY KEY (" + strKeyFieldName + ") )";
            return strSqlCommand;
        }
        override protected string GetDropTableSqlCommand(bool bCheckTableExists, string strTableName)
        {
            return bCheckTableExists ? ("DROP TABLE IF EXISTS " + strTableName) : ("DROP TABLE " + strTableName);
        }
        override protected string GetAddColumnsSqlCommand<T>(string strTableName, params T[] szFieldNameAndValues)
        {
            // ALTER TABLE TableName ADD (field1 LONGTEXT, field2 LONGTEXT)
            string strSqlAddColumnsCommand = "ALTER TABLE " + strTableName + " ADD ( ";
            strSqlAddColumnsCommand += GetKeyValueConnectString(" ", ",", false, false, szFieldNameAndValues) + " )";
            return strSqlAddColumnsCommand;
        }
        override protected string GetDropColumnsSqlCommand(string strTableName, params string[] szFieldNames)
        {
            // ALTER TABLE TableName DROP COLUMN field1, DROP COLUMN field1
            if (0 < szFieldNames.Length)
            {
                StringBuilder strSqlDropColumnsCommand = new StringBuilder("ALTER TABLE " + strTableName);
                strSqlDropColumnsCommand.Append(" DROP COLUMN " + szFieldNames[0]);
                for (int i = 1; i < szFieldNames.Length; ++i)
                {
                    strSqlDropColumnsCommand.Append(", DROP COLUMN " + szFieldNames[i]);
                }
                return strSqlDropColumnsCommand.ToString();
            }
            return "";
        }
        #endregion

        protected override EMSFB_DBEXCUTEEXCEPTION TranfromDBException(DbException dbException)
        {
            EMSFB_DBEXCUTEEXCEPTION result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_Unknow;
            //You have an error in your SQL syntax; check the manual that corresponds to your MySQL server version for the right syntax to use near 'l' at line 1
            if (dbException.Message.IndexOf("You have an error in your SQL syntax", StringComparison.OrdinalIgnoreCase) > -1)
            {
                result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_SyntaxError;
            }
            //Duplicate column name 'chatcategoryidentity'
            else if (dbException.Message.IndexOf("Duplicate column name", StringComparison.OrdinalIgnoreCase) > -1)
            {
                result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_DuplicateColumn;
            }
            else if (dbException.Message.IndexOf("Duplicate entry", StringComparison.OrdinalIgnoreCase) > -1)
            {
                result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_DuplicateEntity;
            }
            //Table 'commonconfiguretable' already exists
            else if ((dbException.Message.IndexOf("already exists", StringComparison.OrdinalIgnoreCase) > -1) && dbException.Message.IndexOf("Table", StringComparison.OrdinalIgnoreCase) > -1)
            {
                result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_DuplicateTable;
            }
            //Duplicate key name 'displayname'
            else if (dbException.Message.IndexOf("Duplicate key name", StringComparison.OrdinalIgnoreCase) > -1)
            {
                result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_DuplicateKey;
            }
            //A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: TCP Provider, error: 0 - A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond.)
            else if (dbException.Message.IndexOf("The server was not found or was not accessible", StringComparison.OrdinalIgnoreCase) > -1)
            {
                result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_ServerNotFound;
            }
            //Authentication to host '10.23.60.33' for user 'kim' using method 'mysql_native_password' failed with message: Unknown database 'kimtestdb1'
            else if (dbException.Message.IndexOf("Authentication to host", StringComparison.OrdinalIgnoreCase) > -1)
            {
                result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_LoginFailed;
            }
            return result;
        }
    }
}
