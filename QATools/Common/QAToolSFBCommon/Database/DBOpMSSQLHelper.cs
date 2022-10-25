using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

// Current project
using QAToolSFBCommon.NLLog;

// C# ADO.NET
// ado.net private us many functions to operate database. The basic steps is:
// 1. Establish database connection by using SqlConnection object
// 2. Execute SQL command or procedure by create SqlCommand object
// 3. Get result by execute SQL command or procedure.
// There are two result type: 
// 1. SqlDataReader, using this object to read data one by one
// 2. DataSet & SqlDataAdapter, using this two object to operate database: add/delete/update/select
// Note:
// 1. SqlDataReader always connect with database.
// 2. SqlDataReader is readonly.
// 3. SqlDataReader read data is faster.
// 4. DataSet is local data which using SqlDataAdapter fill in.
// 5. DataSet support add/delete/update/select data.
// 6. DataSet is local data and need using SqlDataAdapter to update data to Database.

namespace QAToolSFBCommon.Database
{
    /// <summary>
    /// Current class is thread unsafe. 
    /// When you want to use current class to operate you database please create a new object for every thread.
    /// </summary>
    class DBOpMSSQLHelper : AbstractDBOpHelper
    {
        #region Constructors
        public DBOpMSSQLHelper(string strDBServerAddr, uint unPortNumber, string strCatalogName, string strUserName, string strPassword)
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

        override public void Dispose()
        {
            // Note, no need dispose CurDbConnection, we have a manager thread to do this
            // here we can release some resource for current object
        }

        #region Override, Ole Database
        protected override DbConnection EstablishDBConnection(string strDBServerAddr, uint unPortNumber, string strCatalogName, string strUserName, string strPassword)
        {
            // Create SQL connection string
            SqlConnectionStringBuilder obSqlConnectionString = new SqlConnectionStringBuilder();
            obSqlConnectionString.DataSource = strDBServerAddr + "," + unPortNumber.ToString();
            obSqlConnectionString.InitialCatalog = strCatalogName;
            obSqlConnectionString.UserID = strUserName;
            obSqlConnectionString.Password = strPassword;
            return new SqlConnection(obSqlConnectionString.ConnectionString);
        }
        protected override DbDataAdapter CreateDbDataAdapter(string strSqlCmd)
        {
            try
            {
                SqlConnection theSqlConnection = CurDbConnection as SqlConnection;
                return new SqlDataAdapter(strSqlCmd, theSqlConnection);
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Exception in CreateDataAdapter, {0}\n", ex.Message);
            }
            return null;
        }
        protected override DbCommandBuilder CreateDbCommandBuilder(DataAdapter obDataAdapter)
        {
            return new SqlCommandBuilder(obDataAdapter as SqlDataAdapter);
        }
        protected override DbCommand CreateDbCommand(string strSQLCommand, DbConnection dbConnection)
        {
            try
            {
                SqlConnection theSqlConnection = dbConnection as SqlConnection;
                return new SqlCommand(strSQLCommand, theSqlConnection);
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
            return strValue.Replace("'", "''");
        }
        override protected string GetCreateTableSqlCommand<T>(bool bCheckTableExists, bool bKeyNotNull, string strTableName, string strKeyFieldName, string strKeyFieldType, params T[] szFieldNameAndTeypes)
        {
            // MSSQL do not support bCheckTableExists, ignore
            string strCreateTable = "CREATE TABLE ";
            if (bKeyNotNull)
            {
                strKeyFieldType = strKeyFieldType + " NOT NULL ";
            }
            string strSqlCommand = strCreateTable + strTableName + " ( " + strKeyFieldName + " " + strKeyFieldType + " PRIMARY KEY, ";
            strSqlCommand += GetKeyValueConnectString(" ", ",", false, false, szFieldNameAndTeypes) + " ) ";
            return strSqlCommand;
        }
        override protected string GetDropTableSqlCommand(bool bCheckTableExists, string strTableName)
        {
            return "DROP TABLE " + strTableName;
        }
        override protected string GetAddColumnsSqlCommand<T>(string strTableName, params T[] szFieldNameAndValues)
        {
            // ALTER TABLE TableName ADD field1 LONGTEXT, field2 LONGTEXT;
            string strSqlAddColumnsCommand = "ALTER TABLE " + strTableName + " ADD ";
            strSqlAddColumnsCommand += GetKeyValueConnectString(" ", ",", false, false, szFieldNameAndValues);
            return strSqlAddColumnsCommand;
        }
        override protected string GetDropColumnsSqlCommand(string strTableName, params string[] szFieldNames)
        {
            // ALTER TABLE TableName DROP COLUMN field1, field1
            if (0 < szFieldNames.Length)
            {
                StringBuilder strSqlDropColumnsCommand = new StringBuilder("ALTER TABLE " + strTableName + " DROP COLUMN ");
                strSqlDropColumnsCommand.Append(" " + szFieldNames[0]);
                for (int i = 1; i < szFieldNames.Length; ++i)
                {
                    strSqlDropColumnsCommand.Append(", " + szFieldNames[i]);
                }
                return strSqlDropColumnsCommand.ToString();
            }
            return "";
        }
        #endregion

        protected override EMSFB_DBEXCUTEEXCEPTION TranfromDBException(DbException dbException)
        {
            EMSFB_DBEXCUTEEXCEPTION result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_Unknow;
            //Incorrect syntax near '1'.
            if (dbException.Message.IndexOf("Incorrect syntax near", StringComparison.OrdinalIgnoreCase) > -1)
            {
                result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_SyntaxError;
            }
            //Column names in each table must be unique. Column name 'value' in table 'commonconfiguretable' is specified more than once.
            else if (dbException.Message.IndexOf("Column names in each table must be unique", StringComparison.OrdinalIgnoreCase) > -1)
            {
                result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_DuplicateColumn;
            }
            //iolation of PRIMARY KEY constraint 'PK__sfbusert__91965DFF1F97F9C4'. Cannot insert duplicate key in object 'dbo.sfbusertable'. The duplicate key value is (cat1@lab11.com). 
            else if (dbException.Message.IndexOf("Cannot insert duplicate key in object", StringComparison.OrdinalIgnoreCase) > -1)
            {
                result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_DuplicateEntity;
            }
            //There is already an object named 'commonconfiguretable' in the database.
            else if ((dbException.Message.IndexOf("There is already an object named", StringComparison.OrdinalIgnoreCase) > -1) && dbException.Message.IndexOf("Table", StringComparison.OrdinalIgnoreCase) > -1)
            {
                result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_DuplicateTable;
            }
            //The operation failed because an index or statistics with name 'displayname' already exists on table 'sfbusertable'.
            else if (dbException.Message.IndexOf("The operation failed because an index or statistics", StringComparison.OrdinalIgnoreCase) > -1)
            {
                result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_DuplicateKey;
            }
            //A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: TCP Provider, error: 0 - No such host is known.)
            else if (dbException.Message.IndexOf("A network-related or instance-specific error occurred while establishing a connection to SQL Server", StringComparison.OrdinalIgnoreCase) > -1)
            {
                result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_ServerNotFound;
            }
            //Login failed for user 'sa'.
            else if (dbException.Message.IndexOf("Login failed for user", StringComparison.OrdinalIgnoreCase) > -1)
            {
                result = EMSFB_DBEXCUTEEXCEPTION.emDBEx_LoginFailed;
            }
            return result;
        }
    }
}
