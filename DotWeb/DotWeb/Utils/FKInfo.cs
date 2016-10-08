using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotWeb.Utils
{
    public class FKInfo
    {
        public string ConstraintName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public int OrdinalPosition { get; set; }
        public string RefConstraintName { get; set; }
        public string RefTableName { get; set; }
        public string RefColumnName { get; set; }
        public int RefOrdinalPosition { get; set; }
    }
}
