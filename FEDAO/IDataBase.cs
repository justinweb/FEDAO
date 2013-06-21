using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace ProgLab.DAO.DAOBase
{
    /// <summary>
    /// 定義資料庫的操作及型別轉換函式
    /// </summary>
    public interface IDataBase
    {
        string ConnectionString
        {
            get;
        }

        #region 型別轉換函式
        string FieldToSQL(DateTime date);
        string FieldToSQL(bool bValue);
        string FieldToSQL(object o);
        string FieldToSQL(string s);        
        #endregion        

        string MakeSELECT(string field, bool isDistinct, int topN, string where, string tableName);

        void Close();

        int ExecuteCommand(string cmd);
        DataTable QueryCommand(string cmd);

        /// <summary>
        /// 資料庫是否使用Transaction模式，是的話，在INSERT/UPDATE/DELETE指令後要加commit
        /// </summary>
        bool UseTransaction { get; }
    }

    public abstract class AbstractDataBase : IDataBase
    {       
        #region IDataBase 成員        
        public string ConnectionString
        {
            get;
            protected set;
        }

        public bool UseTransaction
        {
            get;
            protected set;
        }

        public abstract string FieldToSQL(DateTime date);

        public abstract string FieldToSQL(bool bValue);

        public abstract string FieldToSQL(string s);       

        /// <summary>
        /// 因為C#的generic overloading是在Compile時進行的，所以還是得自行依型別來呼叫正確的轉換函式，
        /// 無法讓編譯器自行決定
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public virtual string FieldToSQL(object o)
        {
            Type t = o.GetType();
            if (t.Equals(typeof(DateTime)))
                return FieldToSQL((DateTime)o);
            else if (t.Equals(typeof(string)))
                return FieldToSQL((string)o);
            else if (t.Equals(typeof(bool)))
                return FieldToSQL((bool)o);
            else if (t.Equals(typeof(OracleRowID)))
                return FieldToSQL((OracleRowID)o);
            else
                // 這裏不能再呼叫FieldToSQL(object)，因為會造成無限迴圈
                return o.ToString();
        }

        public virtual string FieldToSQL(OracleRowID rawID)
        {
            return "HEXTORAW('" + rawID.RowID + "')"; 
        }

        /// <summary>
        /// 實作產生SELECT指令的函式
        /// </summary>
        /// <param name="field">欄位名稱(可以是多個欄位)</param>
        /// <param name="isDistinct">是否要DISTINCT</param>
        /// <param name="topN">是否取前N筆。0表示不指定前N筆</param>
        /// <param name="where">where條件式</param>
        /// <param name="tableName">資料表名稱</param>
        /// <returns></returns>
        public abstract string MakeSELECT(string field, bool isDistinct, int topN, string where, string tableName);

        /// <summary>
        /// 關閉資料庫連線
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// 執行非SELECT指令，例如UPDATE、INSERT、DELETE
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns>傳回受影響的筆數</returns>
        public abstract int ExecuteCommand(string cmd);        

        /// <summary>
        /// 執行查詢式指令，也就是SELECT
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns>傳回查詢的結果</returns>
        public abstract DataTable QueryCommand(string cmd);        

        #endregion


        public static string RawToString(byte[] raw)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in raw)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }

        public static byte[] StringToRaw(string s)
        {
            int totalLength = s.Length / 2;
            byte[] result = new byte[totalLength];
            for (int index = 0; index < totalLength; index++)
            {
                result[index] = Convert.ToByte(s.Substring(index*2,2),16);
            }

            return result;

        }
    }
}
