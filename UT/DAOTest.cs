//#define _Use_Oracle_
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data;

using ProgLab.DAO.DAOBase;
using ProgLab.DAO.MSSQL;

namespace UT
{
    public class DAOTest
    {
        /// <summary>
        /// 測試Operator將Record的資料轉成SQL指令
        /// </summary>
        public static void Test()
        {
            TestRecord record = new TestRecord();
            record.TxDate = new DateTime(2012, 3, 3);
            record.TID = 1008;
            record.PID = "TW.2330";
            record.AID = 104;

            int effectedRows = 0;

            TestOperator opr = new TestOperator(new MSSQLDataBase(""));
            bool result = opr.Insert(record, ref effectedRows);
            string cmd = opr.LastCommand;

#if _Use_Oracle_
            TestOperator oprOracle = new TestOperator(new OracleDataBase(""));
            result = oprOracle.Insert(record, ref effectedRows);
            cmd = oprOracle.LastCommand;
#endif
        }

#if _Use_Oracle_
        /// <summary>
        /// 測試連接Oracle資料庫
        /// </summary>
        public static void TestYuantaOracle()
        {
            OracleDataBase db = new OracleDataBase("User Id=bpi;Password=yuantacps;Data Source=TS03");
            PTTradingUnitOperator opr = new PTTradingUnitOperator(db);

            //int totalRecords = opr.Select(null);
            //foreach (PTTradingUnitRecord record in opr.RecordList)
            //{
            //    Console.WriteLine( record.PTCorpID + "," + record.PTCostCenter + "," + record.PTCustID + "," + record.PTDeptID +
            //        "," + record.PTTradeGoal + "," + record.PTUserID ); 
            //}

            foreach (PTTradingUnitRecord record in opr.SelectIterator(null))
            {
                Console.WriteLine(record.PTCorpID + "," + record.PTCostCenter + "," + record.PTCustID + "," + record.PTDeptID +
                    "," + record.PTTradeGoal + "," + record.PTUserID); 
            }

            string[] corpID = opr.SelectFields<string>("ptCorpID", false, 10, null,AbstractOperator<PTTradingUnitRecord>.ConvertFirstColumn<string> );            
            foreach (string cid in corpID)
            {
                Console.WriteLine(cid); 
            }

            int totalCount = opr.SelectCount(null);
            Console.WriteLine("TotalCount " + totalCount);

            db.Close();

        }

        /// <summary>
        /// 測試多執行緒同時存取OracleDataBase的狀況
        /// </summary>
        protected static OracleDataBase conDB = null;
        public static void TestConcurrentOracle()
        {
            conDB = new OracleDataBase("User Id=bpi;Password=yuantacps;Data Source=TS03");

            List<Thread> listThread = new List<Thread>();
            for (int i = 0; i < 3; i++)
            {
                Thread t = new Thread(OracelAccessThread);
                t.IsBackground = true;
                t.Start();

                listThread.Add(t);
            }

            foreach (Thread t in listThread)
            {
                t.Join();
            }

            conDB.Close();
        }

        protected static void OracelAccessThread()
        {
            PTTradingUnitOperator opr = new PTTradingUnitOperator(conDB);
            int counter = 100;
            while (counter-- > 0)
            {
                int totalRecords = opr.Select(null);
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString() + "," + opr.RecordList.Count);             
            }
        }
#endif

        public static void TestPolarisMSSQL()
        {
        }

#if _Use_Oracle_
        public static void TestRO()
        {
            //OracleDataBase db = new OracleDataBase("User Id=system;Password=spmll1l3x96;Data Source=XE80");
            OracleDataBase db = new OracleDataBase("User Id=bpi;Password=yuantacps;Data Source=TS03");

            OracleDAOTestOperator opr = new OracleDAOTestOperator(db);
            OracleDAOTestRecord record = new OracleDAOTestRecord();
            record.ID = 13;
            record.Name = "Justin 13";
            record.ModifyDate = new DateTime(2011, 1, 1);

            int effectedRows = 0;
            opr.Insert( record, ref effectedRows); 
        }
#endif
    }

    #region 測試用的實作類別    
    public class PTTradingUnitRecord : IRecord
    {
        //public string PTCorpID = "";
        public GenericColumn<string> PTCorpID = new GenericColumn<string>("ptCorpID");
        public string PTCustID = "";
        public string PTCostCenter = "";
        public string PTDeptID = "";
        public string PTTradeGoal = "";
        public string PTUserID = "";
    }

    public class PTTradingUnitOperator : AbstractOperator<PTTradingUnitRecord>
    {
        public static string MyTableName = "ptTradingUnit";

        public PTTradingUnitOperator(IDataBase crtDataBase):base(crtDataBase)
        {
            CurrentDataBase = crtDataBase;
            TableName = MyTableName;
        }       

        public override bool Insert(PTTradingUnitRecord record, ref int effecedRows)
        {
            throw new NotImplementedException();
        }

        public override bool Update(PTTradingUnitRecord record, AbstractWHEREStatement where, ref int effecedRows)
        {
            throw new NotImplementedException();
        }

        public override bool Delete(AbstractWHEREStatement where, ref int effecedRows)
        {
            throw new NotImplementedException();
        }

        public override PTTradingUnitRecord ConvertRecord(DataRow row)
        {
            try
            {
                PTTradingUnitRecord record = new PTTradingUnitRecord();
                //record.PTCorpID = row["PTCorpID"].ToString().Trim();
                record.PTCorpID.FillData(row); 
                record.PTCostCenter = row["PTCostCenter"].ToString().Trim();
                record.PTCustID = row["PTCustID"].ToString().Trim();
                record.PTDeptID = row["PTDeptID"].ToString().Trim();
                record.PTTradeGoal = row["PTTradeGoal"].ToString().Trim();
                record.PTUserID = row["PTUserID"].ToString().Trim();

                return record;
            }
            catch (Exception exp)
            {
                this.LastErrorMsg = exp.Message;
                return null;
            }
        }
    }
    #endregion

    #region Test RO
    public class TestRecord : IRecord
    {
        public DateTime TxDate = DateTime.Today;
        public int TID = 0;
        public int AID = 0;
        public string PID = "";
    }

    public class TestOperator : AbstractOperator<TestRecord>
    {
        public static string MyTableName = "TestTable";

        public TestOperator(IDataBase crtDataBase) : base(crtDataBase)
        {
            CurrentDataBase = crtDataBase;
            this.TableName = MyTableName;
        }

        public override int Select(AbstractWHEREStatement where)
        {
            return 0;
        }

        public override bool Insert(TestRecord record, ref int effecedRows)
        {
            string cmd = "INSERT into " + TableName + " (TxDate,TID,AID,PID} VALUES (" +
                this.CurrentDataBase.FieldToSQL(record.TxDate)
                + "," + this.CurrentDataBase.FieldToSQL(record.TID)
                + "," + this.CurrentDataBase.FieldToSQL(record.AID)
                + "," + this.CurrentDataBase.FieldToSQL(record.PID)
                + ")";

            LastCommand = cmd;

            return true;
        }

        public override bool Update(TestRecord record, AbstractWHEREStatement where, ref int effecedRows)
        {
            throw new NotImplementedException();
        }

        public override bool Delete(AbstractWHEREStatement where, ref int effecedRows)
        {
            throw new NotImplementedException();
        }

        public override TestRecord ConvertRecord(DataRow row)
        {
            return null;
        }
    }
    #endregion
    
#region  RO for OracleDAOTest
    public class OracleDAOTestRecord : IRecord
    {
        public int ID = 0;
        public string Name = "";
        public DateTime ModifyDate = DateTime.Today;
    }

    public class OracleDAOTestOperator : AbstractOperator<OracleDAOTestRecord>
    {
        public static string myTableName = "OracleDAOTest";

        public OracleDAOTestOperator( IDataBase database ) : base( database )
        {
            TableName = myTableName;
        }

        public override bool Insert(OracleDAOTestRecord record, ref int effectedRows)
        {
            if (CheckDataBase() == false)
                return false;

            this.LastCommand = "INSERT INTO " + TableName + " (ID,Name,ModifyDate) VALUES ("
                + CurrentDataBase.FieldToSQL(record.ID)
                + "," + CurrentDataBase.FieldToSQL(record.Name)
                + "," + CurrentDataBase.FieldToSQL(record.ModifyDate)
                // 2012/3/16 Justin
                // 好像不用下commit也可以!!
                //+ ");"
                //+ (CurrentDataBase.UseTransaction ? " COMMIT;" : "");
                + ")";

            try
            {
                effectedRows = CurrentDataBase.ExecuteCommand(LastCommand);

                return true;
            }
            catch (Exception exp)
            {
                LastErrorMsg = exp.Message;
                return false;
            }
        }

        public override bool Update(OracleDAOTestRecord record, AbstractWHEREStatement where, ref int effectedRows)
        {
            if (CheckDataBase() == false)
                return false;

            this.LastCommand = "UPDATE " + TableName + " SET "
                + "ID=" + CurrentDataBase.FieldToSQL(record.ID)
                + ",Name=" + CurrentDataBase.FieldToSQL(record.Name)
                + ",ModifyDate=" + CurrentDataBase.FieldToSQL(record.ModifyDate)
                + (where != null && where.ConditionCount > 0 ? " WHERE " + where.ToString(CurrentDataBase) : "");

            try
            {
                effectedRows = CurrentDataBase.ExecuteCommand(LastCommand);
                return true;
            }
            catch (Exception exp)
            {
                LastErrorMsg = exp.Message;
                return false;
            }
        }

        public override OracleDAOTestRecord ConvertRecord(DataRow row)
        {
            OracleDAOTestRecord record = new OracleDAOTestRecord();

            try
            {
                int.TryParse(row["ID"].ToString(), out record.ID);
                DateTime.TryParse(row["ModifyDate"].ToString(), out record.ModifyDate);
                record.Name = row["Name"].ToString();

                return record;
            }
            catch (Exception exp)
            {
                LastErrorMsg = exp.Message;
                return null;

            }
        }
    }

#endregion
}
