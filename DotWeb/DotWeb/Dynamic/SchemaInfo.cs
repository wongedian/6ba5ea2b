using System.Collections.Generic;

namespace DotWeb
{
    /// <summary>
    /// This is the wrapper class for <see cref="App"/> and list of <see cref="TableMeta"/>.
    /// </summary>
    public class SchemaInfo
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SchemaInfo()
        {
            Tables = new List<TableMeta>();
        }

        /// <summary>
        /// The application object, see <see cref="App"/>.
        /// </summary>
        public App App { get; set; }

        /// <summary>
        /// List of <see cref="TableMeta"/> object.
        /// </summary>
        public List<TableMeta> Tables { get; set; }
    }
}
