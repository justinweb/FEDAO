using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YT.PT3.DAOBase;
using System.Data;
using YT.PT3.DAOOracle;

namespace UT
{
    class Program
    {
        static void Test(object o)
        {
            Console.WriteLine("Invoke Test(object)"); 
        }

        static void Test(string o)
        {
            Console.WriteLine("Invoke Test(string)");  
        }

        /// <summary>
        /// 測試AbstractWHEREStatement
        /// </summary>
        static void TestWHEREStatement()
        {
            IDataBase db = new OracleDataBase("User Id=bpi;Password=yuantacps;Data Source=TS03");

            // 基本用法
            AbstractWHEREStatement where = new AbstractWHEREStatement("TxDate", RelationEnum.Equal, new DateTime(2011, 3, 3));
            string s = where.ToString(db);
            Console.WriteLine(s);

            where.AddCondition(ConditionEnum.AND,
                "TID", RelationEnum.Equal, 1008);

            where.AddCondition(ConditionEnum.AND,
                "AID", RelationEnum.GreatThan, 104);

            s = where.ToString(db);
            Console.WriteLine(s);

            // 組成between
            where.Clear();
            where.AddCondition("TxDate", RelationEnum.Equal, new DateTime(2012, 3, 3));
            where.AddUnaryOperator(ConditionEnum.AND); 
            where.AddUnaryOperator(BraketEnum.OpenBraket);
            where.AddCondition("TID", RelationEnum.Equal, 1008);
            where.AddCondition(ConditionEnum.OR, "TID", RelationEnum.Equal, 1026);
            where.AddUnaryOperator(BraketEnum.CloseBraket);

            // order by
            where.AddOrderBy("TxDate", SortEnum.DESC);
            where.AddOrderBy("TID", SortEnum.ASC);

            s = where.ToString(db);
            Console.WriteLine(s);

            // 在AbstractWHEREStatement中再加入其它AbstractWHEREStatement
            AbstractWHEREStatement whereOut = new AbstractWHEREStatement();
            whereOut.AddCondition(where);

            s = whereOut.ToString(db);
            Console.WriteLine(s);
        }

        static void TestGenericColumn()
        {
            GenericColumn<int> ti = new GenericColumn<int>("TInt");
            GenericColumn<int?> tin = new GenericColumn<int?>("TIntN");

            OracleDataBase db = new OracleDataBase("User Id=bpi;Password=yuantacps;Data Source=TS03");
            DataTable dt = db.QueryCommand("SELECT NULL as TInt, NULL as TIntN FROM ptUnitTrader");

            DataRow dr = dt.Rows[0];
            ti.FillData(dr);
            tin.FillData(dr);

            bool isSupportNull = true;
            string s1 = dr.IsNull("TInt") ? (isSupportNull ? null : "default") : dr["TInt"].ToString();
        }

        static void TestOracleRaw()
        {
            //byte b = Convert.ToByte("01", 16);

            OracleDataBase db = new OracleDataBase("User Id=bpi;Password=yuantacps;Data Source=TS02G");
            DataTable dt = db.QueryCommand("SELECT * FROM ptDeptRiskCtrl");
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                byte[] rawData = (byte[])(row["PTDeptAuthID"]);
                string strRaw = AbstractDataBase.RawToString(rawData);
                byte[] raw2 = AbstractDataBase.StringToRaw(strRaw);

                System.Diagnostics.Debug.Assert(rawData.Length == raw2.Length);
                for (int index = 0; index < raw2.Length; ++index)
                {
                    System.Diagnostics.Debug.Assert(rawData[index] == raw2[index]);
                }

                string tmps = db.FieldToSQL(new OracleRowID(strRaw));
                
            }

            db.Close();
        }


        static void Main(string[] args)
        {
            DateTime theTime = DateTime.Parse("2012/07/17 11:47:37");
            OracleDataBase db = new OracleDataBase("Data Source=ts03.tw.yuanta.com;User Id=gets;Password=gets123");

            //TestGenericColumn();
            //TestOracleRaw();

            //// test function overloading
            //string s = "aa";
            //Test(s);

            //GOTest.UT();

            //TestWHEREStatement();
            //DAOTest.Test();
            //DAOTest.TestYuantaOracle(); 
            //DAOTest.TestRO(); 
            //DAOTest.TestConcurrentOracle(); 
            
        }

        #region 測試Generic跟overloading的問題        
        public class GOTest
        {
            public class GTest<DataType>
            {
                public DataType v = default(DataType);

                public GTest(DataType d)
                {
                    v = d;
                }

                public string TS(object o)
                {
                    return o.ToString();
                }

                public string TS(DateTime o)
                {
                    return o.ToString();
                }

                /// <summary>
                /// 透過其它類別來轉換時，好像就無法得知所給定的Generic型別
                /// </summary>
                /// <param name="h"></param>
                /// <returns></returns>
                public string MyTS(Helper h)
                {
                    return Helper.TS(v);
                }
            }

            public class Helper
            {
                public static string TS(object o)
                {
                    return o.ToString();
                }

                public static string TS(DateTime o)
                {
                    return o.ToString();
                }

                public string TS2(object o)
                {
                    return o.ToString();
                }

                public string TS2(DateTime o)
                {
                    return o.ToString();
                }   
            }

            public static void UT()
            {
                GTest<DateTime> g1 = new GTest<DateTime>(new DateTime(2011, 1, 1));
                Helper.TS(g1.v);    // OK TS(DateTime) is invoked;
                g1.TS(g1.v);        // OK GTest<>::TS(DateTime) is invoked;
                g1.MyTS(new Helper());

                Helper h = new Helper();
                h.TS2(g1.v);

                // g1的Generic型別，只在這個區段中有用，只要進入其它區段就好像看不見一樣
                PassGenericObject(g1);
            }

            public static void PassGenericObject<DataType>(GTest<DataType> g1)
            {
                Helper.TS(g1.v);    // wrong
                g1.TS(g1.v);        // wrong
                g1.MyTS(new Helper());

                Helper h = new Helper();
                h.TS2(g1.v); 
            }
        }
        #endregion
    }

    
}
