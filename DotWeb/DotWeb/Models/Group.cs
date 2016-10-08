﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotWeb
{
    /// <summary>
    /// Group represent grouping of modules in an application. There are two main purposes of grouping:
    /// first for navigation, second for security. For navigation, left menu items are grouped based on
    /// their functionalities. For security, we can grant or restrict access on group basis.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Group()
        {
            Modules = new List<Module>();
            ShowInLeftMenu = true;
        }

        /// <summary>
        /// Id of group, auto-generated.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Title of group which is displayed on the left menu navigation.
        /// Initially the value is taken from Name property, but it can be changed later in
        /// web admin.
        /// </summary>
        [Required, MaxLength(100)]
        public string Title { get; set; }

        /// <summary>
        /// Name of group. In autogenerated scenario, the value matches the table name.
        /// </summary>
        [Required, MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Order number in left menu navigation.
        /// </summary>
        public int OrderNo { get; set; }

        /// <summary>
        /// Collection of module belonging to this group.
        /// </summary>
        public ICollection<Module> Modules { get; set; }

        /// <summary>
        /// If you don't want this group to show in left menu navigation, set this to false.
        /// </summary>
        public bool ShowInLeftMenu { get; set; }

        /// <summary>
        /// The application Id which this group belongs to.
        /// </summary>
        public int AppId { get; set; }

        /// <summary>
        /// This property is related to <see cref="AppId"/> property.
        /// </summary>
        public virtual App App { get; set; }

        /// <summary>
        /// String representation of group.
        /// </summary>
        /// <returns>String, should match the Name property.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}