using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace YT.PT3.DAOBase
{
    /// <summary>
    /// 用來表示Record中的欄位屬性
    /// 使用泛型的方式，可支援各種型別
    /// 並且它本身會帶有欄位的名稱，這樣就算要使用WHEREStatement時，也不用直接用到欄位名稱，方便未來統一修改欄位名稱以及避免用錯名稱。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericColumn<T>
    {
        protected string colName = "";      // 欄位名稱
        public string ColName
        {
            get { return colName; }
        }

        protected T data = default(T);      // 值
        public T Value
        {
            get { return data; }
            set { data = value; }
        }

        /// <summary>
        /// 建構子，需指定欄位名稱
        /// </summary>
        /// <param name="col"></param>
        public GenericColumn(string col)
        {
            colName = col;
        }

        /// <summary>
        /// 建構子，除指定欄位名稱外，還可指定預設值
        /// </summary>
        /// <param name="col"></param>
        /// <param name="defaultValue"></param>
        public GenericColumn(string col, T defaultValue)
        {
            colName = col;
            data = defaultValue;
        }

        /// <summary>
        /// 由DataRow中取出資料，並轉成指定型別
        /// 它會一併看DataRow中的值是否為NULL
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public virtual bool FillData(DataRow row)
        {
            // http://msdn.microsoft.com/en-us/library/ms366789.aspx
            bool isNullableType = false;
            System.Type vType = typeof(T);
            isNullableType = vType.IsGenericType && vType.GetGenericTypeDefinition() == typeof(Nullable<>);

            if (row.IsNull(ColName) == true)
            {
                if (isNullableType)
                    data = default(T);
                else
                    return false;
            }
            else
            {
                data = (T)Convert.ChangeType(row[ColName].ToString(), typeof(T));
            }

            return true;
        }

        /// <summary>
        /// 由指定的字串中，轉成指定的型別
        /// </summary>
        /// <param name="rowValue"></param>
        /// <returns></returns>
        public virtual bool FillData(string rowValue)
        {
            try
            {
                data = (T)Convert.ChangeType(rowValue, typeof(T));

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 轉型operator
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static explicit operator T(GenericColumn<T> field)
        {
            return field.Value;
        }

        public override string ToString()
        {
            return (this.Value != null ? this.Value.ToString() : "");
        }
    }

    /// <summary>
    /// Oracle Raw 欄位類別
    /// </summary>
    public class OracleRowID
    {
        public string RowID
        {
            get;
            set;
        }

        public OracleRowID(byte[] data)
        {
            RowID = AbstractDataBase.RawToString(data);
        }

        public OracleRowID(string rowID)
        {
            RowID = rowID;
        }

        public byte[] ToOracleRowID()
        {
            return AbstractDataBase.StringToRaw(RowID);
        }

        public override string ToString()
        {
            return RowID;
        }
    }
}
