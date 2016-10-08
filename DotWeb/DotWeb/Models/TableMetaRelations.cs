using System.ComponentModel.DataAnnotations;

namespace DotWeb
{
    /// <summary>
    /// This class holds information about table relations in a database.
    /// A table relation can have 1:1, 1:M, M:1, and M:M. 
    /// For simplicity, we call the principal table (left side) as parent, 
    /// and dependant table (right side) as child.
    /// </summary>
    public class TableMetaRelation
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TableMetaRelation()
        {
            IsRendered = true;
        }

        /// <summary>
        /// Id of TableMetaRelation, auto-generated.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Id of the parent table.
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// This property is related to <see cref="ParentId"/>.
        /// </summary>
        public virtual TableMeta Parent { get; set; }

        /// <summary>
        /// Id of the child table.
        /// </summary>
        public int ChildId { get; set; }

        /// <summary>
        /// This property is related to <see cref="ChildId"/>
        /// </summary>
        public virtual TableMeta Child { get; set; }

        /// <summary>
        /// The foreign key in the child table to parent table.
        /// </summary>
        [Required, MaxLength(100)]
        public string ForeignKeyName { get; set; } 

        /// <summary>
        /// When rendered in master detail grid view, this property can serve as the title of
        /// detail table.
        /// </summary>
        [MaxLength(100)]
        public string Caption { get; set; }

        /// <summary>
        /// Set to false if you don't want the child table rendered in a master-detail grid view.
        /// </summary>
        public bool IsRendered { get; set; }

        /// <summary>
        /// String representation of <see cref="TableMetaRelation"/> class.
        /// </summary>
        /// <returns>String, should match the string representaion of <see cref="Child"/> property.</returns>
        public override string ToString()
        {
            return Child.ToString();
        }
    }
}
