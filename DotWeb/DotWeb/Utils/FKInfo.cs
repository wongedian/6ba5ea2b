
namespace DotWeb.Utils
{
    /// <summary>
    /// Foreign key info. Used in inspection process to store meta data information about foreign key.
    /// </summary>
    public class FKInfo
    {
        /// <summary>
        /// The foreign key name.
        /// </summary>
        public string ConstraintName { get; set; }

        /// <summary>
        /// Table containing the foreign key.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Foreign key column name.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Foreign key order no.
        /// </summary>
        public int OrdinalPosition { get; set; }

        /// <summary>
        /// Constraint (primary key) name in the referenced table.
        /// </summary>
        public string RefConstraintName { get; set; }

        /// <summary>
        /// Referenced table name.
        /// </summary>
        public string RefTableName { get; set; }

        /// <summary>
        /// Column (primary key) name in the referenced table.
        /// </summary>
        public string RefColumnName { get; set; }

        /// <summary>
        /// Order no of referenced column.
        /// </summary>
        public int RefOrdinalPosition { get; set; }
    }
}
