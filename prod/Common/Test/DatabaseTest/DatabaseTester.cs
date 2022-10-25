using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Threading;

// Other project
using SFBCommon.NLLog;
using SFBCommon.Database;
using SFBCommon.Common;
using SFBCommon.SFBObjectInfo;

namespace TestProject.DatabaseTest
{
    class DatabaseTester : Logger
    {
        #region Const values
#if true
        private const string kstrAddr = "10.23.60.33";
        private const int knPort = 3306;
        private const string kstrCatalog = "KimTestDB";
        private const string kstrUserName = "kim";
        private const string kstrPassword = "123blue!";
        private const EMSFB_DBTYPE kemDbType = EMSFB_DBTYPE.emDBTypeMYSQL;
//         private const string kstrAddr = "10.23.56.166";
//         private const int knPort = 1433;
//         private const string kstrCatalog = "sfb";
//         private const string kstrUserName = "sfb";
//         private const string kstrPassword = "123blue!";
//         private const EMSFB_DBTYPE kemDbType = EMSFB_DBTYPE.emDBTypeMSSQL;
#else
        private const string kstrAddr = "10.23.60.33";
        private const int knPort = 3306;
        private const string kstrCatalog = "SFB";
        private const string kstrUserName = "kim";
        private const string kstrPassword = "123blue!";
        private const EMSFB_DBTYPE kemDbType = EMSFB_DBTYPE.emDBTypeMYSQL;
#endif
        const EMSFB_INFOTYPE kemInfoType = EMSFB_INFOTYPE.emInfoSFBUserInfo;
        const string kstrNameFlag = "fullname";
        const string kstrNameHeader = "TestName";
        const string kstrValueFlag = "displayname";
        const string kstrValueHeader = "TestValue";
        #endregion

        static public void Test()
        {
            SFBDBMgr obSFBDBMgr = new SFBDBMgr(kstrAddr, knPort, kstrCatalog, kstrUserName, kstrPassword, kemDbType);

#if false
            AbstractDBOpHelper obSFBDBHelper = obSFBDBMgr.m_DBHelper;

            const string strTableName = "TestTableNewTest";
            const string strKeyFieldName = "name";
            const string strKeyFieldType = "varchar(255)";
            const string strFieldValue = "value";
            const string strFieldValueType = "varchar(255)";
            const string strFieldTags = "tags";
            const string strFieldTagsType = "varchar(255)";

            const string kstrFieldNewColumn1 = "NewColumn1";
            const string kstrFieldNewColumn1Type = "varchar(255)";
            const string kstrFieldNewColumn2 = "NewColumn2";
            const string kstrFieldNewColumn2Type = "varchar(255)";

            obSFBDBHelper.CreateTable(kemDbType, true, strTableName, strKeyFieldName, strKeyFieldType, strFieldValue, strFieldValueType, strFieldTags, strFieldTagsType);

            string[] szTableKey = { "name1", "name2", "name3", "name4", "name5" };
            string[] szTableValue = { "value1", "value2", "value3", "value4", "value5" };
            string[] szTableTags = { "tag1", "tag2", "tag3", "tag4", "tag5" };
            string[] szTableNewColumn1 = { "new11", "new12", "new13", "new14", "new15" };
            string[] szTableNewColumn2 = { "new21", "new22", "new23", "new24", "new25" };
            int nLen = szTableKey.Length;

            for (int i=0; i<nLen; ++i)
            {
                obSFBDBHelper.AddItem(strTableName, strKeyFieldName, szTableKey[i], strFieldValue, szTableValue[i], strFieldTags, szTableTags[i]);
            }

            for (int i = 0; i < nLen; ++i)
            {
                obSFBDBHelper.AddItem(strTableName, strKeyFieldName, szTableKey[i], strFieldValue, szTableValue[i]+"Update");
            }

            for (int i = 0; i < nLen; ++i)
            {
                obSFBDBHelper.AddItem(strTableName, strKeyFieldName, szTableKey[i], strFieldValue, szTableValue[i]+"AddAgain");
            }

            // Add new columns
            obSFBDBHelper.AddColumn(kemDbType, strTableName, kstrFieldNewColumn1, kstrFieldNewColumn1Type, kstrFieldNewColumn2, kstrFieldNewColumn2Type);
            for (int i = 0; i < nLen; ++i)
            {
                obSFBDBHelper.AddItem(strTableName, strKeyFieldName, szTableKey[i], kstrFieldNewColumn1, szTableNewColumn1[i]);
            }
            bool bRet = obSFBDBHelper.AddColumn(kemDbType, strTableName, kstrFieldNewColumn1, kstrFieldNewColumn1Type); // Add a repeat column
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Add repeat column {0}\n", bRet);
            
            // Delete column
            obSFBDBHelper.DropColumn(kemDbType, strTableName, kstrFieldNewColumn1);
            obSFBDBHelper.DropColumn(kemDbType, strTableName, kstrFieldNewColumn2);

            // Select
            for (int nIndex = 0; nIndex < nLen; ++nIndex)
            {
                DataTable obDataTable = obSFBDBHelper.SelectItem(strTableName, true, strKeyFieldName, szTableKey[nIndex]);
                {
                    foreach (DataRow obDataRow in obDataTable.Rows)
                    {
                        for (int j = 0; j < obDataRow.ItemArray.Length; ++j)
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "obDataColumn:{0}:{1}", j, obDataRow.ItemArray[j]);
                        }
                    }
                }
                {
                    Dictionary<string, string> obDirInfo = new Dictionary<string, string>();
                    DataRow obDataRow = obDataTable.Rows[0];
                    DataColumnCollection obDataColumns = obDataTable.Columns;
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Start get all values\n");
                    for (int i = 0; i < obDataColumns.Count; ++i)
                    {
                        string strCurColumnValue = obDataRow.ItemArray[i].ToString();
                        if (null == strCurColumnValue)
                        {
                            strCurColumnValue = "";
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!! Current column is not string type, please check you database");
                        }
                        obDirInfo.Add(obDataColumns[i].ColumnName, strCurColumnValue);
                    }
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "End\n");
                }
            }

            for (int i = 0; i < nLen; ++i)
            {
                obSFBDBHelper.DeleteItem(strTableName, strKeyFieldName, szTableKey[i]);
            }

            obSFBDBHelper.DeleteTable(kemDbType, true, strTableName);
#endif
        }

        static public void TestEx()
        {
#if false
            EMSFB_DBTYPE emDBType = EMSFB_DBTYPE.emDBTypeMYSQL;
            SFBDBMgr obSFBDBMgr = new SFBDBMgr("10.23.60.242", 3306, "KimTestDB", "kim", "123blue!", emDBType);
            AbstractDBOpHelper obSFBDBHelper = obSFBDBMgr.m_DBHelper;

            string strTableName = "KimTest1";
            KeyValuePair<string, string> pairKeyFieldNameAndTypes = new KeyValuePair<string,string>("name", "varchar(255)");
            KeyValuePair<string, string> pairValueFieldNameAndTypes = new KeyValuePair<string,string>("value", "varchar(255)");
            KeyValuePair<string, string> pairTagsFieldNameAndTypes = new KeyValuePair<string, string>("tags", "varchar(255)");

            obSFBDBHelper.CreateTable(emDBType, true, strTableName, pairKeyFieldNameAndTypes.Key, pairKeyFieldNameAndTypes.Value, pairValueFieldNameAndTypes, pairTagsFieldNameAndTypes);

            string[] szTableKey = { "name1", "name2", "name3", "name4", "name5" };
            string[] szTableValue = { "value1", "value2", "value3", "value4", "value5" };
            string[] szTableTags = { "tag1", "tag2", "tag3", "tag4", "tag5" };
            int nLen = szTableKey.Length;

            for (int i = 0; i < nLen; ++i)
            {
                obSFBDBHelper.AddItem(strTableName, pairKeyFieldNameAndTypes.Key, szTableKey[i], pairValueFieldNameAndTypes.Key, szTableValue[i], pairTagsFieldNameAndTypes.Key, szTableTags[i]);
            }

            for (int i = 0; i < nLen; ++i)
            {
                obSFBDBHelper.AddItem(strTableName, pairKeyFieldNameAndTypes.Key, szTableKey[i], pairValueFieldNameAndTypes.Key, szTableValue[i] + "Update");
            }

            for (int i = 0; i < nLen; ++i)
            {
                obSFBDBHelper.AddItem(strTableName, pairKeyFieldNameAndTypes.Key, szTableKey[i], pairValueFieldNameAndTypes.Key, szTableValue[i] + "AddAgain");
            }

            KeyValuePair<string, string> pairNewField0NameAndTypes = new KeyValuePair<string,string>("NewColumn0", "varchar(255)");
            KeyValuePair<string, string> pairNewField1NameAndTypes = new KeyValuePair<string, string>("NewColumn1", "varchar(255)");

            obSFBDBHelper.AddColumn(strTableName, pairNewField0NameAndTypes, pairNewField1NameAndTypes);
            for (int i = 0; i < nLen; ++i)
            {
                obSFBDBHelper.AddItem(strTableName, pairKeyFieldNameAndTypes.Key, szTableKey[i], pairNewField0NameAndTypes.Key, szTableValue[i] + "NewColumn0");
            }

            obSFBDBHelper.DropColumn(strTableName, pairValueFieldNameAndTypes.Key);
            obSFBDBHelper.DropColumn(strTableName, pairNewField0NameAndTypes.Key);
            obSFBDBHelper.DropColumn(strTableName, pairNewField1NameAndTypes.Key);

            for (int nIndex = 0; nIndex < nLen; ++nIndex)
            {
                DataTable obDataTable = obSFBDBHelper.SelectItem(strTableName, pairKeyFieldNameAndTypes.Key, szTableKey[nIndex]);
                {
                    foreach (DataRow obDataRow in obDataTable.Rows)
                    {
                        for (int j = 0; j < obDataRow.ItemArray.Length; ++j)
                        {
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "obDataColumn:{0}:{1}", j, obDataRow.ItemArray[j]);
                        }
                    }
                }
                {
                    Dictionary<string, string> obDirInfo = new Dictionary<string, string>();
                    DataRow obDataRow = obDataTable.Rows[0];
                    DataColumnCollection obDataColumns = obDataTable.Columns;
                    for (int i = 0; i < obDataColumns.Count; ++i)
                    {
                        string strCurColumnValue = obDataRow.ItemArray[i].ToString();
                        if (null == strCurColumnValue)
                        {
                            strCurColumnValue = "";
                            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelFatal, "!!! Current column is not string type, please check you database");
                        }
                        obDirInfo.Add(obDataColumns[i].ColumnName, strCurColumnValue);
                    }
                }
            }

            for (int i = 0; i < nLen; ++i)
            {
                obSFBDBHelper.DeleteItem(strTableName, pairKeyFieldNameAndTypes.Key, szTableKey[i]);
            }

            obSFBDBHelper.DeleteTable(true, strTableName);
#endif
        }

        static public void TestDBConnection()
        {
            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Start test DBConnection\n");

            SFBDBMgr obSFBDBMgr = new SFBDBMgr(kstrAddr, knPort, kstrCatalog, kstrUserName, kstrPassword, kemDbType);
            const int nThreads = 10;
            const int nItems = 1000;
            List<string>[] szLsNames = null;
            List<Dictionary<string, string>[]> lsSzDicTest = EstablishTestDatas(nThreads, nItems, out szLsNames);

            for (int i=0; i<nThreads; ++i)
            {
                ThreadHelper.AsynchronousInvokeHelper(true, PersistentSaveData, lsSzDicTest[i]);
                ThreadHelper.AsynchronousInvokeHelper(true, ReadPersistentData, szLsNames[i]);
            }
        }

        static public void TestSearchEx()
        {
            SFBDBMgr obSFBDBMgr = new SFBDBMgr(kstrAddr, knPort, kstrCatalog, kstrUserName, kstrPassword, kemDbType);

            // select sfb.sfbmeetingvariabletable.uri, sfb.sfbmeetingtable.creator, isstaticmeeting from sfb.sfbmeetingtable, sfb.sfbmeetingvariabletable where (sfb.sfbmeetingtable.uri = sfb.sfbmeetingvariabletable.uri and sfb.sfbmeetingtable.expirytime > '2017-02-00T17:33:06Z' and sfb.sfbmeetingvariabletable.donemanulclassify = 'Yes') or (sfb.sfbmeetingtable.uri = sfb.sfbmeetingvariabletable.uri and sfb.sfbmeetingtable.isstaticmeeting = 'True')
            // sfb.sfbmeetingvariabletable.uri, sfb.sfbmeetingtable.creator, isstaticmeeting
            List<STUSFB_INFOFIELD> lsSpecifyOutFields = new List<STUSFB_INFOFIELD>()
            {
                {new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeetingVariable, SFBMeetingVariableInfo.kstrUriFieldName)},
                {new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeeting, SFBMeetingInfo.kstrCreatorFieldName)},
                {new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoUnknown, SFBMeetingInfo.kstrMeetingTypeFieldName)}
            };
            // sfb.sfbmeetingtable, sfb.sfbmeetingvariabletable
            List<EMSFB_INFOTYPE> lsSearchScopes = new List<EMSFB_INFOTYPE>()
            {
                {EMSFB_INFOTYPE.emInfoSFBMeeting},
                {EMSFB_INFOTYPE.emInfoSFBMeetingVariable}
            };
            /*
             * (sfb.sfbmeetingtable.uri = sfb.sfbmeetingvariabletable.uri and sfb.sfbmeetingtable.expirytime > '2017-02-00T17:33:06Z' and sfb.sfbmeetingvariabletable.donemanulclassify = 'Yes')
             * or 
             * (sfb.sfbmeetingtable.uri = sfb.sfbmeetingvariabletable.uri and sfb.sfbmeetingtable.isstaticmeeting = 'True')
            */
            List<STUSFB_INFOITEM> lsComditonItemsA = new List<STUSFB_INFOITEM>()
            {
                {new STUSFB_INFOITEM(new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeeting, SFBMeetingInfo.kstrUriFieldName), new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeetingVariable, SFBMeetingVariableInfo.kstrUriFieldName), EMSFB_INFOCOMPAREOP.emSearchOp_Equal)},
                {new STUSFB_INFOITEM(new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeeting, SFBMeetingInfo.kstrExpiryTimeFieldName), new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoUnknown, "2017-02-00T17:33:06Z"), EMSFB_INFOCOMPAREOP.emSearchOp_Above)},
                {new STUSFB_INFOITEM(new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeetingVariable, SFBMeetingVariableInfo.kstrDoneManulClassifyFieldName), new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoUnknown, SFBMeetingVariableInfo.kstrDoneManulClassifyYes), EMSFB_INFOCOMPAREOP.emSearchOp_Equal)}
            
            };
            STUSFB_CONDITIONGROUP stuConditionsGroupA = new STUSFB_CONDITIONGROUP(lsComditonItemsA, EMSFB_INFOLOGICOP.emSearchLogicAnd);
            List<STUSFB_INFOITEM> lsComditonItemsB = new List<STUSFB_INFOITEM>()
            {
                {new STUSFB_INFOITEM(new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeeting, SFBMeetingInfo.kstrUriFieldName), new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeetingVariable, SFBMeetingVariableInfo.kstrUriFieldName), EMSFB_INFOCOMPAREOP.emSearchOp_Equal)},
                {new STUSFB_INFOITEM(new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoSFBMeeting, SFBMeetingInfo.kstrMeetingTypeFieldName), new STUSFB_INFOFIELD(EMSFB_INFOTYPE.emInfoUnknown, "True"), EMSFB_INFOCOMPAREOP.emSearchOp_Equal)}
            };
            STUSFB_CONDITIONGROUP stuConditionsGroupB = new STUSFB_CONDITIONGROUP(lsComditonItemsB, EMSFB_INFOLOGICOP.emSearchLogicAnd);
            List<KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>> lsSearchConditions = new List<KeyValuePair<EMSFB_INFOLOGICOP,STUSFB_CONDITIONGROUP>>()
            {
                {new KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>(EMSFB_INFOLOGICOP.emSearchLogicOr, stuConditionsGroupA)},
                {new KeyValuePair<EMSFB_INFOLOGICOP, STUSFB_CONDITIONGROUP>(EMSFB_INFOLOGICOP.emSearchLogicOr, stuConditionsGroupB)}
            };
            {
                Dictionary<EMSFB_INFOTYPE, Dictionary<string, string>>[] szAllObjInfo = obSFBDBMgr.GetObjInfoWithFullSearchConditions(lsSpecifyOutFields, lsSearchScopes, lsSearchConditions);
                if (null != szAllObjInfo)
                {
                    for (int i = 0; i < szAllObjInfo.Length; ++i)
                    {
                        TestProject.CommonTools.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Index:{0}:\n", i);
                        foreach (KeyValuePair<EMSFB_INFOTYPE, Dictionary<string, string>> pairObjInfo in szAllObjInfo[i])
                        {
                            TestProject.CommonTools.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\tType:{0}:\n", pairObjInfo.Key);
                            foreach (KeyValuePair<string, string> pairKeyValues in pairObjInfo.Value)
                            {
                                TestProject.CommonTools.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "\t\tKey:[{0}],Value:[{1}]\n", pairKeyValues.Key, pairKeyValues.Value);
                            }
                        }
                    }
                }
            }
        }

        #region Tools
        static private List<Dictionary<string, string>[]> EstablishTestDatas(int nThreads, int nItems, out List<string>[] szLsNames)
        {
            Dictionary<string, int> dicUniqueNames = new Dictionary<string, int>();
            int nNamesCount = 0;
            szLsNames = new List<string>[nThreads];
            List<Dictionary<string, string>[]> lsSzDicTest = new List<Dictionary<string, string>[]>();
            for (int i = 0; i < nThreads; ++i)
            {
                szLsNames[i] = new List<string>();
                Dictionary<string, string>[] szDicTest = new Dictionary<string, string>[nItems];
                for (int j = 0; j < nItems; ++j)
                {
                    szDicTest[j] = new Dictionary<string, string>()
                    {
                        { kstrNameFlag, kstrNameHeader + i.ToString() + j.ToString()   },
                        { kstrValueFlag, false ? "Te'st\\" : kstrValueHeader + i.ToString() + j.ToString() }
                    };
                    szLsNames[i].Add(kstrNameHeader + i.ToString() + j.ToString());

                    CommonHelper.AddKeyValuesToDir(dicUniqueNames, kstrNameHeader + i.ToString() + j.ToString(), ++nNamesCount);
                }
                lsSzDicTest.Add(szDicTest);
            }
            return lsSzDicTest;
        }
        static private void PersistentSaveData(object obSzDicTest)
        {
            Console.WriteLine("Current save data thead id:[{0}]\n", Thread.CurrentThread.ManagedThreadId);
            SFBDBMgr obSFBDBMgr = new SFBDBMgr(kstrAddr, knPort, kstrCatalog, kstrUserName, kstrPassword, kemDbType);
            if (obSFBDBMgr.GetEstablishSFBMgrFlag())
            {
                Dictionary<string, string>[] szDicTest = obSzDicTest as Dictionary<string, string>[];
                foreach (Dictionary<string, string> dicTest in szDicTest)
                {
                    bool bSaveInfo = obSFBDBMgr.SaveObjInfo(kemInfoType, dicTest);
                    theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Save info {0}: {1}\n", bSaveInfo.ToString(), dicTest[kstrNameFlag]);
                }
            }
            else
            {
                Console.Write("Establish SFB Manager failed in PersistentSaveData\n");
            }
        }
        static private void ReadPersistentData(object obLsNames)
        {
            Console.WriteLine("Current read data thead id:[{0}]\n", Thread.CurrentThread.ManagedThreadId);
            SFBDBMgr obSFBDBMgr = new SFBDBMgr(kstrAddr, knPort, kstrCatalog, kstrUserName, kstrPassword, kemDbType);
            if (obSFBDBMgr.GetEstablishSFBMgrFlag())
            {
                Dictionary<string, string> dicTest = null;
                List<string> lsNames = obLsNames as List<string>;
                foreach (string strName in lsNames)
                {
                    Console.Write("Begin read item: {0}\n", strName);
                    dicTest = obSFBDBMgr.GetObjInfo(kemInfoType, kstrNameFlag, strName);
                    foreach (KeyValuePair<string, string> pairItem in dicTest)
                    {
                        Console.Write("\tPair:{0}:{1}\n", pairItem.Key, pairItem.Value);
                    }
                    Console.Write("End read item: {0}\n", strName);
                }
            }
            else
            {
                Console.Write("Establish SFB Manager failed in ReadPersistentData\n");
            }
        }
        #endregion
    }
}
