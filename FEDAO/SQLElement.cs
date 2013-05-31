using System;
using System.Collections.Generic;
using System.Text;

namespace YT.PT3.DAOBase
{
    #region 列舉型別
    /// <summary>
    /// 條件表示式
    /// </summary>
    public enum RelationEnum
    {
        Equal,
        NonEqual,
        Like,
        IN,
        SmallThan,
        SmallEqualThan,
        GreatThan,
        GreatEqualThan,
        IsNULL,
        IsNotNULL,
    }

    /// <summary>
    /// 關係表示式
    /// </summary>
    public enum ConditionEnum
    {
        AND,
        OR
    }

    /// <summary>
    /// 括號
    /// </summary>
    public enum BraketEnum
    {
        /// <summary>
        /// 左括號
        /// </summary>
        OpenBraket,
        /// <summary>
        /// 右括號
        /// </summary>
        CloseBraket
    }

    public enum SortEnum
    {
        ASC,
        DESC
    }
    #endregion

    /// <summary>
    /// SQL指令中的條件式
    /// 主要是將條件式依不同的資料庫實作來轉成各資料庫所能支援的SQL條件式
    /// </summary>
    public interface ISQLElement
    {
        string ToString(IDataBase database);
    }

    #region SQL指令中的條件式
    /// <summary>
    /// SQL指令中的條件式
    /// </summary>
    /// <typeparam name="DataType"></typeparam>
    public class SQLElement<DataType> : ISQLElement
    {
        public string ColName = "";
        public RelationEnum Relation = RelationEnum.Equal;
        public DataType SQLValue = default(DataType);

        public SQLElement(string colName, RelationEnum relation, DataType sqlValue)
        {
            ColName = colName;
            Relation = relation;
            SQLValue = sqlValue;
        }

        public string ToString(IDataBase database)
        {
            switch (Relation)
            {
                case RelationEnum.Equal:
                    return ColName + " = " + database.FieldToSQL((DataType)SQLValue);
                case RelationEnum.NonEqual:
                    return ColName + " <> " + database.FieldToSQL(SQLValue);
                case RelationEnum.GreatThan:
                    return ColName + " > " + database.FieldToSQL(SQLValue);
                case RelationEnum.GreatEqualThan:
                    return ColName + " >= " + database.FieldToSQL(SQLValue);
                case RelationEnum.SmallThan:
                    return ColName + " < " + database.FieldToSQL(SQLValue);
                case RelationEnum.SmallEqualThan:
                    return ColName + " <= " + database.FieldToSQL(SQLValue);
                case RelationEnum.Like:
                    return ColName + " LIKE " + database.FieldToSQL(SQLValue);
                case RelationEnum.IN:
                    return ColName + " IN (" + database.FieldToSQL(SQLValue) + ")";
                case RelationEnum.IsNULL:
                    return ColName + " IS NULL";
                case RelationEnum.IsNotNULL:
                    return ColName + " IS NOT NULL";
                default:
                    throw new Exception("Unknown RelationEnum");

            }
        }
    }

    /// <summary>
    /// SQL指令的Between條件式
    /// </summary>
    /// <typeparam name="DataType"></typeparam>
    public class SQLBetweenElement<DataType> : ISQLElement
    {
        public string ColName = "";
        public DataType SQLValue1 = default(DataType);
        public DataType SQLValue2 = default(DataType);

        public SQLBetweenElement(string colName, DataType sqlValue1, DataType sqlValue2)
        {
            ColName = colName;
            SQLValue1 = sqlValue1;
            SQLValue2 = sqlValue2;
        }

        public string ToString(IDataBase database)
        {
            return ColName + " between " + database.FieldToSQL(SQLValue1) + " and " + database.FieldToSQL(SQLValue2);
        }
    }

    ///// <summary>
    ///// SQL指令中的括號元素
    ///// </summary>
    //public class SQLBraketElement : ISQLElement
    //{
    //    protected BraketEnum braket = BraketEnum.OpenBraket; 

    //    public SQLBraketElement( BraketEnum braket )
    //    {
    //        this.braket = braket;
    //    }

    //    public string ToString( IDataBase database )
    //    {
    //        switch( braket)
    //        {
    //            case BraketEnum.OpenBraket:
    //                return "(";
    //            case BraketEnum.CloseBraket:
    //                return ")";
    //            default:
    //                throw new Exception("Unknown BraketEnum");                       
    //        }
    //    }
    //}

    /// <summary>
    /// SQL指令中的單一條件，例如 (,),AND,OR
    /// </summary>
    public class SQLUnaryElement : ISQLElement
    {
        protected string unaryOperator = "";

        public SQLUnaryElement(string unaryOperator)
        {
            this.unaryOperator = unaryOperator;
        }

        public string ToString(IDataBase database)
        {
            return unaryOperator;
        }
    }
    #endregion
}
