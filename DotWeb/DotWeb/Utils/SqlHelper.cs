using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotWeb.Utils
{
    public static class SqlHelper
    {
        public static string GenerateSelectQuery(TableMeta tableMeta)
        {
            string columnList = string.Join(", ", tableMeta.Columns.Select(m => "[" + m.Name + "]"));
            return string.Format("SELECT {0} FROM {1}.{2}", columnList, tableMeta.SchemaName, tableMeta.Name);
        }

        public static string GenerateSelectQueryDetail(TableMeta tableMeta, ColumnMeta foreignKey)
        {
            string columnList = string.Join(", ", tableMeta.Columns.Select(m => "[" + m.Name + "]"));
            return string.Format("SELECT {0} FROM {1}.{2} WHERE {3} = @{3}", columnList, tableMeta.SchemaName, tableMeta.Name, foreignKey.Name);
        }

        public static string GenerateInsertQuery(TableMeta tableMeta)
        {
            string columnList = string.Format("{0}{1}{2}", "(", string.Join(", ", tableMeta.Columns.Where(c => c.IsPrimaryKey == false).Select(m => "[" + m.Name + "]")), ")");
            string argumentList = string.Format("{0}{1}{2}", "(", string.Join(", ", tableMeta.Columns.Where(c => c.IsPrimaryKey == false).Select(m => "@" + m.Name )), ")");

            return string.Format("INSERT INTO {0}.{1} {2} VALUES {3}", tableMeta.SchemaName, tableMeta.Name, columnList, argumentList);
        }

        public static string GenerateUpdateQuery(TableMeta tableMeta)
        {
            var sb = new StringBuilder();
            foreach (var column in tableMeta.Columns)
            {
                if (!column.IsPrimaryKey)
                    sb = sb.AppendFormat("[{0}] = @{0},", column.Name);
            }
            // remove the last comma
            sb.Length--;

            return string.Format("UPDATE {0}.{1} SET {2} WHERE {3} ", tableMeta.SchemaName, tableMeta.Name, sb.ToString(),
                GenerateWhereConditionForPrimaryKeys(tableMeta.PrimaryKeys));
        }

        public static string GenerateDeleteQuery(TableMeta tableMeta)
        {
            return string.Format("DELETE FROM {0}.{1} WHERE {2} = @{2}", tableMeta.SchemaName, tableMeta.Name,
                GenerateWhereConditionForPrimaryKeys(tableMeta.PrimaryKeys));
        }

        private static string GenerateWhereConditionForPrimaryKeys(ColumnMeta[] primaryKeys)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < primaryKeys.Length; i++)
            {
                sb = sb.AppendFormat("{0} = @{0} ", primaryKeys[i].Name);
                if (i < primaryKeys.Length)
                    sb = sb.Append("AND ");
            }
            return sb.ToString();
        }

        public static DataTable GetDataTable(string sqlCommand, string connectionStringName)
        {
            using (var connection = OpenConnection(connectionStringName))
            {
                var dataAdapter = new SqlDataAdapter(sqlCommand, connection as SqlConnection);
                var dataSet = new DataSet();
                dataAdapter.Fill(dataSet);

                return dataSet.Tables[0];
            }
        }

        public static DbConnection OpenConnection(string connectionStringName)
        {
            if (ConfigurationManager.ConnectionStrings[connectionStringName] == null)
                throw new ArgumentException("connectionStringName");

            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ToString();
            var connection = new SqlConnection(connectionString);
            connection.Open();

            return connection;
        }

        public static List<FKInfo> GetDbSchemaInfo(string connectionStringName)
        {
            var sql = "SELECT  " +
                      "   KCU1.CONSTRAINT_NAME AS FK_CONSTRAINT_NAME " +
                      "  ,KCU1.TABLE_NAME AS FK_TABLE_NAME " +
                      "  ,KCU1.COLUMN_NAME AS FK_COLUMN_NAME " +
                      "  ,KCU1.ORDINAL_POSITION AS FK_ORDINAL_POSITION " +
                      "  ,KCU2.CONSTRAINT_NAME AS REFERENCED_CONSTRAINT_NAME " +
                      "  ,KCU2.TABLE_NAME AS REFERENCED_TABLE_NAME " +
                      "  ,KCU2.COLUMN_NAME AS REFERENCED_COLUMN_NAME " +
                      "  ,KCU2.ORDINAL_POSITION AS REFERENCED_ORDINAL_POSITION " +
                      "FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS RC " +

                      "INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU1 " +
                      "  ON KCU1.CONSTRAINT_CATALOG = RC.CONSTRAINT_CATALOG  " +
                      "  AND KCU1.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA " +
                      "  AND KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME " +

                      "INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU2 " +
                      "  ON KCU2.CONSTRAINT_CATALOG = RC.UNIQUE_CONSTRAINT_CATALOG  " +
                      "  AND KCU2.CONSTRAINT_SCHEMA = RC.UNIQUE_CONSTRAINT_SCHEMA " +
                      "  AND KCU2.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME " +
                      "  AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION ";

            var dt = GetDataTable(sql, connectionStringName);
            var result = new List<FKInfo>();
            foreach (DataRow row in dt.Rows)
            {
                var fkInfo = new FKInfo();
                fkInfo.ConstraintName = row["FK_CONSTRAINT_NAME"].ToString();
                fkInfo.TableName = row["FK_TABLE_NAME"].ToString();
                fkInfo.ColumnName = row["FK_COLUMN_NAME"].ToString();
                fkInfo.OrdinalPosition = int.Parse(row["FK_ORDINAL_POSITION"].ToString());
                fkInfo.RefConstraintName = row["REFERENCED_CONSTRAINT_NAME"].ToString();
                fkInfo.RefTableName = row["REFERENCED_TABLE_NAME"].ToString();
                fkInfo.RefColumnName = row["REFERENCED_COLUMN_NAME"].ToString();
                fkInfo.RefOrdinalPosition = int.Parse(row["REFERENCED_ORDINAL_POSITION"].ToString());
                result.Add(fkInfo);
            }
            return result;
        }

    }
}
