using System;
using System.Collections.Generic;
using System.Data;

namespace ProgLab.DAO.DAOBase
{
    /// <summary>
    /// 利用IDataBase的功能執行對應之IRecord的CRUD
    /// Operator需實作將Record資料組成CRUD的SQL指令的部份，並使用給定的IDataBase來存取資料庫
    /// 在轉換CRUD的SQL指令時，需使用所給定的IDataBase的型別轉換函式，以便支援各式資料庫
    /// </summary>
    /// <typeparam name="MyRecord"></typeparam>
    public interface IOperator<MyRecord> where MyRecord : IRecord
    {
        /// <summary>
        /// 目前要使用的資料庫物件
        /// </summary>
        IDataBase CurrentDataBase { get; }
        /// <summary>
        /// 資料表名稱
        /// </summary>
        string TableName { get; }
        /// <summary>
        /// 記錄最後一次的查詢指令
        /// </summary>
        string LastCommand { get; }
        /// <summary>
        /// 記錄最後一次查詢失敗的錯誤訊息
        /// </summary>
        string LastErrorMsg { get; }

        /// <summary>
        /// 查詢資料庫
        /// </summary>
        /// <param name="where">要查詢的條件式。可以是null，表示全部查詢</param>
        /// <returns>傳回所查詢到的筆數</returns>
        int Select(AbstractWHEREStatement where);

        bool Insert(MyRecord record, ref int effectedRows);

        bool Update(MyRecord record, AbstractWHEREStatement where, ref int effectedRows);

        bool Delete(AbstractWHEREStatement where, ref int effectedRows);
    }
}
