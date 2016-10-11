using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DotWeb
{
    /// <summary>
    /// TableMeta holds meta data information about database tables for auto-generation purpose.
    /// </summary>
    public class TableMeta
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TableMeta()
        {
            Columns = new List<ColumnMeta>();
            Children = new List<TableMetaRelation>();
            Parents = new List<TableMetaRelation>();
        }

        /// <summary>
        /// Id of TableMeta, auto-generated.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the table being inspected.
        /// </summary>
        [Required, MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Caption can be used as page title when displaying data to a web page.
        /// Separating Caption and Name makes it possible to rename Caption without affecting 
        /// the table being inspected.
        /// </summary>
        [Required, MaxLength(100)]
        public string Caption { get; set; }

        /// <summary>
        /// SQL Server schema, by default dbo, if a table has no explicit schema.
        /// </summary>
        [Required, MaxLength(100)]
        public string SchemaName { get; set; }

        /// <summary>
        /// The list of all table column meta information.
        /// </summary>
        public IList<ColumnMeta> Columns { get; set; }

        /// <summary>
        /// A list of child tables related to this table. Child table is a table with foreign
        /// key to this table.
        /// </summary>
        public IList<TableMetaRelation> Children { get; set; }

        /// <summary>
        /// A list of parent tables related to this table. Parent table is a table which a foreign key
        /// in this table refers to.
        /// </summary>
        public IList<TableMetaRelation> Parents { get; set; }

        /// <summary>
        /// The application Id which this table meta belongs to.
        /// </summary>
        public int AppId { get; set; }

        /// <summary>
        /// This property is related to <see cref="AppId"/> property.
        /// </summary>
        public virtual App App { get; set; }

        /// <summary>
        /// An array of references to the primary key column.
        /// </summary>
        [NotMapped]
        public ColumnMeta[] PrimaryKeys
        {
            get 
            {
                var primaryKeys = Columns.Where(c => c.IsPrimaryKey == true).ToArray();
                if (primaryKeys.Length == 0)
                    throw new ArgumentException(string.Format("Table {0} does not have primary keys.", this.Name));
                return primaryKeys;
            }
        }

        /// <summary>
        /// The column which should become the text for lookup purpose (for example in combobox).
        /// </summary>
        public int? LookUpDisplayColumnId { get; set; }

        /// <summary>
        /// This property is related to <see cref="LookUpDisplayColumnId"/>.
        /// </summary>
        public virtual ColumnMeta LookUpDisplayColumn { get; set; } 

        /// <summary>
        /// String representation of TableMeta.
        /// </summary>
        /// <returns>String, should match the Name property.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
