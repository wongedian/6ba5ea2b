using System;

namespace DotWeb
{
    /// <summary>
    /// This option corresponds to DevExpress grid view edit mode. 
    /// </summary>
    public enum EditMode
    {
        Default,
        Inline,
        Popup,
        EditPage
    }

    /// <summary>
    /// Used in entity scaffolding to mark entities to scaffold.
    /// </summary>
    public class ScaffoldAttribute : Attribute
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ScaffoldAttribute()
        {
            Group = "";
            OrderNo = 0;
            EditMode = EditMode.Default;
        }

        /// <summary>
        /// Scaffolded entity also generates menu item. This property indicates the group where this menu item belongs to.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Menu item order number in the group.
        /// </summary>
        public int OrderNo { get; set; }

        /// <summary>
        /// The edit mode for this entity.
        /// </summary>
        public EditMode EditMode { get; set; }
    }
}
