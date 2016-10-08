using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotWeb
{
    /// <summary>
    /// App entity, represents application or web project.
    /// </summary>
    public class App
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public App() 
        {
            Groups = new List<Group>();
            GridTextColumnMaxLength = Constants.DefaultGridTextColumnMaxLength;
            PageSize = Constants.DefaultPageSize;
            DefaultGroupName = Constants.DefaultDefaultGroupName;
        }

        /// <summary>
        /// Id for application, should be manually assigned for flexibility choosing desired id for a particular application.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        /// <summary>
        /// Name of application, should be descriptive.
        /// </summary>
        [Required, MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Long description of application, if you wish to provide it.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Group represents group of modules. In the website interface, it will be displayed as left menu group.
        /// </summary>
        public ICollection<Group> Groups { get; set; }

        /// <summary>
        /// Used in grid view column to set maximum length of a string that can be displayed. 
        /// Exceeding this value will cause auto-truncation in grid view only (data is untouched).
        /// </summary>
        public int GridTextColumnMaxLength { get; set; }

        /// <summary>
        /// Default page size for gridview paging.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Default group name for auto-generation purpose.
        /// </summary>
        [Required, MaxLength(100)]
        public string DefaultGroupName { get; set; }

        /// <summary>
        /// String representation of application.
        /// </summary>
        /// <returns>String, should match name property.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
