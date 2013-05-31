using System;
using System.Collections.Generic;
using System.Text;

namespace YT.PT3.DAOBase
{
    /// <summary>
    /// SQL指令中的WHERE條件式
    /// </summary>
    public class AbstractWHEREStatement : ISQLElement
    {  
        protected List<ISQLElement> listSQLElement = new List<ISQLElement>();
        /// <summary>
        /// 記錄OrderBy的條件
        /// </summary>
        protected List<string> listOrderBy = new List<string>();

        #region Constructor
        public AbstractWHEREStatement()
        {
        }

        public AbstractWHEREStatement(string colName, RelationEnum relation, object sqlValue)
        {
            listSQLElement.Add(new SQLElement<object>(colName, relation, sqlValue));
        }

        public AbstractWHEREStatement(string colName, RelationEnum relation, DateTime sqlValue)
        {
            listSQLElement.Add(new SQLElement<DateTime>(colName, relation, sqlValue));
        }

        public AbstractWHEREStatement(string colName, RelationEnum relation, string sqlValue)
        {
            listSQLElement.Add(new SQLElement<string>(colName, relation, sqlValue));
        }
        #endregion        

        /// <summary>
        /// 
        /// NOTE : 沒有實際往下算到如果含有子AbstractWHEREStatement的條件數，只是單純用來看看是否有條件設定用的
        /// </summary>
        public int ConditionCount
        {
            get { return listSQLElement.Count; }
        }

        /// <summary>
        /// 加入SQL查詢條件式
        /// </summary>
        /// <param name="condition">跟前個條件式間的關係式</param>
        /// <param name="colName"></param>
        /// <param name="relation"></param>
        /// <param name="sqlValue"></param>
        public void AddCondition(ConditionEnum condition, string colName, RelationEnum relation, object sqlValue)
        {
            listSQLElement.Add(CreateConditionElement(condition));
            listSQLElement.Add(new SQLElement<object>(colName, relation, sqlValue));            
        }

        /// <summary>
        /// 加入SQL查詢條件式
        /// </summary>
        /// <param name="condition">跟前個條件式間的關係式</param>
        /// <param name="colName"></param>
        /// <param name="relation"></param>
        /// <param name="sqlValue"></param>
        public void AddCondition(ConditionEnum condition, string colName, RelationEnum relation, DateTime sqlValue)
        {
            listSQLElement.Add(CreateConditionElement(condition));
            listSQLElement.Add(new SQLElement<DateTime>(colName, relation, sqlValue));
        }

        /// <summary>
        /// 加入SQL查詢條件式
        /// </summary>
        /// <param name="condition">跟前個條件式間的關係式</param>
        /// <param name="colName"></param>
        /// <param name="relation"></param>
        /// <param name="sqlValue"></param>
        public void AddCondition(ConditionEnum condition, string colName, RelationEnum relation, string sqlValue)
        {
            listSQLElement.Add(CreateConditionElement(condition));
            listSQLElement.Add(new SQLElement<string>(colName, relation, sqlValue));
        }

        /// <summary>
        /// 加入SQL查詢條件式。一般是用在加入第一個條件時用的，因為不用指定跟前個條件的關係式
        /// </summary>
        /// <typeparam name="DataType"></typeparam>
        /// <param name="colName"></param>
        /// <param name="relation"></param>
        /// <param name="sqlValue"></param>
        public void AddCondition<DataType>(string colName, RelationEnum relation, DataType sqlValue)
        {
            listSQLElement.Add(new SQLElement<DataType>(colName, relation, sqlValue));
        }

        /// <summary>
        /// 加入SQL查詢條件式。只要是支援ISQLElement介面的都可以。
        /// 這個函式一開始主要是用來加入一個AbstractWHEREStatement用的。
        /// </summary>
        /// <param name="sqlElement"></param>
        public void AddCondition(ISQLElement sqlElement)
        {
            listSQLElement.Add(sqlElement);
        }

        public void AddOrderBy( string colName, SortEnum sort )
        {
            listOrderBy.Add(colName + " " + sort.ToString());
        }

        /// <summary>
        /// 加入括號
        /// </summary>
        /// <param name="braket"></param>
        public void AddUnaryOperator(BraketEnum braket)
        {
            switch (braket)
            {
                case BraketEnum.OpenBraket:
                    listSQLElement.Add(new SQLUnaryElement("("));
                    break;
                case BraketEnum.CloseBraket:
                    listSQLElement.Add(new SQLUnaryElement(")"));
                    break;
                default:
                    throw new Exception("Unknown BraketEnum");
            }
        }

        /// <summary>
        /// 加入關係式
        /// </summary>
        /// <param name="braket"></param>
        public void AddUnaryOperator(ConditionEnum cnd)
        {
            listSQLElement.Add(CreateConditionElement(cnd)); 
        }

        protected ISQLElement CreateConditionElement( ConditionEnum condition )
        {
            switch (condition)
            {
                case ConditionEnum.AND:
                    return new SQLUnaryElement("AND");                    
                case ConditionEnum.OR:
                    return new SQLUnaryElement("OR");                    
                default:
                    throw new Exception("Unknown ");
            }
        }

        /// <summary>
        /// 清除所有條件
        /// </summary>
        public void Clear()
        {
            listSQLElement.Clear();
            listOrderBy.Clear();
        }       

        public string ToString(IDataBase database)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ISQLElement sqlElement in listSQLElement)
            {
                sb.Append( 
                    (sb.Length <= 0 ? "" : " ") 
                    + sqlElement.ToString(database));
            }

            // 組OrderBy的部份
            if (listOrderBy.Count > 0)
            {
                sb.Append(" ORDER BY ");
                sb.Append( string.Join(",", listOrderBy.ToArray()) );               
            }

            return sb.ToString();
        }
    }
}
