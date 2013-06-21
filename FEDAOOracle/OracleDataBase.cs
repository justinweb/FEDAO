using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Oracle.DataAccess.Client;
using ProgLab.DAO.DAOBase;

namespace YT.PT3.DAOOracle
{
    /// <summary>
    /// Oracle資料庫存取類別
    /// 連線會一直保持開啟，直到呼叫Close()為止才會關閉
    /// </summary>
    public class OracleDataBase : AbstractDataBase
    {
        protected OracleConnection connection = null;
        protected OracleCommand command = null;
        //protected OracleDataAdapter dataAdapter = null;
        //protected DataSet dataSet = null;

        public OracleDataBase( string connStr )
        {
            UseTransaction = true;

            this.ConnectionString = connStr;

            connection = new OracleConnection(ConnectionString);
            connection.Open();
        }

        #region IDataBase 成員

        public override string FieldToSQL(DateTime date)
        {
            return string.Format("to_date('{0}','yyyy/mm/dd')", date.ToShortDateString());
        }

        public override string FieldToSQL(bool bValue)
        {
            return (bValue ? "1" : "0");
        }

        public override string FieldToSQL(string sValue)
        {
            return string.Format("'{0}'", sValue);
        }

        //public string FieldToSQL(object o)
        //{
        //    return o.ToString();
        //}

        public override string MakeSELECT(string field, bool isDistinct, int topN, string where, string tableName)
        {
            string cmd = string.Format("SELECT {0} {1} FROM {2} {3}",
                (isDistinct ? "DISTINCT" : ""), field, tableName,
                (where.Length > 0 ? " WHERE " + where : "")
                );

            if (topN > 0)
            {
                cmd = string.Format( "SELECT {0} FROM ({1}) WHERE rownum <= {2}",
                    field, cmd, topN
                    );
            }

            return cmd;
        }

        public override void Close()
        {
            if (connection != null)
            {
                connection.Close();
                connection = null;
            }
        }

        public override int ExecuteCommand(string cmd)
        {
            if (connection == null)
                throw new Exception("Connection is null");

            OracleCommand command = new OracleCommand(cmd, connection);
            return command.ExecuteNonQuery();            
        }

        public override DataTable QueryCommand(string cmd)
        {
            if (connection == null)
                throw new Exception("Connection is null");

            OracleDataAdapter dataAdapter = new OracleDataAdapter(cmd, connection);
            DataSet ds = new DataSet();
            dataAdapter.Fill(ds, "DataTable");            

            return ds.Tables["DataTable"];            
        }

        #endregion

        
    }

    
}
