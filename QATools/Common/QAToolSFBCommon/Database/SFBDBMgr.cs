using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using QAToolSFBCommon.NLLog;
using QAToolSFBCommon.Common;

namespace QAToolSFBCommon.Database
{
    public enum EMSFB_DBTYPE
    {
        emDBTypeUnknown,

        emDBTypeMSSQL,
        emDBTypeMYSQL
    }

    public struct StuTableInfo
    {
        public string strTableName;
        public string strKeyFieldName;
        public string strKeyFieldType;
        public string[] szStrFieldNameAndTypes;
        public List<string> lsIndexFieldNames;      // The extra index without PRIMARY KEY
        public bool bKeyNotNull;

        public StuTableInfo(string strParamTableName, string strParamKeyFieldName, string strParamKeyFieldType, List<string> lsParamIndexFieldNames, bool bParamKeyNotNull, params string[] szStrParamFieldNameAndTypes)
        {
            strTableName = strParamTableName;
            strKeyFieldName = strParamKeyFieldName;
            strKeyFieldType = strParamKeyFieldType;
            szStrFieldNameAndTypes = szStrParamFieldNameAndTypes;

            lsIndexFieldNames = null;
            if (null != lsParamIndexFieldNames)
            {
                lsIndexFieldNames = lsParamIndexFieldNames;
            }
            bKeyNotNull = bParamKeyNotNull;
        }
        public StuTableInfo(StuTableInfo obStuTableInfo)
        {
            strTableName = obStuTableInfo.strTableName;
            strKeyFieldName = obStuTableInfo.strKeyFieldName;
            strKeyFieldType = obStuTableInfo.strKeyFieldType;
            szStrFieldNameAndTypes = obStuTableInfo.szStrFieldNameAndTypes;
            lsIndexFieldNames = obStuTableInfo.lsIndexFieldNames;
            bKeyNotNull = obStuTableInfo.bKeyNotNull;
        }
    }

    public class SFBDBMgr : IDisposable, IPersistentStorage
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger("SFBDBMgr");
        #endregion

        #region SFB Table define

        public static readonly Dictionary<EMSFB_INFOTYPE, StuTableInfo> kdirTableNames = new Dictionary<EMSFB_INFOTYPE, StuTableInfo>()
        {

        };
        #endregion

        #region Init SFB Database flag
        static private bool m_bInitSFBDatabase = false;
        static private bool GetInitSFBDatabaseFlag() { return m_bInitSFBDatabase; }
        static private void SetInitSFBDatabaseFlag(bool bInitSFBDatabase) { m_bInitSFBDatabase = bInitSFBDatabase; }
        #endregion

        #region Members
        private EMSFB_DBTYPE m_emDBType = EMSFB_DBTYPE.emDBTypeUnknown;
        private AbstractDBOpHelper m_DBHelper = null;
        private bool m_bEstablishSFBMgrSuccess = false;
        #endregion

        #region Constructors
        public SFBDBMgr(string strDBServerAddr, uint unPortNumber, string strCatalogName, string strUserName, string strPassword, EMSFB_DBTYPE emDBType = EMSFB_DBTYPE.emDBTypeUnknown)
        {
            SetEstablishSFBMgrFlag(false);
            m_emDBType = emDBType;
            switch (emDBType)
            {
            case EMSFB_DBTYPE.emDBTypeMSSQL:
            {
                m_DBHelper = new DBOpMSSQLHelper(strDBServerAddr, unPortNumber, strCatalogName, strUserName, strPassword);
                break;
            }
            case EMSFB_DBTYPE.emDBTypeMYSQL:
            {
                m_DBHelper = new DBOpMYSQLHelper(strDBServerAddr, unPortNumber, strCatalogName, strUserName, strPassword);
                break;
            }
            default:
            {
                m_emDBType = EMSFB_DBTYPE.emDBTypeMYSQL;
                m_DBHelper = new DBOpMYSQLHelper(strDBServerAddr, unPortNumber, strCatalogName, strUserName, strPassword);
                break;
            }
            }
            if (m_DBHelper.GetEstablishConnectionFlag())
            {
                InitSFBDatabase();  // this function only need invoke once, if the database address is incorrect this will be take a lot of time
                SetEstablishSFBMgrFlag(true);
            }
            else
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
            }
        }
        #endregion

        #region Interface: IDisposable
        public void Dispose()
        {
            m_DBHelper.Dispose();
        }
        #endregion

        #region Interface: IPersistantStorage
        public Dictionary<string, string>[] GetAllObjInfo(EMSFB_INFOTYPE emInfoType)
        {
            Dictionary<string, string>[] szDirAllObjInfo = null;
            if (GetEstablishSFBMgrFlag())
            {
                StuTableInfo obStuTableInfo = kdirTableNames[emInfoType];

                DataTable obDataTable = m_DBHelper.SelectItem<string>(obStuTableInfo.strTableName, false, false);
                if (null != obDataTable)
                {
                    szDirAllObjInfo = ConvertDataTableToDictionary(obDataTable);
                }
                else
                {
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "user try to get obj info for database but failed:[{0}], it maybe no value or read failed", LastErrorRecorder.GetLastError());
                }
            }
            else
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error: The SFBMgr establish flag is false, please check you  database!");
            }
            return szDirAllObjInfo;
        }
        public Dictionary<string, string>[] GetObjInfoEx(EMSFB_INFOTYPE emInfoType, string strFieldName, params string[] szStrValues)
        {
            Dictionary<string, string>[] szDirAllObjInfo = null;
            if (GetEstablishSFBMgrFlag())
            {
                StuTableInfo obStuTableInfo = kdirTableNames[emInfoType];
                if ((!string.IsNullOrEmpty(strFieldName)) && (null != szStrValues))
                {
                    int nValueLength = szStrValues.Length;
                    string[] szStrFieldKeyAndValues = new string[nValueLength * 2];
                    for (int i = 0; i < nValueLength; ++i)
                    {
                        szStrFieldKeyAndValues[(2 * i)] = strFieldName;
                        szStrFieldKeyAndValues[(2 * i) + 1] = szStrValues[i];
                    }
                    DataTable obDataTable = m_DBHelper.SelectItem(obStuTableInfo.strTableName, false, false, szStrFieldKeyAndValues);
                    if (null != obDataTable)
                    {
                        szDirAllObjInfo = ConvertDataTableToDictionary(obDataTable);
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "user try to get obj info for database but failed:[{0}]. it maybe a wrong read operation or read out wrong data\n", LastErrorRecorder.GetLastError());
                    }
                }
                else
                {
                    LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_KEY_INCORRECT);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error in GetObjInfo, user try to get obj info but pass in a incorrect field name\n");
                }
            }
            else
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error: The SFBMgr establish flag is false, please check you  database!");
            }
            return szDirAllObjInfo;
        }
        public Dictionary<string, string> GetObjInfo(EMSFB_INFOTYPE emInfoType, string strKey, string strValue) // Return null => select failed, return new Dictionary<string,string>() => no value
        {
            Dictionary<string, string> obDirInfo = null;
            if (GetEstablishSFBMgrFlag())
            {
                StuTableInfo obStuTableInfo = kdirTableNames[emInfoType];
                if (obStuTableInfo.strKeyFieldName.Equals(strKey, StringComparison.OrdinalIgnoreCase))
                {
                    DataTable obDataTable = m_DBHelper.SelectItem(obStuTableInfo.strTableName, false, false, strKey, strValue);
                    if ((null != obDataTable))
                    {
                        obDirInfo = new Dictionary<string,string>();
                        if (1 == obDataTable.Rows.Count)
                        {
                            Dictionary<string, string>[] szDirAllObjInfo = ConvertDataTableToDictionary(obDataTable);
                            if ((null != szDirAllObjInfo) && (1 == szDirAllObjInfo.Length))
                            {
                                obDirInfo = szDirAllObjInfo[0];
                            }
                        }
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "user try to get obj info for database but failed:[{0}]. it maybe a wrong read operation\n", LastErrorRecorder.GetLastError());
                    }
                }
                else
                {
                    LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_KEY_INCORRECT);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error in GetObjInfo, user try to get obj info but pass in a incorrect key\n");
                }
            }
            else
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error: The SFBMgr establish flag is false, please check you  database!");
            }
            return obDirInfo;
        }
        public Dictionary<string, string>[] GetObjInfoByLikeValue(EMSFB_INFOTYPE emInfoType, string strKeyFieldName, string strLikeValue)
        {
            Dictionary<string, string>[] szDirAllObjInfo = null;
            if (GetEstablishSFBMgrFlag())
            {
                StuTableInfo obStuTableInfo = kdirTableNames[emInfoType];
                if ((!string.IsNullOrEmpty(strKeyFieldName)) && obStuTableInfo.strKeyFieldName.Equals(strKeyFieldName, StringComparison.OrdinalIgnoreCase) && (!string.IsNullOrEmpty(strLikeValue)))
                {
                    DataTable obDataTable = m_DBHelper.SelectItem(obStuTableInfo.strTableName, false, true, strKeyFieldName, strLikeValue);
                    if (null != obDataTable)
                    {
                        szDirAllObjInfo = ConvertDataTableToDictionary(obDataTable);
                    }
                    else
                    {
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "user try to GetObjInfoByLikeValue from database failed:[{0}]. it maybe a wrong read operation or read out wrong data\n", LastErrorRecorder.GetLastError());
                    }
                }
                else
                {
                    LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_KEY_INCORRECT);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error in GetObjInfo, user try to get obj info but pass in a incorrect field name\n");
                }
            }
            else
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error: The SFBMgr establish flag is false, please check you  database!");
            }
            return szDirAllObjInfo;
        }
        public bool SaveObjInfo(EMSFB_INFOTYPE emInfoType, Dictionary<string, string> dirParamInfos)
        {
            bool bRet = false;
            if (GetEstablishSFBMgrFlag())
            {
                StuTableInfo obStuTableInfo = kdirTableNames[emInfoType];
                if (dirParamInfos.Keys.Contains(obStuTableInfo.strKeyFieldName))
                {
                    Dictionary<string, string> dirInfos = new Dictionary<string,string>(dirParamInfos); // Clone
                 
                    string strKeyFieldValue = dirInfos[obStuTableInfo.strKeyFieldName];
                    dirInfos.Remove(obStuTableInfo.strKeyFieldName);
                    KeyValuePair<string, string>[] szFieldKeyAndValues = dirInfos.ToArray();
                    bRet = m_DBHelper.AddItem(obStuTableInfo.strTableName, obStuTableInfo.strKeyFieldName, strKeyFieldValue, szFieldKeyAndValues);
                    if (!bRet)
                    {
                        LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DATA_WRITE_FAILED);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error in SaveObjInfo, user try to save obj info but failed\n");
                    }
                }
                else
                {
                    LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_KEY_NOT_EXIST);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "Error in SaveObjInfo, user try to save obj info but do not contains the key value\n");
                }
            }
            else
            {
                LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error: The SFBMgr establish flag is false, please check you  database!");
            }
            return bRet;
        }

        // lsSpecifyOutFields:
        //      1. can be null or empty means return all
        //      2. do not suggest to using null or empty
        //      3. if some filed is same in difference tables only return the last one
        // lsSearchScopes cannot be null or empty
        // lsSearchConditions cannot be null or empty
        public Dictionary<EMSFB_INFOTYPE, Dictionary<string, string>>[] GetObjInfoWithFullSearchConditions(List<STUSFB_INFOFIELD> lsSpecifyOutFields, List<EMSFB_INFOTYPE> lsSearchScopes, List<KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>> lsSearchConditions)
        {
            try
            {
                if (GetEstablishSFBMgrFlag())
                {
                    // Check parameters
                    if (((null != lsSearchScopes) && (0 < lsSearchScopes.Count)) && ((null != lsSearchConditions) && (0 < lsSearchConditions.Count)))
                    {
                        // Establish output filed string
                        DataTable obDataTable = m_DBHelper.SelectItemEx(lsSpecifyOutFields, lsSearchScopes, lsSearchConditions);
                        if (null != obDataTable)
                        {
                            // If lsSpecifyOutFields is null or empty and obDataTable contains the same clomun value, only return the last one
                            return ConvertDataTableToDictionaryArray(obDataTable, lsSpecifyOutFields);
                        }
                        else
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "user try to GetObjInfoByLikeValue from database failed:[{0}]. it maybe a wrong read operation or read out wrong data\n", LastErrorRecorder.GetLastError());
                        }
                    }
                    else
                    {
                        LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_KEY_INCORRECT);
                        theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Error in GetObjInfoWithFullSearchConditions, user try to get obj info but pass in a incorrect field name\n");
                    }
                }
                else
                {
                    LastErrorRecorder.SetLastError(LastErrorRecorder.ERROR_DBCONNECTION_FAILED);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!!Error in GetObjInfoWithFullSearchConditions: The SFBMgr establish flag is false, please check you  database!");
                }
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!!Exception in GetObjInfoWithFullSearchConditions, [{0}]\n", ex.Message);
            }
            return null;
        }
        #endregion

        #region SFB Database
        private void InitSFBDatabase()
        {
            if (!GetInitSFBDatabaseFlag())
            {
                // Create table
                foreach (var obItem in kdirTableNames)
                {
                    StuTableInfo obStuTableInfo = obItem.Value;
                    m_DBHelper.CreateTable(true, obStuTableInfo.bKeyNotNull, obStuTableInfo.strTableName, obStuTableInfo.strKeyFieldName, obStuTableInfo.strKeyFieldType, obStuTableInfo.szStrFieldNameAndTypes);

                    // Check columns
                    for (int i=1; i<obStuTableInfo.szStrFieldNameAndTypes.Length; i += 2)
                    {
                        m_DBHelper.AddColumn(obStuTableInfo.strTableName, obStuTableInfo.szStrFieldNameAndTypes[i-1], obStuTableInfo.szStrFieldNameAndTypes[i]);
                    }

                    // Add Index
                    if (null != obStuTableInfo.lsIndexFieldNames)
                    {
                        foreach (string strFieldName in obStuTableInfo.lsIndexFieldNames)
                        {
                            if ((!string.IsNullOrEmpty(strFieldName)) && (!strFieldName.Equals(obStuTableInfo.strKeyFieldName, StringComparison.OrdinalIgnoreCase)))
                            {
                                m_DBHelper.AddIndex(obStuTableInfo.strTableName, strFieldName);
                            }
                        }
                    }
                }

                SetInitSFBDatabaseFlag(true);
            }
            else
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "The SFB database already initialized\n");
            }
        }
        private void UnInitSFBDatabase()
        {
#if false   // Back up, no need delete table
            // Delete table
            foreach (var obItem in kdirTableNames)
            {
                StuTableInfo obStuTableInfo = obItem.Value;
                m_DBHelper.DeleteTable(m_emDBType, true, obStuTableInfo.strTableName);
            }
#endif
            SetInitSFBDatabaseFlag(false);
        }
        private void SetEstablishSFBMgrFlag(bool bEstablishSFBMgrSuccess) { m_bEstablishSFBMgrSuccess = bEstablishSFBMgrSuccess; }
        public bool GetEstablishSFBMgrFlag() { return m_bEstablishSFBMgrSuccess; }
        #endregion

        #region Private tools
        private Dictionary<string, string>[] ConvertDataTableToDictionary(DataTable obDataTable)
        {
            Dictionary<string, string>[] szDicAllObjInfo = null;
            if (null != obDataTable)
            {
                int nRowsCount = obDataTable.Rows.Count;
                szDicAllObjInfo = new Dictionary<string, string>[nRowsCount];
                DataColumnCollection obDataColumns = obDataTable.Columns;
                for (int nRow = 0; nRow < nRowsCount; ++nRow)
                {
                    DataRow obDataRow = obDataTable.Rows[nRow];
                    szDicAllObjInfo[nRow] = new Dictionary<string, string>();
                    for (int nColumn = 0; nColumn < obDataColumns.Count; ++nColumn)
                    {
                        string strCurColumnValue = obDataRow.ItemArray[nColumn] as string;
                        if (null == strCurColumnValue)
                        {
                            strCurColumnValue = obDataRow.ItemArray[nColumn].ToString();
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!! Current column:{0}:{1} is not string type or is empty", obDataColumns[nColumn].ColumnName, strCurColumnValue);
                        }
                        szDicAllObjInfo[nRow].Add(obDataColumns[nColumn].ColumnName, strCurColumnValue);
                    }
                }
            }
            return szDicAllObjInfo;
        }
        private Dictionary<EMSFB_INFOTYPE, Dictionary<string, string>>[] ConvertDataTableToDictionaryArray(DataTable obDataTable, List<STUSFB_INFOFIELD> lsSpecifyOutFields)
        {
            Dictionary<EMSFB_INFOTYPE, Dictionary<string, string>>[] szDicAllObjInfo = null;
            if (null != obDataTable)
            {
                int nRowsCount = obDataTable.Rows.Count;
                DataColumnCollection obDataColumns = obDataTable.Columns;
 
                    szDicAllObjInfo = new Dictionary<EMSFB_INFOTYPE,Dictionary<string,string>>[nRowsCount];
                    for (int nRow = 0; nRow < nRowsCount; ++nRow)
                    {
                        szDicAllObjInfo[nRow] = new Dictionary<EMSFB_INFOTYPE,Dictionary<string,string>>();
                        DataRow obDataRow = obDataTable.Rows[nRow];
                        for (int nColumn = 0; nColumn < obDataColumns.Count; ++nColumn)
                        {
                            STUSFB_INFOFIELD stuCurOutField = GetInfoFiedFormList(lsSpecifyOutFields, nColumn);
                            string strCurOutFieldName = stuCurOutField.strField;
                            string strCurColumnName = obDataColumns[nColumn].ColumnName;
                            if (!strCurOutFieldName.Equals(strCurColumnName, StringComparison.OrdinalIgnoreCase))
                            {
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "!!! Attention:Current out field name:[{0}] is not the same as the column name:[{1}], please check, maybe something error.", strCurOutFieldName, strCurColumnName);
                            }

                            EMSFB_INFOTYPE emCurInfoType = stuCurOutField.emTableInfoType;
                            string strCurColumnValue = obDataRow.ItemArray[nColumn] as string;
                            if (null == strCurColumnValue)
                            {
                                strCurColumnValue = obDataRow.ItemArray[nColumn].ToString();
                                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Attention: current column:{0}:{1} is not string type or is empty", strCurColumnName, strCurColumnValue);
                            }
                            AddDateTableItemInfo(szDicAllObjInfo[nRow], emCurInfoType, strCurColumnName, strCurColumnValue);
                        }
                    }
            }
            return szDicAllObjInfo;
        }
        private void AddDateTableItemInfo(Dictionary<EMSFB_INFOTYPE, Dictionary<string, string>> dicAllObjInfo, EMSFB_INFOTYPE emTableInfoType, string strColumnName, string strColumnValue)
        {
            Dictionary<string,string> dicCurInfo = CommonHelper.GetValueByKeyFromDir(dicAllObjInfo, emTableInfoType, new Dictionary<string,string>());
            if (null != dicCurInfo)
            {
                CommonHelper.AddKeyValuesToDir(dicCurInfo, strColumnName, strColumnValue);
                CommonHelper.AddKeyValuesToDir(dicAllObjInfo, emTableInfoType, dicCurInfo);
            }
        }
        private STUSFB_INFOFIELD GetInfoFiedFormList(List<STUSFB_INFOFIELD> lsSpecifyOutFields, int nIndex)
        {
            if (null != lsSpecifyOutFields)
            {
                if ((0 <= nIndex) && (nIndex < lsSpecifyOutFields.Count))
                {
                    return lsSpecifyOutFields[nIndex];
                }
            }
            return new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoUnknown, "");
        }
        #endregion
    }
}
