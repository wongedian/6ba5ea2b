using System;
using System.ComponentModel.DataAnnotations;

namespace DotWeb
{
    /// <summary>
    /// Table column meta information for auto-generation purpose.
    /// </summary>
    public class ColumnMeta
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ColumnMeta()
        {
            DataType = TypeCode.Empty;
            IsForeignKey = false;
            IsPrimaryKey = false;
            DisplayInGrid = true;
        }

        /// <summary>
        /// Id of ColumnMeta, auto-generated.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the column being inspected.
        /// </summary>
        [Required, MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Caption can be used as column title in a grid view or label in a form view.
        /// Separating Caption and Name makes it possible to rename Caption without affecting 
        /// the column being inspected.
        /// </summary>
        [Required, MaxLength(100)]
        public string Caption { get; set; }

        /// <summary>
        /// Data type of the column.
        /// </summary>
        public TypeCode DataType { get; set; }

        /// <summary>
        /// True means the column is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Some data types can have maximum character length it can accept. 
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// It determines the position when rendered in grid view or form view.
        /// </summary>
        public int OrderNo { get; set; }

        /// <summary>
        /// You can turn this off to hide it from a grid view.
        /// </summary>
        public bool DisplayInGrid { get; set; }

        /// <summary>
        /// Special case if the integer data type has a meaning that can be translated to
        /// enumeration that comes from system libraries. The enumeration type name can be
        /// put here, so the editor can display combo box of enum values instead of plain text box.
        /// </summary>
        [MaxLength(100)]
        public string EnumTypeName { get; set; }

        /// <summary>
        /// If the column is a foreign key to other table, set this to true.
        /// </summary>
        public bool IsForeignKey { get; set; }

        /// <summary>
        /// If the column is a primary key, set this to true.
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// If the column is a foreign key, this property holds the table referenced by that key.
        /// </summary>
        public int? ReferenceTableId { get; set; }

        /// <summary>
        /// This property is related to <see cref="ReferenceTableId"/> property.
        /// </summary>
        public virtual TableMeta ReferenceTable { get; set; }

        /// <summary>
        /// The Id of TableMeta which this column belongs to.
        /// </summary>
        public int TableId { get; set; }

        /// <summary>
        /// This property is related to TableId.
        /// </summary>
        public virtual TableMeta Table { get; set; }

        /// <summary>
        /// String representation of ColumnMeta.
        /// </summary>
        /// <returns>String, should match the Name: DataType combination.</returns>
        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, DataType);
        }
    }
}
