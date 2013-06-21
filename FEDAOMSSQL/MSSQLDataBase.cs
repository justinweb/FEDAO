using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using ProgLab.DAO.DAOBase;

namespace ProgLab.DAO.MSSQL
{
    public class MSSQLDataBase : AbstractDataBase
    {
        protected SqlConnection connection = null;
        protected SqlCommand command = null;
        protected SqlDataAdapter dataAdapter = null;

        public MSSQLDataBase(string connStr)
        {
            UseTransaction = false;

            this.ConnectionString = connStr;

            connection = new SqlConnection(ConnectionString);
            connection.Open();
        }

        public static string GenConnectionString(string server, string db, string uid, string pwd)
        {
            return string.Format("server={0};database={1};uid={2};pwd={3}", server, db, uid, pwd);
        }

        #region IDataBase 成員        

        public override string FieldToSQL(DateTime date)
        {
            return string.Format("'{0}'", date.ToShortDateString());
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
            string cmd = string.Format("SELECT {0} {1} {2} FROM {3} {4}",
                (topN > 0 ? topN.ToString() : ""),
                (isDistinct ? "DISTINCT" : ""), field, tableName,
                (where.Length > 0 ? " WHERE " + where : "")
                );

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

            SqlCommand command = new SqlCommand(cmd, connection);
            return command.ExecuteNonQuery(); 
        }

        public override DataTable QueryCommand(string cmd)
        {
            if (connection == null)
                throw new Exception("Connection is null");

            dataAdapter = new SqlDataAdapter(cmd, connection);
            DataSet ds = new DataSet();
            dataAdapter.Fill(ds, "DataTable");

            return ds.Tables["DataTable"]; 
        }

        #endregion
    }  
}
