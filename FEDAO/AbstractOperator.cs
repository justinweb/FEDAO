using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Globalization;

namespace ProgLab.DAO.DAOBase
{
    /// <summary>
    /// 實作IOperator的基底類別
    /// CLR3.0 only
    /// </summary>
    /// <typeparam name="MyRecord"></typeparam>
    public abstract class AbstractOperator<MyRecord> : IOperator<MyRecord>  where MyRecord : IRecord 
    {
        public AbstractOperator( IDataBase database )
        {
            CurrentDataBase = database;
        }
        
        //protected IDataBase currentDataBase = null;
        
        //protected string lastCommand = "";
        
        //protected string lastErrorMsg = "";
        
        //protected string tableName = "";
        /// <summary>
        /// 記錄查詢後的資料
        /// </summary>
        public List<MyRecord> RecordList = new List<MyRecord>();

        #region IOperator<MyRecord> 成員       

        public IDataBase CurrentDataBase
        {
            get;
            protected set;
        }

        public string LastCommand
        {
            get;
            protected set;
        }

        public string LastErrorMsg
        {
            get;
            protected set;
        }

        public string TableName
        {
            get;
            protected set;
        }

        public virtual int Select(AbstractWHEREStatement where)
        {           
            LastErrorMsg = "";
            RecordList.Clear();

            if (CurrentDataBase == null)
            {
                LastErrorMsg = "沒有指定要使用的IDataBase";
                return 0;
            }

            // 因為Oracle跟MSSQL在實作TOPN的方式不同，只好再改由底層的IDataBase來組指令了
            this.LastCommand = CurrentDataBase.MakeSELECT("*", false, 0,
                (where != null ? where.ToString(CurrentDataBase) : ""), TableName);            

            try
            {
                DataTable dt = CurrentDataBase.QueryCommand(LastCommand);
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        // 衍生類別只要實作ConvertRecord()函式即可
                        MyRecord record = ConvertRecord(row);

                        RecordList.Add(record);
                    }
                }
            }
            catch (Exception exp)
            {
                this.LastErrorMsg = exp.Message;
            }

            return RecordList.Count;        
        }

        public abstract bool Insert(MyRecord record,ref int effectedRows);

        public abstract bool Update(MyRecord record, AbstractWHEREStatement where, ref int effectedRows);

        public virtual bool Delete(AbstractWHEREStatement where, ref int effectedRows)
        {
            effectedRows = 0;

            if (CheckDataBase() == false)
            {                
                return false;
            }

            this.LastErrorMsg = "";
            this.LastCommand = "DELETE FROM " + TableName +
                (where != null && where.ConditionCount > 0 ? where.ToString(CurrentDataBase) : "")
                + (CurrentDataBase.UseTransaction ? "; commit;" : "");

            effectedRows = CurrentDataBase.ExecuteCommand(this.LastCommand);

            return true;
        }

        #endregion

        /// <summary>
        /// 將第一欄轉成指定型別用的基本實作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T ConvertFirstColumn<T>(DataRow row)
        {
            return (T)row[0];
        }

        /// <summary>
        /// 由衍生類別實作如何將DataRow轉成MyRecord型別
        /// </summary>
        /// <param name="row"></param>
        /// <returns>轉換成的MyRecord。無法轉換時傳回null</returns>
        public abstract MyRecord ConvertRecord(DataRow row);

        /// <summary>
        /// 查詢單一欄位的結果
        /// 可指定是否要用DISTINCT、top N
        /// </summary>
        /// <typeparam name="T">要取出的欄位資料型別</typeparam>
        /// <param name="field">欄位名稱</param>
        /// <param name="isDistinct">是否要DISTINCT</param>
        /// <param name="where">where條件式</param>
        /// <param name="topN">是否取前N筆</param>
        /// <param name="DataConvertFunc">資料轉換函式，如果將查詢出來的DataRow轉換成T型別</param>
        /// <returns>查詢結果。如果有錯誤時，會傳回null</returns>
        public T[] SelectFields<T>(string field, bool isDistinct, int topN, AbstractWHEREStatement where, Func<DataRow, T> DataConvertFunc )
        {
            if (CheckDataBase() == false)
                return new T[0];

            string strWhere = "";
            if (where != null && where.ConditionCount > 0)
                strWhere = where.ToString(CurrentDataBase);

            // 因為Oracle跟MSSQL在實作TOPN的方式不同，只好再改由底層的IDataBase來組指令了
            this.LastCommand = CurrentDataBase.MakeSELECT(field, isDistinct, topN, strWhere, TableName);

            try
            {
                DataTable dt = CurrentDataBase.QueryCommand(LastCommand);
                if (dt != null && dt.Rows.Count > 0)
                {
                    T[] result = new T[dt.Rows.Count];

                    int index = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        //result[index++] = (T)row[0];
                        result[index++] = DataConvertFunc(row); 
                    }

                    return result;
                }
                else
                    return null;
            }
            catch( Exception exp)
            {
                this.LastErrorMsg = exp.Message;
                return null;
            }
        }

        protected bool CheckDataBase()
        {
            if (CurrentDataBase == null)
            {
                this.LastErrorMsg = "CurrentDataBase is null";
                return false;
            }
            else
                return true;
        }

        /// <summary>
        /// 查詢單一欄位的結果
        /// 可指定是否要用DISTINCT、top N
        /// </summary>
        /// <typeparam name="T">要取出的欄位資料型別</typeparam>
        /// <param name="field">欄位名稱</param>
        /// <param name="isDistinct">是否要DISTINCT</param>
        /// <param name="where">where條件式</param>
        /// <param name="topN">是否取前N筆</param>        
        /// <returns>查詢結果。如果有錯誤時，會傳回null</returns>
        public T[] SelectFields<T>(string field, bool isDistinct, int topN, AbstractWHEREStatement where)
        {
            return SelectFields<T>(field, isDistinct, topN, where,                
                ConvertFirstColumn<T>);
        }

        /// <summary>
        /// 查詢資料筆數
        /// 只單純查詢筆數，不會將資料取出放到RecordList中。
        /// </summary>
        /// <param name="where">where條件式</param>
        /// <returns>查詢到的資料筆數</returns>
        public int SelectCount(AbstractWHEREStatement where)
        {
            if( CheckDataBase() == false )
                return 0;

            this.LastCommand = string.Format("SELECT Count(*) FROM {0} {1}",
                TableName,
                (where != null && where.ConditionCount > 0 ? "WHERE " + where.ToString(CurrentDataBase) : ""));

            DataTable dt = CurrentDataBase.QueryCommand(LastCommand);
            if (dt != null)
            {
                try
                {
                    return int.Parse(dt.Rows[0][0].ToString());
                }
                catch (Exception exp)
                {
                    this.LastErrorMsg = exp.Message;
                    return 0;
                }
            }
            else
                return 0;
        }

        /// <summary>
        /// 以yield return的方式，每完成一筆MyRecord就傳回，不用等到全就產生好再傳回
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public IEnumerable<MyRecord> SelectIterator(AbstractWHEREStatement where)
        {
            LastErrorMsg = "";
            RecordList.Clear();

            if (CheckDataBase() == false)
            {                
                yield break;
            }

            // 因為Oracle跟MSSQL在實作TOPN的方式不同，只好再改由底層的IDataBase來組指令了
            this.LastCommand = CurrentDataBase.MakeSELECT("*", false, 0,
                (where != null ? where.ToString(CurrentDataBase) : ""), TableName);

            // 因為yield不能被包在try/catch中，只好…折開來寫

            DataTable dt = null;
            try
            {
                dt = CurrentDataBase.QueryCommand(LastCommand);               
            }
            catch (Exception exp)
            {
                this.LastErrorMsg = exp.Message;
                yield break;
            }

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    MyRecord record = default(MyRecord);
                    try
                    {
                        // 衍生類別只要實作ConvertRecord()函式即可
                        record = ConvertRecord(row);
                    }
                    catch (Exception exp)
                    {
                        this.LastErrorMsg = exp.Message;
                        yield break;
                    }

                    yield return record;
                }
            }
        }

        /// <summary>
        /// 2011/5/26 Justin
        /// 用來轉換資料庫欄位到nullable參數中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static Nullable<T> TryParseNullable<T>(DataRow row, string colName) where T : struct
        {
            if (row.IsNull(colName))
                return null;
            else
            {
                string rawData = row[colName].ToString();

                IConvertible convertibleStr = (IConvertible)rawData;
                return new Nullable<T>((T)convertibleStr.ToType(typeof(T), CultureInfo.CurrentCulture));
            }
        }


    }
}
